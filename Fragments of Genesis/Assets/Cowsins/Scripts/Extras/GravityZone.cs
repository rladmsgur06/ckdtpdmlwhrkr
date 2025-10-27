using UnityEngine;
using UnityEngine.Events;

namespace cowsins2D
{
    public class GravityZone : Trigger
    {
        [SerializeField, Tooltip("Override the gravity scale value. Notice that the default gravity scale value is 9.8.")] private float gravityScale;

        public override void StayTrigger(GameObject target)
        {
            if (!target.CompareTag("Player")) return; // If this is the player, perform the operations

            // Grab a reference for the player
            PlayerMovement player = target.GetComponent<PlayerMovement>();

            // Enable player external gravity and set the value.
            player.externalGravityScale = true;
            player.SetGravityScale(gravityScale);
            if (gravityScale == 0) player.rb.velocity = Vector2.zero;

            triggerEvents.triggerStayEvent?.Invoke();
            base.StayTrigger(target);
        }
        public override void ExitTrigger(GameObject target)
        {
            if (!target.CompareTag("Player")) return; // If this is the player, perform the operations

            // Grab a reference for the player
            PlayerMovement player = target.GetComponent<PlayerMovement>();

            // Disable external gravity
            player.externalGravityScale = false;

            triggerEvents.triggerExitEvent?.Invoke();
            base.ExitTrigger(target);
        }
    }

}