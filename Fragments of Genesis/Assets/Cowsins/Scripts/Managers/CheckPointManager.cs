using UnityEngine;

namespace cowsins2D
{
    public class CheckPointManager : MonoBehaviour
    {
        public static CheckPointManager Instance;

        // Stores the player's last checkpoint position.
        public Vector3? lastCheckpoint { get; private set; } = null;

        private void Awake()
        {
            Instance = this;
        }
        public void SetCheckpoint(Transform obj)
        {
            // Store a new position as the last checkpoint.
            lastCheckpoint = obj.position;
        }
    }
}