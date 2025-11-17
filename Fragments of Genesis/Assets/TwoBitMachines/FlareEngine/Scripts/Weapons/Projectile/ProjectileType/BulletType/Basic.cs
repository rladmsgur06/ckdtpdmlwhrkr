using UnityEngine;

namespace TwoBitMachines.FlareEngine.BulletType
{
        [AddComponentMenu("")]
        public class Basic : BulletBase
        {
                [SerializeField] private int bulletRays = 1;
                [SerializeField] private Vector2 bulletSize = Vector2.one;
                [SerializeField] private bool addMomentum = false;

                public override void OnReset (Vector2 characterVelocity)
                {
                        AddMomentum(addMomentum, characterVelocity);
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
