using UnityEngine;
using UnityEngine.Events;

namespace TwoBitMachines.FlareEngine
{
        public class GrabItem : MonoBehaviour
        {
                [SerializeField] public bool deactivate;
                [SerializeField] public UnityEvent onGrab;
                [SerializeField] public bool foldOut;

                public void OnGrab ()
                {
                        onGrab.Invoke();
                        if (deactivate)
                        {
                                this.gameObject.SetActive(false);
                        }
                }
        }
}
