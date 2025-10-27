using UnityEngine;
using UnityEngine.Events;

namespace cowsins2D
{
    public enum LoopType
    {
        None,
        Back,
        Reverse
    }

    public class MovingPlatform : Trigger
    {
        [SerializeField, Tooltip("Configure the behaviour of the platform. None: Stays idle. " +
        "Reverse: Once it reaches the last platform position it lerps back, when it reaches the initial position, it starts again. " +
        "Back: Once it reaches the last position, it lerps back to the first one."), Title("Moving Platform", upMargin = 10)]
        private LoopType loopType; // Enum field for loop type selection
        [SerializeField, Tooltip("Array that stores the points where the moving platform will pass through.")] private Transform[] platformPositions;
        [SerializeField, Tooltip("Velocity Magnitude at which the platform moves. ")] private float speed;
        [SerializeField, Tooltip("Start moving from the start")] private bool startOnAwake;

        public UnityEvent onStartMoving;

        public int currentPosition { get; private set; } = 0;
        private int direction = 1; // Direction flag for reversing

        private bool started;

        private void Awake()
        {
            // Begin to move only if we set it to behave like that.
            if (startOnAwake) StartMovement();
        }

        private void Update()
        {
            if (!started) return; // Avoid performing movement operations if it has not started yet.

            if (transform.position != platformPositions[currentPosition].position)
            {
                transform.position = Vector3.MoveTowards(transform.position, platformPositions[currentPosition].position, Time.deltaTime * speed);
            }
            else
            {
                if (currentPosition + direction > platformPositions.Length - 1)
                {
                    // Loop type selection logic
                    switch (loopType)
                    {
                        case LoopType.None:
                            // Do nothing, keeping the last position
                            break;
                        case LoopType.Back:
                            currentPosition = 0;
                            break;
                        case LoopType.Reverse:
                            direction = -direction; // Reverse the direction
                            currentPosition += direction;
                            break;
                    }
                }
                else if (currentPosition + direction < 0)
                {
                    direction = 1; // Set direction to go forward
                    currentPosition = 0;
                }
                else
                {
                    currentPosition += direction;
                }
            }
        }

        public override void EnterTrigger(GameObject other)
        {
            if (!other.transform.CompareTag("Player")) return;

            // This sets the object as the parent of the player.
            // This helps the player movement not to be jittery when moving on a moving platform.
            // You are also allowed to jump and perform other movement abilities without worrying about getting stuck or having jittery movement.
            if (other.transform.position.y > transform.position.y)
            {
                other.transform.SetParent(transform);
            }
            base.EnterTrigger(other);
        }


        public override void StayTrigger(GameObject other)
        {
            Grapple grapple = other.GetComponent<Grapple>();

            if (!other.transform.CompareTag("Player") || grapple != null && grapple.isGrappling)
            {
                other.transform.SetParent(null);
                return;
            }

            if (InputManager.PlayerInputs.HorizontalMovement != 0)
            {
                other.transform.SetParent(null);
            }
            else if (other.transform.position.y > transform.position.y)
            {
                other.transform.SetParent(transform);
            }
            base.StayTrigger(other);
        }

        public override void ExitTrigger(GameObject other)
        {
            if (!other.transform.CompareTag("Player")) return;

            // Make sure we clear the parent if the player exits the platform.
            other.transform.SetParent(null);
            base.ExitTrigger(other);
        }

        public void StartMovement()
        {
            started = true;

            onStartMoving?.Invoke();
        }
    }

}