using UnityEngine;
using UnityEngine.Events;

namespace cowsins2D
{
    [System.Serializable]
    public enum InventoryMethod
    {
        None, HotbarOnly, Full
    }

    public class InventoryManager : MonoBehaviour
    {
        [System.Serializable]
        public class Events
        {
            public UnityEvent onToggleInventory, onOpenInventory, onCloseInventory;
        }

        [SerializeField] private InventoryMethod inventoryMethod;

        [Min(1), SerializeField] private int hotbarSize;

        [SerializeField, Tooltip("Reference to the inventory object.")] private GameObject inventory;

        [SerializeField, Tooltip("Number of rows and columns, the total number of items is rows*columns.")] private int inventoryRowsAmount, inventoryColumnsAmount;

        [SerializeField, Tooltip("If enabled, whe the Inventory is opened, the player can drop items outside the " +
            "inventory by releasing them outside the bounds of the Inventory Container")]
        private bool dropOnOutsideRelease = true;

        [SerializeField, Tooltip("Sounds played on opening & closing the inventory. ")] private AudioClip openInventorySFX, closeInventorySFX;

        [SerializeField] private Events events;

        public InventoryMethod InventoryMethod => inventoryMethod;
        public int HotbarSize => hotbarSize;
        public int InventorySize => inventoryRowsAmount * inventoryColumnsAmount;
        public bool DropOnOutsideRelease => dropOnOutsideRelease;

        public delegate void ToggleInventoryMethod();

        public ToggleInventoryMethod toggleInventory;

        private bool inventoryOpen = false;
        public bool InventoryOpen => inventoryOpen;

        private PlayerControl playerControl;
        private WeaponController weaponController;

        private void OnEnable()
        {
            playerControl = GetComponent<PlayerControl>();
            weaponController = GetComponent<WeaponController>();

            weaponController.events.onShoot.AddListener(UpdateCurrentWeaponBullets);
            weaponController.events.onStopReload.AddListener(UpdateCurrentWeaponBullets);

            InputManager.Instance.onOpenInventory += PerformToggleInventoryEvent;
        }

        private void OnDisable()
        {
            weaponController.events.onShoot.RemoveListener(UpdateCurrentWeaponBullets);
            weaponController.events.onStopReload.RemoveListener(UpdateCurrentWeaponBullets);

            InputManager.Instance.onOpenInventory -= PerformToggleInventoryEvent;
        }
        private void Start()
        {
            InitializeInventory();
        }

        private void Update()
        {
            HandleHotBar();
            if (inventoryOpen) UIController.Instance.HandleInventoryNavigation();
        }

        private void InitializeInventory()
        {
            // Checks what methods to use depending on the inventory method
            switch (inventoryMethod)
            {
                case InventoryMethod.None: break;

                case InventoryMethod.HotbarOnly:
                    UIController.Instance.InitializeHotBar(hotbarSize);
                    break;

                case InventoryMethod.Full:
                    UIController.Instance.InitializeHotBar(hotbarSize);
                    UIController.Instance.InitializeFullInventory(SlotType.Inventory, inventoryRowsAmount, inventoryColumnsAmount, ref UIController.Instance.inventorySlots);
                    toggleInventory = ToggleFullInventory;
                    break;
            }

            if(inventory !=  null) inventory.SetActive(false);
        }
        private void HandleHotBar()
        {
            // You cannot select a weapon while reloading
            if (!weaponController || weaponController.reloading || !playerControl.Controllable) return;

            // Based on the mouse wheel input, change the weapon
            float scrollDelta = InputManager.PlayerInputs.MouseWheel.y;
            bool next = scrollDelta > 0 || InputManager.PlayerInputs.NextWeapon;
            bool prev = scrollDelta < 0 || InputManager.PlayerInputs.PreviousWeapon;

            if (next && weaponController.currentWeapon < hotbarSize - 1)
            {
                weaponController.currentWeapon++;
                weaponController.UnholsterWeapon();
                UIController.updateHotbarSelection?.Invoke();
            }
            else if (prev && weaponController.currentWeapon > 0)
            {
                weaponController.currentWeapon--;
                weaponController.UnholsterWeapon();
                UIController.updateHotbarSelection?.Invoke();
            }
        }

        private void PerformToggleInventoryEvent()
        {
            if (!playerControl.Controllable && inventory.activeSelf == true || playerControl.Controllable)
                toggleInventory?.Invoke();
        }

        private void ToggleFullInventory()
        {
            if(PauseMenu.isPaused) return;

            bool shouldOpen = !inventory.gameObject.activeSelf;

            if (shouldOpen) OpenInventory();
            else CloseInventory();

            events.onToggleInventory?.Invoke();
        }

        public void OpenInventory()
        {
            inventoryOpen = true;
            inventory.SetActive(true);

            playerControl.LoseControl();

            SoundManager.Instance.PlaySound(openInventorySFX, 1);
            Crosshair.Instance.Hide(true);

            UIController.Instance.SetHighlightedSlot(UIController.Instance.inventorySlots[0,0], false);
            UIController.Instance.HideInteractionUI();

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            events.onOpenInventory?.Invoke();
        }

        public void CloseInventory()
        {
            inventoryOpen = false;
            inventory.SetActive(false);
            playerControl.CheckIfCanGrantControl();

            UIController.Instance.currentInventorySlot = null;
            UIController.Instance.highlightedInventorySlot?.OnPointerExit(null);
            UIController.Instance.highlightedInventorySlot = null;
            UIController.Instance.CloseChest();

            Tooltip.Instance.Hide();
            SoundManager.Instance.PlaySound(closeInventorySFX, 1);
            Cursor.visible = false;
            if (Crosshair.Instance.CheckIfCanShow()) Crosshair.Instance.Show();

            events.onCloseInventory?.Invoke();
        }

        private void UpdateCurrentWeaponBullets()
        {
            WeaponIdentification id = weaponController.id;
            if (id == null) return;

            UIController.Instance.OverrideHotbarSlotBullets(weaponController.currentWeapon, id.currentBullets, id.totalBullets); 
        }
    }
}