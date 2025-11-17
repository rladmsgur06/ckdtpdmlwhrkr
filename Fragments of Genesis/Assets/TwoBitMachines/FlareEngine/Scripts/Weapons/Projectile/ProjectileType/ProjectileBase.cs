using System.Collections.Generic;
using TwoBitMachines.FlareEngine.ThePlayer;
using UnityEngine;

namespace TwoBitMachines.FlareEngine
{
        [AddComponentMenu("")]
        public class ProjectileBase : MonoBehaviour
        {
                [SerializeField] public string projectileName;
                [SerializeField] public float fireRate;
                [SerializeField] public float damage = 1f;
                [SerializeField] public float damageForce = 1f;
                [SerializeField] public Ammunition ammunition = new Ammunition();
                [SerializeField] public Pattern pattern = new Pattern();

                [System.NonSerialized] public Vector2 playerVelocity;
                [System.NonSerialized] public bool triggerReleased = false;
                [System.NonSerialized] public ProjectileInventory inventory;

                public static List<ProjectileBase> projectiles = new List<ProjectileBase>();

                #region ▀▄▀▄▀▄ Editor Variables ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField] private bool foldOut = true;
                [SerializeField] private bool eventFoldOut;
                [SerializeField] private bool ammoEventFoldOut;
                [SerializeField] private bool propertiesFoldOut;
                [SerializeField] private bool propertiesEventFoldOut;
                [SerializeField] private bool setupFoldOut;
                [SerializeField] private bool foldOutEvents;
                [SerializeField] private bool foldOutOnFire;
                [SerializeField] private bool foldOutOnRelease;
                [SerializeField] private bool impactFoldOut;
                [SerializeField] private bool hitFoldOut;
                [SerializeField] private bool timerFoldOut;
                [SerializeField] private int tabIndex;
#pragma warning restore 0414
#endif
                #endregion

                private void OnEnable ()
                {
                        if (!projectiles.Contains(this))
                        {
                                projectiles.Add(this);
                        }
                        OnEnableNow();
                }

                private void OnDisable ()
                {
                        if (projectiles.Contains(this))
                        {
                                projectiles.Remove(this);
                        }
                        OnDisableNow();
                }

                private void OnDestroy ()
                {
                        ammunition.Save();
                }

                public static void Projectiles ()
                {
                        for (int i = projectiles.Count - 1; i >= 0; i--)
                        {
                                if (projectiles[i] == null)
                                {
                                        projectiles.RemoveAt(i);
                                }
                                else
                                {
                                        projectiles[i].Execute();
                                }
                        }
                }

                public static void ResetProjectiles ()
                {
                        for (int i = 0; i < projectiles.Count; i++)
                        {
                                if (projectiles[i] != null)
                                {
                                        projectiles[i].ResetAll();
                                }
                        }
                }

                public virtual bool FireProjectileStatic (Transform transform, Vector2 direction)
                {
                        Quaternion rotation = Quaternion.FromToRotation(Vector3.right, direction);
                        return FireProjectile(transform, rotation, Vector2.zero);
                }

                public virtual bool FireProjectileStatic (Transform transform, Quaternion rotation)
                {
                        return FireProjectile(transform, rotation, Vector2.zero);
                }

                public virtual bool FireProjectileStatic (Vector2 position, Vector2 direction)
                {
                        Quaternion rotation = Quaternion.FromToRotation(Vector3.right, direction);
                        return FireProjectile(position, rotation, Vector2.zero);
                }

                public virtual bool FireProjectile (Transform transform, Vector2 direction)
                {
                        return FireProjectile(transform, Quaternion.LookRotation(direction, Vector3.forward), Vector2.zero);
                }

                public virtual bool FireProjectile (Transform transform, Vector2 direction, Vector2 characterVelocity)
                {
                        return FireProjectile(transform, Quaternion.LookRotation(direction, Vector3.forward), characterVelocity);
                }

                public virtual bool FireProjectile (Transform transform, Quaternion rotation, Vector2 characterVelocity)
                {
                        return false;
                }

                public virtual bool FireProjectile (Vector2 position, Quaternion rotation, Vector2 characterVelocity)
                {
                        return false;
                }

                public virtual void LateExecute (Firearm firearm, AbilityManager player, ref Vector2 velocity)
                {

                }

                public virtual void TurnOff (Firearm firearm, AbilityManager player)
                {

                }

                public virtual bool Fire (Vector2 position, Quaternion rotation)
                {
                        return false;
                }

                public virtual void Execute () { }

                public virtual void ResetAll () { }

                public virtual void OnDisableNow () { }

                public virtual void OnEnableNow () { }

                public virtual void Activate (bool value) { }

                public void ChangeAmmo (float value)
                {
                        ammunition.ammunition = Mathf.Clamp(ammunition.ammunition + value, 0, ammunition.max);
                        if (inventory != null)
                                inventory.SetUI();
                        if (value > 0)
                                ammunition.onAmmoReload.Invoke();
                }

                public void SetAmmoDamage (float value)
                {
                        damage = value;
                }

                public void ChangeAmmo (ItemEventData itemEventData)
                {
                        if (ammunition.ammunition > ammunition.max)
                        {
                                itemEventData.success = false;
                                return;
                        }
                        ammunition.ammunition = Mathf.Clamp(ammunition.ammunition + itemEventData.genericFloat, 0, ammunition.max);

                        if (inventory != null)
                        {
                                inventory.SetUI();
                        }

                        if (itemEventData.genericFloat > 0)
                        {
                                ammunition.onAmmoReload.Invoke();
                        }
                        itemEventData.success = true;
                }

                public float AmmoCount ()
                {
                        return ammunition.ammunition;
                }

                public bool EnoughAmmo ()
                {
                        return ammunition.EnoughAmmo();
                }

                public bool AmmoIsInfinite ()
                {
                        return ammunition.type == AmmoType.Infinite;
                }

                public float AmmoMax ()
                {
                        return ammunition.max;
                }

                public float AmmoPercent ()
                {
                        return ammunition.max <= 0 ? 0 : ammunition.ammunition / ammunition.max;
                }
        }
}
