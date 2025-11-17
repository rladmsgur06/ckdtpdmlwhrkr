using UnityEngine;

namespace TwoBitMachines.FlareEngine.BulletType
{
        [AddComponentMenu("")]
        public class TargetArch : BulletBase
        {
                [SerializeField] public Transform target;
                [SerializeField] public float arch;
                [SerializeField] private int bulletRays = 1;
                [SerializeField] private Vector2 bulletSize = Vector2.one;

                public override void OnReset (Vector2 characterVelocity)
                {
                        if (target != null)
                        {
                                velocity = Compute.ArchObject(position, target.position, arch, -gravity);
                                if (gravity != 0)
                                        ApplyRotation(this.transform);
                        }
                }

                public override void Execute ()
                {
                        if (SetToSleep())
                        {
                                return;
                        }
                        LifeSpanTimer();
                        ApplyGravity();
                        if (gravity != 0)
                                ApplyRotation(this.transform);
                        CollisionDetection(bulletRays, bulletSize);
                        transform.position = position;
                        transform.rotation = rotation;
                }

        }

}
