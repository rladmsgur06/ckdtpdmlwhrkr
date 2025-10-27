using UnityEngine;
using TMPro;
using UnityEngine.Events; 

namespace cowsins2D
{

    public class CountdownTimer : MonoBehaviour
    {
        [SerializeField, Tooltip("Measured in seconds")] private float initialCountdownTime = 30f; // Measured in seconds
        [SerializeField, Tooltip("Displays the time remaining")] private TMP_Text timerText;
        [SerializeField, Tooltip("Start counting from the start of the game")] private bool startCountdownAtStart = true;
        [SerializeField] private bool showHours = true;
        [SerializeField] private bool showMinutes = true;
        [SerializeField] private bool showSeconds = true;

        private float targetTime;
        private bool isCountdownRunning = false;

        public UnityEvent onStartTimer, OnTimerComplete;

        private void Start()
        {
            // Start the countdown in the beginning of the game if we are allowed to do that.
            if (startCountdownAtStart)
                StartCountdown();
        }

        private void Update()
        {
            if (isCountdownRunning)
            {
                // Update the timer
                float remainingTime = Mathf.Max(targetTime - Time.time, 0f);
                UpdateTimerText(remainingTime);

                // Handle timer completion
                if (remainingTime <= 0f)
                {
                    isCountdownRunning = false;
                    OnTimerComplete?.Invoke();
                }
            }
        }

        public void StartCountdown()
        {
            onStartTimer?.Invoke();
            targetTime = Time.time + initialCountdownTime;
            isCountdownRunning = true;
        }

        private void UpdateTimerText(float time)
        {
            int hours = Mathf.FloorToInt(time / 3600);
            int minutes = Mathf.FloorToInt((time % 3600) / 60);
            int seconds = Mathf.FloorToInt(time % 60);

            string timerString = "";

            if (showHours)
                timerString += hours.ToString("00") + ":";

            if (showMinutes)
                timerString += minutes.ToString("00") + ":";

            if (showSeconds)
                timerString += seconds.ToString("00");

            timerText.text = timerString;
        }
    }
}