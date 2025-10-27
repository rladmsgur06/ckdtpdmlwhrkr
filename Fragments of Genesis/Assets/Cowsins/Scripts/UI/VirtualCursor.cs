using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI; 

namespace cowsins2D
{
    public class VirtualCursor : MonoBehaviour
    {
        [SerializeField, Tooltip("Reference to the RectTransform of this object.")] private RectTransform cursorRectTransform;
        [SerializeField, Tooltip("Sensitivity of the movement.")] private float moveSpeed = 100.0f;

        private float minX, maxX, minY, maxY;

        private InventorySlot slot;

        public static VirtualCursor Instance;

        private void OnEnable()
        {
            if (Instance == null) Instance = this;
        }
        private void Start()
        {
            CalculateScreenBounds();
        }

        private void Update()
        {
            if (DeviceDetection.Instance.mode == DeviceDetection.InputMode.Keyboard) GetComponent<Image>().enabled = false; 
            else GetComponent<Image>().enabled = true;
            if (InputManager.PlayerInputs.JumpingUp && UIController.Instance.currentInventorySlot != null)
            {
                slot.OnPointerUp(null);
            }

            if (Gamepad.current != null)
            {
                Vector2 moveInput = Gamepad.current.leftStick.ReadValue();

                Vector2 newPosition = cursorRectTransform.anchoredPosition + moveInput * moveSpeed * Time.deltaTime;

                // Clamp the position within screen bounds
                newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);
                newPosition.y = Mathf.Clamp(newPosition.y, minY, maxY);

                cursorRectTransform.anchoredPosition = newPosition;
            }

        }

        private void CalculateScreenBounds()
        {
            Vector2 screenSize = new Vector2(Screen.width, Screen.height);
            Vector2 cursorHalfSize = cursorRectTransform.sizeDelta * 0.5f;

            minX = -screenSize.x / 2 + cursorHalfSize.x;
            maxX = screenSize.x / 2 - cursorHalfSize.x;
            minY = -screenSize.y / 2 + cursorHalfSize.y;
            maxY = screenSize.y / 2 - cursorHalfSize.y;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            slot = other.GetComponent<InventorySlot>();
            if (slot == null) return;
            UIController.Instance.highlightedInventorySlot = slot;
            slot.OnPointerEnter(null);

        }

        private void OnTriggerStay2D(Collider2D other)
        {
            InventorySlot slot = other.GetComponent<InventorySlot>();
            if (slot == null) return;

            if (InputManager.PlayerInputs.Jump && UIController.Instance.currentInventorySlot == null)
            {
                slot.OnSelect();
            }

            if (InputManager.PlayerInputs.Interact) slot.OnDrop();

            if (InputManager.PlayerInputs.Reload) slot.OnUse();
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            InventorySlot slot = other.GetComponent<InventorySlot>();
            if (slot == null) return;

            slot.OnPointerExit(null);

        }
    }

}