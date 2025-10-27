using UnityEngine;
using UnityEngine.Events;

namespace cowsins2D
{
    public enum FiringMode
    {
        Hitscan,
        Projectile
    }

    public class Turret : MonoBehaviour
    {
        [SerializeField, Tooltip("Object to pivot the muzzle from.")]
        private Transform pivot;

        [SerializeField, Tooltip("Velocity to lerp the aim")]
        private float aimSpeed;

        [SerializeField, Tooltip("Damage to apply to the target on hit")]
        private float damage;

        [SerializeField, Tooltip("Configure the way the firing works.")]
        private FiringMode firingMode;

        [SerializeField, Tooltip("Speed of the shooting")]
        private float fireRate;

        [SerializeField, Tooltip("Projectile object to instantiate on shoot [ONLY FOR PROJECTILE TURRETS]")]
        private GameObject projectilePrefab;

        [SerializeField, Tooltip("Speed of the projectile object")]
        private float projectileSpeed;

        [SerializeField, Tooltip("Duration of the projectile before getting destroyed")]
        private float projectileLifetime;

        [SerializeField, Tooltip("Object to shoot from")]
        private Transform muzzle;

        [SerializeField, Tooltip("Start shooting on awake")]
        private bool shootFromStart;

        [SerializeField, Tooltip("Player layer")]
        private LayerMask whatIsPlayer;

        [SerializeField, Tooltip("Effect to instantiate on the player on shoot")]
        private GameObject muzzleFlash;

        [SerializeField, Tooltip("Sound to play on shoot.")]
        private AudioClip shootSFX;

        [SerializeField, Tooltip("Volume of the shoot SFX.")]
        private float volume;

        [SerializeField, Tooltip("Radius for detecting the player")]
        private float detectionRadius;

        public UnityEvent onFire;

        public float timer { get; private set; }

        [HideInInspector] public bool canShoot;

        private delegate void Fire();

        private Fire fire;

        private Transform player;

        private void Start()
        {
            player = GameObject.FindWithTag("Player").transform;

            fire = firingMode == FiringMode.Hitscan ? HitscanFire : ProjectileFire;

            if (shootFromStart)
            {
                canShoot = true;
                timer = fireRate;
            }
        }

        private void Update()
        {
            timer -= Time.deltaTime;

            Vector2 directionToPlayer = player.position - pivot.position;
            float angle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;
            pivot.rotation = Quaternion.Lerp(pivot.rotation, Quaternion.Euler(0f, 0f, angle), Time.deltaTime * aimSpeed);

            // Check if the player is within the detection radius
            if (Vector2.Distance(player.position, pivot.position) <= detectionRadius)
            {
                // Only allow shooting when the timer is up and canShoot is true
                if (timer <= 0 && canShoot)
                {
                    fire?.Invoke();
                }
            }
        }

        private void HitscanFire()
        {
            onFire?.Invoke();

            Vector2 directionToPlayer = player.position - pivot.position;
            RaycastHit2D hit = Physics2D.Raycast(pivot.position, directionToPlayer, whatIsPlayer);

            if (hit.collider != null)
            {
                PlayerStats stats = hit.collider.GetComponent<PlayerStats>();
                if (stats != null)
                {
                    // Deal damage to the player
                    stats.Damage(damage);
                }
            }
            // Play the shoot sound and instantiate the muzzle flash
            Instantiate(muzzleFlash, muzzle.position, Quaternion.identity);
            SoundManager.Instance.PlaySound(shootSFX, 1);
            // Reset the firing timer
            timer = fireRate;
        }

        private void ProjectileFire()
        {
            onFire?.Invoke();

            // Instantiate the projectile
            GameObject projectile = Instantiate(projectilePrefab, muzzle.position, Quaternion.identity);

            // Calculate the direction towards the player
            Vector2 directionToPlayer = player.position - pivot.position;

            // Get the angle
            float angle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;

            // Rotate the projectile to face the player's direction
            projectile.transform.rotation = Quaternion.Euler(0f, 0f, angle);

            Rigidbody2D projectileRigidbody = projectile.GetComponent<Rigidbody2D>();

            // Set the velocity of the projectile to make it move towards the player
            projectileRigidbody.velocity = directionToPlayer.normalized * projectileSpeed;

            // Set the damage of the projectile
            TurretProjectile projectileDamage = projectile.GetComponent<TurretProjectile>();
            if (projectileDamage != null) projectileDamage.damage = damage;

            // Destroy the projectile after its lifetime
            Destroy(projectile, projectileLifetime);

            // Play the shoot sound and instantiate the muzzle flash
            Instantiate(muzzleFlash, muzzle.position, Quaternion.identity);
            SoundManager.Instance.PlaySound(shootSFX, 1);
            // Reset the firing timer
            timer = fireRate;
        }
    }
}
