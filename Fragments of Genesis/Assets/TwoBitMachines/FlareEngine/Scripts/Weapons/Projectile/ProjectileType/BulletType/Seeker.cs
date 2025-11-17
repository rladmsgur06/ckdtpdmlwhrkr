using UnityEngine;

namespace TwoBitMachines.FlareEngine.BulletType
{
        [AddComponentMenu("")]
        public class Seeker : BulletBase
        {
                [SerializeField] public float searchRadius = 20f;
                [SerializeField] public float searchRate = 0.25f;
                [SerializeField] public float turnSpeed = 2; // between 0 and 1
                [SerializeField] public int bulletRays = 1;
                [SerializeField] public Vector2 bulletSize = Vector2.one;
                [SerializeField] public ProjectileSeekerType find;
                [SerializeField] private Transform followTarget;
                [SerializeField] private float randomTargetOffset = 0;
                [SerializeField] private bool addMomentum = false;
                [SerializeField] private bool collideWithWorld = false;

                [System.NonSerialized] private Transform target = null;
                [System.NonSerialized] private float searchRateCounter;
                [System.NonSerialized] private Vector3 randomOffset;

                private bool targetEmpty => target == null || target.transform == null || !target.gameObject.activeInHierarchy;//|| !target.enabled


                public override void OnReset (Vector2 characterVelocity)
                {
                        searchRateCounter = 0;
                        target = followTarget;
                        AddMomentum(addMomentum, characterVelocity * 0.5f);
                        randomOffset = randomTargetOffset != 0 ? RandomOffset(randomTargetOffset) : Vector2.zero;
                }

                public override void Execute ()
                {
                        if (SetToSleep())
                        {
                                return;
                        }
                        LifeSpanTimer();
                        ApplyRotation(this.transform);
                        Follow();
                        if (collideWithWorld)
                        {
                                layer |= WorldManager.collisionMask;
                        }
                        CollisionDetection(bulletRays, bulletSize);
                        if (collideWithWorld)
                        {
                                layer &= ~WorldManager.collisionMask;
                        }
                        transform.position = position;
                        transform.rotation = rotation;
                }

                private void Follow ()
                {
                        if (!targetEmpty || (followTarget != null && target != null))
                        {
                                FollowTarget(ref position, ref velocity, ref rotation, target.transform.position + randomOffset, turnSpeed);
                        }
                        else
                        {
                                position += velocity * Time.deltaTime;
                                if (Clock.Timer(ref searchRateCounter, searchRate)) //                        find target
                                {
                                        int targets = Compute.OverlapCircle(position, searchRadius, layer); // target layer should only have enemy targets
                                        if (find == ProjectileSeekerType.RandomTarget)
                                        {
                                                Collider2D col2D = Compute.HitContactRandomResult(targets, position);
                                                if (col2D != null)
                                                        target = col2D.transform;
                                        }
                                        else
                                        {
                                                Collider2D col2D = Compute.HitContactNearestResult(targets, position);
                                                if (col2D != null)
                                                        target = col2D.transform;
                                        }
                                }
                        }
                }

                private void FollowTarget (ref Vector2 position, ref Vector2 velocity, ref Quaternion rotation, Vector2 target, float turnSpeed)
                {
                        Vector2 velNormal = velocity.normalized;
                        Vector2 direction = (target - position).normalized;
                        float rotateDirection = Compute.CrossSign(direction, velNormal);
                        velocity = Compute.RotateVector(velocity, (turnSpeed * Time.deltaTime) * -rotateDirection * Vector2.Angle(direction, velNormal));
                        position += velocity * Time.deltaTime;
                        rotation = Compute.LookAtDirection(velocity);
                }

                private Vector2 RandomOffset (float length = 1f)
                {
                        float x = Random.Range(-length, length);
                        float y = Random.Range(-length, length);
                        return new Vector2(x, y);
                }

        }
}
