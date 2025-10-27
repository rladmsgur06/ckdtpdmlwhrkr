using UnityEngine;
using System.Collections.Generic;

namespace cowsins2D
{
    [CreateAssetMenu(fileName = "New Weapon", menuName = "Cowsins/New Weapon")]
    public class Weapon_SO : Item_SO
    {
        public override bool Use(WeaponController controller)
        {
            return false;
        }

        public enum ShootingStyle
        {
            Raycast, Projectile, Melee, Custom
        };
        public enum ShootingMethod
        {
            Press, PressAndHold, ReleaseWhenReady, ShootWhenReady
        };
        public enum ReloadingMethod
        {
            Default, Overheat
        };

        [System.Serializable]
        public enum AimingMethod
        {
            None, Horizontal, BothAxis, Free, OrientationBased
        }


        public WeaponIdentification weaponObject;

        public AimingMethod aimingMethod;

        public ShootingStyle shootingStyle;

        public Projectile projectile;

        public float projectileSpeed;

        public float projectileDuration;

        public float projectileCollisionSize;

        public bool explosiveProjectile;

        public float explosiveDamage;

        public float explosiveForce;

        public float explosionRadius;

        public float meleeAttackRadius;

        public float meleeDistance;

        public ShootingMethod shootingMethod;

        public float chargeRequiredToShoot;

        public float damage;

        public float fireRate;

        public float spread;

        public float visualRecoil;

        public float visualRecoilRecovery;

        public ReloadingMethod reloadingMethod;

        public AmmoType_SO ammoType;

        public float coolSpeed;

        [Range(1, 100)]
        public float allowShootingAfterCoolingPercentage;

        public bool autoReload;

        public float reloadTime;

        public bool infiniteBullets;

        public bool limitedMagazines;

        public int amountOfMagazines;

        public int magazineSize;

        public int ammoPerShot;

        public int bulletsPerShot;

        public float timeBetweenBullets;

        public List<HitEffect> hitEffects = new List<HitEffect>();

        public bool camShake;

        public float camShakeAmount;

        public GameObject muzzleFlashVFX;

        public float crosshairShootingSpread;

        [Range(.1f, 10)] public float weight;

        public bool canParry;

        [Range(.1f, 100)] public float parryProjectileSpeed;

        [System.Serializable]
        public class Sounds
        {
            public AudioClip shotSFX, reloadSFX, unholsterSFX;
        }

        public Sounds sounds;
    }

}