using UnityEngine;
using UnityEngine.Events;

namespace cowsins2D
{
    public class Door : Interactable, ITriggerable
    {
        [System.Serializable]
        public enum DoorMethod
        {
            OpenByTrigger, OpenByInteract
        }
        [SerializeField, Tooltip("Configure the behaviour of the door. OpenByTrigger will automatically open the door when the Player approaches, no need to interact. " +
            "OpenByInteract will force you to interact to open it.")] private DoorMethod doorMethod;

        [SerializeField, Tooltip("After a certain period, close the door?")] private bool autoCloseDoor;

        [SerializeField, Tooltip("Seconds to close the door.")] private float autoCloseDoorTimer; 

        [SerializeField, Tooltip("Already configured in the prefab.")] private Collider2D doorCollider; 

        [System.Serializable] 
        public class Events
        {
            public UnityEvent onOpenDoor, onCloseDoor;
        }

        [SerializeField] private Events events;

        public void EnterTrigger(GameObject target)
        {
            if (doorMethod == DoorMethod.OpenByInteract) return; // If we should open the door by interacting, we shouldnt open it when triggering
            OpenDoor(); 
        }
        public void ExitTrigger(GameObject target) 
        {
            if (doorMethod == DoorMethod.OpenByInteract) return;// If we should open the door by interacting, we shouldnt close it when triggering
            CloseDoor(); 
        }
        public override void Interact(InteractionManager source)
        {
            if (doorMethod == DoorMethod.OpenByTrigger) return;// If we should open the door by triggering, we shouldnt open it when interacting
            if (doorCollider.enabled) OpenDoor();
            else CloseDoor(); 
        }

        private void OpenDoor()
        {
            // Opening a door means that the graphics are updated and that the collision is disabled for a certain period of time.
            // This period of time is defined by autoCloseDoorTimer
            doorCollider.enabled = false;

            events.onOpenDoor?.Invoke();

            if(autoCloseDoor)
            {
                CancelInvoke(nameof(CloseDoor));
                Invoke(nameof(CloseDoor), autoCloseDoorTimer); 
            }
        }

        private void CloseDoor()
        {
            // Closing a door means to enable the collision back as it was in the origin.
            doorCollider.enabled = true;
            events.onCloseDoor?.Invoke();
        }

        public void StayTrigger(GameObject target) { }
    }
}