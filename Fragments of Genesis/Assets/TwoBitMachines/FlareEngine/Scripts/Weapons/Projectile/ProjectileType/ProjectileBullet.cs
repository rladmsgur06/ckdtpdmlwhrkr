using System.Collections.Generic;
using TwoBitMachines.FlareEngine.BulletType;
using UnityEngine;

namespace TwoBitMachines.FlareEngine
{
        [AddComponentMenu("")]
        public class ProjectileBullet : ProjectileBase
        {
                [SerializeField] public string bulletType;
                [SerializeField] public int poolSize = 10;
                [SerializeField] public GameObject projectileObject;

                [System.NonSerialized] public List<BulletBase> projectile = new List<BulletBase>();

                public void Start ()
                {
                        ammunition.RestoreValue();
                        if (projectileObject != null)
                        {
                                projectile.Add(projectileObject.GetComponent<BulletBase>());
                                projectileObject.SetActive(false);
                        }
                }

                public override void ResetAll ()
                {
                        for (int i = 0; i < projectile.Count; i++)
                        {
                                projectile[i].SetToSleep();
                        }
                }

                public override void Execute ()
                {
                        for (int i = 0; i < projectile.Count; i++)
                        {
                                if (!projectile[i].sleep && projectile[i].gameObject.activeInHierarchy)
                                {
                                        projectile[i].Execute();
                                }
                        }
                }

                public override bool FireProjectile (Transform transform, Quaternion rotation, Vector2 playerVelocity)
                {
                        this.playerVelocity = Time.deltaTime <= 0 ? Vector2.zero : playerVelocity / Time.deltaTime;

                        if (ammunition.Consume(pattern.projectileRate, inventory))
                        {
                                return pattern.Execute(this, transform.position, rotation);
                        }
                        return false;
                }

                public override bool Fire (Vector2 position, Quaternion rotation)
                {
                        for (int i = 0; i < projectile.Count; i++)
                        {
                                if (projectile[i] != null && projectile[i].gameObject != null && !projectile[i].gameObject.activeInHierarchy)
                                {
                                        ResetBullet(projectile[i], position, rotation);
                                        return true;
                                }
                        }
                        if (projectile.Count < poolSize && projectileObject != null)
                        {
                                GameObject newBullet = MonoBehaviour.Instantiate(projectileObject, this.transform);
                                BulletBase newBulletBase = newBullet.GetComponent<BulletBase>();
                                projectile.Add(newBulletBase);
                                ResetBullet(newBulletBase, position, rotation);
                                return true;
                        }
                        return false;
                }

                public void ResetBullet (BulletBase bullet, Vector2 position, Quaternion rotation)
                {
                        bullet.Reset(position, rotation, damage, damageForce);
                        bullet.OnReset(playerVelocity);
                        bullet.transform.position = bullet.position;
                        bullet.transform.rotation = bullet.rotation;
                        bullet.SetGameObjectTrue();
                        bullet.onFire.Invoke(ImpactPacket.impact.Set("", position, rotation * Vector3.forward));

                        //     ImpactPacket impact = ImpactPacket.impact.Set(worldEffect, position, direction);
                }

        }
}
