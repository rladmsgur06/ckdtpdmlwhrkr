using TwoBitMachines.Editors;
using UnityEditor;
using UnityEngine;

namespace TwoBitMachines.FlareEngine.Editors
{
        [CustomEditor(typeof(ItemSO))]
        public class ItemSOEditor : UnityEditor.Editor
        {
                private ItemSO main;
                private SerializedObject parent;
                public static string inputName = " Name";

                private void OnEnable ()
                {
                        main = target as ItemSO;
                        parent = serializedObject;
                        Layout.Initialize();
                }

                public override void OnInspectorGUI ()
                {
                        Layout.Update();
                        Layout.VerticalSpacing(10);
                        parent.Update();
                        ItemSO itemSO = main;
                        bool open = parent.Bool("foldOut");

                        FoldOut.Bar(parent, Tint.Orange)
                                .Label(itemSO.itemName, Color.white)
                                .RightButton("deleteData", "Delete", toolTip: "Delete Saved Data", execute: open);

                        if (parent.ReadBool("deleteData"))
                        {
                                WorldManagerEditor.DeleteSavedData(itemSO.itemName);
                        }
                        if (parent.ReadBool("delete") && itemSO != null)
                        {
                                string assetPath = AssetDatabase.GetAssetPath(itemSO);
                                AssetDatabase.DeleteAsset(assetPath);
                                DestroyImmediate(itemSO, true);
                                return;
                        }

                        int droppable = (int) parent.Enum("droppable");

                        FoldOut.Box(6, FoldOut.boxColor, offsetY: -2);
                        {
                                if (parent.FieldAndButton("Name", "itemName", "Sort", toolTip: "Update Name"))
                                {
                                        string assetPath = AssetDatabase.GetAssetPath(itemSO.GetInstanceID());
                                        AssetDatabase.RenameAsset(assetPath, parent.String("itemName"));
                                        AssetDatabase.SaveAssets();
                                        EditorUtility.SetDirty(itemSO);
                                }
                                parent.Field("Key Name", "keyName");
                                parent.Field("Icon", "icon");
                                parent.Field("For Inventory", "forInventory");
                                parent.Field("Droppable", "droppable", execute: droppable == 0);
                                parent.FieldDouble("Droppable", "droppable", "prefab", execute: droppable == 1);
                                parent.FieldAndEnable("Consumable", "stackLimit", "consumable");
                                Labels.FieldText("Stack Limit", rightSpacing: 17);
                        }
                        Layout.VerticalSpacing(3);

                        FoldOut.Box(3, FoldOut.boxColor);
                        {
                                parent.Field("Generic Float", "genericFloat");
                                parent.Field("Generic String", "genericString");
                                parent.FieldDouble("Cost", "cost", "vendorItem");
                        }
                        Layout.VerticalSpacing(5);

                        if (FoldOut.FoldOutBoxButton(parent.Get("descriptionFoldOut"), "Description", FoldOut.boxColor))
                        {
                                SerializedProperty description = parent.Get("description");
                                Rect rect = Layout.CreateRect(width: Layout.longInfoWidth, height: 150, offsetX: -11, offsetY: -1);
                                description.stringValue = GUI.TextArea(rect, description.stringValue);
                        }
                        if (FoldOut.FoldOutBoxButton(parent.Get("extraInfoFoldOut"), "Extra Info", FoldOut.boxColor))
                        {
                                SerializedProperty description = parent.Get("extraInfo");
                                Rect rect = Layout.CreateRect(width: Layout.longInfoWidth, height: 150, offsetX: -11, offsetY: -1);
                                description.stringValue = GUI.TextArea(rect, description.stringValue);
                        }

                        parent.ApplyModifiedProperties();
                }
        }
}
