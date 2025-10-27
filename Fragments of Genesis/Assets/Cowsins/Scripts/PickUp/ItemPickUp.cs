using UnityEngine;
using UnityEngine.Events;

namespace cowsins2D
{
    public class ItemPickUp : PickUp
    {
        [Tooltip("Amount to pick up, these will be stored in the Inventory in case you use a full inventory system.")] public int amount;

        [Tooltip("Item_SO to pick up.")] public Item_SO item;

        private UnityEvent onPickUp;

        protected override void Start()
        {
            base.Start();
            SetPickeableGraphics(item);
        }

        public override string GetInteractText()
        {
            string text = includeNamePickUpText ? $"{interactText} {item?.itemName}" : interactText;
            return text;
        }

        public override void Interact(InteractionManager source)
        {
            // Perform Basic or Full operations depending on the Player´s current inventory system selected.

            // Get a reference to the player WeaponController.
            WeaponController wc = source.GetComponent<WeaponController>();
            InventoryManager inventoryManager = source.GetComponent<InventoryManager>();

            switch (inventoryManager.InventoryMethod)
            {
                case InventoryMethod.HotbarOnly:
                    BasicInteraction(wc);
                    break;
                case InventoryMethod.Full:
                    FullInteraction(wc);
                    break;
            }

            //Perform custom interaction operations.
            onPickUp?.Invoke();
        }

        private void BasicInteraction(WeaponController wc)
        {
            // Use the item on interact.
            item.Use(wc);
            Destroy(this.gameObject);
        }

        private void FullInteraction(WeaponController wc)
        {
            // Checks if the inventory is full. If it is, the item will not be picked up.
            if (UIController.Instance.IsInventoryFull()) return;

            // Calculates the remaining amount of the item after picking it up.
            int remainingAmount = amount;

            // While there is still remaining amount, add it to the inventory.
            while (remainingAmount > 0)
            {
                int ammoToAdd = Mathf.Min(remainingAmount, item.maxStack);
                UIController.Instance.PopulateInventory(item, ammoToAdd);
                remainingAmount -= ammoToAdd;
            }

            Destroy(this.gameObject);
        }
    }
}