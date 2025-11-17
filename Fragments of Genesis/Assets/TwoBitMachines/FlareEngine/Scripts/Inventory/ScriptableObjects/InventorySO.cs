using System.Collections.Generic;
using TwoBitMachines.FlareEngine;
using UnityEngine;

namespace TwoBitMachines
{
        [CreateAssetMenu(menuName = "FlareEngine/InventorySO")]
        public class InventorySO : JournalInventory
        {
                [SerializeField] public List<ItemSO> referenceInventory = new List<ItemSO>();
                [SerializeField] public List<ItemSO> defaultItems = new List<ItemSO>();

                [System.NonSerialized] public PickUpItems pickUpItem = null; // This is the character
                [System.NonSerialized] public InventoryUI inventory = new InventoryUI();
                [System.NonSerialized] private List<Inventory> inventories = new List<Inventory>();
                [System.NonSerialized] public Dictionary<string, ItemSO> search = new Dictionary<string, ItemSO>();

                public List<ItemUI> list => inventory.itemList;
                public static List<InventorySO> inventorySOList = new List<InventorySO>();

                #region ▀▄▀▄▀▄ Editor Variables ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField, HideInInspector] private bool defaultFoldOut;
                [SerializeField, HideInInspector] private int signalIndex;
                [SerializeField, HideInInspector] private bool active;
#endif
                #endregion

                public static void SaveData ()
                {
                        for (int i = 0; i < inventorySOList.Count; i++)
                        {
                                inventorySOList[i].Save();
                        }
                }

                public static void ClearTempChildren ()
                {
                        inventorySOList.Clear();
                }

                public void Save ()
                {
                        Storage.Save(inventory, WorldManager.saveFolder, this.name);
                }

                public override List<ItemSO> ItemList ()
                {
                        return referenceInventory;
                }

                public void Initialize (PickUpItems pickUpItem)
                {
                        inventorySOList.Add(this);
                        this.pickUpItem = pickUpItem;
                        search.Clear();

                        //* Initialize search method
                        for (int i = 0; i < referenceInventory.Count; i++)
                        {
                                if (referenceInventory[i] != null)
                                {
                                        search.Add(referenceInventory[i].itemName, referenceInventory[i]);
                                }
                        }

                        //* Retrieve saved data
                        InventoryUI savedInventory = Storage.Load<InventoryUI>(inventory, WorldManager.saveFolder, this.name);
                        inventory = savedInventory == null ? inventory : savedInventory;

                        for (int i = 0; i < defaultItems.Count; i++)
                        {
                                if (defaultItems[i] != null && !inventory.ContainsItem(defaultItems[i].itemName))
                                {
                                        AddToInventory(defaultItems[i], 0);
                                }
                        }
                }

                public List<ItemUI> ItemList (ShowItems viewItems, string keyName)
                {
                        return viewItems == ShowItems.All ? list : inventory.GetKeyNameList(keyName);
                }

                public void AddListener (Inventory inventory)
                {
                        if (!inventories.Contains(inventory))
                        {
                                inventories.Add(inventory);
                        }
                }

                public void ClearInventories ()
                {
                        inventories.Clear();
                }

                public void ClearItemSlotID ()
                {
                        for (int i = 0; i < inventory.itemList.Count; i++)
                        {
                                inventory.itemList[i].slotID = -1;
                        }
                }

                public bool ContainsItem (string itemName)
                {
                        return inventory.ContainsItem(itemName);
                }

                public void RemoveItem (string itemName)
                {
                        for (int i = 0; i < inventory.itemList.Count; i++)
                        {
                                if (inventory.itemList[i].name == itemName)
                                {
                                        RemoveFromInventory(inventory.itemList[i]);
                                        return;
                                }
                        }
                }

                public bool AddToInventory (ItemSO itemSO, int droppedValue)
                {
                        if (itemSO != null && inventory.AddItem(itemSO, droppedValue, out ItemUI newItemUI))
                        {
                                SlotsUpdate_NewItem(newItemUI, itemSO.icon);
                                return true;
                        }
                        return false;
                }

                public bool AddToInventory (string itemName)
                {
                        if (search.TryGetValue(itemName, out ItemSO itemSO) && inventory.AddItem(itemSO, 0, out ItemUI newItemUI))
                        {
                                SlotsUpdate_NewItem(newItemUI, itemSO.icon);
                                return true;
                        }
                        return false;
                }

                public void RemoveFromInventory (ItemUI removeItemUI)
                {
                        if (removeItemUI == null || pickUpItem == null)
                                return;

                        if (search.ContainsKey(removeItemUI.name))
                        {
                                ItemSO itemSO = search[removeItemUI.name];
                                if (itemSO != null && itemSO.isDroppable)
                                {
                                        itemSO.DropItem(pickUpItem.transform, removeItemUI.amount);
                                }
                                inventory.RemoveItem(removeItemUI);
                                SlotsUpdate_ClearItem(removeItemUI);
                                return;

                        }
                }

                public void DeselectItem (ItemUI useItemUI)
                {
                        if (useItemUI != null && pickUpItem != null && search.ContainsKey(useItemUI.name))
                        {
                                search[useItemUI.name]?.DeselectItem(pickUpItem);
                        }
                }

                public bool UseSlotItem (ItemUI useItemUI, out bool used, bool toggleTool = true)
                {
                        used = false;
                        if (useItemUI != null && pickUpItem != null && search.ContainsKey(useItemUI.name))
                        {
                                ItemSO itemSO = search[useItemUI.name];
                                if (itemSO != null && itemSO.UseItem(pickUpItem, toggleTool))
                                {
                                        used = true;
                                        if ((!pickUpItem.isVendor && itemSO.consumable) || (pickUpItem.isVendor && itemSO.vendorItem == VendorItemRemove.RemoveItemAfterSell))
                                        {
                                                if (--useItemUI.amount <= 0)
                                                {
                                                        inventory.RemoveItem(useItemUI);
                                                        SlotsUpdate_ClearItem(useItemUI);
                                                        return true;
                                                }
                                                else
                                                {
                                                        SlotsUpdate_Text();
                                                }
                                        }
                                }
                        }
                        return false;
                }

                private void SlotsUpdate_NewItem (ItemUI newItemUI, Sprite icon)
                {
                        for (int i = 0; i < inventories.Count; i++)
                        {
                                inventories[i].slot.UpdateSlotTextAmount(); //        needs to be called in case it was a stacked consumable
                                inventories[i].slot.SetSlotItem(newItemUI, icon); //   will check for null in case new item was just a text update
                        }
                }

                public void SlotsUpdate_ClearItem (ItemUI removeItemUI)
                {
                        for (int i = 0; i < inventories.Count; i++)
                        {
                                inventories[i].slot.ClearSlotItem(removeItemUI);
                        }
                }

                public void SlotsUpdate_Text ()
                {
                        for (int i = 0; i < inventories.Count; i++)
                        {
                                inventories[i].slot.UpdateSlotTextAmount();
                        }
                }

                public void DeleteSavedData ()
                {
                        for (int i = 0; i < inventories.Count; i++)
                        {
                                inventories[i].slot.ClearSlots();
                        }
                        inventory.itemList.Clear();
                        InventoryUI.temp.Clear();
                        Save();
                }

        }

        [System.Serializable]
        public class InventoryUI
        {
                [SerializeField] public List<ItemUI> itemList = new List<ItemUI>();
                [System.NonSerialized] public static List<ItemUI> temp = new List<ItemUI>();
                [SerializeField] public int barIndex = 0;

                public bool ContainsItem (string itemName)
                {
                        for (int i = 0; i < itemList.Count; i++)
                        {
                                if (itemList[i].name == itemName)
                                {
                                        return true;
                                }
                        }
                        return false;
                }

                public bool AddItem (ItemSO itemSO, int droppedValue, out ItemUI newItemUI)
                {
                        newItemUI = null;
                        int count = droppedValue > 0 ? droppedValue : 1;
                        for (int i = 0; i < itemList.Count; i++)
                        {
                                if (itemList[i].name == itemSO.itemName)
                                {
                                        if (itemSO.consumable)
                                        {
                                                if ((itemList[i].amount + count) <= itemSO.stackLimit)
                                                {
                                                        itemList[i].amount += count; //                            increase amount of existing item. If full, create another item
                                                        return true;
                                                }
                                        }
                                        else
                                        {
                                                return false; //                                                   already exists and is not consumable, do not add.
                                        }
                                }
                        }
                        newItemUI = new ItemUI(itemSO.itemName, itemSO.keyName, itemSO.isDroppable, count); //    new item added
                        itemList.Add(newItemUI);
                        return true;
                }

                public void RemoveItem (ItemUI removeItem)
                {
                        for (int i = 0; i < itemList.Count; i++)
                        {
                                if (itemList[i] == removeItem)
                                {
                                        itemList.RemoveAt(i);
                                        return;
                                }
                        }
                }

                public void RemoveItem (string removeItem)
                {
                        for (int i = 0; i < itemList.Count; i++)
                        {
                                if (itemList[i].name == removeItem)
                                {
                                        itemList.RemoveAt(i);
                                        return;
                                }
                        }
                }

                public List<ItemUI> GetKeyNameList (string keyName)
                {
                        temp.Clear();
                        for (int i = 0; i < itemList.Count; i++)
                        {
                                if (itemList[i].keyName == keyName)
                                {
                                        temp.Add(itemList[i]);
                                }
                        }
                        return temp;
                }
        }

        [System.Serializable]
        public class ItemUI
        {
                [SerializeField] public string name;
                [SerializeField] public string keyName;
                [SerializeField] public bool droppable;
                [SerializeField] public int amount = 0;
                [SerializeField] public int slotID = -1;

                public ItemUI (string name, string keyName, bool canDrop, int amount)
                {
                        this.name = name;
                        this.keyName = keyName;
                        this.droppable = canDrop;
                        this.amount = amount;
                }
        }

}
