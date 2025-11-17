using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace TwoBitMachines
{
        public class OnButtonHold : MonoBehaviour, IUpdateSelectedHandler, IPointerDownHandler, IPointerUpHandler
        {
                public UnityEvent onHold;
                private bool isPressed;

                public void OnUpdateSelected (BaseEventData data)
                {
                        if (isPressed)
                        {
                                onHold.Invoke();
                        }
                }

                public void OnPointerDown (PointerEventData data)
                {
                        isPressed = true;
                }

                public void OnPointerUp (PointerEventData data)
                {
                        isPressed = false;
                }
        }
}
