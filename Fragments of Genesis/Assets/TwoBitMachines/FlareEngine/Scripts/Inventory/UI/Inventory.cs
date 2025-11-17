using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TwoBitMachines.FlareEngine
{
        [AddComponentMenu("Flare Engine/一Inventory/Inventory")]
        public class Inventory : MonoBehaviour
        {
                [SerializeField] public InventorySO inventorySO;
                [SerializeField] public SlotUseItem slotUseItem;
                [SerializeField] public SlotNavigation navigation;
                [SerializeField] public InputButtonSO toggle;
                [SerializeField] public InputButtonSO useItem;
                [SerializeField] public InputButtonSO buttonLeftUp;
                [SerializeField] public InputButtonSO buttonRightDown;
                [SerializeField] public PauseGameType pauseType;
                [SerializeField] public List<Inventory> block = new List<Inventory>();

                [SerializeField] public Image itemSelected;
                [SerializeField] public Image selectFrame;
                [SerializeField] public Image currentItem;
                [SerializeField] public DragItem dragItem;
                [SerializeField] public RectTransform removeItem;
                [SerializeField] public TextMeshProUGUI itemDescription;
                [SerializeField] public TextMeshProUGUI itemName;
                [SerializeField] public TextMeshProUGUI itemCost;
                [SerializeField] public SlotManager slot = new SlotManager();
                [SerializeField] public float selectFrameSpeed = 0.25f;
                [SerializeField] public bool autoItemLoad = false;
                [SerializeField] public int autoLoadIndex = 0;

                [SerializeField] public UnityEvent onUseItem;
                [SerializeField] public UnityEvent onRemoveItem;
                [SerializeField] public UnityEvent onClose;
                [SerializeField] public UnityEvent onOpen;
                [SerializeField] public UnityEvent onMove;

                [System.NonSerialized] public Canvas canvas;
                [System.NonSerialized] private bool initialized;
                [System.NonSerialized] private bool skippedFirstFrame;
                [System.NonSerialized] private GameObject lastSelect;
                [System.NonSerialized] private ItemUI previousItem;
                [System.NonSerialized] public static bool blockInventories;

                private Dictionary<string, ItemSO> search => inventorySO.search;

                #region ▀▄▀▄▀▄ Editor Variables ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField, HideInInspector] public int childCount;
                [SerializeField, HideInInspector] public bool loadSlots;
                [SerializeField, HideInInspector] public bool defaultFoldOut;
                [SerializeField, HideInInspector] public bool deleteInventory;
                [SerializeField, HideInInspector] public bool uiFoldOut;
                [SerializeField, HideInInspector] public bool blockFoldOut;
                [SerializeField, HideInInspector] public bool eventFoldOut;
                [SerializeField, HideInInspector] public bool onUseItemFoldOut;
                [SerializeField, HideInInspector] public bool onRemoveItemFoldOut;
                [SerializeField, HideInInspector] public bool onMoveFoldOut;
                [SerializeField, HideInInspector] public bool onOpenFoldOut;
                [SerializeField, HideInInspector] public bool onCloseFoldOut;
#pragma warning restore 0414
#endif
                #endregion

                public void Initialize (PickUpItems pickUpItem)
                {
                        if (inventorySO == null || initialized)
                                return;

                        initialized = true;
                        WorldManager.RegisterInput(toggle);
                        WorldManager.RegisterInput(useItem);
                        WorldManager.RegisterInput(buttonLeftUp);
                        WorldManager.RegisterInput(buttonRightDown);

                        inventorySO.Initialize(pickUpItem);
                        skippedFirstFrame = false;
                        lastSelect = slot.firstGameObject;
                        canvas = GetComponent<Canvas>();
                        inventorySO.AddListener(this);
                        slot.Initialize(this);

                        for (int i = 0; i < inventorySO.list.Count; i++) // Put saved items, if any, back into slots
                        {
                                slot.SetSlotItem(inventorySO.list[i], GetItemIcon(inventorySO.list[i].name), initialize: true);
                        }
                        if (navigation == SlotNavigation.Manual)
                        {
                                AutoItemLoad();
                        }
                        if (slot.count > 0)
                        {
                                slot.Get(0).OnSelect(null);
                        }
                }

                public void ClearInventories ()
                {
                        inventorySO.ClearInventories();
                }

                public void OnEnable ()
                {
                        if (pauseType == PauseGameType.PauseGame)
                        {
                                WorldManager.get.Pause();
                        }
                        if (pauseType == PauseGameType.BlockPlayerInput)
                        {
                                ThePlayer.Player.BlockAllInputs(true);
                        }
                        onOpen.Invoke();
                        if (slot.count > 0 && slot.gridIndex <= 0)
                        {
                                slot.Get(0).OnSelect(null);
                        }
                        if (slot.count > 0 && slot.gridIndex <= 0)
                        {
                                GameObject currentSelected = EventSystem.current.currentSelectedGameObject;
                                if (currentSelected == null || currentSelected.GetComponent<InventorySlot>() == null)
                                {
                                        EventSystem.current.SetSelectedGameObject(slot.Get(0).gameObject);
                                }
                        }
                }

                public void OnDisable ()
                {
                        if (pauseType == PauseGameType.PauseGame)
                                WorldManager.get.Unpause();
                        if (pauseType == PauseGameType.BlockPlayerInput)
                                ThePlayer.Player.BlockAllInputs(false);
                        onClose.Invoke();
                }

                private void LateUpdate ()
                {
                        if (useItem != null && useItem.Pressed())
                        {
                                if (navigation == SlotNavigation.Manual && slot.count > 0)
                                {
                                        UseItem(slot.Get(0).itemUI);
                                }
                                if (navigation == SlotNavigation.UnityNavigation && slot.count > 0 && slot.gridIndex >= 0 && slot.gridIndex < slot.count)
                                {
                                        UseItem(slot.Get(slot.gridIndex).itemUI);
                                }
                        }
                        if (navigation == SlotNavigation.Manual)
                        {
                                if (buttonLeftUp != null && buttonLeftUp.Pressed())
                                        MoveLeftUp();
                                if (buttonRightDown != null && buttonRightDown.Pressed())
                                        MoveRightDown();
                        }
                        else if (EventSystem.current)
                        {
                                if (EventSystem.current.currentSelectedGameObject == null)
                                {
                                        EventSystem.current.SetSelectedGameObject(lastSelect);
                                }
                                else
                                {
                                        lastSelect = EventSystem.current.currentSelectedGameObject;
                                }
                        }
                }

                public void UseItem (int index)
                {
                        if (slot.count > 0 && index < slot.count)
                        {
                                UseItem(slot.Get(index).itemUI);
                        }
                }

                private void AutoItemLoad ()
                {
                        ItemUI newItemUI = slot.Get(autoLoadIndex).itemUI;
                        if (previousItem != newItemUI)
                        {
                                inventorySO.DeselectItem(previousItem);
                                previousItem = newItemUI;
                        }
                        if (autoItemLoad && slot.count > 0 && autoLoadIndex < slot.count)
                        {
                                UseItem(newItemUI, false);
                        }
                }

                public void UseSelectedGridItem ()
                {
                        if (slot.count > 0 && slot.gridIndex >= 0 && slot.gridIndex < slot.count)
                        {
                                UseItem(slot.Get(slot.gridIndex).itemUI);
                        }
                }

                public void RemoveSelectedGridItem ()
                {
                        if (slot.count > 0 && slot.gridIndex >= 0 && slot.gridIndex < slot.count)
                        {
                                RemoveItem(slot.Get(slot.gridIndex).itemUI);
                        }
                }

                public void UseItem (ItemUI useItemUI, bool toggleTool = true)
                {
                        if (useItemUI != null && inventorySO != null)
                        {
                                if (inventorySO.UseSlotItem(useItemUI, out bool used, toggleTool))
                                {
                                        ClearSelected();
                                }
                                if (currentItem != null && !GetItemConsumable(useItemUI.name))
                                {
                                        currentItem.sprite = GetItemIcon(useItemUI.name);
                                        currentItem.enabled = true;
                                }
                                if (used)
                                {
                                        onUseItem.Invoke();
                                }
                        }
                }

                public void RemoveItem (ItemUI removeItemUI)
                {
                        if (removeItemUI != null && inventorySO != null)
                        {
                                inventorySO.RemoveFromInventory(removeItemUI);
                                onRemoveItem.Invoke();
                                ClearSelected();
                        }
                }

                public void ClearSelected () // called automatically when inventory slot is selected
                {
                        if (itemDescription != null)
                        {
                                itemDescription.text = "";
                        }
                        if (itemCost != null)
                        {
                                itemCost.text = "";
                        }
                        if (itemName != null)
                        {
                                itemName.text = "";
                        }
                        if (itemSelected != null)
                        {
                                itemSelected.sprite = null;
                                itemSelected.enabled = false;
                        }
                }

                #region Visual
                public Sprite GetItemIcon (string name)
                {
                        return search.ContainsKey(name) ? search[name].icon : null;
                }

                public string GetItemDescription (string name)
                {
                        return search.ContainsKey(name) ? search[name].description : "";
                }

                public string GetItemCost (string name)
                {
                        return search.ContainsKey(name) ? search[name].cost.ToString() : "";
                }

                public bool GetItemConsumable (string name)
                {
                        return search.ContainsKey(name) ? search[name].consumable : false;
                }

                public void MoveSelectFrame (Vector2 position)
                {
                        if (selectFrame == null || !skippedFirstFrame)
                        {
                                skippedFirstFrame = true;
                                return;
                        }

                        if (selectFrameSpeed <= 0)
                        {
                                selectFrame.rectTransform.position = position;
                                return;
                        }
                        Wiggle.StopTween(selectFrame.gameObject);
                        Wiggle.Target(selectFrame.gameObject).MoveTo2D(position, selectFrameSpeed, Tween.EaseInOut).UnscaledTime(true);
                }

                public void MoveLeftUp ()
                {
                        if (inventorySO != null)
                                MoveSlots(inventorySO.ItemList(slot.viewItems, slot.keyName), -1, 1);
                }

                public void MoveRightDown ()
                {
                        if (inventorySO != null)
                                MoveSlots(inventorySO.ItemList(slot.viewItems, slot.keyName), 1, -1);
                }

                private void MoveSlots (List<ItemUI> list, int left, int right)
                {
                        int totalItems = list.Count;
                        int direction = slot.count >= totalItems ? left : right;
                        slot.ChangeBarIndex(totalItems, direction);
                        slot.ClearSlots();
                        inventorySO.ClearItemSlotID();
                        slot.Carousel(list, this);
                        AutoItemLoad();
                        inventorySO.inventory.barIndex = slot.barIndex;
                }

                public void MoveToFirstSlot (int offset)
                {
                        List<ItemUI> list = inventorySO.ItemList(slot.viewItems, slot.keyName);
                        int totalItems = list.Count;
                        slot.barIndex = 0;
                        if (offset != 0)
                                slot.ChangeBarIndex(totalItems, offset);
                        slot.ClearSlots();
                        inventorySO.ClearItemSlotID();
                        slot.Carousel(list, this);
                        AutoItemLoad();
                        inventorySO.inventory.barIndex = slot.barIndex;
                }
                #endregion
        }

        public enum SlotNavigation
        {
                UnityNavigation,
                Manual
        }

        public enum PauseGameType
        {
                PauseGame,
                BlockPlayerInput,
                LeaveAsIs
        }

}
