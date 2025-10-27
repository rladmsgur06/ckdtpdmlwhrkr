using UnityEngine;
using TMPro;

namespace cowsins2D
{
    public class FPSDisplay : MonoBehaviour
    {
        [SerializeField, Tooltip("TMPro Text to display the frame rate:")] TextMeshProUGUI frameRateText;
        [SerializeField, Tooltip("Seconds interval to display the information.")] float updateFrequency = 0.5f;
        [SerializeField, Tooltip("")] bool showCurrentFrameRate = true;
        [SerializeField, Tooltip("")] bool showAverageFrameRate = true;
        [SerializeField, Tooltip("")] bool showMaximumFrameRate = true;

        private float deltaTime = 0.0f;
        private float currentFPS;
        private float avgFPS;
        private float maxFPS = 0f;

        private Color redColor = Color.red;
        private Color yellowColor = Color.yellow;
        private Color greenColor = Color.green;

        private int lastFrameIndex;
        private float[] frameDeltaTimeArray;

        private void Start()
        {
            // Set the initial text values
            UpdateFrameRateText();
            frameDeltaTimeArray = new float[50];
        }

        private void Update()
        {
            deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
            frameDeltaTimeArray[lastFrameIndex] = Time.deltaTime;
            lastFrameIndex = (lastFrameIndex + 1) % frameDeltaTimeArray.Length;
            currentFPS = CalculateFramerate();

            // Update avg and max frame rate values
            avgFPS = Time.frameCount / Time.time;
            maxFPS = Mathf.Max(maxFPS, currentFPS);

            // Update the frame rate text at the specified update frequency
            if (Time.unscaledTime % updateFrequency <= 0.02f)
            {
                UpdateFrameRateText();
            }
        }

        private float CalculateFramerate()
        {
            float total = 0;
            foreach (float deltaTime in frameDeltaTimeArray)
            {
                total += deltaTime;
            }
            return frameDeltaTimeArray.Length / total;
        }

        private void UpdateFrameRateText()
        {
            string text = "";

            if (showCurrentFrameRate)
                text += "Current FPS: " + GetColoredFPSText(currentFPS) + "\n";

            if (showAverageFrameRate)
                text += "Avg FPS: " + GetColoredFPSText(avgFPS) + "\n";

            if (showMaximumFrameRate)
                text += "Max FPS: " + GetColoredFPSText(maxFPS);

            frameRateText.text = text;
        }

        private string GetColoredFPSText(float fps)
        {
            Color fpsColor;

            if (fps < 15f)
            {
                fpsColor = redColor;
            }
            else if (fps < 45f)
            {
                fpsColor = yellowColor;
            }
            else
            {
                fpsColor = greenColor;
            }

            return "<color=#" + ColorUtility.ToHtmlStringRGB(fpsColor) + ">" + fps.ToString("F0") + "</color>";
        }
    }
}