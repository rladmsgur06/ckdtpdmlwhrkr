using TwoBitMachines.FlareEngine.ThePlayer;
using UnityEngine;
using UnityEngine.Events;

namespace TwoBitMachines.FlareEngine
{
        [System.Serializable]
        public class DefaultProjectile
        {
                [SerializeField] public WaitForAnimation waitForAnimation = new WaitForAnimation();
                [SerializeField] public Recoil recoil = new Recoil();
                [SerializeField] public ProjectileBase projectile;
                [SerializeField] public UnityEventEffect onFireSuccess;
                [SerializeField] public UnityEvent onOutOfAmmo;
                [SerializeField] public float autoDischarge;

                [System.NonSerialized] private float counter;
                [System.NonSerialized] private float dischargeCounter;
                [System.NonSerialized] private bool beginDischarge;

                #region ▀▄▀▄▀▄ Editor Variables ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField] private bool foldOut = false;
                [SerializeField] private bool eventsFoldOut = false;
                [SerializeField] private bool fireFoldOut = false;
                [SerializeField] private bool outOfAmmoFoldOut = false;
#pragma warning restore 0414
#endif
                #endregion

                public void Reset ()
                {
                        counter = 0;
                        dischargeCounter = 0;
                        beginDischarge = false;
                        waitForAnimation.Reset();
                        recoil.Reset();
                }

                public void Execute (Firearm fireArm, bool fire, Character equipment)
                {
                        if (waitForAnimation.AnimationNotPlaying(ref fire, ref counter, equipment.signals))
                        {
                                if (fireArm.FireProjectile(projectile, recoil, fire, ref counter))
                                {
                                        // onFireSuccess.Invoke ( ImpactPacket.impact.Set("", fireArm.firePoint.position, fireArm.transform.rotation * Vector3.forward));
                                        beginDischarge = autoDischarge > 0;
                                        dischargeCounter = 0;
                                }
                                else if (beginDischarge && Clock.TimerInverse(ref dischargeCounter, autoDischarge))
                                {
                                        if (fireArm.FireProjectile(projectile, recoil, true, ref counter))
                                        {
                                                // onFireSuccess.Invoke ( ImpactPacket.impact.Set("", fireArm.firePoint.position, fireArm.transform.rotation * Vector3.forward));
                                        }
                                }
                                else
                                {
                                        beginDischarge = false;
                                }
                        }
                }

                public void OnOutOfAmmo ()
                {
                        onOutOfAmmo.Invoke();
                }

                public virtual void LateExecute (Firearm firearm, AbilityManager player, ref Vector2 velocity)
                {
                        projectile.LateExecute(firearm, player, ref velocity);
                }

                public virtual void TurnOff (Firearm firearm, AbilityManager player)
                {
                        projectile.TurnOff(firearm, player);
                }

        }
}
