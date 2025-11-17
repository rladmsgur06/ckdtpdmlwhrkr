using System.Collections.Generic;
using UnityEngine;

namespace TwoBitMachines.FlareEngine.ThePlayer
{
        [System.Serializable]
        public class AbilityManager
        {
                [SerializeField] public Walk walk = new Walk(); //* default abilities, always run
                [SerializeField] public Gravity gravity = new Gravity();

                [System.NonSerialized] public UserInputs inputs;
                [System.NonSerialized] public Character character;
                [System.NonSerialized] public WorldCollision world;
                [System.NonSerialized] public AnimationSignals signals;

                [System.NonSerialized] public Vector2 velocity;
                [System.NonSerialized] public Vector2 finalVelocity;
                [System.NonSerialized] public Vector2 externalVelocity;

                [System.NonSerialized] public float speed;
                [System.NonSerialized] public float inputX;
                [System.NonSerialized] public float velocityX;
                [System.NonSerialized] public float jumpBoost;
                [System.NonSerialized] public float maxJumpVel;
                [System.NonSerialized] public float minJumpVel;
                [System.NonSerialized] public float dashBoost;
                [System.NonSerialized] public float velocityOnGround;

                [System.NonSerialized] public bool jumpButtonHold;
                [System.NonSerialized] public bool jumpButtonActive;
                [System.NonSerialized] public bool jumpButtonPressed;
                [System.NonSerialized] public bool jumpButtonReleased;

                [System.NonSerialized] public bool wasJumping;
                [System.NonSerialized] public bool hasJumped;
                [System.NonSerialized] public bool onSurface;
                [System.NonSerialized] public bool onVehicle;
                [System.NonSerialized] public bool holdingDown;
                [System.NonSerialized] public bool pressedDown;
                [System.NonSerialized] public bool pushBackActive;
                [System.NonSerialized] public bool checkForAirJumps;
                [System.NonSerialized] public bool checkForMomentum;
                [System.NonSerialized] public bool airMomentumActive;
                [System.NonSerialized] public bool dashAirJumpCheck;
                [System.NonSerialized] public bool lockVelX;

                [System.NonSerialized] public int setAirJump;
                [System.NonSerialized] public int airJumpCount;
                [System.NonSerialized] public int playerDirection;

                [System.NonSerialized] private int priorityID;
                [System.NonSerialized] private int priorityIndex;

                [System.NonSerialized] private List<Ability> ability;
                [System.NonSerialized] private List<Ability> buffer = new List<Ability>();

                public float gravityEffect => gravity.gravityEffect;
                public bool ground => onSurface || world.onGround;
                public bool tryingToJump => jumpButtonPressed || jumpButtonHold;
                public bool crouching => world.boxCollider.size.y != world.box.boxSize.y;

                public void Initialize (Character characterRef, UserInputs inputRef, List<Ability> abilityRef, Player playerRef)
                {
                        character = characterRef;
                        world = characterRef.world;
                        signals = playerRef.signals;
                        ability = abilityRef;
                        inputs = inputRef;

                        dashBoost = 1f;
                        playerDirection = 1;
                        gravity.Initialize();
                        walk.Initialize();

                        for (int i = 0; i < abilityRef.Count; i++)
                        {
                                abilityRef[i].Initialize(playerRef);
                        }
                }

                public void ResetAll ()
                {
                        dashBoost = 1f;
                        playerDirection = 1;
                        velocityOnGround = 0;
                        velocity = Vector2.zero;
                        finalVelocity = Vector2.zero;
                        externalVelocity = Vector2.zero;

                        jumpButtonReleased = false;
                        jumpButtonPressed = false;
                        airMomentumActive = false;
                        dashAirJumpCheck = false;
                        jumpButtonActive = false;
                        checkForAirJumps = false;
                        checkForMomentum = false;
                        pushBackActive = false;
                        jumpButtonHold = false;
                        holdingDown = false;
                        hasJumped = false;
                        onSurface = false;
                        onVehicle = false;
                        wasJumping = false;
                        lockVelX = false;

                        walk.Reset();
                        world.Reset();
                        signals.SetDirection(1);

                        for (int i = 0; i < ability.Count; i++)
                        {
                                ability[i].Reset(this);
                        }
                }

                public void Execute ()
                {
                        buffer.Clear();
                        world.hasJumped = false;
                        wasJumping = hasJumped;
                        onSurface = false;
                        hasJumped = false;
                        lockVelX = false;
                        priorityIndex = 0;
                        priorityID = int.MaxValue;

                        GetDownAndJumpInputs();
                        gravity.Execute(world.onCeiling || ground, ref velocity);
                        walk.Execute(this, world, ref velocity);


                        FindActiveAbilities();
                        FindTopPriority();
                        TurnOffAbilitiesSafely();
                        ExecuteAbilities();
                        LateExecuteAbilities();


                        finalVelocity = velocity + externalVelocity;
                        character.initialVelocity = finalVelocity;
                        externalVelocity = Vector2.zero;
                }

                private void FindActiveAbilities ()
                {
                        for (int i = 0; i < ability.Count; i++)
                        {
                                ability[i].EarlyExecute(this, ref velocity); // executes before isAbilityRequired
                        }
                        for (int i = 0; i < ability.Count; i++)
                        {
                                if (ability[i].IsAbilityRequired(this, ref velocity))
                                {
                                        buffer.Add(ability[i]);
                                }
                        }
                }

                private void FindTopPriority ()
                {
                        for (int i = 0; i < buffer.Count; i++)
                        {
                                if (buffer[i].ID < priorityID)
                                {
                                        priorityID = buffer[i].ID;
                                        priorityIndex = i;
                                }
                        }
                }

                private void TurnOffAbilitiesSafely ()
                {
                        if (buffer.Count == 0 || priorityIndex >= buffer.Count)
                        {
                                return;
                        }

                        for (int i = 0; i < buffer.Count; i++)
                        {
                                if (i != priorityIndex && !buffer[priorityIndex].ContainsException(buffer[i].abilityName))
                                {
                                        if (!buffer[i].TurnOffAbility(this)) //                ability can't turn off safely, so it takes priority, ie crouch and wall
                                        {
                                                buffer[priorityIndex].TurnOffAbility(this); // turn off main ability and remove from list
                                                buffer.RemoveAt(priorityIndex); //             priority Index should not execute since an ability it's trying to override can't turn off safely
                                                FindTopPriority();
                                                TurnOffAbilitiesSafely(); //                  keep removing abilities until none of them conflict
                                                return;
                                        }
                                }
                        }
                }
                private void ExecuteAbilities ()
                {
                        if (buffer.Count > 0 && priorityIndex < buffer.Count)
                        {
                                buffer[priorityIndex].ExecuteAbility(this, ref velocity); //   execute ability with highest priority

                                for (int i = 0; i < buffer.Count; i++)
                                {
                                        if (i != priorityIndex && buffer[priorityIndex].ContainsException(buffer[i].abilityName))
                                        {
                                                buffer[i].ExecuteAbility(this, ref velocity, true); //  execute abilities with exceptions
                                        }
                                }
                        }
                }

                private void LateExecuteAbilities ()
                {
                        for (int i = 0; i < ability.Count; i++)
                        {
                                ability[i].LateExecute(this, ref velocity);
                        }
                }

                public bool HigherPriority (string priorityCheck, int ID)
                {
                        return priorityID < ID && priorityIndex >= 0 && priorityIndex < ability.Count && !ability[priorityIndex].ContainsException(priorityCheck);
                }

                public void PostCollisionExecute (Vector2 velocity)
                {
                        for (int i = 0; i < ability.Count; i++)
                        {
                                ability[i].PostCollisionExecute(this, velocity);
                        }
                }

                public void PostAIExecute ()
                {
                        for (int i = 0; i < ability.Count; i++)
                        {
                                ability[i].PostAIExecute(this);
                        }
                }

                public void ClearYVelocity ()
                {
                        velocity.y = 0;
                }

                public void OnSurface (bool value = true)
                {
                        onSurface = value;
                }

                public void StopRun ()
                {
                        walk.runSmoothInVelocity = 0; // will need to make run into its own ability in Flare 2.0
                        walk.isRunning = false;
                }

                public void CheckForAirJumps (int setAirJumps = 0, bool setHasJumped = true)
                {
                        checkForAirJumps = true;
                        setAirJump = setAirJumps;
                        hasJumped = setHasJumped;
                }

                public void UpdateVelocityGround ()
                {
                        velocityOnGround = velocity.x;
                }

                private void GetDownAndJumpInputs ()
                {
                        holdingDown = inputs.Holding("Down");
                        pressedDown = inputs.Pressed("Down");
                        jumpButtonHold = inputs.Holding("Jump");
                        jumpButtonPressed = inputs.Pressed("Jump");
                        jumpButtonReleased = inputs.Released("Jump");
                        world.holdingDown = holdingDown;
                        world.pressedDown = holdingDown && jumpButtonHold;
                }

        }

}
