using UnityEngine;
using UnityEngine.Events;

namespace cowsins2D
{
    public class Destructible : EnemyHealth
    {
        [Title("Destructible"), SerializeField, Tooltip("Elements instantiated on destroying the object, more likely to be used to display broken elements of the object.")] private GameObject destroyedGraphics;

        [SerializeField, Tooltip("Reward elements instantiated on destroying the object, such as coins, experience, or other items.")] private GameObject[] rewardsOnDestroy;

        [SerializeField, Tooltip("Sound played on destroying the object.")] private AudioClip destroyedSFX;

        [SerializeField] private UnityEvent onDestroy;

        public override void Die(bool condition)
        {
            // When destroying an object, show graphics that indicate that the behaviour is working
            Instantiate(destroyedGraphics, transform.position, Quaternion.identity);

            // Check if there are rewards available
            if (rewardsOnDestroy.Length > 0)
            {
                // If so, pick a random reward and instantiateit
                int random = Random.Range(0, rewardsOnDestroy.Length);
                Instantiate(rewardsOnDestroy[random], transform.position, Quaternion.identity);
            }

            onDestroy?.Invoke();
            SoundManager.Instance.PlaySound(destroyedSFX, 1);
            base.Die(true);
        }
    }

}