using UnityEngine;
using UnityEngine.Events;

namespace cowsins2D
{
    public class JumpPad : Trigger
    {
        [SerializeField, Tooltip("Force to eject the player in the vertical axis."), Title("Jump Pad", upMargin = 10)] private float jumpForce;

        [SerializeField, Tooltip("Sound played on the player triggering the jump pad.")] private AudioClip bounceSFX;

        public UnityEvent onTrigger;

        private AudioSource source;

        private void Awake() => source = GetComponent<AudioSource>();

        public override void EnterTrigger(GameObject target)
        {
            if (!target.CompareTag("Player")) return;// Check if its the player. In case the player is not colliding with this object, do not keep going.

            // Play required SFX on bouncing.
            source.PlayOneShot(bounceSFX);

            Rigidbody2D rb = target.GetComponent<Rigidbody2D>();
            PlayerStats player = target.GetComponent<PlayerStats>();

            player.UseJumpPad();

            // Reset the player velocity to avoid glitchy or jittery movement
            rb.velocity = Vector2.zero;

            // Apply a force ( vertically ) on the player.
            rb.AddForce(target.transform.up * jumpForce, ForceMode2D.Impulse);

            // Custom events
            onTrigger?.Invoke();
            base.EnterTrigger(target);
        }
    }
}