using UnityEngine;
namespace cowsins2D
{
    public class Interactable : MonoBehaviour, IInteractable
    {
        [SerializeField] protected string interactText;

        [SerializeField] protected bool allowInstantInteraction;

        public string InteractText => interactText; 

        public bool AllowInstantInteraction => allowInstantInteraction;

        /// <summary>
        /// Override this on your custom Interactables
        /// </summary>
        /// <param name="source">Who interacted with this interactable. ( The Player )</param>
        public virtual void Interact(InteractionManager source)
        {
            Debug.Log(source.name);
        }

        public virtual string GetInteractText()
        {
            return interactText;
        }
    }
}