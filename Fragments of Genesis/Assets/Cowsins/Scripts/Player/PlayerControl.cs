using UnityEngine;
using UnityEngine.Events;

namespace cowsins2D
{
    public class PlayerControl : MonoBehaviour
    {
        [System.Serializable]
        public class Events
        {
            public UnityEvent onGrantControl, onLoseControl;
        }

        public Events events;
        public bool Controllable { get; private set; }

        private PlayerStats playerStats;
        private InventoryManager inventoryManager;
        private Rigidbody2D rb;

        private void Awake()
        {
            playerStats = GetComponent<PlayerStats>();
            inventoryManager = GetComponent<InventoryManager>();
            rb = GetComponent<Rigidbody2D>();
            GrantControl();
        }

        public void GrantControl()
        {
            Controllable = true;
            events.onGrantControl?.Invoke();
            rb.drag = 0;
        }

        public void LoseControl()
        {
            Controllable = false;
            events.onLoseControl?.Invoke();
            rb.drag = 2;
        }

        public void ToggleControl()
        {
            Controllable = !Controllable;
            if (Controllable) events.onGrantControl?.Invoke();
            else events.onLoseControl?.Invoke();
        }

        public void CheckIfCanGrantControl()
        {
            if (playerStats.IsDead || inventoryManager.InventoryOpen) return;

            GrantControl();
        }
    }
}