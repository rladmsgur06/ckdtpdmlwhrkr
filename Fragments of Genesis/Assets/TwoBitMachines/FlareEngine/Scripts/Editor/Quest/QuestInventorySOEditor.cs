using TwoBitMachines.Editors;
using UnityEditor;
using UnityEngine;

namespace TwoBitMachines.FlareEngine.Editors
{
        [CustomEditor(typeof(QuestInventorySO), true)]
        public class QuestInventorySOEditor : UnityEditor.Editor
        {
                private QuestInventorySO main;
                private SerializedObject parent;
                public static string inputName = "Quest Name";

                private void OnEnable ()
                {
                        main = target as QuestInventorySO;
                        parent = serializedObject;
                        Layout.Initialize();
                }

                public override void OnInspectorGUI ()
                {
                        Layout.Update();
                        Layout.VerticalSpacing(10);
                        parent.Update();

                        SerializedProperty array = parent.Get("questSO");

                        if (Fields.InputAndButtonBox("Create New Quest", "Add", Tint.Blue, ref inputName))
                        {
                                CreateScriptableObject(array, inputName);
                                inputName = "Quest Name";
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
                                QuestSO questSO = (QuestSO) element.objectReferenceValue;
                                bool open = newObj.Bool("foldOut");
                                bool deleteAsk = newObj.Bool("deleteAsk");

                                if (
                                        FoldOut.Bar(newObj, Tint.Orange, 0)
                                        .Grip(parent, array, i, color: Tint.WarmWhite)
                                        .Label(questSO.title, Color.white)
                                        .RightButton(toolTip: "Add Reward", execute: open)
                                        .RightButton("remove", "X", toolTip: "Remove From Inventory", execute: open)
                                        .RightButton("deleteData", "Delete", toolTip: "Delete Saved Data", execute: open)
                                        .RightButton("deleteAsk", "Delete", on: Tint.Delete, off: Tint.Delete, toolTip: "Delete Quest", execute: open && !deleteAsk)
                                        .RightButton("deleteAsk", "Close", toolTip: "Return", execute: open && deleteAsk)
                                        .RightButton("delete", "Yes", toolTip: "Delete", execute: open && deleteAsk)
                                        .FoldOut())
                                {

                                        if (newObj.ReadBool("deleteData"))
                                        {
                                                WorldManagerEditor.DeleteSavedData(questSO.title);
                                        }
                                        if (newObj.ReadBool("delete") && questSO != null)
                                        {
                                                string assetPath = AssetDatabase.GetAssetPath(questSO);
                                                AssetDatabase.DeleteAsset(assetPath);
                                                DestroyImmediate(questSO, true);
                                                return;
                                        }
                                        if (newObj.ReadBool("remove") && questSO != null)
                                        {
                                                array.DeleteArrayElement(i);
                                                break;
                                        }

                                        FoldOut.Box(3, FoldOut.boxColor, offsetY: -2);
                                        {
                                                if (newObj.FieldAndButton("Title", "title", "Sort", toolTip: "Update Name"))
                                                {
                                                        string assetPath = AssetDatabase.GetAssetPath(questSO.GetInstanceID());
                                                        AssetDatabase.RenameAsset(assetPath, newObj.String("title"));
                                                        AssetDatabase.SaveAssets();
                                                        EditorUtility.SetDirty(questSO);
                                                }
                                                newObj.Field("Icon", "icon");
                                                newObj.Field("Goal", "goal");
                                        }
                                        Layout.VerticalSpacing(3);

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

                                        SerializedProperty rewards = newObj.Get("rewards");

                                        if (newObj.ReadBool("add"))
                                        {
                                                rewards.arraySize++;
                                                rewards.LastElement().Get("name").stringValue = "Reward Name";
                                        }

                                        for (int r = 0; r < rewards.arraySize; r++)
                                        {
                                                SerializedProperty reward = rewards.Element(r);

                                                FoldOut.Box(3, Tint.BoxTwo);
                                                {
                                                        if (reward.FieldAndButton("Reward", "reward", "Delete"))
                                                        {
                                                                rewards.DeleteArrayElement(r);
                                                                break;
                                                        }
                                                        reward.Field("Name", "name");
                                                        reward.Field("Reward Icon", "icon");
                                                }
                                                Layout.VerticalSpacing(5);

                                                FoldOut.Box(2, Tint.BoxTwo, offsetY: -2);
                                                {
                                                        reward.Field("For World Float", "worldFloat");
                                                        reward.FieldDouble("For Inventory", "inventorySO", "itemSO");
                                                }
                                                Layout.VerticalSpacing(3);

                                                // reward.Field ("Description", "description");

                                                // if (FoldOut.FoldOutButton (reward.Get ("descriptionFoldOut"), "Description"))
                                                // {
                                                //         SerializedProperty descriptionR = reward.Get ("description");
                                                //         Rect rectR = Layout.CreateRect (width: Layout.longInfoWidth, height: 60, offsetX: -11, offsetY: -1);
                                                //         descriptionR.stringValue = GUI.TextArea (rectR, descriptionR.stringValue);
                                                // }
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
                        Labels.Centered(dropArea, "-- Drag Existing Quest Here --", Tint.BoxTwo, fontSize: 13, shiftY: 0);
                        Fields.DropAreaGUI<QuestSO>(dropArea, array);
                }

                public void CreateScriptableObject (SerializedProperty array, string name)
                {
                        string path = "Assets/TwoBitMachines/FlareEngine/AssetsFolder/Quests/QuestSO/" + name + ".asset";
                        QuestSO newSO = AssetDatabase.LoadAssetAtPath(path, typeof(QuestSO)) as QuestSO;
                        if (newSO != null)
                        {
                                Debug.LogWarning("Scriptable Object with name " + name + " already exists.");
                                return;
                        }

                        QuestSO asset = ScriptableObject.CreateInstance<QuestSO>();
                        asset.name = name;
                        asset.title = name;
                        AssetDatabase.CreateAsset(asset, path);
                        AssetDatabase.SaveAssets();

                        array.arraySize++;
                        array.LastElement().objectReferenceValue = asset;
                }

        }
}
