using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
namespace cowsins2D
{
    public class Grapple : MonoBehaviour
    {
        [SerializeField, Tooltip("Reference to the player Camera.")] private Camera grappleCamera;
        [SerializeField, Tooltip("Reference to the player Line Renderer component.")] private LineRenderer grappleLineRenderer;
        [SerializeField, Tooltip("Sets the allowed grapple layers. You won´t be able to use the grapple on surfaces that are not included in this mask.")] private LayerMask grappleMask;
        [SerializeField, Tooltip("Velocity from the initial grapple position to the grapple target.")] private float moveSpeed = 2f;
        [SerializeField, Tooltip("Maximum distance allowed to grapple.")] private float grappleLength = 5f;
        [Min(1)]
        [SerializeField, Tooltip(" It ensures that the list only retains the most recent grapple points up to the specified maximum value, " +
            "discarding the oldest points if necessary.")]
        private int maxPoints = 3;

        private Rigidbody2D rb;
        private List<Vector2> grapplePoints = new List<Vector2>();

        public bool isGrappling => grapplePoints.Count > 0;

        private PlayerStates states;
        private PlayerControl playerControl;

        [System.Serializable]
        public class Events
        {
            public UnityEvent onTryStartGrapple, onGrappling, onDetachGrapple;
        }

        [SerializeField] private Events events;

        private void Start()
        {
            // Initial settings
            rb = GetComponent<Rigidbody2D>();
            states = GetComponent<PlayerStates>();
            playerControl = GetComponent<PlayerControl>();
            grappleLineRenderer.positionCount = 0;

        }

        void Update()
        {
            if (!playerControl.Controllable) return;

            // If the player is not controllable stop grappling completely
            if (isGrappling)
            {
                if(!playerControl.Controllable)
                {
                    states.CurrentState.ExitState();
                    states.CurrentState = states._States.Default();
                    states.CurrentState.EnterState();
                    Detach();
                    return;
                }
            }
            // if the right mouse button has been pressed, try to grapple
            if (Input.GetMouseButtonDown(1) && !isGrappling)
            {
                TryStartGrapple();
            }

            // If currently grappling, handle all the logic
            if (isGrappling)
            {
                HandleGrapple();
            }
        }

        private void TryStartGrapple()
        {
            // Tries to grapple if there is an object in the grapple direction within the grapple range

            // Gets the position of the mouse as well as the grapple direction
            Vector2 mousePos = grappleCamera.ScreenToWorldPoint(Input.mousePosition);
            Vector2 direction = (mousePos - (Vector2)transform.position).normalized;

            // Check if there is any grappable object within the range and direction of the grapple
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, grappleLength, grappleMask);
            
            if (hit.collider != null)
            {
                grapplePoints.Add(hit.point);

                if (grapplePoints.Count > maxPoints)
                {
                    grapplePoints.RemoveAt(0);
                }
            }

            events.onTryStartGrapple?.Invoke();
        }

        private void HandleGrapple()
        {
            // Handle player movement while grappling
            Vector2 moveTo = Centroid(grapplePoints.ToArray());
            rb.MovePosition(Vector2.MoveTowards((Vector2)transform.position, moveTo, Time.deltaTime * moveSpeed));

            // Line renderer settings while grappling ( Visuals )
            grappleLineRenderer.positionCount = grapplePoints.Count * 2;
            for (int n = 0, j = 0; n < grapplePoints.Count * 2; n += 2, j++)
            {
                grappleLineRenderer.SetPosition(n, transform.position);

                grappleLineRenderer.SetPosition(n + 1, grapplePoints[j]);
            }
            float distanceThreshold = 1f; // Adjust this value as needed
            if (Input.GetMouseButtonUp(1) || Vector2.Distance(transform.position, moveTo) <= distanceThreshold)
            {
                Detach();
            }

            events.onGrappling?.Invoke();
        }

        public void Detach()
        {
            // Stop grappling and remove the visuals
            grappleLineRenderer.positionCount = 0;
            grapplePoints.Clear();

            events.onDetachGrapple?.Invoke();
        }

        Vector2 Centroid(Vector2[] points)
        {
            Vector2 center = Vector2.zero;
            foreach (Vector2 point in points)
            {
                center += point;
            }
            center /= points.Length;
            return center;
        }
    }
}