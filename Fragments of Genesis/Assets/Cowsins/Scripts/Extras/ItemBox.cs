using UnityEngine;
using UnityEngine.Events;

namespace cowsins2D
{
    public class ItemBox : MonoBehaviour
    {
        [SerializeField, Title("Item Box", upMargin = 10)] private GameObject itemToSpawn; // The item that will appear when the box is hit
        [SerializeField] private float jumpForce = 10f; // The force applied to the box when hit
        [SerializeField] private AudioClip destroyClip;
        private bool isHit = false; // To track if the box has been hit already

        public UnityEvent onCollide;

        // Called when something collides with the box
        private void OnCollisionEnter2D(Collision2D collision)
        {
            // Check if the collision is from below and it's the player character
            if (collision.contacts[0].normal.y > 0 && collision.gameObject.CompareTag("Player"))
            {
                if (!isHit)
                {
                    // Play a sound effect
                    SoundManager.Instance.PlaySound(destroyClip, 1);
                    // Spawn the item (coin, power-up, etc.)
                    SpawnItem();

                    // Apply an upward force to the box
                    Rigidbody2D rb = GetComponent<Rigidbody2D>();
                    if (rb != null)
                    {
                        rb.velocity = Vector2.zero;
                        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
                    }
                    // Mark the box as hit
                    isHit = true;
                }
            }
        }

        // Spawn the item inside the box
        private void SpawnItem()
        {
            if (itemToSpawn != null)
            {
                Instantiate(itemToSpawn, transform.position, Quaternion.identity);
            }
            onCollide?.Invoke();
        }
    }

}