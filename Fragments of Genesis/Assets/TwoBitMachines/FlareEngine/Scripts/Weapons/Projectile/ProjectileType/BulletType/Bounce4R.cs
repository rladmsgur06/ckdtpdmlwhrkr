using UnityEngine;

namespace TwoBitMachines.FlareEngine.BulletType
{
    [AddComponentMenu("")]
    public class Bounce4R : BulletBase
    {
        [SerializeField] private float bounceFriction = 0.05f;
        [SerializeField] private float bounceRadius = 0.5f;
        [SerializeField] private float bounceSpin;
        [SerializeField] private bool addMomentum = false;

        [System.NonSerialized] private float dissipate = 1;
        [System.NonSerialized] private float angleCount = 0;

        public override void OnReset(Vector2 characterVelocity)
        {
            dissipate = 1;
            angleCount = 0;
            AddMomentum(addMomentum, characterVelocity);
        }

        public override void Execute()
        {
            if (SetToSleep()) return;
            LifeSpanTimer();
            ApplyGravity(0.5f);
            if (bounceSpin == 0) ApplyRotation(this.transform);

            Vector3 actualVel = velocity * Time.deltaTime;
            float magnitude = actualVel.magnitude;
            Vector3 normal = actualVel / magnitude;
            Collision(position, Compute.Abs(velocity), normal, magnitude * Mathf.Abs(normal.x), magnitude * Mathf.Abs(normal.y), dissipate);
            transform.position = position;
            transform.rotation = rotation;
        }

        private void Collision(Vector3 origin, Vector3 originVel, Vector3 normal, float magnitudeX, float magnitudeY, float originDissipate)
        {
            Vector2 appliedVelocity = velocity;

            if (velocity.x != 0)
            {
                for (int i = -1; i < 2; i += 2) // -1, and 1
                {
                    Vector2 corner = origin + Vector3.up * (bounceRadius - 0.01f) * i;
                    RaycastHit2D ray = Physics2D.Raycast(corner, Vector2.right * Mathf.Sign(normal.x), bounceRadius + magnitudeX, layer);
                    #region Debug
#if UNITY_EDITOR
                    if (WorldManager.viewDebugger)
                    {
                        Debug.DrawRay(corner, Vector2.right * Mathf.Sign(normal.x) * (bounceRadius + magnitudeX), Color.green);
                    }
#endif
                    #endregion
                    if (ray && !IgnoreEdges(ray.collider))
                    {
                        bool bounce = true;
                        if (Health.IsDamageable(ray.transform))
                        {
                            bounce = !DealDamage(ray.transform, ray.distance > 0 ? ray.point : position, normal);
                        }
                        if (bounce && ray.distance > 0)
                        {
                            dissipate = originDissipate < 0.1f ? 0f : originDissipate * (1f - bounceFriction); // So it doesn't get applied twice
                            position.x = ray.point.x + bounceRadius * Mathf.Sign(ray.normal.x);
                            velocity.x = originVel.x * Mathf.Sign(ray.normal.x);
                            appliedVelocity.x = 0; // set to zero so that bullet looks like it's colliding with wall
                        }
                    }
                }
            }

            if (velocity.y != 0)
            {
                origin = position; // reset origin incase collision occurred in x direction
                for (int i = -1; i < 2; i += 2)
                {
                    Vector2 corner = origin + (Vector3.right * (bounceRadius - 0.01f) * i) + (Vector3.right * appliedVelocity.x * Time.deltaTime);
                    RaycastHit2D ray = Physics2D.Raycast(corner, Vector2.up * Mathf.Sign(normal.y), bounceRadius + magnitudeY, layer);
                    #region Debug
#if UNITY_EDITOR
                    if (WorldManager.viewDebugger)
                    {
                        Debug.DrawRay(corner, Vector2.up * Mathf.Sign(normal.y) * (bounceRadius + magnitudeX), Color.red);
                    }
#endif
                    #endregion
                    if (ray && !IgnoreEdges(ray.collider))
                    {
                        if (Health.IsDamageable(ray.transform))
                        {
                            DealDamage(ray.transform, ray.distance > 0 ? ray.point : position, normal);
                        }
                        else if (ray.distance > 0)
                        {
                            dissipate = originDissipate < 0.1f ? 0f : originDissipate * (1f - bounceFriction);
                            position.y = ray.point.y + bounceRadius * Mathf.Sign(ray.normal.y);
                            velocity.y = originVel.y * Mathf.Sign(ray.normal.y);
                            appliedVelocity.y = 0;
                        }
                    }
                }
            }

            ApplyGravity(0.5f);
            if (bounceSpin != 0 && velocity.x != 0)
            {
                rotation = Quaternion.Euler(new Vector3(0, 0, (angleCount += bounceSpin * 10f * Time.deltaTime) * -velocity.x));
            }
            position += appliedVelocity * Time.deltaTime;
        }

    }
}