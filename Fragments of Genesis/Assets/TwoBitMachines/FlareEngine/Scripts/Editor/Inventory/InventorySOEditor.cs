using TwoBitMachines.Editors;
using UnityEditor;
using UnityEngine;

namespace TwoBitMachines.FlareEngine.Editors
{
        [CustomEditor(typeof(InventorySO), true)]
        public class InventorySOEditor : UnityEditor.Editor
        {
                private InventorySO main;
                private SerializedObject parent;
                public static string inputName = " Name";

                private void OnEnable ()
                {
                        main = target as InventorySO;
                        parent = serializedObject;
                        Layout.Initialize();
                }

                public override void OnInspectorGUI ()
                {
                        Layout.Update();
                        Layout.VerticalSpacing(10);
                        parent.Update();

                        SerializedProperty array = parent.Get("referenceInventory");
                        SerializedProperty arrayDefault = parent.Get("defaultItems");

                        if (Fields.InputAndButtonBox("Create New Item", "Add", Tint.Blue, ref inputName))
                        {
                                CreateScriptableObject(array, inputName);
                                inputName = "Item Name";
                        }
                        if (FoldOut.Bar(parent, Tint.Blue).Label("Default Items", Color.black, bold: false).FoldOut("defaultFoldOut"))
                        {
                                arrayDefault.ArrayBox("Default Item", FoldOut.boxColor, offsetY: -2);
                        }

                        for (int i = 0; i < array.arraySize; i++)
                        {
                                SerializedProperty element = array.Element(i);

                                if (element.objectReferenceValue == null)
                                {
                                        array.DeleteArrayElement(i);
                                        break;
                                }

                                SerializedObject newObj = new SerializedObject(element.objectReferenceValue);
                                newObj.Update();
                                ItemSO itemSO = (ItemSO) element.objectReferenceValue;
                                bool open = newObj.Bool("foldOut");
                                bool deleteAsk = newObj.Bool("deleteAsk");

                                if (
                                        FoldOut.Bar(newObj, Tint.Orange, 0)
                                        .Grip(parent, array, i, color: Tint.WarmWhite)
                                        .Label(itemSO.itemName, Color.white)
                                        .RightButton("remove", "X", toolTip: "Remove From Inventory", execute: open)
                                        .RightButton("deleteData", "Delete", toolTip: "Delete Saved Data", execute: open)
                                        .RightButton("deleteAsk", "Delete", on: Tint.Delete, off: Tint.Delete, toolTip: "Delete Item", execute: open && !deleteAsk)
                                        .RightButton("deleteAsk", "Close", toolTip: "Return", execute: open && deleteAsk)
                                        .RightButton("delete", "Yes", toolTip: "Delete", execute: open && deleteAsk)
                                        .FoldOut())
                                {

                                        if (newObj.ReadBool("deleteData"))
                                        {
                                                WorldManagerEditor.DeleteSavedData(itemSO.itemName);
                                        }
                                        if (newObj.ReadBool("delete") && itemSO != null)
                                        {
                                                string assetPath = AssetDatabase.GetAssetPath(itemSO);
                                                AssetDatabase.DeleteAsset(assetPath);
                                                DestroyImmediate(itemSO, true);
                                                return;
                                        }
                                        if (newObj.ReadBool("remove") && itemSO != null)
                                        {
                                                array.DeleteArrayElement(i);
                                                break;
                                        }

                                        int droppable = (int) newObj.Enum("droppable");

                                        FoldOut.Box(6, FoldOut.boxColor, offsetY: -2);
                                        {
                                                if (newObj.FieldAndButton("Name", "itemName", "Sort", toolTip: "Update Name"))
                                                {
                                                        string assetPath = AssetDatabase.GetAssetPath(itemSO.GetInstanceID());
                                                        AssetDatabase.RenameAsset(assetPath, newObj.String("itemName"));
                                                        AssetDatabase.SaveAssets();
                                                        EditorUtility.SetDirty(itemSO);
                                                }
                                                newObj.Field("Key Name", "keyName");
                                                newObj.Field("Icon", "icon");
                                                newObj.Field("For Inventory", "forInventory");
                                                newObj.Field("Droppable", "droppable", execute: droppable == 0);
                                                newObj.FieldDouble("Droppable", "droppable", "prefab", execute: droppable == 1);
                                                newObj.FieldAndEnable("Consumable", "stackLimit", "consumable");
                                                Labels.FieldText("Stack Limit", rightSpacing: 17);
                                        }
                                        Layout.VerticalSpacing(3);

                                        FoldOut.Box(3, FoldOut.boxColor);
                                        {
                                                newObj.Field("Generic Float", "genericFloat");
                                                newObj.Field("Generic String", "genericString");
                                                newObj.FieldDouble("Cost", "cost", "vendorItem");
                                        }
                                        Layout.VerticalSpacing(5);

                                        if (FoldOut.FoldOutBoxButton(newObj.Get("descriptionFoldOut"), "Description", FoldOut.boxColor))
                                        {
                                                SerializedProperty description = newObj.Get("description");
                                                Rect rect = Layout.CreateRect(width: Layout.longInfoWidth, height: 150, offsetX: -11, offsetY: -1);
                                                description.stringValue = GUI.TextArea(rect, description.stringValue);
                                        }
                                        if (FoldOut.FoldOutBoxButton(newObj.Get("extraInfoFoldOut"), "Extra Info", FoldOut.boxColor))
                                        {
                                                SerializedProperty description = newObj.Get("extraInfo");
                                                Rect rect = Layout.CreateRect(width: Layout.longInfoWidth, height: 150, offsetX: -11, offsetY: -1);
                                                description.stringValue = GUI.TextArea(rect, description.stringValue);
                                        }

                                }
                                newObj.ApplyModifiedProperties();
                        }
                        DragAndDropArea(array);

                        parent.ApplyModifiedProperties();
                        Layout.VerticalSpacing(10);

                }

                private void DragAndDropArea (SerializedProperty array)
                {
                        Color color = Tint.Blue;
                        Rect dropArea = Layout.CreateRect(Layout.longInfoWidth, 27, offsetX: -11, offsetY: 0); //, texture : Icon.Get ("BackgroundLight"), color : Color.clear);
                        Labels.Centered(dropArea, "-- Drag Existing Item Here --", Tint.BoxTwo, fontSize: 13, shiftY: 0);
                        Fields.DropAreaGUI<ItemSO>(dropArea, array);
                }

                public void CreateScriptableObject (SerializedProperty array, string name)
                {
                        string path = "Assets/TwoBitMachines/FlareEngine/AssetsFolder/Inventory/ItemsSO/" + name + ".asset";
                        ItemSO newSO = AssetDatabase.LoadAssetAtPath(path, typeof(ItemSO)) as ItemSO;
                        if (newSO != null)
                        {
                                Debug.LogWarning("Scriptable Object with name " + name + " already exists.");
                                return;
                        }

                        ItemSO asset = ScriptableObject.CreateInstance<ItemSO>();
                        asset.name = name;
                        asset.itemName = name;
                        AssetDatabase.CreateAsset(asset, path);
                        AssetDatabase.SaveAssets();

                        array.arraySize++;
                        array.LastElement().objectReferenceValue = asset;
                }

        }
}
