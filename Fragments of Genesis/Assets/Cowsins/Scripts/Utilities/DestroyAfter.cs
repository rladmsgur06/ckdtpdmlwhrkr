using UnityEngine;

namespace cowsins2D
{
    public class DestroyAfter : MonoBehaviour
    {
        [SerializeField, Tooltip("Time to destroy this gameObject from start. ")] private float timeToDestroy;

        private void Start()
        {
            // Destroys the object after "timeToDestroy" seconds.
            Destroy(this.gameObject, timeToDestroy);
        }
    }
}