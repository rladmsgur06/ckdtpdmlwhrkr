using System.Collections.Generic;
using UnityEngine;

namespace TwoBitMachines.FlareEngine
{
        [System.Serializable]
        public class SlotManager
        {
                [SerializeField] public List<InventorySlot> slots = new List<InventorySlot> ( );
                [SerializeField] public int barIndex = 0;
                [SerializeField] public int gridIndex = 0;
                [SerializeField] public ShowItems viewItems;
                [SerializeField] public string keyName;
                public GameObject firstGameObject => slots.Count > 0 ? slots[0].gameObject : null;
                public int count => slots.Count;

                public void ClearList ( )
                {
                        slots.Clear ( );
                }

                public InventorySlot Get (int index)
                {
                        return slots[index];
                }

                public void Add (InventorySlot inventorySlot)
                {
                        slots.Add (inventorySlot);
                }

                public void Initialize (Inventory inventory)
                {
                        gridIndex = -1;
                        barIndex = inventory.inventorySO.inventory.barIndex;
                        for (int i = 0; i < slots.Count; i++)
                        {
                                if (slots[i] == null) continue;
                                slots[i].GetRectTransform ( );
                                slots[i].inventory = inventory;
                                slots[i].ID = i;
                        }
                }

                public void SetSlotItem (ItemUI setItemUI, Sprite icon, bool initialize = false)
                {
                        if (setItemUI == null) return;

                        for (int i = 0; i < slots.Count; i++) // if resetting, we match item with slot id, or else we skip
                        {
                                if (slots[i] != null && slots[i].empty && (!initialize || (setItemUI.slotID == i || setItemUI.slotID == -1))) // If -1, it means the item is a default item and can be placed on the first empty slot
                                {
                                        if (!ValidItem (setItemUI)) continue;
                                        slots[i].Set (setItemUI, icon);
                                        return;
                                }
                        }
                }

                public void ClearSlotItem (ItemUI oldItemUI)
                {
                        if (oldItemUI == null) return;

                        for (int i = 0; i < slots.Count; i++)
                        {
                                if (slots[i].itemUI == oldItemUI)
                                {
                                        slots[i].Clear ( );
                                        return;
                                }
                        }
                }

                public void UpdateSlotTextAmount ( )
                {
                        for (int i = 0; i < slots.Count; i++)
                                slots[i].UpdateTextAmount ( );
                }

                public void ClearSlots ( )
                {
                        for (int i = 0; i < slots.Count; i++)
                        {
                                slots[i].Clear ( );
                        }
                }

                public void ChangeBarIndex (int totalItems, int direction)
                {
                        int size = Mathf.Max (totalItems, count);
                        if (direction > 0)
                                barIndex = barIndex + 1 >= size ? 0 : barIndex + 1;
                        else
                                barIndex = barIndex - 1 < 0 ? size - 1 : barIndex - 1;
                }

                public void Carousel (List<ItemUI> list, Inventory search)
                {
                        if (list.Count > slots.Count) //                                       more items than slots
                        {
                                int slotID = 0;
                                WrapItem (list, search, barIndex, list.Count, ref slotID); //  going right
                                WrapItem (list, search, 0, barIndex, ref slotID); //           from start point, ensure items loop over
                        }
                        else
                        {
                                int itemID = 0;
                                WrapSlots (list, search, barIndex, slots.Count, ref itemID);
                                WrapSlots (list, search, 0, barIndex, ref itemID);
                        }
                }

                private void WrapItem (List<ItemUI> list, Inventory search, int start, int end, ref int slotID)
                {
                        for (int i = start; i < end; i++)
                                if (i < list.Count && slotID < slots.Count)
                                {
                                        if (!ValidItem (list[i])) continue;
                                        slots[slotID].Set (list[i], search.GetItemIcon (list[i].name));
                                        slotID++;
                                }
                }

                private void WrapSlots (List<ItemUI> list, Inventory search, int start, int end, ref int itemID)
                {
                        for (int i = start; i < end; i++)
                                if (itemID < list.Count && i < slots.Count)
                                {
                                        if (!ValidItem (list[itemID])) continue;
                                        slots[i].Set (list[itemID], search.GetItemIcon (list[itemID].name));
                                        itemID++;
                                }
                }

                private bool ValidItem (ItemUI itemUI)
                {
                        return (viewItems == ShowItems.All) || (viewItems == ShowItems.KeyName && itemUI.keyName == keyName);
                }

        }

        public enum ShowItems
        {
                All,
                KeyName
        }

        public enum SlotUseItem
        {
                OnSlotSelection,
                LeaveAsIs
        }
}