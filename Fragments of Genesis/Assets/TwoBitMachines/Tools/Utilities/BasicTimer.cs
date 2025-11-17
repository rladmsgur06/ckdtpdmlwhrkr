using UnityEngine;
using UnityEngine.Events;

namespace TwoBitMachines
{
        public class BasicTimer : MonoBehaviour
        {
                [SerializeField] public float time = 1f;
                [SerializeField] public UnityEvent onRestart;
                [SerializeField] public UnityEvent onEnable;
                [System.NonSerialized] public float counter = 0f;

                #region ▀▄▀▄▀▄ Editor Variables ▄▀▄▀▄▀
                #if UNITY_EDITOR
                #pragma warning disable 0414
                [SerializeField, HideInInspector] private bool restartFoldOut;
                [SerializeField, HideInInspector] private bool enableFoldOut;
                #pragma warning restore 0414
                #endif
                #endregion

                void OnEnable ( )
                {
                        counter = 0;
                        onEnable.Invoke ( );
                }

                public void Restart ( )
                {
                        counter = 0;
                        onRestart.Invoke ( );
                        this.gameObject.SetActive (true);
                }

                void LateUpdate ( )
                {
                        if (Clock.Timer (ref counter, time))
                        {
                                this.gameObject.SetActive (false);
                        }
                }
        }
}