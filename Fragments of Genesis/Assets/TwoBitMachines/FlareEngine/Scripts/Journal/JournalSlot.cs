using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TwoBitMachines.FlareEngine
{
        public class JournalSlot : MonoBehaviour, IPointerDownHandler
        {
                [SerializeField] public Image icon;
                [SerializeField] public Image complete;
                [SerializeField] public Image incomplete;

                [SerializeField] public TextMeshProUGUI title;
                [SerializeField] public TextMeshProUGUI extraInfo;
                [SerializeField] public TextMeshProUGUI description;
                [SerializeField] public UnityEvent slotClicked = new UnityEvent();

                [System.NonSerialized] private Journal parent;
                [System.NonSerialized] public JournalObject journalObject;

                #region ▀▄▀▄▀▄ Editor Variables ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField, HideInInspector] private bool eventsFoldOut;
                [SerializeField, HideInInspector] private bool clickFoldOut;
#pragma warning restore 0414
#endif
                #endregion

                public void Set (Journal parentRef, JournalObject journalObjectRef)
                {
                        journalObject = journalObjectRef;
                        parent = parentRef;
                        gameObject.SetActive(true);
                        complete?.gameObject.SetActive(false);
                        incomplete?.gameObject.SetActive(false);

                        title?.SetText(journalObject.Name());
                        extraInfo?.SetText(journalObject.ExtraInfo());
                        description?.SetText(journalObject.Description());

                        if (icon != null)
                        {
                                icon.sprite = journalObject.Icon();
                        }
                }

                public void Select (bool callEvent = true)
                {
                        if (gameObject.activeInHierarchy)
                        {
                                if (callEvent)
                                {
                                        slotClicked.Invoke();
                                }
                                if (EventSystem.current != null)
                                {
                                        EventSystem.current.SetSelectedGameObject(gameObject);
                                }
                        }
                        if (parent != null && journalObject != null)
                        {
                                RefreshCompleteStatus();
                                parent.icon?.gameObject.SetActive(true);
                                parent.title?.gameObject.SetActive(true);
                                parent.extraInfo?.gameObject.SetActive(true);
                                parent.description?.gameObject.SetActive(true);

                                parent.title?.SetText(journalObject.Name());
                                parent.extraInfo?.SetText(journalObject.ExtraInfo());
                                parent.description?.SetText(journalObject.Description());

                                if (parent.icon != null)
                                {
                                        parent.icon.sprite = journalObject.Icon();
                                }
                        }
                }

                public void RefreshCompleteStatus ()
                {
                        complete?.gameObject.SetActive(false);
                        incomplete?.gameObject.SetActive(false);

                        if (journalObject is QuestSO && journalObject.Complete())
                        {
                                complete?.gameObject.SetActive(true);
                        }
                        if (journalObject is QuestSO && !journalObject.Complete())
                        {
                                incomplete?.gameObject.SetActive(true);
                        }
                }

                public void OnPointerDown (PointerEventData eventData) // called automatically when inventory slot is selected
                {
                        if (parent != null)
                        {
                                parent.Select(this);
                        }
                }
        }
}
