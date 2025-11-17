using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace TwoBitMachines.FlareEngine
{
        public class SaveSlotUI : MonoBehaviour, IDeselectHandler, IPointerDownHandler
        {
                [SerializeField] public TextMeshProUGUI playTime;
                [SerializeField] public TextMeshProUGUI levelNumber;
                [SerializeField] public Vector2 highlightOffset;
                [SerializeField] public Vector2 selectedOffset;

                [SerializeField] public UnityEvent slotClicked = new UnityEvent ( );
                [SerializeField] public UnityEvent slotSelected = new UnityEvent ( );
                [SerializeField] public UnityEvent slotDataDeleted = new UnityEvent ( );
                [SerializeField] public UnityEvent slotIsInitialized = new UnityEvent ( );
                [SerializeField] public UnityEvent slotHasBeenInitialized = new UnityEvent ( );

                [System.NonSerialized] private SaveMenu master;
                [System.NonSerialized] private SaveOptions save;
                [System.NonSerialized] public int slotIndex;

                #region ▀▄▀▄▀▄ Editor Variables ▄▀▄▀▄▀
                #if UNITY_EDITOR
                #pragma warning disable 0414
                [SerializeField, HideInInspector] private bool slotSelectedFoldOut;
                [SerializeField, HideInInspector] private bool slotClickedFoldOut;
                [SerializeField, HideInInspector] private bool slotHasBeenInitializedFoldOut;
                [SerializeField, HideInInspector] private bool slotDataDeletedFoldOut;
                [SerializeField, HideInInspector] private bool slotIsInitializedFoldOut;
                #pragma warning restore 0414
                #endif
                #endregion

                public void Initialize (SaveMenu master, SaveOptions save, SaveSlot slot, int slotIndex)
                {
                        this.master = master;
                        this.save = save;
                        this.slotIndex = slotIndex;

                        SetText (slot);
                        IsSlotInitialized (slot);
                }

                public void SetText (SaveSlot slot)
                {
                        SetTotalPlayTime (slot);
                        SetLevelNumber (slot);
                }

                public void SetTotalPlayTime (SaveSlot slot)
                {
                        if (slot == null || playTime == null) return;

                        int hours = TimeSpan.FromSeconds (slot.totalTime).Hours;
                        int minutes = TimeSpan.FromSeconds (slot.totalTime).Minutes;
                        int seconds = TimeSpan.FromSeconds (slot.totalTime).Seconds;

                        if (hours == 0)
                        {
                                playTime.text = minutes.ToString ("00") + ":" + seconds.ToString ("00");
                        }
                        else
                        {
                                playTime.text = hours.ToString ("00") + ":" + minutes.ToString ("00") + ":" + seconds.ToString ("00");
                        }
                }

                public void SetLevelNumber (SaveSlot slot)
                {
                        if (slot == null || levelNumber == null) return;

                        levelNumber.text = slot.level.ToString ( );
                }

                public void IsSlotInitialized (SaveSlot slot)
                {
                        if (slot != null && slot.initialized)
                        {
                                slotIsInitialized.Invoke ( );
                        }
                }

                public void DeleteSlotData ( )
                {
                        SaveOptions.Load (ref save);
                        if (save == null)
                        {
                                return;
                        }

                        //  if (save.currentSlot == slotIndex)
                        //  {
                        InitializeFalse ( );
                        slotDataDeleted.Invoke ( );
                        levelNumber.text = "0";
                        playTime.text = "00:00";
                        //  }

                        WorldManager.get.save = save;
                        save.DeleteSlotData (slotIndex);
                }

                public void SelectSlot ( )
                {
                        SaveOptions.Load (ref save);

                        if (save == null)
                        {
                                return;
                        }

                        if (save.currentSlot != slotIndex)
                        {
                                slotSelected.Invoke ( );
                        }

                        InitializeTrue ( );

                        save.currentSlot = slotIndex;
                        save.Save ( );
                        WorldManager.get.save = save;
                }

                public void InitializeTrue ( )
                {
                        for (int i = 0; i < save.slot.Count; i++)
                        {
                                if (i == slotIndex && !save.slot[i].initialized)
                                {
                                        save.slot[i].initialized = true;
                                        slotHasBeenInitialized.Invoke ( );
                                }
                        }
                }

                private void InitializeFalse ( )
                {
                        for (int i = 0; i < save.slot.Count; i++)
                        {
                                if (i == slotIndex)
                                {
                                        save.slot[i].initialized = false;
                                }
                        }
                }

                public void OnDeselect (BaseEventData eventData) // called automatically when inventory slot is selected
                {

                }

                public void OnPointerDown (PointerEventData eventData) // called automatically when inventory slot is selected
                {
                        master.OnSelect (this);
                        slotClicked.Invoke ( );
                }
        }
}