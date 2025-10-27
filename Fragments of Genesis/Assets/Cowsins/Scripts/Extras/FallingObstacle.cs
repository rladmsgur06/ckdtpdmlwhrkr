using UnityEngine;
using System.Collections;  

namespace cowsins2D
{
    public class FallingObstacle : MonoBehaviour
    {
        [SerializeField] private float height;
        [SerializeField, Tooltip("Time to move up")] private float upTime = 3f; // Time to move up
        [SerializeField, Tooltip("Time to stay idle after moving up")] private float idleTimeUp = 2f; // Time to stay idle after moving up
        [SerializeField, Tooltip("Time to move down")] private float downTime = 0.3f; // Time to move down
        [SerializeField, Tooltip("Time to stay idle after moving down")] private float idleTimeDown = 1f; // Time to stay idle after moving down

        private Vector3 initialPosition;
        private Vector3 targetPosition;
        private bool isMovingUp = true;

        void Start()
        {
            initialPosition = transform.position;
            targetPosition = initialPosition + Vector3.up * height; // Move up one unit
            StartCoroutine(MoveObject());
        }

        private IEnumerator MoveObject()
        {
            // Perform movement operations
            // while (true) allows us to loop through this process.
            while (true)
            {
                float timer = 0f;
                // Define where to go and where it came from depending on the current state of the falling obstacle
                Vector3 startPos = isMovingUp ? initialPosition : targetPosition;
                Vector3 endPos = isMovingUp ? targetPosition : initialPosition;
                float moveTime = isMovingUp ? upTime : downTime;
                float idleTime = isMovingUp ? idleTimeUp : idleTimeDown;

                // Lerp the movement from the startPos, and the endPos.
                while (timer < moveTime)
                {
                    timer += Time.deltaTime;
                    float t = Mathf.Clamp01(timer / moveTime);
                    transform.position = Vector3.Lerp(startPos, endPos, t);
                    yield return null;
                }

                // Wait for an interval
                yield return new WaitForSeconds(idleTime);

                // Switch sides
                isMovingUp = !isMovingUp;
            }
        }
    }
}