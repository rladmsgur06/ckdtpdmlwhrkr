using UnityEngine;
using UnityEngine.Events;
using System.Collections;
namespace cowsins2D
{
    public class Coin : Trigger
    {
        [SerializeField, Min(0), Tooltip("Minimum number of coins to pick up."), Title("Coins", upMargin = 10)] private int minimumAmount;

        [SerializeField, Tooltip("Maximum number of coins to pick up.")] private int maximumAmount;

        [SerializeField, Tooltip("Sound on picking the coin. ")] private AudioClip pickUpSFX;

        public UnityEvent onPickUp;


        [SerializeField] private float moveSpeed = 1.0f; // Speed of the movement
        [SerializeField] private float moveDistance = 1.0f; // Distance to move up and down
        private Vector3 startPos;

        private void Start()
        {
            startPos = transform.position;
            StartCoroutine(StartMovingWithDelay());
        }
        private IEnumerator StartMovingWithDelay()
        {
            // Generate a random delay before starting movement
            float offset = Random.Range(.9f, 1.4f);

            // Start moving
            while (true)
            {
                // Calculate vertical movement using a sine wave
                float verticalMovement = Mathf.Sin(Time.time * moveSpeed * offset) * moveDistance;
                transform.position = startPos + new Vector3(0, verticalMovement, 0);
                yield return null;
            }
        }


        public override void EnterTrigger(GameObject target)
        {
            // Perform custom events
            onPickUp?.Invoke();

            // Add the coins to the player
            CoinManager.AddCoins(Random.Range(minimumAmount, maximumAmount));

            // Play SFX
            SoundManager.Instance.PlaySound(pickUpSFX, 1);

            Destroy(this.gameObject);

            base.EnterTrigger(target);  
        }
    }
}