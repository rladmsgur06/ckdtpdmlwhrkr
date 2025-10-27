using UnityEngine;
using UnityEngine.Events;

namespace cowsins2D
{
    public class LootChest : Interactable, ITriggerable
    {
        [SerializeField, Range(0,5), Tooltip("Rows and columns to procedurally generate the chest UI Slots.")] private int rows, columns;

        [SerializeField, Tooltip("Reference to the UI Inventory Slot.")] private InventorySlot inventorySlot;

        [SerializeField, Tooltip("Loot inside the chest, notice that you cannot have more elements than Row*Columns.")] private SlotData[] initialLoot;

        private bool isOpen = false;
        private bool allowClose = false;

        private InventorySlot[,] chestSlots;
        private SlotData[] currentLoot;
        public InventorySlot[,] ChestSlots => chestSlots;

        private InventoryManager inventoryManager;

        public UnityEvent onInteract;

        private void Awake()
        {
            if(initialLoot.Length > rows * columns )
            {
                Debug.LogError("Loot items amount cannot be greater than the amount of available slots.");
                return;
            }

            currentLoot = initialLoot;
        }

        private void Update()
        {
            if (!allowClose || !isOpen) return;
            // Because opening the inventory disables the control over the player, we need a way to close the loot chest without giving the player control permissions.
            // This simulates the interaction from the interaction manager inside the loot chest, and it is only applicable for this loot chest.
            // Subject to change for future versions of the asset.
            if (InputManager.PlayerInputs.Interact) CloseChest(); 
        }

        public override void Interact(InteractionManager source)
        {
            InventoryManager inventoryManager = source.GetComponent<InventoryManager>();
            this.inventoryManager = inventoryManager;

            allowClose = false;
            isOpen = !isOpen;

            if (isOpen)
            {
                UIController.Instance.OpenChest(this);
                UIController.Instance.InitializeFullChest(SlotType.Chest, rows, columns, ref chestSlots, currentLoot);
            }
            else
            {
                UIController.Instance.CloseChest();
            }

            // Enable the UI if its closed, and disable if its open.
            onInteract?.Invoke();
            inventoryManager.toggleInventory?.Invoke();

            Invoke(nameof(CanClose), source.TimeToInteract); 

            // If this is the first time we opened the chest, then we need to initialize or generate the inventory
        }
        private void CloseChest() 
        {
            inventoryManager?.CloseInventory();
            UIController.onInteractAvailable?.Invoke(GetInteractText());

            CancelInvoke(nameof(CanClose));
            isOpen = false;
        }

        private void CanClose() => allowClose = true;

        public void SetCurrentLoot(SlotData[] newLoot)
        {
            this.currentLoot = newLoot;
        }

        public void EnterTrigger(GameObject target)
        {

        }
        public void StayTrigger(GameObject target)
        {

        }
        public void ExitTrigger(GameObject target) {}
    }
}