using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace cowsins2D
{
    public class CaptureThePoint : Trigger
    {
        [SerializeField, Tooltip("How much time the player needs to stay inside the point to capture it."), Title("Capture The Point)", upMargin = 10)] private float progressRequiredToCapture;

        // This field specifies a UI GameObject that displays the progress on the capture.
        [SerializeField, Tooltip("User interface that displays the progress on the capture. This can be a canvas that holds progressUI")] private GameObject captureUI;

        [SerializeField, Tooltip("The image that specifically displays the progress.")] private Image progressUI;

        [System.Serializable]
        public class Events
        {
            public UnityEvent onStartCapture, onCapturing, onLeaveCapture, onFinishCapture;
        }

        [SerializeField] private Events events;

        // This field stores the amount of time the player has been inside the capture point.
        public float captureTimeElapsed { get; private set; } = 0;

        // This method is called when the player enters the capture point.
        public override void EnterTrigger(GameObject target)
        {
            // Activate the capture UI.
            captureUI.SetActive(true);

            // Invoke the onStartCapture UnityEvent.
            events.onStartCapture?.Invoke();

            base.EnterTrigger(target);
        }

        // This method is called when the player leaves the capture point.
        public override void ExitTrigger(GameObject target)
        {
            // Deactivate the capture UI.
            captureUI.SetActive(false);

            // Invoke the onLeaveCapture UnityEvent.
            events.onLeaveCapture?.Invoke();

            base.ExitTrigger(target);
        }
        // This method is called while the player is inside the capture point.
        public override void StayTrigger(GameObject target)
        {
            // Invoke the onCapturing UnityEvent.
            events.onCapturing?.Invoke();

            // Update the capture time elapsed.
            captureTimeElapsed += Time.deltaTime;

            // Update the progress UI.
            progressUI.fillAmount = captureTimeElapsed / progressRequiredToCapture;

            // Check if the capture is complete.
            if (captureTimeElapsed >= progressRequiredToCapture) CapturePoint();

            base.StayTrigger(target);
        }

        // This method is called when the player has captured the point.
        private void CapturePoint()
        {
            // Invoke the onFinishCapture UnityEvent.
            events.onFinishCapture?.Invoke();
        }
    }
}