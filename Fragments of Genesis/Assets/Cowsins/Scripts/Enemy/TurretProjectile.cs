using UnityEngine;

namespace cowsins2D
{
    public class TurretProjectile : MonoBehaviour
    {
        // Damage passed from the turret to this projectile.
        [HideInInspector]public float damage; 

        // Handle collisions
        private void OnTriggerEnter2D(Collider2D other)
        {
            // Check if the colliding object is the player
            PlayerStats playerStats = other.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                // Apply damage to the player
                playerStats.Damage(damage);

                // Destroy the projectile on collision with the player
                Destroy(gameObject);
            }
        }
    }
}