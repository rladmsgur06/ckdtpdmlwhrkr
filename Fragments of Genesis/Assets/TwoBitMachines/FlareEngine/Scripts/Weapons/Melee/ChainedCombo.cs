using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TwoBitMachines.FlareEngine
{
        [System.Serializable]
        public class ChainedCombo
        {
                [SerializeField] public LayerMask layer;
                [SerializeField] public InputButtonSO input;
                [SerializeField] public InputButtonSO input2;
                [SerializeField] public float delayTime = 0.25f;
                [SerializeField] public float earlyTime = 0.25f;
                [SerializeField] public bool hitToContinue;
                [SerializeField] public bool oneHitPerCombo;
                [SerializeField] public MeleeCollider enableCollider;
                [SerializeField] public List<MeleeCombo> combo = new List<MeleeCombo>();
                [SerializeField] public ButtonTrigger trigger = ButtonTrigger.OnPress;
                [SerializeField] public ButtonTrigger trigger2 = ButtonTrigger.OnPress;

                [SerializeField] public bool attackFromSleep = true;
                [SerializeField] public bool cancelOtherAttacks = false;

                [System.NonSerialized] public Collider2D collider2DRef;
                [System.NonSerialized] public bool inMelee;
                [System.NonSerialized] private bool clearSignal;
                [System.NonSerialized] private bool goToNextMelee;
                [System.NonSerialized] private bool blockOtherHits;
                [System.NonSerialized] private bool firstFrame = true;
                [System.NonSerialized] private float lateCounter = 0;
                [System.NonSerialized] private float earlyCounter = 0;
                [System.NonSerialized] private int originalDirection = 0;
                [System.NonSerialized] private int index = 0;

                [System.NonSerialized] private ContactFilter2D filter = new ContactFilter2D();
                [System.NonSerialized] private List<Collider2D> list = new List<Collider2D>();
                [System.NonSerialized] private List<Transform> targetList = new List<Transform>();

                private bool indexSafe => index >= 0 && index < combo.Count;
                public bool earlyFlag => indexSafe && combo[index].earlyFlag;
                public int inputs => input != null && input2 != null ? 2 : input != null ? 1 : input2 != null ? 1 : 0;

                #region
#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField, HideInInspector] private bool active;
                [SerializeField, HideInInspector] private bool foldOut;
                [SerializeField, HideInInspector] private bool comboFoldOut;
                [SerializeField, HideInInspector] private bool inputFoldOut;
                [SerializeField, HideInInspector] private bool propertiesFoldOut;
                [SerializeField, HideInInspector] private bool eventsFoldOut;
                [SerializeField, HideInInspector] private int signalIndex;
#pragma warning restore 0414
#endif
                #endregion

                public void Initialize (Collider2D collider2DRef)
                {
                        this.collider2DRef = collider2DRef;
                        WorldManager.RegisterInput(input);
                        WorldManager.RegisterInput(input2);
                }

                public bool Reset ()
                {
                        index = -1;
                        lateCounter = 0;
                        earlyCounter = 0;
                        firstFrame = true;
                        goToNextMelee = false;
                        inMelee = false;
                        if (collider2DRef != null)
                                collider2DRef.enabled = false;
                        return true;
                }

                public bool Attack ()
                {
                        bool mouseOverUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
                        if (mouseOverUI && EventSystem.current.currentSelectedGameObject != null && EventSystem.current.currentSelectedGameObject.gameObject.CompareTag("UIControl"))
                                mouseOverUI = false;
                        return !mouseOverUI && ValidateInputs();
                }

                public bool ValidateInputs ()
                {
                        if (input2 == null && input != null)
                                return input.Active(trigger);
                        return input != null && input2 != null ? input.Active(trigger) && input2.Active(trigger2) : false;
                }

                public bool ExecuteMelee (AnimationSignals signals, int direction, bool onGround, bool crouching, Vector2 position, float coolDown, ref Vector2 velocity)
                {
                        clearSignal = false;
                        bool isFirst = false;
                        if (firstFrame)
                        {
                                Reset();
                                firstFrame = false;
                                blockOtherHits = false;
                                inMelee = true;
                                isFirst = true;
                                goToNextMelee = true; // check immediately
                                filter.useLayerMask = true;
                                filter.useTriggers = true;
                                filter.layerMask = layer;
                                originalDirection = direction;
                                if (collider2DRef != null && enableCollider == MeleeCollider.EnableOnStart)
                                {
                                        collider2DRef.enabled = true;
                                }
                                for (int i = 0; i < combo.Count; i++)
                                        combo[i].Reset();
                                targetList.Clear();
                        }

                        if (indexSafe && combo[index].exitOnStateChange && combo[index].condition != AttackCondition.Anytime && combo[index].StateChanged(onGround, crouching))
                        {
                                return Reset();
                        }
                        if (goToNextMelee && SequenceFailed(isFirst || Attack() || earlyFlag, onGround, crouching, coolDown)) // play after runmeleeattack so animation signals can clear for a least a frame or they might get stuck
                        {
                                return true;
                        }
                        if (clearSignal)
                        {
                                return false;
                        }
                        if (!goToNextMelee && indexSafe)
                        {
                                RunMeleeAttack(signals, direction, onGround, position, ref velocity);
                        }
                        return false;
                }

                private bool SequenceFailed (bool attack, bool onGround, bool crouching, float coolDown)
                {
                        if (hitToContinue && indexSafe && !combo[index].readHitBox)
                        {
                                return Reset();
                        }
                        if (attack) //    go to next combo
                        {
                                int previousIndex = index;
                                if (indexSafe)
                                {
                                        int oldIndex = index;
                                        if (combo[index].goToFirstAirAttack) // jump attack hit ground, reset to first, or it will move to the next air attack in sequence
                                        {
                                                for (int i = 0; i < combo.Count; i++)
                                                {
                                                        if (combo[i].isAirAttack)
                                                        {
                                                                index = i - 1;
                                                                break;
                                                        }
                                                }
                                        }
                                        combo[oldIndex].Reset();
                                }
                                if (!GoToNextAttack(onGround, crouching, true))
                                {
                                        return Reset();
                                }
                                if (index >= combo.Count && attack && coolDown <= 0) // loop
                                {
                                        if (!GoToNextAttack(onGround, crouching, false))
                                        {
                                                return Reset();
                                        }
                                }
                                if (index >= combo.Count) // no more attacks
                                {
                                        return Reset();
                                }
                                if (previousIndex == index)
                                {
                                        clearSignal = true;
                                }
                                ResetNext();
                        }
                        else if (combo.Count <= 1 || index >= combo.Count || Clock.Timer(ref lateCounter, delayTime)) //  player did not press button in time to continue to next combo, exit
                        {
                                return Reset();
                        }
                        return false;
                }

                private bool GoToNextAttack (bool onGround, bool crouching, bool increment)
                {
                        index = increment ? index + 1 : 0;
                        for (int i = index; i < combo.Count; i++)
                        {
                                if (i < 0 || combo[i].isLocked || combo[i].Skip(onGround, crouching))
                                {
                                        index = i + 1;
                                        continue;
                                }
                                else if (combo[i].Exit(onGround, crouching))
                                {
                                        return false;
                                }
                                else
                                {
                                        break;
                                }
                        }
                        return true;
                }

                private void RunMeleeAttack (AnimationSignals signals, int direction, bool onGround, Vector2 position, ref Vector2 velocity)
                {
                        combo[index].PlayAnimation(collider2DRef, signals, direction, originalDirection, onGround, position, ref velocity); // set animation signal and apply velocity

                        if (Clock.TimerExpired(ref earlyCounter, earlyTime) && Attack())
                        {
                                combo[index].earlyFlag = true;
                        }
                        if (combo[index].earlyNext && Clock.TimerExpired(ref combo[index].earlyCounter, combo[index].earlyTime) && Attack()) // go to next attack early
                        {
                                combo[index].earlyFlag = true;
                                goToNextMelee = true; // exit out of current attack
                        }
                        if (collider2DRef != null && !blockOtherHits)
                        {
                                bool hit = false;
                                int size = collider2DRef.OverlapCollider(filter, list);
                                for (int i = 0; i < size; i++)
                                {
                                        if (!targetList.Contains(list[i].transform))
                                        {
                                                combo[index].recoil = true;
                                                Vector2 newForceDirection = new Vector2(combo[index].forceDirection.x * direction, combo[index].forceDirection.y);
                                                if (Health.IncrementHealth(collider2DRef.transform, list[i].transform, -combo[index].damage, newForceDirection))
                                                {
                                                        hit = true;
                                                        combo[index].readHitBox = true;
                                                        targetList.Add(list[i].transform);
                                                        if (oneHitPerCombo)
                                                        {
                                                                blockOtherHits = true;
                                                                break;
                                                        }
                                                }
                                        }
                                }
                                if (hit)
                                {
                                        combo[index].OnHit(collider2DRef, direction);
                                }
                        }
                }

                private void ResetNext ()
                {
                        lateCounter = 0;
                        earlyCounter = 0;
                        goToNextMelee = false;
                        blockOtherHits = false;
                        targetList.Clear();
                        combo[index].ResetBeginVelocity();
                }

                public void AttackComplete ()
                {
                        goToNextMelee = true;
                }

                public void BeginVelX (bool value)
                {
                        if (index >= 0 && index < combo.Count)
                        {
                                combo[index].beginVelX = value;
                        }
                }

                public void BeginVelY (bool value)
                {
                        if (index >= 0 && index < combo.Count)
                        {
                                combo[index].beginVelY = value;
                        }
                }
        }

        [System.Serializable]
        public class MeleeCombo
        {
                [SerializeField] public float velX;
                [SerializeField] public float velY;
                [SerializeField] public float damage = 1f;
                [SerializeField] public float earlyTime = 0.5f;
                [SerializeField] public bool earlyNext;
                [SerializeField] public bool isLocked;
                [SerializeField] public string animationSignal;
                [SerializeField] public string meleeWE;
                [SerializeField] public string hitWE;
                [SerializeField] public AttackCondition condition;
                [SerializeField] public MeleeVelocityX velXType;
                [SerializeField] public MeleeVelocityY velYType;
                [SerializeField] public UnityEventEffect onMeleeBegin;
                [SerializeField] public UnityEventEffect onHit;
                [SerializeField] public Vector2 forceDirection = Vector2.right;
                [SerializeField] public bool canRecoilX;
                [SerializeField] public bool canRecoilY;
                [SerializeField] public float recoilX = 5f;
                [SerializeField] public float recoilY = 15f; // treated as jump force
                [SerializeField] public bool exitOnStateChange;
                [SerializeField] public bool keepInitialDirection;

                [SerializeField] public float impulseDuration = 0.5f;
                [SerializeField] public float impulseForce = 15f;

                [SerializeField] public float fixedDuration = 0.5f;
                [SerializeField] public float fixedDistance = 3f;

                [System.NonSerialized] public bool recoil;
                [System.NonSerialized] public bool recoilYApplied;
                [System.NonSerialized] public bool beginVelX;
                [System.NonSerialized] public bool beginVelY;
                [System.NonSerialized] public bool earlyFlag;
                [System.NonSerialized] public bool readHitBox;
                [System.NonSerialized] public bool alreadyJumped;
                [System.NonSerialized] public bool firstFrame;
                [System.NonSerialized] public bool goToFirstAirAttack;
                [System.NonSerialized] public float earlyCounter;
                [System.NonSerialized] public float timer;
                [System.NonSerialized] public bool fixedInstant;

                public bool isAirAttack => condition == AttackCondition.MustBeInAirOrExit || condition == AttackCondition.MustBeInAirOrSkip;

                #region 
#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField, HideInInspector] public bool delete;
                [SerializeField, HideInInspector] public bool foldOut;
                [SerializeField, HideInInspector] public bool eventFoldOut;
                [SerializeField, HideInInspector] public bool hitFoldOut;
                [SerializeField, HideInInspector] public bool eventsFoldOut;
                [SerializeField, HideInInspector] public bool optionsFoldOut;
#pragma warning restore 0414
#endif
                #endregion

                public void Reset ()
                {
                        timer = 0;
                        earlyCounter = 0;
                        firstFrame = true;
                        recoil = false;
                        beginVelX = false;
                        beginVelY = false;
                        earlyFlag = false;
                        readHitBox = false;
                        fixedInstant = false;
                        alreadyJumped = false;
                        recoilYApplied = false;
                        goToFirstAirAttack = false;
                }

                public void ResetBeginVelocity ()
                {
                        beginVelX = false;
                        beginVelY = false;
                }

                public void OnHit (Collider2D collider, int direction)
                {
                        onHit.Invoke(ImpactPacket.impact.Set(hitWE, collider.bounds.center + Vector3.right * collider.bounds.extents.x * direction, new Vector2(forceDirection.x * direction, forceDirection.y)));
                }

                public void PlayAnimation (Collider2D collider2D, AnimationSignals signals, int direction, int originalDirection, bool onGround, Vector2 position, ref Vector2 playerVelocity)
                {
                        direction = keepInitialDirection ? originalDirection : direction;
                        if (isAirAttack && onGround)
                        {
                                goToFirstAirAttack = true;
                        }
                        if (firstFrame)
                        {
                                firstFrame = false;
                                Vector2 p = collider2D != null ? (Vector2) collider2D.bounds.center : position;
                                onMeleeBegin.Invoke(ImpactPacket.impact.Set(meleeWE, p, new Vector2(forceDirection.x * direction, forceDirection.y)));
                        }
                        signals.Set("meleeCombo", true);
                        signals.Set(animationSignal, true);
                        if (direction != originalDirection)
                        {
                                signals.Set("meleeLeft", originalDirection < 0);
                                signals.Set("meleeRight", originalDirection > 0);
                        }

                        ApplyVelocity(signals, onGround, ref playerVelocity, direction);
                }

                public void ApplyVelocity (AnimationSignals signals, bool onGround, ref Vector2 playerVelocity, int direction)
                {
                        if (velXType != MeleeVelocityX.NoVelocity)
                        {
                                if (velXType == MeleeVelocityX.AbsoluteVelocity)
                                {
                                        playerVelocity.x = velX * direction;
                                }
                                else if (velXType == MeleeVelocityX.AdditiveVelocity)
                                {
                                        playerVelocity.x += velX * direction;
                                }
                                else if (velXType == MeleeVelocityX.ScaleVelocity)
                                {
                                        playerVelocity.x *= velX;
                                }
                                else if (velXType == MeleeVelocityX.Impulse)
                                {
                                        if (timer < impulseDuration)
                                        {
                                                timer += Time.deltaTime;
                                                float t = timer / impulseDuration;
                                                // diminishing force 
                                                float force = impulseForce * (1f - t * t * t);
                                                playerVelocity.x = force * direction;
                                        }
                                        else
                                                playerVelocity.x = 0.0001f * direction;
                                }
                                else if (velXType == MeleeVelocityX.FixedDistance)
                                {
                                        if (fixedInstant)
                                        {
                                                playerVelocity.x = 0.0001f * direction;
                                        }
                                        else if (fixedDuration <= 0f)
                                        {
                                                // instant move: all distance in one frame
                                                playerVelocity.x = fixedDistance / Time.deltaTime * direction;
                                                fixedInstant = true;
                                        }
                                        else
                                        {

                                                playerVelocity.x = (fixedDistance / fixedDuration) * direction;
                                        }
                                }
                                else if (beginVelX)
                                {
                                        playerVelocity.x = velX * direction;
                                }
                        }
                        if (velYType != MeleeVelocityY.NoVelocity && !recoilYApplied)
                        {
                                if (velYType == MeleeVelocityY.JumpVelocity)
                                {
                                        playerVelocity.y = !alreadyJumped && onGround ? velY : playerVelocity.y;
                                        alreadyJumped = true;
                                }
                                else if (velYType == MeleeVelocityY.AbsoluteVelocity)
                                {
                                        playerVelocity.y = velY;
                                }
                                else if (velYType == MeleeVelocityY.AdditiveVelocity)
                                {
                                        playerVelocity.y += velY;
                                }
                                else if (velYType == MeleeVelocityY.ScaleVelocity)
                                {
                                        playerVelocity.y *= velY;
                                }
                                else if (velYType == MeleeVelocityY.HoldGravity)
                                {
                                        playerVelocity.y = 0; // -= gravity;
                                }
                                else if (beginVelY)
                                {
                                        playerVelocity.y = velY;
                                }
                        }


                        if (recoil)
                        {
                                if (canRecoilX && forceDirection.x != 0)
                                {
                                        playerVelocity.x = -forceDirection.x * direction * recoilX;
                                        signals.ForceDirection((int) (playerVelocity.x * -1f));
                                }
                                if (canRecoilY && forceDirection.y != 0 && !recoilYApplied)
                                {
                                        recoilYApplied = true;
                                        playerVelocity.y = recoilY * -forceDirection.y;
                                }
                        }
                }

                public bool Exit (bool onGround, bool crouching)
                {
                        if (condition == AttackCondition.Anytime)
                                return false;
                        if (condition == AttackCondition.MustBeOnGroundOrExit && (!onGround || crouching))
                                return true;
                        if (condition == AttackCondition.MustBeInAirOrExit && onGround)
                                return true;
                        if (condition == AttackCondition.MustBeCrouchingOrExit && !crouching)
                                return true;
                        return false;
                }

                public bool Skip (bool onGround, bool crouching)
                {
                        if (condition == AttackCondition.Anytime)
                                return false;
                        if (condition == AttackCondition.MustBeOnGroundOrSkip && (!onGround || crouching))
                                return true;
                        if (condition == AttackCondition.MustBeInAirOrSkip && onGround)
                                return true;
                        if (condition == AttackCondition.MustBeCrouchingOrSkip && !crouching)
                                return true;
                        return false;
                }

                public bool StateChanged (bool onGround, bool crouching)
                {
                        if ((condition == AttackCondition.MustBeInAirOrExit || condition == AttackCondition.MustBeInAirOrSkip) && (onGround || crouching))
                                return true;
                        if ((condition == AttackCondition.MustBeOnGroundOrExit || condition == AttackCondition.MustBeOnGroundOrSkip) && (!onGround || crouching))
                                return true;
                        if ((condition == AttackCondition.MustBeCrouchingOrExit || condition == AttackCondition.MustBeCrouchingOrSkip) && (!onGround || !crouching))
                                return true;
                        return false;
                }
        }

        public enum MeleeVelocityX
        {
                NoVelocity,
                AdditiveVelocity,
                AbsoluteVelocity,
                EventVelocity,
                ScaleVelocity,
                Impulse,
                FixedDistance
        }

        public enum MeleeVelocityY
        {
                NoVelocity,
                AdditiveVelocity,
                AbsoluteVelocity,
                JumpVelocity,
                HoldGravity,
                EventVelocity,
                ScaleVelocity,
        }

        public enum AttackCondition
        {
                Anytime,
                MustBeOnGroundOrExit,
                MustBeInAirOrExit,
                MustBeOnGroundOrSkip,
                MustBeInAirOrSkip,
                MustBeCrouchingOrExit,
                MustBeCrouchingOrSkip,
        }

        public enum MeleeCollider
        {
                EnableOnStart,
                LeaveAsIs
        }

}
