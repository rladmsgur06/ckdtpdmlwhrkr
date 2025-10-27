using UnityEngine;

namespace cowsins2D
{
    public class OneWayPlatform : MonoBehaviour
    {
        private BoxCollider2D col;

        private void Start() => col = GetComponent<BoxCollider2D>();

        private void OnCollisionStay2D(Collision2D other)
        {
            if (!other.transform.CompareTag("Player")) return; // If the collision is not the player then we shall not proceed.

            // In case we are the player and in fact we are crouching, we can Go down the one way platform,
            if(InputManager.PlayerInputs.Crouch)
                GoDown();
        }
        public void GoDown()
        {
            // When going down the platform simply disables its collision to allow the player to go through it.
            CancelInvoke(nameof(Reset));
            col.enabled = false;
            Invoke(nameof(Reset), .5f);
        }

        private void Reset() => col.enabled = true;
    }

}