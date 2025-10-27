using UnityEngine;
using UnityEngine.Events; 

namespace cowsins2D
{
    public class DestructiblePlatform : Trigger
    {
        [SerializeField, Tooltip("After a player triggers it, time to destroy in seconds."), Title("Destructible Platform", upMargin = 10)] private float timeToDestroyAfterActivation;

        [SerializeField] private UnityEvent OnActivation, OnDestroyed;

        public override void EnterTrigger(GameObject target)
        {
            if (!target.transform.CompareTag("Player")) return; // If the collision is not the player, do not keep going

            // Destroy after activation.
            Destroy(this.gameObject, timeToDestroyAfterActivation);
            OnActivation?.Invoke();
            base.EnterTrigger(target);
        }

        private void OnDestroy()
        {
            OnDestroyed?.Invoke();
        }

    }

}