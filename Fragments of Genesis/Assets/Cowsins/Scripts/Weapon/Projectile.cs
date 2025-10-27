using UnityEngine;

namespace cowsins2D
{
    public class Projectile : MonoBehaviour
    {
        private Vector2 direction;

        private float projectileSpeed;

        private LayerMask hitLayer;

        private float collisionSize;

        private float damage;

        private bool explosion;

        private float explosionDamage, explosionForce, explosionRadius;

        public delegate void ProjectileHit(Collider2D target, Vector2 location, Vector2 hitOrientation);

        public ProjectileHit projectileHit;

        private void Update()
        {
            // Move the projectile in the appropriate direction
            transform.Translate(direction * projectileSpeed * Time.deltaTime);

            // Registers collisions with the environment and hitLayer
            HandleCollisions();
        }
        public void SetInitialSettings(Vector2 dir, float speed, LayerMask layer, float colSize, float dmg, bool exp, float explosionDmg, float explosionFrc, float explosionRds)
        {
            // Retrieves and stores all the necessary variablers for the projectiles to work.
            direction = dir;
            projectileSpeed = speed;
            hitLayer = layer;
            collisionSize = colSize;
            damage = dmg;
            explosion = exp;
            explosionDamage = explosionDmg;
            explosionForce = explosionFrc;
            explosionRadius = explosionRds;
        }
        private void HandleCollisions()
        {
            // Check if the projectile collided given a collision size.
            RaycastHit2D hit = Physics2D.BoxCast(transform.position, new Vector2(collisionSize, collisionSize), 0, transform.up, 1, hitLayer);
            if (hit)
            {
                Collider2D collider = hit.collider;
                if (!collider.isTrigger && !explosion) projectileHit?.Invoke(hit.collider, hit.point, hit.normal);
                DestroyProjectile(collider);
            }
        }

        public virtual void DestroyProjectile(Collider2D collider)
        {
            if (explosion)
            {
                Collider2D[] hit = Physics2D.OverlapCircleAll(transform.position, explosionRadius, hitLayer);

                if (hit.Length != 0)
                {
                    foreach (var h in hit)
                    {
                        IDamageable damageable = h.GetComponent<IDamageable>();
                        Rigidbody2D rigidbody2D = h.GetComponent<Rigidbody2D>();
                        damageable?.Damage(explosionDamage / Vector3.Distance(h.transform.position, transform.position));
                        rigidbody2D?.AddForce((h.transform.position - transform.position) / Vector3.Distance(h.transform.position, transform.position) * explosionForce * Time.deltaTime, ForceMode2D.Impulse);
                    }
                }
            }
            Destroy(this.gameObject);
        }
    }
}