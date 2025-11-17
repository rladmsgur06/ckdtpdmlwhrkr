using UnityEngine;

namespace TwoBitMachines.FlareEngine
{
        [System.Serializable]
        public class Recoil
        {
                [SerializeField] public RecoilType type;
                [SerializeField] public RecoilCondition condition;
                [SerializeField] public float recoilDistance = 0.125f;
                [SerializeField] public float recoilTime = 0.1f;
                [SerializeField] public bool hasGravity = false;
                [SerializeField] public bool recoil = false;

                [System.NonSerialized] public bool canRecoil = true;
                [System.NonSerialized] private bool offsetX;
                [System.NonSerialized] private bool startRecoil;
                [System.NonSerialized] private bool isRecoiling;
                [System.NonSerialized] private float acceleration;
                [System.NonSerialized] private float recoilCounter;
                [System.NonSerialized] private Vector2 velOffset;
                [System.NonSerialized] private Vector2 appliedForce;
                [System.NonSerialized] private Vector2 weaponDirection;
                [System.NonSerialized] private Vector2 characterDown;

                #region ▀▄▀▄▀▄ Editor Variables ▄▀▄▀▄▀
                #if UNITY_EDITOR
                #pragma warning disable 0414
                public bool foldOut = false;
                #pragma warning restore 0414
                #endif
                #endregion

                public void Reset ( )
                {
                        recoilCounter = 0;
                        startRecoil = false;
                        isRecoiling = false; // everything else is reset automatically
                        offsetX = false;
                }

                public bool Recoiling ( )
                {
                        return isRecoiling;
                }

                public Vector2 Offset ( )
                {
                        if (offsetX)
                        {
                                offsetX = false;
                                return -velOffset * Time.deltaTime;
                        }
                        return Vector2.zero;
                }

                public void Set (Transform weapon, Transform character, Vector2 velocity, Vector2 characterDown)
                {
                        if (recoil && !isRecoiling && canRecoil && recoilTime > 0)
                        {
                                if (condition == RecoilCondition.VelocityZero)
                                {
                                        if (hasGravity && velocity.x != 0)
                                                return;
                                        if (!hasGravity && velocity != Vector2.zero)
                                                return;
                                }

                                this.characterDown = characterDown;
                                weaponDirection = weapon.right;
                                isRecoiling = true;
                                startRecoil = true;
                                recoilCounter = 0;

                                if (type == RecoilType.SlideBack)
                                {
                                        acceleration = (-2f * recoilDistance) / Mathf.Pow (recoilTime, 2f);
                                        appliedForce = -acceleration * weaponDirection * recoilTime;
                                }
                        }
                }

                public void Execute (ref Vector2 velocity, AnimationSignals signals) // the amount of velocity applied to the character during recoil
                {
                        if (!isRecoiling) return;

                        if (type == RecoilType.Shake)
                        {
                                RecoilShake (ref velocity, signals);
                        }
                        else
                        {
                                RecoilSlideBack (ref velocity, signals);
                        }
                }

                private void RecoilShake (ref Vector2 velocity, AnimationSignals signals)
                {
                        if (Time.deltaTime == 0) return;

                        if (startRecoil)
                        {
                                startRecoil = false;
                                velocity = (recoilDistance / Time.deltaTime) * -weaponDirection;

                                signals.Set ("recoil");
                                signals.Set ("recoilRight", weaponDirection.x > 0);
                                signals.Set ("recoilLeft", weaponDirection.x < 0);
                                signals.Set ("recoilDown", weaponDirection.y < 0);
                                signals.Set ("recoilUp", weaponDirection.y > 0);
                                signals.Set ("recoilShake", true);
                        }
                        else if (Clock.Timer (ref recoilCounter, recoilTime)) // move character forward
                        {
                                Vector2 appliedForce = (recoilDistance / Time.deltaTime) * weaponDirection;
                                velocity.y = hasGravity ? velocity.y : appliedForce.y;
                                velocity.x = appliedForce.x;
                                velOffset = velocity; //                         when recoiling forward, it will change the momentum of a bullet, we don't want this

                                signals.Set ("recoilShake", true);
                                isRecoiling = false;
                                offsetX = true;
                        }
                }

                private void RecoilSlideBack (ref Vector2 velocity, AnimationSignals signals)
                {
                        if (Clock.Timer (ref recoilCounter, recoilTime))
                        {
                                isRecoiling = false;
                                return;
                        }

                        appliedForce += acceleration * weaponDirection * Time.deltaTime;

                        if (hasGravity)
                        {
                                if (Vector2.Angle (weaponDirection, characterDown) < 35f)
                                {
                                        appliedForce.y *= 0.25f; //                   prevent player from flying via upward thrust
                                }
                                velocity.x = -appliedForce.x;
                                velocity.y -= velocity.y > 0 ? 0 : appliedForce.y; // don't apply if character is moving up, helps prevent charcter from flying
                        }
                        else
                        {
                                velocity = -appliedForce;
                        }

                        if (!Compute.SameSign (velocity.x, appliedForce.x))
                        {
                                signals.Set ("recoil");
                                signals.Set ("recoilRight", weaponDirection.x > 0);
                                signals.Set ("recoilLeft", weaponDirection.x < 0);
                                signals.Set ("recoilSlide", true);
                        }
                        if (!Compute.SameSign (velocity.y, appliedForce.y))
                        {
                                signals.Set ("recoil");
                                signals.Set ("recoilDown", weaponDirection.y < 0);
                                signals.Set ("recoilUp", weaponDirection.y > 0);
                                signals.Set ("recoilSlide", true);
                        }
                }
        }

        public enum RecoilType
        {
                SlideBack,
                Shake
        }

        public enum RecoilCondition
        {
                VelocityZero,
                Anytime
        }
}