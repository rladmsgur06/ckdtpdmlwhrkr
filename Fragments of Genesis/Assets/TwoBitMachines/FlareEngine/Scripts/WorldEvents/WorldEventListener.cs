using UnityEngine;
using UnityEngine.Events;

namespace TwoBitMachines.FlareEngine
{
        [AddComponentMenu ("Flare Engine/一WorldEvents/WorldEventListener")]
        public class WorldEventListener : MonoBehaviour
        {
                [SerializeField] public UnityEvent onWorldEvent;
                [SerializeField] public WorldEventSO worldEvent;

                #region ▀▄▀▄▀▄ Editor Variables ▄▀▄▀▄▀
                #if UNITY_EDITOR
                #pragma warning disable 0414
                [SerializeField] private bool foldOut = false;
                [SerializeField] private string eventName = "";
                #pragma warning restore 0414
                #endif
                #endregion

                private void Start ( )
                {
                        if (worldEvent != null)
                        {
                                worldEvent.RegisterListener (this);
                        }
                }

                public void EventTriggered ( )
                {
                        if (onWorldEvent != null)
                        {
                                onWorldEvent.Invoke ( );
                        }
                }

                public void UnregisterListener ( )
                {
                        if (worldEvent != null)
                        {
                                worldEvent.UnregisterListener (this);
                        }
                }

                private void OnDestroy ( )
                {
                        UnregisterListener ( );
                }
        }
}