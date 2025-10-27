using UnityEngine;
using TMPro;
using UnityEngine.Events; 

namespace cowsins2D
{
    public class NPC : Interactable, ITriggerable
    {
        [SerializeField, Tooltip("Time in seconds between interactions. ")] private float interactInterval;

        [SerializeField, Tooltip("Text that displays the NPC texts.")] private GameObject text;

        [SerializeField, Tooltip("Array containing all the welcome messages that this NPC has. Once you trigger an NPC any of these will " +
            "randomly be assigned to the text UI.")] private string[] NPCWelcomeMessages;

        public UnityEvent OnTrigger, OnLeaveTrigger, OnInteract;

        private float timer;

        private void Update()
        {
            timer -= Time.deltaTime;    
        }
        public override void Interact(InteractionManager source)
        {
            // We should not interact if the interval has not been met.
            if (timer > 0) return;

            // Reset the interval and perform the interaction.
            timer = interactInterval; 
            OnInteract?.Invoke();
        }
        public virtual void EnterTrigger(GameObject target)
        {
            // Custom welcome messages.
            text.SetActive(true);
            text.GetComponentInChildren<TextMeshProUGUI>().text = NPCWelcomeMessages[Random.Range(0, NPCWelcomeMessages.Length)];

            OnTrigger?.Invoke(); 
        }

        public void StayTrigger(GameObject target) {}
        public void ExitTrigger(GameObject target)
        {
            OnLeaveTrigger?.Invoke(); 
            text.SetActive(false);
        }
    }
}
