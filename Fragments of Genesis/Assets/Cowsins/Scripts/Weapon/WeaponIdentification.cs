using UnityEngine;

namespace cowsins2D
{
    public class WeaponIdentification : MonoBehaviour
    {
        [Tooltip("Assign this weapon.")] public Weapon_SO weapon;
        [Tooltip("Array that contains all the muzzles. Each muzzle will define a shooting point where the projectile will be instantiated ( for Projectile weapons ) " +
            "and the muzzle flash will be spawned.")]
        public Transform[] muzzles;

        [HideInInspector] public int currentBullets, totalBullets;

        public Animator Animator {  get; private set; }

        private void Awake()
        {
            Animator = GetComponentInChildren<Animator>();
        }
    }
}