using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TwoBitMachines.FlareEngine
{
        public class Journal : MonoBehaviour
        {
                [SerializeField] public string journalName = "journal";
                [SerializeField] public string saveKey;
                [SerializeField] public int range;
                [SerializeField] public bool hasRange;
                [SerializeField] public bool canLoop;

                [SerializeField] public PauseGameType pauseType;
                [SerializeField] public RectTransform highlight;
                [SerializeField] public Vector2 highlightOffset;

                [SerializeField] public JournalSlot slot;
                [SerializeField] public Transform slotParent;

                [SerializeField] public Image icon;
                [SerializeField] public TextMeshProUGUI title;
                [SerializeField] public TextMeshProUGUI extraInfo;
                [SerializeField] public TextMeshProUGUI description;

                [SerializeField] public InputButtonSO toggle;
                [SerializeField] public InputButtonSO back;
                [SerializeField] public InputButtonSO forward;

                [SerializeField] public UnityEvent onOpen;
                [SerializeField] public UnityEvent onClose;

                [SerializeField] public List<JournalInventory> inventory = new List<JournalInventory>();

                [System.NonSerialized] private int rangeIndex = 0;
                [System.NonSerialized] private bool initialized = false;
                [System.NonSerialized] private SaveJournal journal = new SaveJournal();
                [System.NonSerialized] private List<JournalSlot> slots = new List<JournalSlot>();
                public static List<Journal> journalList = new List<Journal>();

                #region ▀▄▀▄▀▄ Editor Variables ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField, HideInInspector] private bool eventsFoldOut;
                [SerializeField, HideInInspector] private bool closeFoldOut;
                [SerializeField, HideInInspector] private bool openFoldOut;
#pragma warning restore 0414
#endif
                #endregion

                #region Enable
                private void Start ()
                {
                        WorldManager.RegisterInput(back);
                        WorldManager.RegisterInput(forward);
                        WorldManager.RegisterInput(toggle);
                        WorldManager.get.endOfFrameUpdate -= EndOfFrame;
                        WorldManager.get.endOfFrameUpdate += EndOfFrame;
                        AddToSystem();
                }

                private void OnEnable ()
                {
                        if (pauseType == PauseGameType.PauseGame)
                                WorldManager.get.PauseNoInvoke();
                        if (pauseType == PauseGameType.BlockPlayerInput)
                                ThePlayer.Player.BlockAllInputs(true);
                        if (slots.Count <= 0 || journal.index >= slots.Count)
                        {
                                icon?.gameObject.SetActive(false);
                                title?.gameObject.SetActive(false);
                                extraInfo?.gameObject.SetActive(false);
                                highlight?.gameObject.SetActive(false);
                                description?.gameObject.SetActive(false);
                        }
                        Restore();
                        Select(journal.index, false);
                        RefreshCompleteStatus();
                        onOpen.Invoke();

                }

                private void OnDisable ()
                {
                        if (pauseType == PauseGameType.PauseGame)
                                WorldManager.get.UnpauseNoInvoke();
                        if (pauseType == PauseGameType.BlockPlayerInput)
                                ThePlayer.Player.BlockAllInputs(false);
                        onClose.Invoke();
                }

                private void RefreshCompleteStatus ()
                {
                        for (int i = 0; i < slots.Count; i++)
                        {
                                slots[i].RefreshCompleteStatus();
                        }
                }

                public void AddToSystem ()
                {
                        if (!journalList.Contains(this))
                        {
                                WorldManager.get.update -= Activate;
                                WorldManager.get.update += Activate;
                                journalList.Add(this);
                        }
                }

                public static void Find (Transform parent)
                {
                        journalList.Clear();
                        for (int i = 0; i < parent.childCount; i++)
                        {
                                parent.GetChild(i).GetComponent<Journal>()?.AddToSystem();
                        }
                }

                public static void AddToJournal (JournalObject journalObject, string journalName)
                {
                        if (journalObject == null)
                        {
                                return;
                        }
                        for (int i = 0; i < journalList.Count; i++)
                        {
                                if (journalList[i].journalName == journalName)
                                {
                                        journalList[i].AddToJournal(journalObject);
                                }
                        }
                }
                #endregion

                #region Save
                public void Save ()
                {
                        Storage.Save(journal, WorldManager.saveFolder, saveKey);
                }

                private void Restore ()
                {
                        if (initialized)
                                return;

                        initialized = true;
                        journal = Storage.Load<SaveJournal>(journal, WorldManager.saveFolder, saveKey);
                        journal.CreateSlots(this);
                }

                public void DeleteSavedData ()
                {
                        Storage.Delete(WorldManager.saveFolder, saveKey);
                        for (int i = 0; i < slots.Count; i++)
                        {
                                slots[i].gameObject.SetActive(false);
                        }
                        slots.Clear();
                        journal.item.Clear();
                        journal.index = 0;
                        Save();
                }
                #endregion

                #region Util
                public void EndOfFrame (bool gameReset = false)
                {
                        if (gameReset)
                        {
                                gameObject.SetActive(false);
                        }
                        if (!gameObject.activeInHierarchy)
                        {
                                return;
                        }
                        if (highlight != null && slots.Count > 0 && journal.index < slots.Count)
                        {
                                if (!highlight.gameObject.activeInHierarchy)
                                {
                                        highlight.gameObject.SetActive(true);
                                }
                                highlight.anchoredPosition = slots[journal.index].gameObject.GetComponent<RectTransform>().anchoredPosition + highlightOffset;
                        }
                }

                private void RangeView ()
                {
                        if (hasRange)
                        {
                                Compute.ListRange(journal.index, ref rangeIndex, slots.Count, range);
                                for (int i = 0; i < slots.Count; i++)
                                {
                                        slots[i].gameObject.SetActive(Compute.IndexInListRange(i));
                                }
                        }
                }

                public void CreateSlot (JournalObject newObject)
                {
                        if (slot != null)
                        {
                                JournalSlot newSlot = Instantiate(slot, Vector3.zero, Quaternion.identity, slotParent);
                                newSlot.Set(this, newObject);
                                slots.Add(newSlot);
                        }
                }

                public void AddToJournal (JournalObject newObject)
                {
                        if (!initialized)
                        {
                                Restore();
                        }
                        if (journal.Add(newObject))
                        {
                                CreateSlot(newObject);
                                Select(journal.size - 1);
                                Save();
                        }
                }

                public void RemoveFromJournal (JournalObject journalObject)
                {
                        if (!initialized)
                        {
                                Restore();
                        }
                        if (journal.Contains(journalObject))
                        {
                                for (int i = 0; i < slots.Count; i++)
                                {
                                        if (slots[i].journalObject == journalObject)
                                        {
                                                slots.RemoveAt(i);
                                                break;
                                        }
                                }
                                journal.Remove(journalObject);
                                Select(journal.index - 1);
                                Save();
                        }
                }

                public void RemoveFromJournal ()
                {
                        if (!initialized)
                        {
                                Restore();
                        }
                        if (slots.Count > 0 && journal.index < slots.Count)
                        {
                                JournalSlot slot = slots[journal.index];
                                if (journal.Contains(slot.journalObject))
                                {
                                        slots.Remove(slot);
                                        journal.Remove(slot.journalObject);
                                        Select(journal.index - 1);
                                        Save();
                                        return;
                                }
                        }
                }
                #endregion

                #region Navigate
                public void Select (JournalSlot newSlot)
                {
                        for (int i = 0; i < slots.Count; i++)
                        {
                                if (slots[i] == newSlot)
                                {
                                        RangeView();
                                        journal.index = i;
                                        slots[journal.index].Select();
                                        return;
                                }
                        }
                }

                private void Select (int newIndex = 0, bool callEvent = true)
                {
                        newIndex = Mathf.Clamp(newIndex, 0, slots.Count - 1);
                        if (slots.Count > 0 && newIndex < slots.Count)
                        {
                                RangeView();
                                slots[newIndex].Select(callEvent);
                                journal.index = newIndex;
                        }
                }

                private void Activate (bool gameReset = false)
                {
                        if (toggle != null && toggle.Pressed())
                        {
                                gameObject.SetActive(!gameObject.activeInHierarchy);
                        }
                }

                private void LateUpdate ()
                {
                        if (back != null && back.Pressed())
                        {
                                Back();
                        }
                        if (forward != null && forward.Pressed())
                        {
                                Forward();
                        }
                        if (Input.mouseScrollDelta.y != 0)
                        {
                                float sign = Mathf.Sign(Input.mouseScrollDelta.y);
                                if (sign > 0)
                                        Back();
                                if (sign < 0)
                                        Forward();
                        }
                }

                public void Back ()
                {
                        canLoop = true;
                        journal.index = journal.index - 1 < 0 ? (canLoop ? slots.Count - 1 : 0) : journal.index - 1;
                        Select(journal.index);
                }

                public void Forward ()
                {
                        canLoop = true;
                        journal.index = journal.index + 1 >= slots.Count ? (canLoop ? 0 : slots.Count - 1) : journal.index + 1;
                        Select(journal.index);
                }
                #endregion
        }

        [System.Serializable]
        public class SaveJournal
        {
                [SerializeField] public int index = 0;
                [SerializeField] public List<string> item = new List<string>();
                public int size => item.Count;

                public bool Add (JournalObject journalObject)
                {
                        if (!item.Contains(journalObject.Name()))
                        {
                                item.Add(journalObject.Name());
                                return true;
                        }
                        return false;
                }

                public bool Remove (JournalObject journalObject)
                {
                        if (item.Contains(journalObject.Name()))
                        {
                                item.Remove(journalObject.Name());
                                return true;
                        }
                        return false;
                }

                public bool Contains (JournalObject journalObject)
                {
                        if (item.Contains(journalObject.Name()))
                        {
                                return true;
                        }
                        return false;
                }

                public void CreateSlots (Journal journal)
                {
                        if (item.Count == 0)
                        {
                                return;
                        }
                        for (int i = 0; i < journal.inventory.Count; i++)
                        {
                                if (journal.inventory[i] is InventorySO)
                                {
                                        List<ItemSO> list = journal.inventory[i].ItemList();
                                        for (int j = 0; j < list.Count; j++)
                                        {
                                                if (list[j] != null && item.Contains(list[j].Name()))
                                                {
                                                        journal.CreateSlot(list[j]);
                                                }
                                        }
                                }
                                else
                                {
                                        List<QuestSO> list = journal.inventory[i].QuestList();
                                        for (int j = 0; j < list.Count; j++)
                                        {
                                                if (list[j] != null && item.Contains(list[j].Name()))
                                                {
                                                        journal.CreateSlot(list[j]);
                                                }
                                        }
                                }
                        }
                }
        }
}
