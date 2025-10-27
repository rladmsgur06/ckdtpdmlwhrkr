using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace cowsins2D
{
    public class CowsinsButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [System.Serializable]
        public class ButtonEvents
        {
            public UnityEvent OnMouseEnter;
            public UnityEvent OnMouseStay;
            public UnityEvent OnMouseLeave;
            public UnityEvent OnMouseClick;
            public UnityEvent OnMouseRelease;
        }
        public ButtonEvents events;

        private bool isMouseOver = false;
        private bool pressed;
        private Vector3 targetScale, initialScale;
        [SerializeField] private float scaleSpeed = 5f;
        public void OnPointerEnter(PointerEventData eventData)
        {
            isMouseOver = true;
            if (events.OnMouseEnter != null)
            {
                events.OnMouseEnter.Invoke();
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isMouseOver = false;
            if (events.OnMouseLeave != null)
                events.OnMouseLeave.Invoke();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (events.OnMouseClick != null)
            {
                events.OnMouseClick.Invoke();
                pressed = true;
            }
        }

        private void Start()
        {
            initialScale = transform.localScale;
            targetScale = initialScale;
        }

        private void Update()
        {
            if (isMouseOver && !Input.GetMouseButtonUp(0))
            {
                if (events.OnMouseStay != null)
                    events.OnMouseStay.Invoke();
            }
            if (pressed && Input.GetMouseButtonUp(0))
            {
                if (events.OnMouseRelease != null)
                {
                    events.OnMouseRelease.Invoke();
                    pressed = false;
                }
            }

            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * scaleSpeed);
        }

        public void Scale(float amount)
        {
            targetScale = transform.localScale + Vector3.one * amount;
        }

        public void ScaleDown(float amount)
        {
            targetScale = initialScale;
        }

        public void LoadApp(int id)
        {
            StartCoroutine(Load(id));
        }

        private IEnumerator Load(int id)
        {
            yield return new WaitForSeconds(.6f);
            SceneManager.LoadSceneAsync(id);
            yield break;
        }
    }
}
