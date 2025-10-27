using UnityEngine;
using UnityEngine.Events;
namespace cowsins2D
{
    public class InteractionManager : MonoBehaviour
    {
        [System.Serializable]
        public class Events
        {
            public UnityEvent onInteract, onDrop;
        }

        [SerializeField, Tooltip("Sounds played on picking up an item. ")] private AudioClip pickUpItemSFX;

        [SerializeField, Tooltip("Time in seconds to hold the interaction key to successfully perform an interaction. ")] private float timeToInteract;

        [Tooltip("If enabled, the player will be able to drop items by pressing the drop key. "), SerializeField] private bool canDrop;
        [SerializeField, Tooltip("Distance to drop the item from the player. ")] private float dropDistance;
        [SerializeField, Tooltip("Force to apply on the dropped item.")] private float dropImpulse;

        [SerializeField, Tooltip("Reference to the Generic Weapon Pickeable Prefab")] private WeaponPickUp genericWeaponPickeable;
        [SerializeField, Tooltip("Reference to the Generic Ammo Pickeable Prefab")] private AmmoPickUp genericAmmoPickeable;
        [SerializeField, Tooltip("Reference to the GenericPickeable Prefab")] private ItemPickUp genericPickeable;

        [SerializeField] private Events events;

        private IInteractable interactable;

        public bool CanDrop => canDrop;
        public float TimeToInteract => timeToInteract;

        private float timeElapsed;

        private PlayerControl playerControl;
        private WeaponController weaponController;
        private PlayerMovement playerMovement;

        #region main

        private void OnEnable()
        {
            InputManager.Instance.onDrop += HandleDropping;
        }
        private void OnDisable()
        {
            InputManager.Instance.onDrop -= HandleDropping;
        }
        private void Start()
        {
            // Initial settings
            weaponController = GetComponent<WeaponController>();
            playerControl = GetComponent<PlayerControl>();
            playerMovement = GetComponent<PlayerMovement>();
            timeElapsed = timeToInteract;
        }

        private void Update()
        {
            if (!playerControl.Controllable) return;

            HandleInteraction();
        }

        #endregion

        #region Interaction

        private void HandleInteraction()
        {
            // if there is no interactable detected we should not handle interaction
            if (interactable == null) return;

            // handle timings when the player interacts
            if (InputManager.PlayerInputs.Interact)
            {
                timeElapsed -= Time.deltaTime;
                UIController.Instance.UpdateInteractProgress(1 - (timeElapsed / timeToInteract));
            }
            else
            {
                timeElapsed = interactable != null && ((Interactable)interactable).AllowInstantInteraction ? .001f : timeToInteract;
                UIController.Instance.UpdateInteractProgress(0);
            }

            // Interaction occured
            if (timeElapsed <= 0 && (interactable is not WeaponPickUp && weaponController.reloading || !weaponController.reloading))
            {
                if (interactable is WeaponPickUp || interactable is ItemPickUp || interactable is AmmoPickUp) SoundManager.Instance.PlaySound(pickUpItemSFX, .5f);

                interactable.Interact(this);
                timeElapsed = timeToInteract;
                events.onInteract?.Invoke();
            }
        }
        #endregion

        #region Dropping
        private void HandleDropping()
        {
            // if the conditions are met and the correct inputs are pressed, handle dropping
            if (canDrop && weaponController.weapon != null && !weaponController.isWeaponHidden && !weaponController.reloading) DropCurrentWeapon();
        }

        private void DropCurrentWeapon() => DropWeapon(weaponController.currentWeapon);

        private void DropWeapon(int index)
        {
            if(weaponController.inventory[index] == null) return;

            // Remove the current weapon, but save it for further operations
            Weapon_SO saveWeapon = weaponController.inventory[index].weapon;
            if(index == weaponController.currentWeapon) weaponController.SetWeapon(null);

            // Instantiate a Pickeable object
            // Calculate where to drop depending on where the player is looking at
            Vector2 dirToDrop = playerMovement.facingRight ? transform.right : -transform.right;
            WeaponPickUp pickeable = Instantiate(genericWeaponPickeable, transform.position + (Vector3)dirToDrop * dropDistance, Quaternion.identity) as WeaponPickUp;
            pickeable.dropped = true;
            pickeable.currentBullets = weaponController.inventory[index].currentBullets;
            pickeable.totalBullets = weaponController.inventory[index].totalBullets;
            pickeable.AssignWeapon(saveWeapon); // Assign the weapon we saved to the new pickeable
            // Grab Rigidbody2D component from the pickeable
            Rigidbody2D rb = pickeable.GetComponent<Rigidbody2D>();
            // Apply forces to the pickeable ( dropping effect )
            rb.AddForce(dirToDrop * dropImpulse, ForceMode2D.Impulse);

            weaponController.ReleaseWeapon(index);

            UIController.Instance.ModifyHotbarSlotAmount(weaponController.currentWeapon, 0);
            UIController.Instance.UpdateWeaponInformation();

            events.onDrop?.Invoke();
        }
        public void DropInventoryItem(InventorySlot inventorySlot)
        {
            SlotData slotData = inventorySlot.slotData;
            Vector2 dirToDrop = GetComponent<PlayerMovement>().facingRight ? transform.right : -transform.right;

            if (slotData.inventoryItem is Weapon_SO)
            {
                if (inventorySlot.isHotbarSlot && slotData.inventoryItem is Weapon_SO) DropWeapon(inventorySlot.column);
                else
                {
                    WeaponPickUp pickeable = Instantiate(genericWeaponPickeable, transform.position + (Vector3)dirToDrop * dropDistance, Quaternion.identity) as WeaponPickUp;
                    pickeable.dropped = true;
                    pickeable.currentBullets = slotData.currentBullets;
                    pickeable.totalBullets = slotData.totalBullets;

                    // Assign the weapon we saved to the new pickeable
                    pickeable.AssignWeapon((Weapon_SO)slotData.inventoryItem);

                    // Grab Rigidbody2D component from the pickeable
                    Rigidbody2D rb = pickeable.GetComponent<Rigidbody2D>();

                    // Apply forces to the pickeable ( dropping effect )
                    rb.AddForce(dirToDrop * dropImpulse, ForceMode2D.Impulse);
                }
            }
            else if (slotData.inventoryItem is AmmoType_SO)
            {
                AmmoPickUp pickeable = Instantiate(genericAmmoPickeable, transform.position + (Vector3)dirToDrop * dropDistance, Quaternion.identity) as AmmoPickUp;
                pickeable.ammoAmount = slotData.amount;
                pickeable.ammoType = (AmmoType_SO)slotData.inventoryItem;


                // Grab Rigidbody2D component from the pickeable
                Rigidbody2D rb = pickeable.GetComponent<Rigidbody2D>();

                // Apply forces to the pickeable ( dropping effect )
                rb.AddForce(dirToDrop * dropImpulse, ForceMode2D.Impulse);
            }
            else
            {
                ItemPickUp pickeable = Instantiate(genericPickeable, transform.position + (Vector3)dirToDrop * dropDistance, Quaternion.identity) as ItemPickUp;
                pickeable.amount = slotData.amount;
                pickeable.item = slotData.inventoryItem;


                // Grab Rigidbody2D component from the pickeable
                Rigidbody2D rb = pickeable.GetComponent<Rigidbody2D>();

                // Apply forces to the pickeable ( dropping effect )
                rb.AddForce(dirToDrop * dropImpulse, ForceMode2D.Impulse);
            }
        }
        #endregion

        #region others
        private void OnTriggerEnter2D(Collider2D other)
        {
            // Handles Trigger once ( on Enter )
            var triggerable = other.GetComponent<ITriggerable>();
            if (triggerable == null) return;

            triggerable.EnterTrigger(gameObject);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            // Check if there is any object to trigger
            var triggerable = other.GetComponent<ITriggerable>();
            if (triggerable != null) triggerable.StayTrigger(this.gameObject);

            // Check if there is any object to interact with
            if (other.tag != "Interactable" || other.GetComponent<IInteractable>() == null || interactable != null) return;

            interactable = other.GetComponent<IInteractable>();
            UIController.onInteractAvailable?.Invoke(((Interactable)interactable).GetInteractText());
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            // Disable the interaction
            if (other.GetComponent<IInteractable>() != null)
            {
                interactable = null;
                UIController.onInteractionDisabled?.Invoke();
            }

            // Exits trigger
            var triggerable = other.GetComponent<ITriggerable>();
            if (triggerable == null) return;

            triggerable.ExitTrigger(this.gameObject);
        }
        #endregion
    }
}