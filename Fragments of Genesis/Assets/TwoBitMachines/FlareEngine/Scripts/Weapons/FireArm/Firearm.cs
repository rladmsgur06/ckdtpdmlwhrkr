using TwoBitMachines.FlareEngine.ThePlayer;
using UnityEngine;
using UnityEngine.Events;

namespace TwoBitMachines.FlareEngine
{
        [AddComponentMenu("Flare Engine/一Weapons/Firearm")]
        public class Firearm : Tool
        {
                [SerializeField] public ProjectileInventory projectileInventory;
                [SerializeField] public DefaultProjectile defaultProjectile = new DefaultProjectile();
                [SerializeField] public ChargedProjectile chargedProjectile = new ChargedProjectile();
                [SerializeField] public LineOfSight lineOfSight = new LineOfSight();
                [SerializeField] public RotateWeapon rotate = new RotateWeapon();
                [SerializeField] public UnityEvent onActivated;
                [SerializeField] public Transform firePoint;

                [SerializeField] public float stopTime = 0.25f;
                [SerializeField] public bool stopVelocity;
                [SerializeField] public bool turnOffNearWall;
                [SerializeField] public bool offOnShoot;
                [SerializeField] public StopType stopType;

                [System.NonSerialized] private bool fireNow;
                [System.NonSerialized] private bool canStop;
                [System.NonSerialized] private float startStopCount = 0;

                public enum StopType { OnShoot, Always }

                public override void ResetAll ()
                {
                        wasHolding = false;
                        fireNow = false;
                        canStop = false;
                        startStopCount = 0;
                        defaultProjectile.Reset();
                        chargedProjectile.Reset();
                        lineOfSight.Reset();
                        rotate.Reset();
                }

                public override void ToolOnEnable ()
                {
                        onActivated.Invoke();
                        if (EquipToolToParent())
                        {
                                bool fire = false;
                                rotate.Execute(this, equipment, ref fire); //                                  when firearm is toggled by inventory, set rotation immediately so it doesn't look glitchy.
                        }
                }

                private void LateUpdate ()
                {
                        bool externalShoot = fireNow;
                        fireNow = false;
                        if (pause || !EquipToolToParent())
                        {
                                return;
                        }
                        if (firePoint == null || input == null || Time.timeScale == 0)
                        {
                                return;
                        }

                        transform.localPosition = localPosition;
                        bool mouseOverUI = MouseOverUI(input);
                        bool fire = (!mouseOverUI && input.Active(buttonTrigger) && HasParent()) || externalShoot;

                        rotate.Execute(this, equipment, ref fire);
                        lineOfSight.Sight(transform, firePoint, ref fire);

                        if (chargedProjectile.Charge(mouseOverUI, input, equipment.signals)) //               charge weapon will take priority if it exists and is active
                        {
                                chargedProjectile.Execute(this, equipment);
                        }
                        else
                        {
                                defaultProjectile.Execute(this, fire, equipment);
                        }
                }

                public override bool ToolActive ()
                {
                        return (!MouseOverUI(input) && input.Any(buttonTrigger) && equipment != null) || fireNow;
                }

                public bool FireProjectile (ProjectileBase projectile, Recoil recoil, bool fire, ref float counter)
                {
                        if (projectile != null && Clock.TimerExpired(ref counter, projectile.fireRate) && fire && CanUseWeapon(projectile))
                        {
                                if (projectile.FireProjectile(firePoint, transform.rotation, equipment.setVelocity + recoil.Offset()))
                                {
                                        counter = 0;
                                        defaultProjectile.onFireSuccess.Invoke(ImpactPacket.impact.Set("", firePoint.position, transform.rotation * Vector3.forward));
                                        recoil.Set(transform, equipment.transform, equipment.setVelocity, equipment.world.box.down);
                                        canStop = true;
                                        startStopCount = 0;
                                        return true;
                                }
                        }
                        return false;
                }

                private bool CanUseWeapon (ProjectileBase projectile)
                {
                        if (!projectile.EnoughAmmo())
                        {
                                chargedProjectile.DischargeOver();
                                defaultProjectile.OnOutOfAmmo();
                                return false;
                        }
                        if (turnOffNearWall)
                        {
                                RaycastHit2D hit = Physics2D.Linecast(transform.position, firePoint.position, WorldManager.collisionMask);
                                if (hit && !(hit.collider is EdgeCollider2D))
                                {
                                        return false;
                                }
                        }
                        return true;
                }

                public override bool IsRecoiling ()
                {
                        return defaultProjectile.recoil.Recoiling() || chargedProjectile.recoil.Recoiling();
                }

                public override void Recoil (ref Vector2 velocity, AnimationSignals signals)
                {
                        chargedProjectile.recoil.Execute(ref velocity, signals);
                        defaultProjectile.recoil.Execute(ref velocity, signals);
                }

                public override void LateExecute (AbilityManager player, ref Vector2 velocity)
                {
                        if (stopVelocity && player.world.onGround)
                        {
                                if (stopType == StopType.OnShoot && canStop)
                                {
                                        velocity.x = 0;
                                        velocity.y = velocity.y > 0 ? 0 : velocity.y;
                                        if (Clock.Timer(ref startStopCount, stopTime))
                                        {
                                                canStop = false;
                                        }
                                }
                                if (stopType == StopType.Always)
                                {
                                        if (velocity.x != 0)
                                        {
                                                player.signals.ForceDirection((int) Mathf.Sign(velocity.x));
                                        }
                                        velocity.x = 0;
                                        velocity.y = velocity.y > 0 ? 0 : velocity.y;
                                }
                        }
                        defaultProjectile.LateExecute(this, player, ref velocity);
                }

                public override void TurnOff (AbilityManager player)
                {
                        defaultProjectile.TurnOff(this, player);
                }

                public override void AnimationComplete ()
                {
                        defaultProjectile.waitForAnimation.AnimationComplete();
                        chargedProjectile.waitForAnimation.AnimationComplete();
                }

                public override void ShootAndWaitForAnimation ()
                {
                        if (equipment != null && equipment.pushBackActive) // cancel shoot if character is being pushed back
                        {
                                AnimationComplete();
                                return;
                        }
                        defaultProjectile.waitForAnimation.ShootAndWaitForAnimation();
                        chargedProjectile.waitForAnimation.ShootAndWaitForAnimation();
                }

                public override void ShootAndAnimationComplete ()
                {
                        if (equipment != null && equipment.pushBackActive) // cancel shoot if character is being pushed back
                        {
                                AnimationComplete();
                                return;
                        }
                        defaultProjectile.waitForAnimation.ShootAndAnimationComplete();
                        chargedProjectile.waitForAnimation.ShootAndAnimationComplete();
                }

                public override bool ChangeProjectile (string name)
                {
                        return projectileInventory != null && projectileInventory.SetProjectile(defaultProjectile.projectile, name);
                }

                public override void ChangeFirearmProjectile (ItemEventData itemDataEvent)
                {
                        itemDataEvent.success = ChangeProjectile(itemDataEvent.genericString);
                }

                public void ChangeDefaultProjectile (ProjectileBase newProjectile)
                {
                        defaultProjectile.projectile = newProjectile;
                }

                public void ChangeChargeProjectile (ProjectileBase newProjectile)
                {

                        chargedProjectile.projectile = newProjectile;
                }

                public void ChangeAmmo (float value)
                {
                        if (defaultProjectile.projectile != null)
                        {
                                defaultProjectile.projectile.ChangeAmmo(value);
                        }
                }

                public void Shoot ()
                {
                        fireNow = true;
                }

                public override float ToolValue ()
                {
                        return defaultProjectile.projectile != null ? defaultProjectile.projectile.ammunition.ammunition : 0;
                }
        }
}
