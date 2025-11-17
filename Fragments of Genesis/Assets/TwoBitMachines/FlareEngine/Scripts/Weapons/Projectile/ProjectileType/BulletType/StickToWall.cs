using UnityEngine;

namespace TwoBitMachines.FlareEngine.BulletType
{
        [AddComponentMenu("")]
        public class StickToWall : BulletBase
        {
                [SerializeField] private float stickTimer = 1f;
                [SerializeField] private int bulletRays = 1;
                [SerializeField] private Vector2 bulletSize = Vector2.one;
                [SerializeField] private bool addMomentum = false;
                [SerializeField] private bool activeAfterHit = false;
                [SerializeField] private UnityEventVector2 onStickToWallExpired;

                [System.NonSerialized] private float stickCounter;
                [System.NonSerialized] private bool isStickingToWall;
                [System.NonSerialized] private Vector2 previousStickPosition;
                [System.NonSerialized] private Transform stickTransform;

                public override void OnReset (Vector2 characterVelocity)
                {
                        isStickingToWall = false;
                        AddMomentum(addMomentum, characterVelocity);
                }

                public override void Execute ()
                {
                        if (SetToSleep())
                        {
                                return;
                        }
                        if (isStickingToWall)
                        {
                                if (stickTransform != null)
                                {
                                        Vector2 movement = (Vector2) stickTransform.position - previousStickPosition; // move holdpoint
                                        previousStickPosition = stickTransform.position;
                                        position += movement;
                                }
                                if (Clock.Timer(ref stickCounter, stickTimer) || stickTransform == null || !stickTransform.gameObject.activeInHierarchy)
                                {
                                        BlastRadius();
                                        ReadyToSleep();
                                        onStickToWallExpired.Invoke(position);
                                }
                        }
                        else
                        {
                                LifeSpanTimer();
                                ApplyGravity();
                                if (gravity != 0)
                                {
                                        ApplyRotation(this.transform);
                                }
                                CollisionDetection(bulletRays, bulletSize);
                        }

                        transform.position = position;
                        transform.rotation = rotation;
                }

                public override bool CastRay (Vector2 origin, Vector2 velocityNormal, float velMagnitude, float size)
                {
                        RaycastHit2D ray = Physics2D.Raycast(origin, velocityNormal, velMagnitude + size, layer);
                        #region Debug
#if UNITY_EDITOR
                        if (WorldManager.viewDebugger)
                        {
                                Debug.DrawRay(origin, velocityNormal * (velMagnitude + size), Color.red);
                        }
#endif
                        #endregion

                        if (!ray || IgnoreEdges(ray.collider))
                        {
                                return false;
                        }
                        if (ray.distance == 0)
                        {
                                return DealDamage(ray.transform, position, velocityNormal);
                        }
                        onImpact.Invoke(ImpactPacket.impact.Set(worldEffect, ray.point, velocityNormal));

                        if (!Health.IsDamageable(ray.transform))
                        {
                                Stick(ray, velocityNormal, velMagnitude, size);
                                return true;
                        }
                        else if (Health.IncrementHealth(transform, ray.transform, -damage, velocityNormal * damageForce))
                        {
                                onHitTarget.Invoke(ImpactPacket.impact.Set(worldEffect, ray.point, velocityNormal));
                                if (!activeAfterHit)
                                {
                                        BlastRadius();
                                        ReadyToSleep();
                                }
                                return true;
                        }
                        return false;
                }

                private void Stick (RaycastHit2D ray, Vector2 velocityNormal, float velMagnitude, float size)
                {
                        stickCounter = 0;
                        isStickingToWall = true;
                        stickTransform = ray.transform;
                        previousStickPosition = ray.transform.position;
                        position = ray.point + velocityNormal * ((velMagnitude + size + 0.2f) - ray.distance) - velocityNormal * size;
                        velocity = Vector2.zero;
                }
        }
}
