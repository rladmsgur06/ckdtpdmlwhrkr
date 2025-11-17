using UnityEngine;

namespace TwoBitMachines.FlareEngine
{
        [System.Serializable]
        public class Pattern
        {
                [SerializeField] public float variance; // position variance
                [SerializeField] public float angle = 25f;
                [SerializeField] public int projectileRate = 1;
                [SerializeField] public Vector2 separation;
                [SerializeField] public FirePatternType fireDirection;
                private float randomize => Random.Range (-variance, variance);

                public bool Execute (ProjectileBase projectile, Vector3 position, Quaternion rotation)
                {
                        int available = (int) projectile.ammunition.available;

                        if (projectileRate <= 1)
                        {
                                return SingleShot (projectile, position, rotation);
                        }
                        else if (fireDirection == FirePatternType.WeaponDirection)
                        {
                                return Multiple (projectile, available, position, rotation);
                        }
                        else
                        {
                                return Circular (projectile, available, position, rotation);
                        }
                }

                public bool SingleShot (ProjectileBase projectile, Vector3 position, Quaternion rotation)
                {
                        Vector3 newPosition = variance > 0 ? position + (rotation * Vector2.up) * randomize : position;
                        if (projectile.Fire (newPosition, rotation))
                        {
                                return true;
                        }
                        return false;
                }

                public bool Multiple (ProjectileBase projectile, int available, Vector2 position, Quaternion rotation)
                {
                        bool success = false;
                        float multiplier = (available - 1) / 2f;
                        Vector2 up = rotation * Vector2.up;
                        Vector2 right = rotation * Vector2.right;
                        Vector2 startOffset = up * separation.y * multiplier;
                        float startAngle = angle * multiplier;
                        float offsetX = Mathf.Abs (separation.x);

                        for (int i = 0; i < available; i++)
                        {
                                Quaternion rotateAngle = Quaternion.AngleAxis (startAngle - angle * i, Vector3.forward);
                                Vector2 newPosition = variance > 0 ? position + up * randomize : position;
                                newPosition += startOffset - up * separation.y * i;
                                float separateX = separation.x < 0 ? Mathf.Abs (multiplier - i) : Mathf.Abs (Mathf.Abs (multiplier - i) - multiplier);
                                newPosition += right * offsetX * separateX;
                                if (projectile.Fire (newPosition, rotation * rotateAngle))
                                {
                                        success = true;
                                }
                        }
                        return success;
                }

                public bool Circular (ProjectileBase projectile, int available, Vector3 position, Quaternion rotation)
                {
                        bool success = false;
                        float startAngle = 360f / available;

                        for (int i = 0; i < available; i++)
                        {
                                float angle = startAngle * i;
                                Quaternion rotateAngle = Quaternion.AngleAxis (angle, Vector3.forward);
                                if (projectile.Fire (position, rotation * rotateAngle))
                                {
                                        success = true;
                                }
                        }
                        return success;
                }

        }
}