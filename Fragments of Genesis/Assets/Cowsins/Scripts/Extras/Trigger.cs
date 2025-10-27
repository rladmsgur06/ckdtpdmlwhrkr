using UnityEngine;
using UnityEngine.Events; 

namespace cowsins2D
{
    public class Trigger : MonoBehaviour, ITriggerable
    {
        [System.Serializable]
        public class TriggerEvents
        {
            public UnityEvent triggerEnterEvent, triggerStayEvent, triggerExitEvent;
        }

        [SerializeField] protected TriggerEvents triggerEvents;

        public virtual void EnterTrigger(GameObject target) 
        {
            // Perform the custom event if the player triggered this object.
            triggerEvents.triggerEnterEvent?.Invoke();
        }
        public virtual void StayTrigger(GameObject target)
        {
            triggerEvents.triggerStayEvent?.Invoke();
        }
        public virtual void ExitTrigger(GameObject target)
        {
            triggerEvents.triggerExitEvent?.Invoke();
        }
    }
}
