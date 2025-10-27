using UnityEngine;
using UnityEngine.Events;
namespace cowsins2D
{
    public class AmmoPickUp : PickUp
    {
        [Tooltip("Amount of bullets to pick up.")] public int ammoAmount;

        [Tooltip("AmmoType_SO to pick up.")] public AmmoType_SO ammoType;

        private UnityEvent onPickUp;

        protected override void Start()
        {
            base.Start();
            SetPickeableGraphics(ammoType);
        }

        public override string GetInteractText()
        {
            string text = includeNamePickUpText ? $"{interactText} {ammoType?.itemName}" : interactText;
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
            // Checks if the weapon has limited magazines and if the ammo type matches the weapon's ammo type.
            if (wc.weapon == null || !wc.weapon.limitedMagazines)
            {
                Debug.Log("Can´t pick up bullets for a weapon that does not have limited bullets or if weapon is null.");
                return;
            }

            // Checks if the ammo type matches the weapon's ammo type.
            if (wc.weapon.ammoType != ammoType)
            {
                Debug.Log("Can´t pick up bullets for a weapon that does not use the same type of bullets.");
                return;
            }

            wc.id.totalBullets += ammoAmount;
            UIController.Instance.UpdateWeaponInformation();
            Destroy(this.gameObject);
        }

        private void FullInteraction(WeaponController wc)
        {
            // Checks if the inventory is full. If it is, the ammo will not be picked up.
            if (UIController.Instance.IsInventoryFull()) return;

            // Calculates the remaining ammo after picking up the ammo.
            int remainingAmmo = ammoAmount;

            // While there is still remaining ammo, add it to the inventory.
            while (remainingAmmo > 0)
            {
                int ammoToAdd = Mathf.Min(remainingAmmo, ammoType.maxStack);
                UIController.Instance.PopulateInventory(ammoType, ammoToAdd);
                remainingAmmo -= ammoToAdd;
            }

            // Checks if the player has any ammo in the weapon and the inventory.
            wc.CheckForBullets();

            Destroy(this.gameObject);
        }
    }
}