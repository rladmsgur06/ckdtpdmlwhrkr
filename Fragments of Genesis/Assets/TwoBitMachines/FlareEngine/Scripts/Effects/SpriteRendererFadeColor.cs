using UnityEngine;

namespace TwoBitMachines
{
        public class SpriteRendererFadeColor : MonoBehaviour
        {
                [SerializeField] public SpriteRenderer rendererRef;
                [SerializeField] public float fadeTime = 0.5f;
                [SerializeField] public float holdTime = 0.5f;
                [SerializeField] public bool reverseFade = false;
                [SerializeField] public bool deactivate = false;

                [System.NonSerialized] private float fadeCounter = 0;
                [System.NonSerialized] private float holdCounter = 0;
                [System.NonSerialized] private bool exit = false;

                void OnEnable ( )
                {
                        if (rendererRef != null)
                        {
                                Color color = rendererRef.color;
                                color.a = reverseFade ? 0f : 1f;
                                rendererRef.color = color;
                        }

                        fadeCounter = 0;
                        holdCounter = 0;
                        exit = false;
                }

                public void Update ( )
                {
                        if (rendererRef == null || exit)
                        {
                                return;
                        }

                        if (Clock.TimerExpired (ref holdCounter, holdTime))
                        {
                                float start = reverseFade ? 0f : 1f;
                                float end = reverseFade ? 1f : 0f;
                                float alpha = Compute.Lerp (start, end, fadeTime, ref fadeCounter, out bool complete);

                                Color color = rendererRef.color;
                                color.a = alpha;
                                rendererRef.color = color;

                                if (deactivate && complete)
                                {
                                        gameObject.SetActive (false);
                                }
                                if (reverseFade && complete)
                                {
                                        exit = true;
                                }
                        }
                }
        }
}