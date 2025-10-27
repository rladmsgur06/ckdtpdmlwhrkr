using UnityEngine;

namespace cowsins2D
{
    public class Crosshair : MonoBehaviour
    {
        [SerializeField, Tooltip("Needed reference to the Player weapon Controller. Used to access important information. ")] private WeaponController player;

        [SerializeField, Tooltip("If the player does not own a weapon, set to true if you want the crosshair to still be visible.")] private bool showCrosshairIfWeaponIsNull;

        [SerializeField, Tooltip("Spread that the crosshair will lerp to (“idle spread”)")] private float defaultSpread, resizeSpeed;

        [SerializeField, Tooltip("Width of each line of the crosshair")] private float defaultWidth;

        [SerializeField, Tooltip("How long each line is.")] private float defaultHeight;

        [SerializeField, Tooltip("Basic color of the crosshair.")] private Color defaultColor;

        private float spread;

        public static Crosshair Instance;

        private bool hidden = false;

        public bool Hidden => hidden;

        private PlayerControl playerControl;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(this.gameObject);

            playerControl = player.GetComponent<PlayerControl>();   

            Cursor.visible = false;
            spread = defaultSpread;
        }
        private void Update()
        {
            transform.position = Camera.main.ScreenToWorldPoint(InputManager.PlayerInputs.MousePos);
            transform.position = new Vector3(transform.position.x, transform.position.y, 0);

            spread = Mathf.MoveTowards(spread, defaultSpread, resizeSpeed * Time.deltaTime);

            if (DeviceDetection.Instance.mode == DeviceDetection.InputMode.Controller && playerControl.Controllable) Hide(false);
        }

        void OnGUI()
        {
            if (hidden || !showCrosshairIfWeaponIsNull && player.weapon == null) return;

            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, defaultColor);
            texture.wrapMode = TextureWrapMode.Repeat;
            texture.Apply();

            Vector2 mousePosition = Event.current.mousePosition;

            // RIGHT
            GUI.DrawTexture(new Rect(mousePosition.x + spread, mousePosition.y - defaultHeight / 2, defaultWidth, defaultHeight), texture);
            // LEFT
            GUI.DrawTexture(new Rect(mousePosition.x - spread - defaultWidth, mousePosition.y - defaultHeight / 2, defaultWidth, defaultHeight), texture);

            // TOP
            GUI.DrawTexture(new Rect(mousePosition.x - defaultHeight / 2, mousePosition.y + spread, defaultHeight, defaultWidth), texture);
            // DOWN
            GUI.DrawTexture(new Rect(mousePosition.x - defaultHeight / 2, mousePosition.y - spread - defaultWidth, defaultHeight, defaultWidth), texture);

        }

        public void Resize(float value)
        {
            spread = value;
        }

        public void Hide(bool displayCursor)
        {
            hidden = true;
            if (displayCursor)
                Cursor.visible = true;
        }

        public void Show()
        {
            hidden = false;
            Cursor.visible = false;
        }

        public bool CheckIfCanShow()
        {
            if (!showCrosshairIfWeaponIsNull && player.weapon == null) return false;
            return true;
        }
    }

}