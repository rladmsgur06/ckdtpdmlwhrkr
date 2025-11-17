#region
#if UNITY_EDITOR
using TwoBitMachines.Editors;
using UnityEditor;
#endif
#endregion
using TwoBitMachines.FlareEngine.Interactables;
using UnityEngine;

namespace TwoBitMachines.FlareEngine.ThePlayer
{
        [AddComponentMenu("")]
        public class Swim : Ability //* animation signals: floating, swimming, inWater
        {
                //*Float
                [SerializeField] private float spring = 1f;
                [SerializeField] private float damping = 0.2f;
                [SerializeField] private float swimEasingX = 4f;
                //*Swim 
                [SerializeField] private float weight = 0.05f;

                //*Both
                [SerializeField] private float jumpUp = 15;
                [SerializeField] private float waterImpact = 0.25f;
                [SerializeField] private float waterResistanceX = 0.5f;
                [SerializeField] private float waterResistanceY = 0.1f;

                [SerializeField] public string exitWE;
                [SerializeField] public string enterWE;
                [SerializeField] public string switchButton;

                [SerializeField] private UnityEventEffect onEnter;
                [SerializeField] private UnityEventEffect onExit;

                //*Bubble particles
                [SerializeField] private WaterBubble bubbles;

                //*Reset variables
                [System.NonSerialized] private SwimType swimType;
                [System.NonSerialized] private Water water = null;

                [System.NonSerialized] private Vector2 waveTopPoint;
                [System.NonSerialized] private float springVelocity;
                [System.NonSerialized] private float floatCounter;
                [System.NonSerialized] private float jumpCounter;
                [System.NonSerialized] private float oldVelX = 0;
                [System.NonSerialized] private float counterX = 0;
                [System.NonSerialized] private int particleIndex;

                [System.NonSerialized] private bool submerged;
                [System.NonSerialized] private bool canJump;
                [System.NonSerialized] private bool inWater;
                [System.NonSerialized] private bool isFloating;
                [System.NonSerialized] private bool wasInWater;
                [System.NonSerialized] private bool wasFloating;
                [System.NonSerialized] private bool jumpedFromWater;

                #region Ability and Search Methods
                private void Start ()
                {
                        bubbles = gameObject.GetComponentInChildren<WaterBubble>(true);
                }

                public override void Reset (AbilityManager player)
                {
                        jumpedFromWater = false;
                        wasFloating = false;
                        wasInWater = false;
                        isFloating = false;
                        inWater = false;
                        submerged = false;
                        springVelocity = 0;
                        particleIndex = 0;
                        floatCounter = 0;
                        jumpCounter = 0;
                        swimType = 0;
                        water = null;
                }

                public override bool TurnOffAbility (AbilityManager player)
                {
                        Reset(player);
                        return true;
                }

                public override bool IsAbilityRequired (AbilityManager player, ref Vector2 velocity)
                {
                        if (pause)
                        {
                                return false;
                        }
                        if (bubbles != null)
                        {
                                bubbles.Execute(water, inWater);
                        }

                        wasFloating = isFloating;
                        wasInWater = inWater;
                        isFloating = false;
                        inWater = false;

                        if (jumpedFromWater && velocity.y > 0)
                        {
                                return false;
                        }
                        return FoundWater(player.world.box, player.ground, ref velocity);
                }

                private bool FoundWater (BoxInfo box, bool onGround, ref Vector2 velocity)
                {
                        jumpedFromWater = false;
                        if (onGround)
                                canJump = false;
                        Vector2 offset = wasFloating ? Vector2.down * 0.5f : Vector2.zero;
                        Vector2 entryPoint = box.bottomCenter + offset;

                        for (int i = 0; i < Water.water.Count; i++)
                        {
                                if (Water.water[i] != null && Water.water[i].FoundWater(entryPoint, wasInWater, ref waveTopPoint, ref particleIndex))
                                {
                                        water = Water.water[i];
                                        if (box.top > waveTopPoint.y && onGround) //   water became shallow
                                        {
                                                return false;
                                        }
                                        if (!wasInWater)
                                        {
                                                EnterWater(box, water, ref velocity);
                                        }
                                        return true;
                                }
                        }
                        return false;
                }

                private void EnterWater (BoxInfo box, Water water, ref Vector2 velocity)
                {
                        jumpCounter = 0;
                        submerged = false;
                        swimType = water.swimType;
                        onEnter.Invoke(ImpactPacket.impact.Set(enterWE, transform, box.collider, waveTopPoint, null, Vector2.up, 1, 0));

                        if (box.top > waveTopPoint.y - 1f) //  dont splash if player enters from the side of the water
                        {
                                water.ApplyImpact(particleIndex, waterImpact * 0.1f * Mathf.Clamp(velocity.y, -10f, 5f));
                        }
                        if (water.swimType == SwimType.Swim)
                        {
                                velocity.y *= 0.35f; //       slow down entry to prevent quick sinking
                        }
                        springVelocity = velocity.y = Mathf.Clamp(velocity.y, -10f, 5f);
                }

                public override void ExecuteAbility (AbilityManager player, ref Vector2 velocity, bool isRunningAsException = false)
                {
                        if (water == null)
                                return;

                        player.world.hitInteractable = true;
                        player.signals.Set("inWater", inWater = true);
                        bool canSwitch = water.canSwitch == SwitchSwimTypes.Yes;
                        SwimType swim = canSwitch ? swimType : water.swimType;

                        if (swim == SwimType.Swim)
                        {
                                Swimming(player, ref velocity, canSwitch);
                        }
                        else
                        {
                                Floating(player, ref velocity, canSwitch);
                        }
                }
                #endregion

                private void Floating (AbilityManager player, ref Vector2 velocity, bool canSwitch = false)
                {
                        player.signals.Set("floating", isFloating = true);

                        //* Spring Mechanics
                        float acceleration = spring * (waveTopPoint.y - (player.world.box.center.y + player.world.box.sizeY * 0.1f)) - springVelocity * damping;
                        springVelocity += acceleration * Time.deltaTime * 10f;
                        springVelocity = Mathf.Clamp(springVelocity, -10f, 6f);
                        velocity.y = springVelocity;

                        if (Time.deltaTime != 0 && velocity.y > 0 && (player.world.box.bottom + velocity.y * Time.deltaTime) >= waveTopPoint.y)
                        {
                                velocity.y = (((waveTopPoint.y - 0.1f) - player.world.box.bottom) / Time.deltaTime) * 0.5f; // clamp player y velocity if being shot out of water too fast
                                springVelocity = 0; //
                        }

                        velocity.x -= water.wave.currentStrength;
                        velocity.x *= (1f - waterResistanceX);
                        SwimEasingX(player, ref velocity);
                        if (Clock.Timer(ref floatCounter, 1f))
                        {
                                water.ApplyImpact(particleIndex, -waterImpact * 0.15f, 3); // floating ripple
                        }
                        if (velocity.x != 0)
                        {
                                water.ApplyImpact(particleIndex + (int) Mathf.Sign(velocity.x), waterImpact * 0.02f * Mathf.Abs(velocity.x * 0.1f), 4);
                        }
                        if (Clock.TimerExpired(ref jumpCounter, 0.1f) || player.world.box.top <= waveTopPoint.y)
                        {
                                submerged = true;
                        }
                        if (canSwitch && player.inputs.Holding(switchButton))
                        {
                                springVelocity += player.gravityEffect * 0.25f; // make player sink beneath water line before switching type
                                if (player.world.box.top < waveTopPoint.y)
                                {
                                        swimType = SwimType.Swim;
                                }
                        }
                        Jump(player, player.jumpButtonPressed, ref velocity);
                }

                private void Swimming (AbilityManager player, ref Vector2 velocity, bool canSwitch = false)
                {
                        player.signals.Set("swimming");
                        velocity.x -= water.wave.currentStrength;
                        velocity.x *= (1f - waterResistanceX);
                        float playerTop = player.world.box.top;

                        SwimEasingX(player, ref velocity);
                        if (playerTop <= waveTopPoint.y)
                        {
                                submerged = canJump = true;
                        }
                        if (player.jumpButtonPressed && canJump)
                        {
                                velocity.y = player.maxJumpVel * (1f - waterResistanceY); //        thrust, jump
                                Jump(player, true, ref velocity);
                        }
                        else if (velocity.y >= 0)
                        {
                                velocity.y -= player.gravityEffect; // undo gravity
                                velocity.y += player.gravityEffect * 0.70f;
                        }
                        else if (velocity.y < 0)
                        {
                                float sinkRate = 1f - weight;
                                velocity.y -= player.gravityEffect * sinkRate;
                        }
                        if (canSwitch && playerTop > waveTopPoint.y)
                        {
                                springVelocity = velocity.y;
                                swimType = SwimType.Float;
                        }
                        if (submerged && player.world.box.bottom > waveTopPoint.y)
                        {
                                ExitWater(player, ref velocity);
                        }
                }

                private void SwimEasingX (AbilityManager player, ref Vector2 velocity)
                {
                        if (player.inputX == 0 && swimEasingX < 4f)
                        {
                                counterX += Time.deltaTime * Mathf.Abs(swimEasingX);
                                velocity.x = Mathf.Lerp(oldVelX, 0f, Mathf.Clamp01(counterX));
                        }
                        else
                        {
                                oldVelX = velocity.x;
                                counterX = 0;
                        }
                }

                private void Jump (AbilityManager player, bool buttonPressed, ref Vector2 velocity)
                {
                        if (jumpUp > 0 && buttonPressed && (submerged || canJump) && player.world.box.top > waveTopPoint.y)
                        {
                                ExitWater(player, ref velocity);
                        }
                }

                private void ExitWater (AbilityManager player, ref Vector2 velocity)
                {
                        velocity.y = jumpUp;
                        jumpCounter = 0;
                        jumpedFromWater = true;
                        onExit.Invoke(ImpactPacket.impact.Set(exitWE, player.world.transform, player.world.boxCollider, waveTopPoint, null, Vector2.down, 1, 0));
                        player.signals.Set("inWater", false);
                        player.signals.Set("swimming", false);
                        player.signals.Set("floating", false);
                        inWater = isFloating = submerged = false;
                        water.ApplyImpact(particleIndex, waterImpact * 0.025f * velocity.y);
                        player.CheckForAirJumps();
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField, HideInInspector] private bool eventsFoldOut;
                [SerializeField, HideInInspector] private bool eventExitFoldOut;
                [SerializeField, HideInInspector] private bool eventEnterFoldOut;
                public override bool OnInspector (SerializedObject controller, SerializedObject parent, string[] inputList, Color barColor, Color labelColor)
                {
                        if (Open(parent, "Swim", barColor, labelColor))
                        {
                                FoldOut.Bar(FoldOut.boxColorLight).Label("Float", FoldOut.titleColor);
                                {
                                        FoldOut.Box(2, FoldOut.boxColorLight, offsetY: -2);
                                        parent.Slider("Spring", "spring", 0.25f, 1f);
                                        parent.Slider("Damping", "damping", 0.01f, 0.5f);
                                }
                                Layout.VerticalSpacing(3);

                                FoldOut.Bar(FoldOut.boxColorLight).Label("Swim", FoldOut.titleColor);
                                {
                                        FoldOut.Box(1, FoldOut.boxColorLight, offsetY: -2);
                                        parent.Slider("Weight", "weight", 0, 1f);
                                }
                                Layout.VerticalSpacing(3);

                                FoldOut.Box(2, FoldOut.boxColorLight);
                                {
                                        parent.DropDownList(inputList, "Switch Button", "switchButton");
                                        parent.Field("Jump Out", "jumpUp");
                                }
                                Layout.VerticalSpacing(5);

                                FoldOut.Box(4, FoldOut.boxColorLight, extraHeight: 3);
                                {
                                        parent.Slider("Water Impact", "waterImpact", 0, 0.5f);
                                        parent.Slider("Water Friction X", "waterResistanceX", 0.01f, 1f);
                                        parent.Slider("Water Friction Y", "waterResistanceY", 0.01f, 1f);
                                        parent.Slider("Swim Easing X", "swimEasingX", 0.01f, 4f);
                                }

                                if (FoldOut.FoldOutButton(parent.Get("eventsFoldOut")))
                                {
                                        Fields.EventFoldOutEffect(parent.Get("onEnter"), parent.Get("enterWE"), parent.Get("eventEnterFoldOut"), "On Enter Water", color: FoldOut.boxColorLight);
                                        Fields.EventFoldOutEffect(parent.Get("onExit"), parent.Get("exitWE"), parent.Get("eventExitFoldOut"), "On Exit Water", color: FoldOut.boxColorLight);
                                }

                        }
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion

        }
}
