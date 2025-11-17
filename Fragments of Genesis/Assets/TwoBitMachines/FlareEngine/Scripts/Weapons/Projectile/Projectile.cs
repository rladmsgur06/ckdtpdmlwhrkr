using UnityEngine;

namespace TwoBitMachines.FlareEngine
{
        [AddComponentMenu("Flare Engine/一Weapons/Projectile")]
        public class Projectile : MonoBehaviour
        {
                [SerializeField] public ProjectileType type;
                [SerializeField] public ProjectileBase projectile;
        }

        public enum ProjectileType
        {
                Bullet,
                Instant,
                ShortRange,
                GrapplingGun
        }

        public enum IgnoreEdge
        {
                IgnoreAlways,
                NeverIgnore,
                IgnoreIfUpDirection
        }

        public enum OnTriggerRelease
        {
                DeactivateGameObject,
                DeactivateGameObjectAfterTimeDelay,
                LeaveAsIs
        }

        public enum ProjectileSeekerType
        {
                NearestTarget,
                RandomTarget
        }

        public enum AmmoType
        {
                Discrete,
                Continuous,
                Infinite
        }

        public enum FirePatternType
        {
                WeaponDirection,
                CircularDirection
        }
}
