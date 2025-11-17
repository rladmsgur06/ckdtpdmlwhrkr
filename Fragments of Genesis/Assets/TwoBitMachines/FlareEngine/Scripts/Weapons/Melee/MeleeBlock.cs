using System.Collections.Generic;
using UnityEngine;

namespace TwoBitMachines.FlareEngine
{
        [System.Serializable]
        public class MeleeBlock
        {
                [SerializeField] public bool canBlock;
                [SerializeField] public bool canRecoil;
                [SerializeField] public bool cancelCombo;
                [SerializeField] public bool stopVelocityX;
                [SerializeField] public bool canDeflect;
                [SerializeField] public string blockSignal;
                [SerializeField] public string recoilTag;
                [SerializeField] public float recoilVelocity = 10f;
                [SerializeField] public float recoilDuration = 0.1f;
                [SerializeField] public InputButtonSO input;
                [SerializeField] public InputButtonSO inputTwo;
                [SerializeField] public LayerMask blockLayer;
                [SerializeField] public BlockHoldType mustHold;

                [System.NonSerialized] public Collider2D collider2DRef;
                [System.NonSerialized] public bool needToRelease;
                [System.NonSerialized] private bool isRecoiling;
                [System.NonSerialized] private int recoilDirection;
                [System.NonSerialized] private float recoilCounter;
                [System.NonSerialized] private Health health;

                #region 
#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField, HideInInspector] public bool foldOut = false;
#pragma warning restore 0414
#endif
                #endregion

                public void Initialize (Collider2D collider2DRef)
                {
                        this.collider2DRef = collider2DRef;
                        WorldManager.RegisterInput(input);
                        WorldManager.RegisterInput(inputTwo);

                        if (canBlock)
                        {
                                health = this.collider2DRef.gameObject.AddComponent<Health>(); // need health to detect projectiles
                                health.onValueChanged.AddListener(OnHit);
                                float value = 100100;
                                health.SetMaxValue(value);
                                health.SetValue(value);
                        }
                }

                public void Reset ()
                {
                        isRecoiling = false;
                        recoilCounter = 0;
                }

                public bool IsBlocking ()
                {
                        if (input == null)
                                return false;

                        if (input.Holding() && (mustHold == BlockHoldType.None || (inputTwo != null && inputTwo.Holding())))
                        {
                                return true;
                        }
                        return false;
                }

                public void Block (AnimationSignals signals, int direction, bool onGround, ref Vector2 velocity)
                {
                        if (collider2DRef != null)
                                collider2DRef.enabled = true;

                        signals.Set("meleeCombo", true);
                        signals.Set(blockSignal);

                        if (stopVelocityX && onGround)
                        {
                                if (velocity.x != 0)
                                {
                                        signals.ForceDirection((int) Mathf.Sign(velocity.x));
                                }
                                velocity.x = 0;
                        }
                        if (health != null)
                        {
                                health.block = canDeflect;
                        }
                        if (health != null && health.GetValue() < 100f)
                        {
                                health.Increment(100000); // keep health active to detect projectiles
                        }
                        Recoil(signals, ref velocity);
                        needToRelease = true;
                }

                public void OnHit (ImpactPacket impact)
                {
                        if (impact.attacker != null && recoilTag != "" && recoilTag != "Untagged" && impact.attacker.gameObject.CompareTag(recoilTag))
                        {
                                isRecoiling = true;
                                recoilCounter = 0;
                                recoilDirection = collider2DRef.transform.position.x < impact.attacker.position.x ? -1 : 1;
                        }
                }

                public void Recoil (AnimationSignals signals, ref Vector2 velocity)
                {
                        if (!canRecoil)
                                return;

                        if (isRecoiling)
                        {
                                velocity.x = recoilDirection * recoilVelocity;
                                signals.ForceDirection(-recoilDirection);
                                if (Clock.Timer(ref recoilCounter, recoilDuration))
                                {
                                        Reset();
                                }
                        }
                }

        }

        public enum BlockHoldType
        {
                None,
                Include
        }
}
