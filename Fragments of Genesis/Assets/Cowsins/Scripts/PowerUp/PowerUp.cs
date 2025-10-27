using UnityEngine;
using UnityEngine.Events;
namespace cowsins2D
{
    public class PowerUp : MonoBehaviour, ITriggerable
    {
        [SerializeField, Tooltip("Set to true if the power up should appear again after being triggered.")] private bool regenerate;

        [SerializeField, Tooltip("Time to regenerate in seconds.")] private float regenerateTime;

        [SerializeField] private AudioClip collectSFX;

        public bool triggerable { get; private set; } = true;

        public UnityEvent onCollect;


        // this method gets called when the power up is triggered
        public void EnterTrigger(GameObject target)
        {
            if (!triggerable) return;

            // handle trigger action
            TriggerAction(target);

            onCollect?.Invoke();
            SoundManager.Instance.PlaySound(collectSFX, 1);

            // Set regeneration if allowed
            if (regenerate)
            {
                triggerable = false;
                transform.GetChild(0).gameObject.SetActive(false);
                Invoke(nameof(Regenerate), regenerateTime);
            }
            else Destroy(this.gameObject);
        }

        public void ExitTrigger(GameObject target) { }

        // called if the power up can regenerate
        // It reenables the graphics and the trigger action
        private void Regenerate()
        {
            triggerable = true;

            transform.GetChild(0).gameObject.SetActive(true);

            CancelInvoke(nameof(Regenerate));
        }

        public virtual void TriggerAction(GameObject target)
        {

        }

        public void StayTrigger(GameObject target) { }
    }

}