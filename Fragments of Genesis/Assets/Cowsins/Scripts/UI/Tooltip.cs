using UnityEngine;
using UnityEngine.UI; 
using TMPro;

namespace cowsins2D
{
    public class Tooltip : MonoBehaviour
    {
        [SerializeField, Tooltip("Image that displays the dragged icon.")] private Image itemIconDisplay;
        [SerializeField, Tooltip("Text that displays the item name on hover.")] private TextMeshProUGUI textComponent;
        [SerializeField, Tooltip("Background.")] private Image imageComponent;
        [SerializeField, Tooltip("Margin for the background to give space to the text.")] private float horizontalMarginSize, verticalMarginSize;
        [SerializeField] private Padding tooltipOffset, controllerTooltipOffset; 

        private RectTransform rectTransform;

        public static Tooltip Instance;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(this.gameObject);

            rectTransform = GetComponent<RectTransform>();

            Hide();
        }

        private void Update()
        {
            FollowCursor();
            UpdateInventorySlotGraphics();
        }

        private void FollowCursor()
        {
            if(DeviceDetection.Instance.mode != DeviceDetection.InputMode.Keyboard && UIController.Instance.highlightedInventorySlot == null)
                return;

            // Update Tooltip position to the current mouse position
            Vector2 mousePosition = DeviceDetection.Instance.mode == DeviceDetection.InputMode.Keyboard ?
                UnityEngine.InputSystem.Mouse.current.position.ReadValue() + tooltipOffset.ToVector2() :
                UIController.Instance.highlightedInventorySlot.transform.position + controllerTooltipOffset.ToVector3(); 
            rectTransform.position = mousePosition;
        }

        private void UpdateInventorySlotGraphics()
        {
            if (UIController.Instance.currentInventorySlot == null)
            {
                itemIconDisplay.gameObject.SetActive(false);
                return;
            }
            itemIconDisplay.gameObject.SetActive(true);
            imageComponent.gameObject.SetActive(false);
            itemIconDisplay.sprite = UIController.Instance.currentInventorySlot.slotData.inventoryItem == null ? null : UIController.Instance.currentInventorySlot.slotData.inventoryItem.itemIcon;
        }
        
        public void Show(string text)
        {
            if(!UIController.Instance.InventoryManager.InventoryOpen || imageComponent == null || textComponent == null || text == null)
            {
                Hide();
                return;
            }

            imageComponent.gameObject.SetActive(true);
            textComponent.text = text.ToUpper();

            ScaleImageToFitText();
        }
        public void Hide()
        {
            imageComponent.gameObject.SetActive(false);
        }
        private void ScaleImageToFitText()
        {
            float textWidth = textComponent.preferredWidth;
            float textHeight = textComponent.preferredHeight;

            Vector2 imageSize = new Vector2(textWidth + horizontalMarginSize, textHeight + verticalMarginSize);
            imageComponent.rectTransform.sizeDelta = imageSize;
        }

    }

}