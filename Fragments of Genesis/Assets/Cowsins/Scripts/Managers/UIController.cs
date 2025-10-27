using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Events;

namespace cowsins2D
{
    public class UIController : MonoBehaviour
    {
        [System.Serializable]
        public enum HealthDisplayMethod
        {
            Numeric, Bar, Both
        }

        [ReadOnly] public InventorySlot currentInventorySlot = null, highlightedInventorySlot = null;

        [SerializeField] private WeaponController player;

        [SerializeField] private HealthDisplayMethod healthDisplayMethod;

        [SerializeField] private Image healthUI, shieldUI;

        [SerializeField] private float lerpBarValueSpeed;

        [SerializeField] private TextMeshProUGUI healthText, shieldText;

        [SerializeField] private Image healthVignette;

        public Color hurtVignetteColor;

        public Color healVignetteColor;

        [SerializeField] private float lerpVignetteSpeed;

        [SerializeField] private TextMeshProUGUI bulletsUI, magazineUI;

        [SerializeField] private Image overheatUI;

        [SerializeField] private Image weaponDisplay;

        [SerializeField] private TextMeshProUGUI lowAmmoUI, reloadUI;

        [SerializeField] private TextMeshProUGUI coinsUI;

        [SerializeField] private Image xpImage;

        [SerializeField] private TextMeshProUGUI currentLevel, nextLevel;

        [SerializeField] private float lerpXpSpeed;

        [SerializeField] private GameObject interactUI;

        [SerializeField] private TextMeshProUGUI interactText;

        [SerializeField] private Image interactProgress; 

        public Transform inventoryContainer, hotbarContainer, inventoryGraphics, chestGraphics, chestContainer;

        [SerializeField] private GameObject inventorySlot;

        [SerializeField] private float gapX = 40;

        [SerializeField] private float gapY = 40;

        [SerializeField] private Padding inventoryPadding;
        [SerializeField] private Padding hotbarPadding;
        [SerializeField] private Padding chestPadding;

        public Color hotbarSelected, hotbarDefault;

        [Tooltip("Our Slider UI Object. Stamina will be shown here."), SerializeField]
        private Slider staminaSlider;

        [SerializeField] private GameObject dashIcon;

        [SerializeField] private Transform dashUIContainer;

        public delegate void AddXP();

        public static AddXP addXP;

        public delegate void UpdateHotbarSelection();
        public static UpdateHotbarSelection updateHotbarSelection;

        public delegate void OnSlotPointerUp();
        public static OnSlotPointerUp onSlotPointerUp;

        private float targetHealthValue, targetShieldValue;
        public delegate void OnUpdateHealth(float health, float shield);
        public OnUpdateHealth onUpdateHealth;

        public delegate void OnInteractAvailable(string text);
        public static OnInteractAvailable onInteractAvailable;

        public delegate void OnInteractionDisabled();
        public static OnInteractionDisabled onInteractionDisabled;

        public static UIController Instance;

        private List<GameObject> dashes = new List<GameObject>();

        private InventorySlot[] hotbarSlots;

        public InventorySlot[,] inventorySlots;

        private PlayerMovement playerMovement;
        private InteractionManager interactionManager;
        private InventoryManager inventoryManager;
        private WeaponController weaponController;
        private PlayerStats playerStats;
        private PlayerControl playerControl;

        public InventoryManager InventoryManager => inventoryManager;   

        public HealthDisplayMethod _HealthDisplayMethod => healthDisplayMethod;

        public UnityEvent onUpdateWeaponInfo;

        private void OnEnable()
        {
            // Ensure there is only 1 instance of UIController in the game
            if (Instance == null) Instance = this;
            else Destroy(this.gameObject);

            // Access the player
            playerMovement = player.GetComponent<PlayerMovement>();
            interactionManager = player.GetComponent<InteractionManager>();
            weaponController = player.GetComponent<WeaponController>();
            inventoryManager = player.GetComponent<InventoryManager>();
            playerStats = player.GetComponent<PlayerStats>();
            playerControl = player.GetComponent<PlayerControl>();

            switch (healthDisplayMethod)
            {
                case HealthDisplayMethod.Numeric:
                    onUpdateHealth = NumericHealth;
                    if (healthUI != null) Destroy(healthUI.transform.parent.gameObject);
                    if (shieldUI != null) Destroy(shieldUI.transform.parent.gameObject);
                    break;
                case HealthDisplayMethod.Bar:
                    onUpdateHealth = BarHealth;
                    if (healthText != null) Destroy(healthText.gameObject);
                    if (shieldText != null) Destroy(shieldText.gameObject);
                    break;
                case HealthDisplayMethod.Both:
                    onUpdateHealth = NumericHealth;
                    onUpdateHealth += BarHealth;
                    break;
            }

            onInteractAvailable = ShowInteractionUI;
            onInteractionDisabled = HideInteractionUI;
            addXP = UpdateXP;

            UpdateWeaponInformation();
        }

        private void OnDisable()
        {
            onUpdateHealth = null;
        }

        private void Start()
        {
            // Reset coins
            UpdateCoins(0);

            // if the player uses stamina we do not want to display the stamina bar mid-game
            if (playerMovement.UsesStamina) staminaSlider.gameObject.SetActive(false);
            updateHotbarSelection?.Invoke();

            // Set XP to the current value
            UpdateXP();

            // Initialize the procedurally generated UI for the dash
            InitializeDashUI();
            HideInteractionUI();

            CloseChest();
        }
        private void Update()
        {
            // Calculate the target color for the health vignette
            Color targetColor = healthVignette.color * new Vector4(1, 1, 1, 0);

            // If the health vignette color is not the target color, lerp towards the target color
            if (healthVignette.color != targetColor) healthVignette.color = Vector4.Lerp(healthVignette.color, targetColor, Time.deltaTime * lerpVignetteSpeed);

            // Check if the health UI and shield UI are not null
            if (healthUI != null && shieldUI != null)
            {
                // Lerp the health UI fill amount towards the target health value
                healthUI.fillAmount = Mathf.Lerp(healthUI.fillAmount, targetHealthValue / playerStats.MaxHealth, lerpBarValueSpeed * Time.deltaTime);

                // Lerp the shield UI fill amount towards the target shield value
                shieldUI.fillAmount = Mathf.Lerp(shieldUI.fillAmount, targetShieldValue / playerStats.MaxShield, lerpBarValueSpeed * Time.deltaTime);
            }

            // Calculate the target XP value
            float targetXp = ExperienceManager.Instance.GetCurrentExperience() / ExperienceManager.Instance.experienceRequirements[ExperienceManager.Instance.playerLevel];

            // Lerp the XP image fill amount towards the target XP value
            xpImage.fillAmount = Mathf.Lerp(xpImage.fillAmount, targetXp, lerpXpSpeed * Time.deltaTime);

            // Check if the stamina slider is not null and the player movement uses stamina
            if (staminaSlider == null || !playerMovement.UsesStamina)
            {
                // Set the stamina slider object to inactive and return
                staminaSlider.gameObject.SetActive(false);
                return;
            }

            // Set the stamina slider max value to the player movement max stamina
            staminaSlider.maxValue = playerMovement.MaxStamina;

            // Set the stamina slider value to the player movement stamina
            staminaSlider.value = playerMovement.stamina;
        }
        public void UpdateWeaponInformation()
        {
            onUpdateWeaponInfo?.Invoke();

            // if weapon is null, remove UI Elements that represent weapon statistics.
            if (player.weapon == null || player.id == null)
            {
                weaponDisplay.gameObject.SetActive(false);
                bulletsUI.gameObject.SetActive(false);
                magazineUI.gameObject.SetActive(false);
                lowAmmoUI.gameObject.SetActive(false);
                reloadUI.gameObject.SetActive(false);
                return; // Do not continue from here
            }

            // The weapon is not null, enable UI Elements that represent weapon statistics.
            weaponDisplay.gameObject.SetActive(true);
            bulletsUI.gameObject.SetActive(true);
            magazineUI.gameObject.SetActive(true);
            weaponDisplay.sprite = player.weapon.itemIcon;

            bool hasWeapon = player.weapon != null;
            int currentBullets = player.id ? player.id.currentBullets : 0;
            int magazineSize = hasWeapon ? player.weapon.magazineSize : 0;
            bool isReloading = player.reloading;
            bool autoReload = hasWeapon && player.weapon.autoReload;

            if (lowAmmoUI != null && hasWeapon && currentBullets > 0 && currentBullets < magazineSize / 3 && !isReloading)
            {
                lowAmmoUI.gameObject.SetActive(true);
                reloadUI?.gameObject.SetActive(false);
            }
            else if (reloadUI != null && hasWeapon && currentBullets <= 0 && !autoReload && !isReloading)
            {
                lowAmmoUI?.gameObject.SetActive(false);
                reloadUI.gameObject.SetActive(true);
            }
            else if ((lowAmmoUI != null && reloadUI != null && hasWeapon && currentBullets > magazineSize / 3) || !hasWeapon)
            {
                lowAmmoUI?.gameObject.SetActive(false);
                reloadUI?.gameObject.SetActive(false);
            }

            // Checks if the player's weapon is an Overheat reloading method.
            if (player.weapon.reloadingMethod == Weapon_SO.ReloadingMethod.Overheat)
            {
                bulletsUI.gameObject.SetActive(false);
                magazineUI.gameObject.SetActive(false);
                overheatUI.transform.parent.gameObject.SetActive(true);
                overheatUI.fillAmount = player.heatAmount / player.weapon.magazineSize;
                return;
            }
            // Checks if the player's weapon is a Melee shooting style.
            if (player.weapon.shootingStyle == Weapon_SO.ShootingStyle.Melee)
            {
                bulletsUI.gameObject.SetActive(false);
                magazineUI.gameObject.SetActive(false);
                return;
            }
            overheatUI.transform.parent.gameObject.SetActive(false);

            // Sets the bullets UI text to the player's current bullets, or the word "infinite" if the weapon has infinite bullets.
            bulletsUI.text = player.weapon.infiniteBullets ? "infinite" : player.id.currentBullets.ToString();

            // Sets the magazine UI text to the player's total bullets (if the weapon has limited magazines) or the weapon's magazine size (if the weapon has unlimited magazines).
            // If the weapon has infinite bullets, the magazine UI text is set to an empty string.
            magazineUI.text = player.weapon.infiniteBullets ? "" : player.weapon.limitedMagazines ? player.id.totalBullets.ToString() : player.weapon.magazineSize.ToString();
        }

        public void EnableStaminaSlider(bool condition)
        {
            staminaSlider.gameObject.SetActive(condition);
        }

        public void SetVignetteColor(Color color)
        {
            healthVignette.color = color;
        }

        public void UpdateCoins(int coins)
        {
            coinsUI.text = coins.ToString();
        }

        public void SetHighlightedSlot(InventorySlot slot, bool pointerEnter)
        {
            highlightedInventorySlot?.OnPointerExit(null);
            highlightedInventorySlot = slot;
            if(pointerEnter) highlightedInventorySlot.OnPointerEnter(null);
        }

        private Vector2 inventoryNavInputDirection;
        private float inventoryNavInputCooldown = 0.2f;
        private float lastInventoryNavInputTime = 0f;

        public void HandleInventoryNavigation()
        {
            // Handle Selection 
            if (highlightedInventorySlot != null)
            {
                if (InputManager.PlayerInputs.UISelect)
                {
                    var pointerData = new PointerEventData(EventSystem.current)
                    {
                        button = PointerEventData.InputButton.Left
                    };

                    if (currentInventorySlot == null)
                        highlightedInventorySlot.OnPointerDown(pointerData);
                    else
                        highlightedInventorySlot.OnPointerUp(pointerData);
                }
                else if (InputManager.PlayerInputs.InventoryDrop)
                {
                    highlightedInventorySlot.OnDrop();
                }
                else if (InputManager.PlayerInputs.InventoryUse)
                {
                    highlightedInventorySlot.OnUse();
                }
            }

            // Prevent Input Spamming
            if (Time.time - lastInventoryNavInputTime < inventoryNavInputCooldown)
                return;

            Vector2 navInput = InputManager.PlayerInputs.UINavigation;
            if (navInput == Vector2.zero)
                return;

            // Determine Direction
            inventoryNavInputDirection = Mathf.Abs(navInput.x) > Mathf.Abs(navInput.y)
                ? new Vector2(Mathf.Sign(navInput.x), 0)
                : new Vector2(0, Mathf.Sign(navInput.y));

            lastInventoryNavInputTime = Time.time;

            // If no slot is highlighted, highlight the first inventory slot
            if (highlightedInventorySlot == null)
            {
                SetHighlightedSlot(inventorySlots[0, 0], true);
                return;
            }

            int invRows = inventorySlots.GetLength(0);
            int invCols = inventorySlots.GetLength(1);
            InventorySlot[,] chestSlots = currentOpenChest?.ChestSlots;
            int chestRows = chestSlots?.GetLength(0) ?? 0;
            int chestCols = chestSlots?.GetLength(1) ?? 0;

            bool isInInventory = highlightedInventorySlot.slotType == SlotType.Inventory;
            bool isInHotbar = highlightedInventorySlot.slotType == SlotType.Hotbar;
            bool isInChest = highlightedInventorySlot.slotType == SlotType.Chest;

            int currentCol = highlightedInventorySlot.column + (int)inventoryNavInputDirection.x;
            int currentRow = highlightedInventorySlot.row - (int)inventoryNavInputDirection.y;

            // Handle Navigation based on current slot type
            if (isInInventory) HandleInventoryNavigationMovement(currentCol, currentRow, invCols, invRows, chestCols, chestRows, chestSlots);
            else if (isInChest && chestSlots != null) HandleChestNavigationMovement(currentCol, currentRow, chestCols, chestRows, invRows);
            else if (isInHotbar) HandleHotbarNavigationMovement(currentCol, invCols, invRows);
        }

        private void HandleInventoryNavigationMovement(int col, int row, int invCols, int invRows, int chestCols, int chestRows, InventorySlot[,] chestSlots)
        {
            // Move from inventory to chest
            if (currentOpenChest != null && inventoryNavInputDirection.x < 0 && highlightedInventorySlot.column == 0)
            {
                int chestRow = Mathf.Clamp(highlightedInventorySlot.row, 0, chestRows - 1);
                int chestCol = chestCols - 1;
                SetHighlightedSlot(chestSlots[chestRow, chestCol], true);
                return;
            }

            // Prevent moving past the last column
            if (inventoryNavInputDirection.x > 0 && highlightedInventorySlot.column == invCols - 1)
                return;

            // Move from inventor to hotbar
            if (inventoryNavInputDirection.y < 0 && highlightedInventorySlot.row == invRows - 1)
            {
                int hotbarIndex = Mathf.Clamp(highlightedInventorySlot.column, 0, hotbarSlots.Length - 1);
                SetHighlightedSlot(hotbarSlots[hotbarIndex], true);
                return;
            }

            // Move within inventory
            col = Mathf.Clamp(col, 0, invCols - 1);
            row = Mathf.Clamp(row, 0, invRows - 1);

            if (row != highlightedInventorySlot.row || col != highlightedInventorySlot.column)
                SetHighlightedSlot(inventorySlots[row, col], true);
        }

        private void HandleChestNavigationMovement(int col, int row, int chestCols, int chestRows, int invRows)
        {
            // Move from chest to inventory
            if (inventoryNavInputDirection.x > 0 && highlightedInventorySlot.column == chestCols - 1)
            {
                int invRow = Mathf.Clamp(highlightedInventorySlot.row, 0, invRows - 1);
                SetHighlightedSlot(inventorySlots[invRow, 0], true);
                return;
            }

            // Prevent moving past the bounds
            if ((inventoryNavInputDirection.x < 0 && highlightedInventorySlot.column == 0) ||
                (inventoryNavInputDirection.x > 0 && highlightedInventorySlot.column == chestCols - 1))
            {
                return;
            }

            // Move within chest
            col = Mathf.Clamp(col, 0, chestCols - 1);
            row = Mathf.Clamp(row, 0, chestRows - 1);

            if (row != highlightedInventorySlot.row || col != highlightedInventorySlot.column)
                SetHighlightedSlot(currentOpenChest.ChestSlots[row, col], true);
        }

        private void HandleHotbarNavigationMovement(int hotbarIndex, int invCols, int invRows)
        {
            // Move from hotbar to inventory
            if (inventoryNavInputDirection.y > 0)
            {
                int targetRow = invRows - 1;
                int targetCol = Mathf.Clamp(highlightedInventorySlot.column, 0, invCols - 1);
                SetHighlightedSlot(inventorySlots[targetRow, targetCol], true);
                return;
            }

            // Move within hotbar
            hotbarIndex = Mathf.Clamp(hotbarIndex, 0, hotbarSlots.Length - 1);

            if (hotbarIndex != highlightedInventorySlot.column)
                SetHighlightedSlot(hotbarSlots[hotbarIndex], true);
        }


        public void InitializeHotBar(int hotbarSize)
        {
            hotbarSlots = new InventorySlot[hotbarSize];

            // Instantiate, Populate & Initialize each Inventory Slot individually.
            for (int i = 0; i < hotbarSize; i++)
            {
                InventorySlot slot = GenerateSlot(SlotType.Hotbar, 0, i, hotbarPadding.ToVector3(), hotbarContainer, null);
                Weapon_SO weapon = i >= 0 && i < weaponController.InitialWeapons.Length ? weaponController.InitialWeapons[i] : null;
                SlotData slotData = weapon != null ? new SlotData(weapon, 1) : new SlotData();
                slot.slotData = slotData;
            }
        }

        public void InitializeFullInventory(SlotType slotType, int rows, int columns, ref InventorySlot[,] slotsArray)
        {
            slotsArray = new InventorySlot[rows, columns];

            float totalWidth = columns * gapX + (inventoryPadding.horizontal);
            float totalHeight = rows * gapY + (inventoryPadding.vertical);

            RectTransform parentRect = inventoryGraphics.GetComponent<RectTransform>();
            if (parentRect != null)
                parentRect.sizeDelta = new Vector2(totalWidth, totalHeight);

            // Generate each slot individually.
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < columns; col++)
                {
                    GenerateSlot(slotType, row, col, inventoryPadding.ToVector3(), inventoryContainer, slotsArray);
                }
            }
        }

        public void InitializeFullChest(SlotType slotType, int rows, int columns, ref InventorySlot[,] slotsArray, SlotData[] lootData)
        {
            slotsArray = new InventorySlot[rows, columns];
            // Starting from the top-left

            float totalWidth = columns * gapX + chestPadding.horizontal;
            float totalHeight = rows * gapY + chestPadding.vertical;

            RectTransform parentRect = chestGraphics.GetComponent<RectTransform>();
            if (parentRect != null)
                parentRect.sizeDelta = new Vector2(totalWidth, totalHeight);

            // Generate each slot individually.
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < columns; col++)
                {
                    InventorySlot inventorySlot = GenerateSlot(slotType, row, col, chestPadding.ToVector3(), chestContainer, slotsArray);

                    if (inventorySlot.slotData == null)
                        inventorySlot.slotData = new SlotData();

                    int lootIndex = row * columns + col;

                    if (lootData != null && lootIndex < lootData.Length && lootData[lootIndex] != null && lootData[lootIndex].inventoryItem != null)
                    {
                        inventorySlot.slotData.inventoryItem = lootData[lootIndex].inventoryItem;
                        inventorySlot.slotData.amount = inventorySlot.slotData.amount <= inventorySlot.slotData.inventoryItem.maxStack 
                            ? lootData[lootIndex].amount
                            : inventorySlot.slotData.inventoryItem.maxStack;
                        if(lootData[lootIndex].inventoryItem is Weapon_SO)
                        {
                            Weapon_SO weapon = (Weapon_SO)lootData[lootIndex].inventoryItem;
                            inventorySlot.slotData.currentBullets = weapon.magazineSize;
                            inventorySlot.slotData.totalBullets = weapon.limitedMagazines ? weapon.magazineSize * weapon.amountOfMagazines : weapon.magazineSize;
                        }
                    }
                    else
                    {
                        inventorySlot.slotData = new SlotData();
                    }
                    inventorySlot.UpdateGraphics();
                }
            }
        }

        private InventorySlot GenerateSlot(SlotType slotType, int row, int col, Vector3 offset, Transform instantiationParent, InventorySlot[,] slotsArray)
        {
            GameObject button = Instantiate(inventorySlot, instantiationParent);
            RectTransform rectTransform = button.GetComponent<RectTransform>();
            bool isHotbar = slotType == SlotType.Hotbar;

            rectTransform.localPosition = new Vector3(col * gapX, -row * gapY - offset.y * 2, 0) + offset;

            button.name = $"Slot [{row},{col}]";

            InventorySlot slot = button.GetComponent<InventorySlot>();

            slot.row = row;
            slot.column = col;

            slot.weaponController = weaponController;
            slot.interactionManager = interactionManager;
            slot.playerControl = playerControl;

            if (isHotbar)
            {
                hotbarSlots[col] = slot;
                slot.slotType = SlotType.Hotbar;
            }
            else
            {
                slotsArray[row, col] = slot;
                slot.slotType = slotType;
            }

            return slot;
        }

        public void ModifyHotbarSlotAmount(int index, int amount)
        {
            if (IndexWithinHotbarBounds(index))
                hotbarSlots[index].slotData.amount = amount;
        }

        public void OverrideHotbarSlotData(int index, SlotData slotData)
        {
            if (IndexWithinHotbarBounds(index))
                hotbarSlots[index].slotData = slotData;
        }

        public void OverrideHotbarSlotBullets(int index, int bulletsLeftInMagazine, int totalBullets)
        {
            if (IndexWithinHotbarBounds(index))
            {
                hotbarSlots[index].slotData.currentBullets = bulletsLeftInMagazine;
                hotbarSlots[index].slotData.totalBullets = totalBullets;
            }
        }

        private bool IndexWithinHotbarBounds(int index)
        {
            return hotbarSlots != null && index >= 0 && index < hotbarSlots.Length;
        }

        private LootChest currentOpenChest = null;

        public void OpenChest(LootChest chest)
        {
            chestGraphics.gameObject.SetActive(true);
            currentOpenChest = chest;
        }

        public void CloseChest()
        {
            // Update Information inside the chest before closing to ensure all loot data ( SlotData ) is stored
            if(currentOpenChest != null)
            {
                int rows = currentOpenChest.ChestSlots.GetLength(0);
                int cols = currentOpenChest.ChestSlots.GetLength(1);
                SlotData[] currentLootData = new SlotData[rows * cols];
                int index = 0;

                for (int row = 0; row < rows; row++)
                {
                    for (int col = 0; col < cols; col++)
                    {
                        SlotData slotData = currentOpenChest.ChestSlots[row, col].slotData;
                        currentLootData[index++] = slotData;
                    }
                }
                currentOpenChest.SetCurrentLoot(currentLootData);
            }
            chestGraphics.gameObject.SetActive(false);
            currentOpenChest = null;

            // Clear all Inventory Slots
            foreach (Transform child in chestContainer)
            {
                Destroy(child.gameObject);
            }
        }

        // Switches the hotbar items between the current item slot and the highlighted item slot.
        // Returns if the current item is not a weapon and the highlighted item slot is a hotbar slot,
        // or if the current item slot is a hotbar slot and the highlighted item is not a weapon,
        // or if the controller is reloading and either the current item or highlighted item is a weapon.
        public void SwitchItems(WeaponController controller)
        {
            // Get the slot data for the current and highlighted item slots.
            SlotData currentData = currentInventorySlot.slotData;
            SlotData highlightData = highlightedInventorySlot.slotData;

            // Check if either item is not a weapon or the controller is reloading.
            if (!(currentData.inventoryItem is Weapon_SO) && highlightedInventorySlot.isHotbarSlot
                || currentInventorySlot.isHotbarSlot && highlightData.inventoryItem != null && !(highlightData.inventoryItem is Weapon_SO)
                || controller.reloading && (currentData.inventoryItem is Weapon_SO || highlightData.inventoryItem is Weapon_SO))
            {
                // Clear the current and highlighted item slots, hide the tooltip, and return.
                currentInventorySlot = null;
                highlightedInventorySlot = null;
                Tooltip.Instance.Hide();
                return;
            }

            if (highlightedInventorySlot.isHotbarSlot && currentInventorySlot.isHotbarSlot)
            {
                WeaponIdentification tempID = controller.inventory[currentInventorySlot.column];
                controller.inventory[currentInventorySlot.column] = controller.inventory[highlightedInventorySlot.column];
                controller.inventory[highlightedInventorySlot.column] = tempID;
                controller.inventory[currentInventorySlot.column]?.gameObject.SetActive(false);
                controller.inventory[highlightedInventorySlot.column]?.gameObject.SetActive(false);
            }

            if(!currentInventorySlot.isHotbarSlot && highlightedInventorySlot.isHotbarSlot
                && currentInventorySlot.slotData?.inventoryItem != null && currentInventorySlot.slotData.inventoryItem is Weapon_SO)
            {
                SlotData slotData = currentInventorySlot.slotData;
                controller.InstantiateWeapon(((Weapon_SO)slotData?.inventoryItem), highlightedInventorySlot.column, slotData.currentBullets, slotData.totalBullets);
            }

            if (currentInventorySlot.isHotbarSlot && !highlightedInventorySlot.isHotbarSlot
               && currentInventorySlot.slotData?.inventoryItem != null && currentInventorySlot.slotData.inventoryItem is Weapon_SO)
            {
                controller.ReleaseWeapon(currentInventorySlot.column);
            }

            // Swap the slot data between the current and highlighted item slots.
            highlightedInventorySlot.slotData = currentData;
            currentInventorySlot.slotData = highlightData;

            onSlotPointerUp?.Invoke();

            // Clear the current and highlighted item slots, and hide the tooltip.
            currentInventorySlot = null;

            Tooltip.Instance.Hide();
        }
        // Returns true if the inventory is full, false otherwise.
        public bool IsInventoryFull()
        {
            for (int x = 0; x < inventorySlots.GetLength(0); x++)
            {
                for (int y = 0; y < inventorySlots.GetLength(1); y++)
                {
                    InventorySlot slot = inventorySlots[x, y];
                    if (slot == null || slot.slotData == null || slot.slotData.inventoryItem == null)
                        return false;
                }
            }

            return true;
        }

        public void PopulateInventory(Item_SO item, int amount)
        {
            // Iterate over the inventory to populate it with a provided item.
            foreach (var slot in inventorySlots)
            {
                InventorySlot _slot = slot;
                if (_slot.slotData.inventoryItem == null)
                {
                    _slot.slotData.inventoryItem = item;
                    _slot.slotData.amount = amount;
                    _slot.UpdateGraphics();
                    break;
                }
            }
        }

        public void PopulateArray(List<GameObject> list, Item_SO item, int amount)
        {
            foreach (var slot in list)
            {
                InventorySlot _slot = slot.GetComponent<InventorySlot>();
                if (_slot.slotData.inventoryItem == null)
                {
                    _slot.slotData.inventoryItem = item;
                    _slot.slotData.amount = amount;
                    _slot.UpdateGraphics();
                    break;
                }
            }
        }

        public void PopulateInventoryWithWeapon(Item_SO item, int amount, int currentBullets, int totalBullets)
        {
            // Iterate over the inventory to populate it with a provided weapon.
            foreach (var slot in inventorySlots)
            {
                InventorySlot _slot = slot;
                if (_slot.slotData.inventoryItem == null)
                {
                    _slot.slotData.inventoryItem = item;
                    _slot.slotData.amount = amount;
                    _slot.slotData.currentBullets = currentBullets;
                    _slot.slotData.totalBullets = totalBullets;
                    _slot.UpdateGraphics();
                    break;
                }
            }
        }

        public void PopulateArrayWithWeapon(List<GameObject> list, Weapon_SO weapon, int amount)
        {
            foreach (var slot in list)
            {
                InventorySlot _slot = slot.GetComponent<InventorySlot>();
                if (_slot.slotData.inventoryItem == null)
                {
                    _slot.slotData.inventoryItem = weapon;
                    _slot.slotData.amount = amount;
                    _slot.slotData.currentBullets = weapon.magazineSize;
                    _slot.slotData.totalBullets = weapon.limitedMagazines ? weapon.magazineSize * weapon.amountOfMagazines : weapon.magazineSize;
                    _slot.UpdateGraphics();
                    break;
                }
            }
        }

        public int CheckForBullets(AmmoType_SO type)
        {
            // Auxiliar variable that will serve as an ammo counter
            int totalAmount = 0;

            foreach (var slot in inventorySlots)
            {
                InventorySlot _slot = slot;

                if (_slot.slotData.inventoryItem == null || !(_slot.slotData.inventoryItem is AmmoType_SO) | _slot.slotData.inventoryItem != type)
                {
                    continue;
                }
                // Add to the total amount if it matches the ammo type in the inventory.
                totalAmount += _slot.slotData.amount;
            }

            return totalAmount;
        }

        public void ReduceInventoryAmount(Item_SO type, int amount)
        {
            // Iterate over the inventory list.
            foreach (var slot in inventorySlots)
            {
                 // Check if the inventory slot is empty or if the item in the slot is not the given item type.
                if (slot.slotData.inventoryItem == null || !(slot.slotData.inventoryItem is AmmoType_SO) || slot.slotData.inventoryItem != type)
                {
                    continue;
                }

                // If the inventory slot contains enough of the item type, reduce the amount by the given amount and break out of the loop.
                if (slot.slotData.amount >= amount)
                {
                    slot.slotData.amount -= amount;
                    break;  // Reduction completed, exit the loop
                }
                // Otherwise, reduce the amount by the amount in the inventory slot and set the amount in the inventory slot to 0.
                else
                {
                    amount -= slot.slotData.amount;
                    slot.slotData.amount = 0;
                }
            }
        }
        public bool IsPointerOutsideInventory(PointerEventData eventData)
        {
            // Avoid checking the bounds of the containers if we are not allowed to drop anyway
            if(!inventoryManager.DropOnOutsideRelease) return false;

            Vector2 mousePosition = Mouse.current.position.ReadValue();

            bool outsideInventory = !RectTransformUtility.RectangleContainsScreenPoint(inventoryContainer.GetComponent<RectTransform>(), mousePosition);
            bool outsideHotbar = !RectTransformUtility.RectangleContainsScreenPoint(hotbarContainer.GetComponent<RectTransform>(), mousePosition);
            bool outsideChest = !RectTransformUtility.RectangleContainsScreenPoint(chestContainer.GetComponent<RectTransform>(), mousePosition);

            return outsideInventory && outsideHotbar && outsideChest;
        }

        private void NumericHealth(float health, float shield)
        {
            healthText.text = health.ToString("F0");
            shieldText.text = shield.ToString("F0");
        }
        private void BarHealth(float health, float shield)
        {
            targetHealthValue = health;
            targetShieldValue = shield;
        }

        public void UpdateInteractProgress(float progress)
        {
            interactProgress.gameObject.SetActive(true);
            interactProgress.fillAmount = progress;
        }

        private void UpdateXP()
        {
            currentLevel.text = (ExperienceManager.Instance.playerLevel + 1).ToString();
            nextLevel.text = (ExperienceManager.Instance.playerLevel + 2).ToString();
        }

        public void RemoveDash()
        {
            var obj = dashes[dashes.Count - 1].gameObject;
            dashes.Remove(obj);
            Destroy(obj);
        }

        public void GainDash()
        {
            var obj = Instantiate(dashIcon, dashUIContainer);
            dashes.Add(obj);
        }

        private void InitializeDashUI()
        {
            for (int i = 0; i < playerMovement.AmountOfDashes; i++)
            {
                var obj = Instantiate(dashIcon, dashUIContainer);
                dashes.Add(obj);
            }
        }

        public void ShowInteractionUI(string message)
        {
            interactUI?.SetActive(true);
            if(interactText) interactText.text = message;
            if(interactProgress) interactProgress.gameObject.SetActive(false);
        }
        public void HideInteractionUI()
        {
            interactUI?.SetActive(false);

            if (interactProgress == null) return;

            interactProgress.gameObject.SetActive(false);
            interactProgress.fillAmount = 0;
        }

    }
}