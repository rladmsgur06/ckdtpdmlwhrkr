using UnityEngine;
using UnityEngine.Events;

namespace cowsins2D
{
    
    public class Spike : Trigger
    {
        [SerializeField, Tooltip("Damage to be applied to the player per interval. "), Title("Spike", upMargin = 10)] private float damage;

        [SerializeField, Tooltip("Seconds in-between damage applicable to the player.")] private float damageInterval;

        private bool canDamage = true;

        public UnityEvent onDamage;


        public override void StayTrigger(GameObject target)
        {
            if (!canDamage) return; // Since we can set up a damage interval, make sure we are ready to take damage again before proceeding.

            // Apply the damage and restart the interval.
            target.GetComponent<PlayerStats>().Damage(damage);
            canDamage = false;
            Invoke(nameof(ReEnableDamage), damageInterval);

            onDamage?.Invoke(); 
            base.StayTrigger(target);
        }

        private void ReEnableDamage() => canDamage = true; 
    }
}