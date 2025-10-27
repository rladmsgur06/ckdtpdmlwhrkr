using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

namespace cowsins2D
{
    public class InventorySlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [System.Serializable]
        public class Events
        {
            public UnityEvent onPointerEnter, onPointerExit, onPointerDown, onPointerUp;
        }

        [ReadOnly] public int row, column;

        public SlotType slotType;

        [HideInInspector] public SlotData slotData = new SlotData();

        [SerializeField] private Image image, background;

        [SerializeField, Tooltip("TMPro texts that displays the number of items the player currently has on this slot.")] private TextMeshProUGUI amountText;

        [SerializeField, Tooltip("Scale of the slot when hovered.")] private float hoverSize, hoverSpeed;
        [SerializeField] private Color hoverColor;

        [SerializeField, Range(.1f, 4f)] private float timeToShowTooltipOnHover = 1f;

        [SerializeField, Tooltip("Sound to play on hover.")] private AudioClip hoverSFX;
        [SerializeField, Tooltip("Sound to play when the slot is selected.")] private AudioClip selectSFX;
        [SerializeField, Tooltip("Sound to play when the slot is deselected.")] private AudioClip deselectSFX;
        [SerializeField, Tooltip("Sound to play when the item is dropped.")] private AudioClip dropSFX;

        [SerializeField] private Events events;

        private Color initialColor;
        private Vector3 initialSize;

        [HideInInspector] public WeaponController weaponController;
        [HideInInspector] public InteractionManager interactionManager;
        [HideInInspector] public PlayerControl playerControl;
        public bool isHotbarSlot => slotType == SlotType.Hotbar;
        public bool isChestSlot => slotType == SlotType.Chest;

        private Vector3 targetScale;

        private void OnEnable()
        {
            UIController.updateHotbarSelection += UpdateSelection;
            UIController.Instance.onUpdateWeaponInfo.AddListener(UpdateGraphics);
        }
        private void OnDisable()
        {
            UIController.updateHotbarSelection -= UpdateSelection;
            UIController.Instance.onUpdateWeaponInfo.RemoveListener(UpdateGraphics);
        }

        private void Awake()
        {
            initialSize = background.transform.localScale;
            initialColor = background.color;
            targetScale = initialSize;
            UpdateGraphics();
        }

        private void Update()
        {
            background.transform.localScale = Vector3.Lerp(background.transform.localScale, targetScale, Time.deltaTime * hoverSpeed);
        }
        public void OnPointerEnter(PointerEventData eventData)
        {
            targetScale = Vector3.one * hoverSize;
            background.color = hoverColor;

            SoundManager.Instance.PlaySound(hoverSFX, .25f);
            UIController.Instance.highlightedInventorySlot = this;
            events.onPointerEnter?.Invoke();

            if (slotData.inventoryItem == null || slotData.inventoryItem != null && UIController.Instance.currentInventorySlot != null) return;
            Invoke(nameof(ShowTooltip), timeToShowTooltipOnHover);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            targetScale = initialSize;
            background.color = initialColor;

            UIController.Instance.highlightedInventorySlot = null;
            Tooltip.Instance?.Hide();
            events.onPointerExit?.Invoke();

            CancelInvoke(nameof(ShowTooltip));
        }
        public void OnPointerDown(PointerEventData eventData)
        {
            if (slotData.inventoryItem == null) return;

            events.onPointerDown?.Invoke();

            if (eventData.button == PointerEventData.InputButton.Right)
            {
                OnUse();
                return;
            } 
            else if (eventData.button == PointerEventData.InputButton.Middle && !playerControl.Controllable)
            {
                OnDrop();
                return;
            }

            OnSelect();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            events.onPointerUp?.Invoke();
            
            if (UIController.Instance.IsPointerOutsideInventory(eventData) && DeviceDetection.Instance.mode == DeviceDetection.InputMode.Keyboard)
            {
                OnDrop();
                Tooltip.Instance.Hide();
                UIController.Instance.currentInventorySlot = null;
                return;
            }
            if (UIController.Instance.currentInventorySlot == null || UIController.Instance.highlightedInventorySlot == null ||
                UIController.Instance.currentInventorySlot.gameObject == UIController.Instance.highlightedInventorySlot.gameObject)
            {
                Tooltip.Instance.Hide();
                UIController.Instance.currentInventorySlot = null;
                return;
            }
            UIController.Instance.SwitchItems(weaponController);

            UIController.updateHotbarSelection?.Invoke();
            UIController.Instance.UpdateWeaponInformation();

            weaponController.UnholsterWeapon();
            SoundManager.Instance.PlaySound(deselectSFX, 5f);
        }

        public void OnUse()
        {
            if (slotData.inventoryItem != null && slotData.inventoryItem.Use(weaponController))
            {
                slotData.amount--;
                UIController.Instance.UpdateWeaponInformation();
            }
        }
        public void OnDrop()
        {
            if (slotData.inventoryItem == null) return;

            interactionManager.DropInventoryItem(this);

            slotData = new SlotData();
            UpdateGraphics();

            weaponController.CheckForBullets();
            UIController.Instance.UpdateWeaponInformation();
            UIController.updateHotbarSelection?.Invoke();

            SoundManager.Instance.PlaySound(dropSFX, 5f);
        }
        public void OnSelect()
        {
            if (slotData.inventoryItem == null) return;
            UIController.Instance.currentInventorySlot = this;
            SoundManager.Instance.PlaySound(selectSFX, .5f);
        }

        private void ShowTooltip()
        {
            if(this.gameObject.activeSelf) Tooltip.Instance?.Show(slotData?.inventoryItem?.itemName);
        }

        private void UpdateSelection()
        {
            if (isHotbarSlot)
                background.color = weaponController.currentWeapon == column ? UIController.Instance.hotbarSelected : UIController.Instance.hotbarDefault;
        }
        public void UpdateGraphics()
        {
            if (slotData.amount <= 0) slotData = new SlotData();

            image.gameObject.SetActive(slotData.inventoryItem != null);
            image.sprite = slotData.inventoryItem == null ? null : slotData.inventoryItem.itemIcon;
            amountText.text = slotData.amount > 1 && slotData.inventoryItem != null ? slotData.amount.ToString() : "";
        }
    }

}