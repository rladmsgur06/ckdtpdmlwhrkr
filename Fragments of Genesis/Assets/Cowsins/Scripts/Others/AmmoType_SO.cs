using UnityEngine;

namespace cowsins2D
{
    [CreateAssetMenu(fileName = "New Ammo Type", menuName = "Cowsins/New Ammo Type")]
    public class AmmoType_SO : Item_SO
    {
        // Always return false. You cannot perform a custom method on ammo.
        public override bool Use(WeaponController controller)
        {
            return false;
        }
    }
}