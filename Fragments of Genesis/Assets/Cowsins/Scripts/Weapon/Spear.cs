using UnityEngine;

namespace cowsins2D
{
    public class Spear : Projectile
    {
        [SerializeField] private GameObject jumpPadSpear;

        // Called when the projectile is destroyed
        public override void DestroyProjectile(Collider2D collider)
        {
            // Instantiate spear visuals
            var spear = Instantiate(jumpPadSpear, transform.position, transform.GetChild(0).rotation);
            // Attach the spear to the collider object
            spear.transform.SetParent(collider.transform);

            // Destroy base function
            base.DestroyProjectile(collider);
        }
    }
}