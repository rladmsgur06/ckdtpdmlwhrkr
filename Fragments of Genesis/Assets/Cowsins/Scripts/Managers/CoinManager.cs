using UnityEngine;
namespace cowsins2D
{
    public class CoinManager : MonoBehaviour
    {
        //Stores the player's current coin count.
        public static int coins { get; private set; }

        public static void AddCoins(int amount)
        {
            // Increase the player's coin count by the specified amount.
            coins += amount;

            // Update the coins UI to reflect the player's new balance.
            UIController.Instance.UpdateCoins(coins);
        }

        public static void RemoveCoins(int amount)
        {
            // Decrease the player's coin count by the specified amount.
            coins -= amount;

            // If the player's coin count is less than zero, set it to zero.
            if (coins < 0) coins = 0;

            // Update the coins UI to reflect the player's new balance.
            UIController.Instance.UpdateCoins(coins);
        }
    }
}

