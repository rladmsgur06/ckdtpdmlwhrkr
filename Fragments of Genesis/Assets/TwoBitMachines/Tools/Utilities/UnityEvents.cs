using UnityEngine;
using UnityEngine.Events;

namespace TwoBitMachines
{
            public class UnityEvents : MonoBehaviour
            {
                        [SerializeField] public UnityEvent onEvent;
                        [SerializeField] public UnityEvent onAwake;
                        [SerializeField] public UnityEvent onStart;
                        [SerializeField] public UnityEvent onEnable;
                        [SerializeField] public UnityEvent onDisable;

                        private void Awake ( )
                        {
                                    onAwake.Invoke ( );
                        }
                        private void Start ( )
                        {
                                    onStart.Invoke ( );
                        }
                        private void OnDisable ( )
                        {
                                    onDisable.Invoke ( );
                        }
                        private void OnEnable ( )
                        {
                                    onEnable.Invoke ( );
                        }
            }
}