using TwoBitMachines.Editors;
using UnityEditor;
using UnityEngine;

namespace TwoBitMachines.FlareEngine.Editors
{
        [CustomEditor(typeof(Inventory))]
        public class InventoryEditor : UnityEditor.Editor
        {
                private Inventory main;
                private SerializedObject parent;

                private void OnEnable ()
                {
                        main = target as Inventory;
                        parent = serializedObject;
                        Layout.Initialize();
                }

                public override void OnInspectorGUI ()
                {
                        Layout.Update();
                        Layout.VerticalSpacing(10);

                        parent.Update();

                        SerializedProperty slot = parent.Get("slot");
                        ShowItems showItems = (ShowItems) slot.Enum("viewItems");

                        FoldOut.Box(3, Tint.BoxTwo);
                        {
                                parent.Field("InventorySO", "inventorySO"); //                                         unique name for saving
                                slot.Field("View Items", "viewItems", execute: showItems != ShowItems.KeyName);
                                slot.FieldDouble("View Items", "viewItems", "keyName", execute: showItems == ShowItems.KeyName);
                                parent.Field("Navigation", "navigation");
                        }
                        Layout.VerticalSpacing(5);

                        if (parent.Enum("navigation") > 0)
                        {
                                FoldOut.Box(2, Tint.BoxTwo, offsetY: -2);
                                parent.FieldDouble("Left, Right", "buttonLeftUp", "buttonRightDown");
                                parent.FieldAndEnable("Auto Item Load", "autoLoadIndex", "autoItemLoad");
                                Labels.FieldText("Slot Index", rightSpacing: 18);
                                Layout.VerticalSpacing(3);
                        }

                        FoldOut.Box(3, Tint.BoxTwo);
                        {
                                parent.FieldAndEnum("Toggle Inventory", "toggle", "pauseType");
                                parent.Field("Use Item", "useItem");
                                parent.Field("Use Item", "slotUseItem");
                        }
                        Layout.VerticalSpacing(5);

                        if (FoldOut.Bar(parent, Tint.BoxTwo).Label("Blocked By", FoldOut.titleColor, false).FoldOut("blockFoldOut"))
                        {
                                SerializedProperty array = parent.Get("block");
                                if (array.arraySize == 0)
                                {
                                        FoldOut.BoxSingle(1, color: FoldOut.boxColor);
                                        {
                                                Fields.ConstructField();
                                                Fields.ConstructString("Block", S.LW);
                                                Fields.ConstructSpace(S.CW - S.B);
                                                if (Fields.ConstructButton("Add"))
                                                {
                                                        array.arraySize++;
                                                }
                                        }
                                        Layout.VerticalSpacing(2);
                                }
                                if (array.arraySize != 0)
                                {
                                        FoldOut.Box(array.arraySize, FoldOut.boxColor);
                                        for (int j = 0; j < array.arraySize; j++)
                                        {
                                                SerializedProperty element = array.Element(j);
                                                Fields.ArrayProperty(array, element, j, "Blocked By");
                                        }
                                        Layout.VerticalSpacing(5);
                                }
                        }

                        if (FoldOut.Bar(parent, Tint.BoxTwo).Label("UI References", FoldOut.titleColor, false).FoldOut("uiFoldOut"))
                        {
                                FoldOut.Box(8, FoldOut.boxColor, offsetY: -2);
                                parent.Field("Item Name", "itemName");
                                parent.Field("Item Selected", "itemSelected");
                                parent.Field("Item Description", "itemDescription");
                                parent.Field("Item Cost", "itemCost");
                                parent.Field("Current Item", "currentItem");
                                parent.Field("Drag Item", "dragItem");
                                parent.Field("Remove Item", "removeItem");
                                parent.FieldDouble("Select Frame", "selectFrame", "selectFrameSpeed");
                                Labels.FieldText("Speed");
                                Layout.VerticalSpacing(3);
                        }

                        if (FoldOut.Bar(parent, Tint.BoxTwo).Label("UI Events", FoldOut.titleColor, false).FoldOut("eventFoldOut"))
                        {
                                Fields.EventFoldOut(parent.Get("onOpen"), parent.Get("onOpenFoldOut"), "On Open", color: FoldOut.boxColor);
                                Fields.EventFoldOut(parent.Get("onClose"), parent.Get("onCloseFoldOut"), "On Close", color: FoldOut.boxColor);
                                Fields.EventFoldOut(parent.Get("onMove"), parent.Get("onMoveFoldOut"), "On Move", color: FoldOut.boxColor);
                                Fields.EventFoldOut(parent.Get("onUseItem"), parent.Get("onUseItemFoldOut"), "On Use Item", color: FoldOut.boxColor);
                                Fields.EventFoldOut(parent.Get("onRemoveItem"), parent.Get("onRemoveItemFoldOut"), "On Remove Item", color: FoldOut.boxColor);
                        }

                        parent.Get("loadSlots").boolValue = FoldOut.LargeButton("Refresh Slots", Tint.Blue, Tint.White, Icon.Get("BackgroundLight"));
                        parent.Get("deleteInventory").boolValue = FoldOut.LargeButton("Delete Saved Data", Tint.Orange, Tint.White, Icon.Get("BackgroundLight"));

                        if (parent.ReadBool("deleteInventory") && main.inventorySO != null)
                        {
                                WorldManagerEditor.DeleteSavedData(main.inventorySO.name);
                        }

                        parent.ApplyModifiedProperties();
                        Layout.VerticalSpacing(10);
                        ReloadSlots();

                }

                public void ReloadSlots () // on draw gizmos only gets called if the scene view is open
                {
                        if (main.transform.childCount != main.childCount || main.loadSlots)
                        {
                                main.slot.ClearList();
                                main.loadSlots = false;
                                main.childCount = main.transform.childCount;
                                InventorySlot[] newSlots = main.transform.GetComponentsInChildren<InventorySlot>(true);

                                for (int i = 0; i < newSlots.Length; i++)
                                {
                                        main.slot.Add(newSlots[i]);
                                }
                                Debug.Log("Slots found: " + main.slot.count);
                        }
                }

        }
}
