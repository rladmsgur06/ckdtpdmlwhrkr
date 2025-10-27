using UnityEngine;

namespace cowsins2D
{
    [System.Serializable]
    public class SlotData
    {
        public Item_SO inventoryItem;
        public int amount;
        [ReadOnly] public int currentBullets;
        [ReadOnly] public int totalBullets;

        public SlotData(Item_SO inventoryItem, int amount)
        {
            this.inventoryItem = inventoryItem;
            this.amount = amount;
        }

        public SlotData() 
        { 
            inventoryItem = null;
            amount = 0;
            currentBullets = 0;
            totalBullets = 0;
        }
    }
}