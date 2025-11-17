using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TwoBitMachines.FlareEngine
{
        public class InventorySlot : MonoBehaviour, ISelectHandler
        {
                [SerializeField] public Image imageIcon;
                [SerializeField] public Image imageDrop;
                [SerializeField] public TextMeshProUGUI textMesh;

                [System.NonSerialized] public Inventory inventory;
                [System.NonSerialized] public ItemUI itemUI = null;
                [System.NonSerialized] public int ID = -1;

                private RectTransform rectTransform;
                public bool empty => itemUI == null;

                public void GetRectTransform ()
                {
                        rectTransform = gameObject.GetComponent<RectTransform>();
                }

                public void UseItem ()
                {
                        OnSelect(null);
                        if (!empty && inventory != null && inventory.slotUseItem == SlotUseItem.OnSlotSelection)
                        {
                                inventory.UseItem(itemUI);
                        }
                }

                public void DropItem ()
                {
                        if (!empty && inventory != null)
                        {
                                inventory.RemoveItem(itemUI);
                        }
                }

                public void Set (ItemUI newItemUI, Sprite icon)
                {
                        if (newItemUI != null)
                        {
                                itemUI = newItemUI;
                                itemUI.slotID = ID;
                                imageIcon.sprite = icon;
                                imageIcon.enabled = icon != null;
                                if (imageDrop != null)
                                        imageDrop.enabled = itemUI.droppable;
                                UpdateTextAmount();
                        }
                }

                public void UpdateTextAmount ()
                {
                        if (itemUI == null || textMesh == null)
                                return;
                        int enabled = itemUI.amount > 1 ? 10 : 0; // using enable was causing a huge lag spike
                        textMesh.maxVisibleCharacters = enabled;
                        if (enabled > 0)
                                textMesh.text = itemUI.amount.ToString("n0");
                }

                public void Clear ()
                {
                        itemUI = null;
                        imageIcon.sprite = null;
                        imageIcon.enabled = false;
                        if (textMesh != null)
                                textMesh.maxVisibleCharacters = 0;
                        if (imageDrop != null)
                                imageDrop.enabled = false;
                }

                public void OnSelect (BaseEventData eventData) // called automatically when inventory slot is selected
                {
                        if (inventory == null)
                                return;

                        int oldID = inventory.slot.gridIndex;
                        inventory.slot.gridIndex = ID;

                        if (rectTransform != null)
                        {
                                inventory.MoveSelectFrame(rectTransform.position);
                        }
                        if (this.gameObject.activeInHierarchy && oldID != ID)
                        {
                                if (oldID >= 0)
                                {
                                        ItemUI oldItemUI = inventory.slot.Get(oldID).itemUI;
                                        inventory.inventorySO.DeselectItem(oldItemUI);
                                }
                                inventory.onMove.Invoke();
                        }
                        if (inventory.itemSelected != null)
                        {
                                inventory.itemSelected.sprite = imageIcon.sprite;
                                inventory.itemSelected.enabled = !empty;
                        }
                        if (inventory.itemDescription != null)
                        {
                                inventory.itemDescription.text = empty ? "" : inventory.GetItemDescription(itemUI.name);
                        }
                        if (inventory.itemCost != null)
                        {
                                inventory.itemCost.text = empty ? "" : inventory.GetItemCost(itemUI.name);
                        }
                        if (inventory.itemName != null)
                        {
                                inventory.itemName.text = empty ? "" : itemUI.name;
                        }
                        if (inventory.removeItem != null && !empty)
                        {
                                inventory.removeItem.gameObject.SetActive(itemUI.droppable);
                        }
                        // if (!empty && inventory != null && inventory.slotUseItem == SlotUseItem.OnSlotSelection)
                        // {
                        //         inventory.UseSelectedGridItem();
                        // }
                }

                public void OnBeginDrag ()
                {
                        if (empty || inventory == null || inventory.dragItem == null || inventory.dragItem.active)
                                return;

                        if (inventory.slot.gridIndex == ID)
                        {
                                inventory.MoveSelectFrame(rectTransform.position);
                        }

                        inventory.dragItem.Enable(itemUI, this, imageIcon.sprite);
                }

                public void OnDrag ()
                {
                        if (empty || inventory == null || inventory.dragItem == null || !inventory.dragItem.active)
                                return;

                        if (inventory.dragItem.rectTransform != null)
                        {
                                inventory.dragItem.rectTransform.position = Util.MousePositionUI(inventory.canvas);
                        }
                }

                public void OnPointerUp ()
                {
                        if (empty || inventory == null || inventory.dragItem == null || !inventory.dragItem.active)
                                return;

                        inventory.dragItem.Disable();
                        InventorySlot newSlot = Util.IsPointerOverUI<InventorySlot>();
                        if (inventory.dragItem.canTransfer && newSlot != null && inventory.dragItem.oldSlot != newSlot)
                        {
                                ItemUI oldItemUI = newSlot.itemUI;
                                newSlot.Transfer(inventory.dragItem.itemUI);
                                inventory.dragItem.oldSlot.Clear();
                                inventory.dragItem.oldSlot.Transfer(oldItemUI);
                                if (newSlot.rectTransform != null)
                                        inventory.MoveSelectFrame(newSlot.rectTransform.position);
                        }
                }

                public void Transfer (ItemUI newItemUI)
                {
                        if (newItemUI == null || inventory == null)
                                return;
                        Set(newItemUI, inventory.GetItemIcon(newItemUI.name));
                }
        }

}
