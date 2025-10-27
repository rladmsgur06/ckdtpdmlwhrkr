using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.Events;

namespace cowsins2D
{
    public class TypewriterEffect : MonoBehaviour
    {
        [SerializeField, Tooltip("Speed at which each character is revealed.")] private float typingInterval = 0.05f; // Speed at which each character is revealed
        [SerializeField, Tooltip("Set to true if the effect should begin on start.")] private bool typeOnStart;
        private TextMeshProUGUI textMeshPro;
        private string fullText;
        private string currentText = "";
        private bool isTyping = true;

        [SerializeField] private UnityEvent onStartTyping, onFinishedTyping;

        private void Start()
        {
            textMeshPro = GetComponent<TextMeshProUGUI>();
            fullText = textMeshPro.text;
            textMeshPro.text = ""; // Clear the initial text
            if (typeOnStart)
                Type();
        }

        public void Type()
        {
            onStartTyping?.Invoke();
            StartCoroutine(TypeText());
        }

        private IEnumerator TypeText()
        {
            foreach (char c in fullText)
            {
                if (isTyping)
                {
                    currentText += c;
                    textMeshPro.text = currentText;
                    yield return new WaitForSeconds(typingInterval);
                }
            }

            onFinishedTyping?.Invoke();
            isTyping = false; // Typing is done
        }
    }

}