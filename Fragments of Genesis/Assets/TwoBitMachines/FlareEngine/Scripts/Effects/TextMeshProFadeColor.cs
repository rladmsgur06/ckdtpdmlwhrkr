using TMPro;
using UnityEngine;

namespace TwoBitMachines
{
        public class TextMeshProFadeColor : MonoBehaviour
        {
                [SerializeField] public TextMeshPro text;
                [SerializeField] public float fadeTime = 0.5f;
                [SerializeField] public float holdTime = 0.5f;
                [SerializeField] public bool deactivate = false;

                [System.NonSerialized] private float fadeCounter = 0;
                [System.NonSerialized] private float holdCounter = 0;

                void OnEnable ( )
                {
                        if (text != null) text.alpha = 1f;
                        fadeCounter = 0;
                        holdCounter = 0;
                }

                public void Update ( )
                {
                        if (text == null) return;

                        if (Clock.TimerExpired (ref holdCounter, holdTime))
                        {
                                text.alpha = Compute.Lerp (1f, 0, fadeTime, ref fadeCounter, out bool complete);
                                if (deactivate && complete) this.gameObject.SetActive (false);
                        }
                }
        }
}