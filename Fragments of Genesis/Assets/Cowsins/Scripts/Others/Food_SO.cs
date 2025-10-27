using UnityEngine;

namespace cowsins2D
{
    [CreateAssetMenu(menuName = "Cowsins/New Food item", fileName = "New Food Item", order = 2)]
    public class Food_SO : Item_SO
    {
        public float healProvided;
        public override bool Use(WeaponController controller)
        {
            PlayerStats player = controller.GetComponent<PlayerStats>();

            // Return false if the function could not be completed
            if (player.Health >= player.MaxHealth && player.MaxShield <= 0 || player.Shield >= player.MaxShield) return false;

            player.Heal(healProvided);

            // Return true = Success. The method could be complete
            return true; 
        }
    }
}