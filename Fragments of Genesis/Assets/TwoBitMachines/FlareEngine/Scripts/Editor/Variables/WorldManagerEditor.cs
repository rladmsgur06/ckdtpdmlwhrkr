using TwoBitMachines.Editors;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TwoBitMachines.FlareEngine.Editors
{
        [CustomEditor(typeof(WorldManager))]
        public class WorldManagerEditor : UnityEditor.Editor
        {
                [System.NonSerialized] private WorldManager main;
                [System.NonSerialized] private SerializedObject parent;
                [System.NonSerialized] private GameDifficultySave difficulty = new GameDifficultySave();// don't delete and Delete All
                public static SaveOptions save = new SaveOptions();
                public static SaveString saveString = new SaveString();

                private void OnEnable ()
                {
                        main = target as WorldManager;
                        parent = serializedObject;
                        Layout.Initialize();
                        main.InitializeSaveState();

                        main.worldEvents.Clear();
                        string path = "Assets/TwoBitMachines/FlareEngine/AssetsFolder/Events";
                        string[] guids = AssetDatabase.FindAssets("t:scriptableObject", new[] { path });
                        for (int i = 0; i < guids.Length; i++)
                        {
                                main.worldEvents.Add(AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guids[i]), typeof(WorldEventSO)) as WorldEventSO);
                        }

                        path = UserFolderPaths.Path(UserFolder.WorldEvents);
                        if (!string.IsNullOrEmpty(path))
                        {
                                guids = AssetDatabase.FindAssets("t:scriptableObject", new[] { path });
                                for (int i = 0; i < guids.Length; i++)
                                {
                                        main.worldEvents.Add(AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guids[i]), typeof(WorldEventSO)) as WorldEventSO);
                                }
                        }

                        CreateTag("NoClimb");
                        CreateTag("Friction");
                        CreateTag("Block");
                        CreateTag("Beam");
                        CreateTag("UIControl");
                        CreateTag("Edge2DUpOnly");
                        CreateTag("Edge2DDownOnly");
                        CreateTag("RotatedWall");

                        CreateLayer("World");
                        CreateLayer("Platform");
                        CreateLayer("Player");
                        CreateLayer("Enemy");
                        CreateLayer("Hide");

                        WorldManager.get = main;
                }

                public override void OnInspectorGUI ()
                {
                        Layout.Update();
                        Layout.VerticalSpacing(10);
                        parent.Update();

                        Color color = parent.Bool("isSaveMenu") ? Tint.PurpleDark : FoldOut.boxColor;

                        FoldOut.Box(3, color, extraHeight: 3);
                        {
                                parent.Get("save").Field("Game Name", "gameName");
                                parent.Field("Game Pause", "pause");
                                parent.FieldToggleAndEnable("View Debug", "viewDebug");
                        }

                        if (FoldOut.FoldOutButton(parent.Get("foldOut")))
                        {
                                Fields.EventFoldOut(parent.Get("onAwake"), parent.Get("onAwakeFoldOut"), "On Awake");
                                Fields.EventFoldOut(parent.Get("onStart"), parent.Get("onStartFoldOut"), "On Start");
                                Fields.EventFoldOut(parent.Get("onPause"), parent.Get("onPauseFoldOut"), "On Pause");
                                Fields.EventFoldOut(parent.Get("onUnpause"), parent.Get("onUnpauseFoldOut"), "On Unpause");
                                Fields.EventFoldOut(parent.Get("onResetAll"), parent.Get("onResetAllFoldOut"), "On Reset All");
                        }

                        WorldEvents();
                        SaveGame(out bool save);
                        DeleteAllSavedData();

                        parent.ApplyModifiedProperties();
                        UpdateSlotsMenu(save);
                }

                public void DeleteAllSavedData ()
                {
                        if (FoldOut.Bar(parent, Tint.Orange).Label("Saved Data").FoldOut("deleteFoldOut"))
                        {
                                if (FoldOut.LargeButton("Delete All Saved Data", Tint.Delete, Tint.White, Icon.Get("BackgroundLight"), minusWidth: 24)) // Bar (parent, FoldOut.boxColor).L ("Save", FoldOut.titleColor, false).BRE ("save").BBR ("Delete"))
                                {
                                        FlareEngine.SaveOptions.Load(ref save);
                                        string saveFolder = save.RetrieveSaveFolder();

                                        difficulty = null;
                                        difficulty = Storage.Load<GameDifficultySave>(difficulty, saveFolder, "GameDifficulty"); // don't delete this

                                        if (save.gameName != null)
                                                Storage.DeleteAll(save.gameName);
                                        if (saveFolder != null)
                                                save.DeleteAllSlotsData();
                                        if (difficulty != null)
                                        {
                                                Storage.Save(difficulty, saveFolder, "GameDifficulty");
                                        }
                                        Debug.Log("Deleted All Saved Data");
                                }
                                if (FoldOut.CornerButtonLR(Tint.BoxTwo, icon: "Folder", tooltip: "Save Folder"))
                                {
                                        EditorUtility.RevealInFinder(Storage.path);
                                }
                        }

                        if (Application.isPlaying && FoldOut.LargeButton("Reset Game", Tint.Orange, Tint.White, Icon.Get("BackgroundLight"))) // Bar (parent, FoldOut.boxColor).L ("Save", FoldOut.titleColor, false).BRE ("save").BBR ("Delete"))
                        {
                                main?.ResetAll();
                        }
                }

                public static void DeleteSavedData (string dataName)
                {
                        FlareEngine.SaveOptions.Load(ref save);
                        string saveFolder = save.RetrieveSaveFolder();

                        if (saveFolder == null || saveFolder == "")
                        {
                                Debug.Log("Folder name does not exist");
                                return;
                        }
                        if (Storage.Delete(saveFolder, dataName))
                        {
                                Debug.Log("Deleted Saved Data");
                        }
                }

                private void SaveGame (out bool save)
                {
                        save = false;
                        if (FoldOut.Bar(parent).Label("Save Options").FoldOut("saveSlotsFoldOut"))
                        {
                                SerializedProperty saveOptions = parent.Get("save");

                                FoldOut.Box(3, FoldOut.boxColor);
                                {
                                        parent.Field("Level Number", "levelNumber");
                                        parent.FieldToggleAndEnable("Is Save Menu", "isSaveMenu");
                                        saveOptions.FieldToggleAndEnable("Encrypt Save", "navigate");
                                }
                                Layout.VerticalSpacing(5);

                                if (Application.isPlaying)
                                {
                                        FoldOut.Box(1, FoldOut.boxColor);
                                        {
                                                Labels.Label("Current Slot: " + main.save.currentSlot);
                                        }
                                        Layout.VerticalSpacing(5);
                                        save = false;
                                        return;
                                }

                                SerializedProperty array = saveOptions.Get("slot");
                                int currentSlot = saveOptions.Get("currentSlot").intValue;
                                bool deleteAsk = saveOptions.Bool("delete");

                                if (deleteAsk)
                                {
                                        FoldOut.Box(1, FoldOut.boxColor);
                                        string slotTitle = "(" + main.save.slot.Count.ToString() + ")" + " Current Slot:";
                                        saveOptions.DropDownListIntsAndDoubleButton(slotTitle, main.save.slot.Count, "currentSlot", "Yes", "Close", out bool yes, out bool no);
                                        Layout.VerticalSpacing(5);

                                        if (no)
                                        {
                                                saveOptions.SetFalse("delete");
                                        }
                                        if (yes && currentSlot >= 0 && currentSlot < array.arraySize)
                                        {
                                                save = true;
                                                saveOptions.Get("currentSlot").intValue = 0;
                                                array.DeleteArrayElement(currentSlot); // will need to delete folder from files
                                                if (array.arraySize > 0)
                                                {
                                                        saveOptions.SetFalse("delete");
                                                        saveOptions.Get("currentSlot").intValue = array.arraySize - 1;
                                                }
                                        }
                                }
                                else
                                {
                                        FoldOut.Box(1, FoldOut.boxColor);
                                        string slotTitle = "(" + main.save.slot.Count.ToString() + ")" + " Current Slot:";
                                        saveOptions.DropDownListIntsAndDoubleButton(slotTitle, main.save.slot.Count, "currentSlot", "Add", "Delete", out bool add, out bool delete);
                                        Layout.VerticalSpacing(5);

                                        if (delete)
                                        {
                                                saveOptions.SetTrue("delete");
                                        }
                                        if (add)
                                        {
                                                save = true;
                                                array.arraySize++;
                                                SerializedProperty element = array.LastElement();
                                                element.Get("initialized").boolValue = false;
                                                element.Get("totalTime").floatValue = 0;
                                                element.Get("level").floatValue = 0;
                                                saveOptions.Get("currentSlot").intValue = array.arraySize - 1;
                                        }
                                }

                        }
                }

                private void UpdateSlotsMenu (bool save)
                {
                        if (save)
                        {
                                main.save.Save();
                                WorldManager[] wm = main.gameObject.GetComponents<WorldManager>();
                                for (int i = 0; i < wm.Length; i++) // in case more than one world manager exists on object, during testing
                                {
                                        wm[i].save = main.save;
                                }
                        }
                }

                private void WorldEvents ()
                {
                        if (FoldOut.Bar(parent).Label("World Events").FoldOut("worldFoldOut"))
                        {
                                FoldOut.Box(1, FoldOut.boxColor);
                                if (parent.FieldAndButton("Add World Event", "createSignal", "Add"))
                                {
                                        CreateWorldEvent(parent.String("createSignal"));
                                }
                                Layout.VerticalSpacing(5);

                                SerializedProperty array = parent.Get("worldEvents");
                                for (int i = 0; i < array.arraySize; i++)
                                {
                                        WorldEventSO worldEvent = array.Element(i).objectReferenceValue as WorldEventSO;
                                        if (worldEvent != null)
                                        {
                                                FoldOut.Box(1, FoldOut.boxColor);
                                                if (Labels.LabelDisplayAndButton(worldEvent.eventName, "Delete"))
                                                {
                                                        if (DeleteScriptableObject(worldEvent))
                                                        {
                                                                break;
                                                        }
                                                }
                                                Layout.VerticalSpacing(5);
                                        }
                                }
                        }
                }

                private void CreateWorldEvent (string name)
                {
                        if (name == "")
                        {
                                Debug.LogWarning("Scriptable Object must have a unique name");
                                return;
                        }

                        string path = UserFolderPaths.Path(UserFolder.WorldEvents);
                        if (string.IsNullOrEmpty(path))
                        {
                                path = "Assets/TwoBitMachines/FlareEngine/AssetsFolder/Events/" + name + ".asset";
                        }
                        else
                        {
                                path += "/" + name + ".asset";
                        }

                        WorldEventSO worldEvent = AssetDatabase.LoadAssetAtPath(path, typeof(WorldEventSO)) as WorldEventSO;
                        if (worldEvent != null)
                        {
                                Debug.LogWarning("Scriptable Object with name " + name + " already exists.");
                                return;
                        }

                        WorldEventSO asset = ScriptableObject.CreateInstance<WorldEventSO>();
                        asset.eventName = name;
                        AssetDatabase.CreateAsset(asset, path);
                        AssetDatabase.SaveAssets();
                        SerializedProperty array = parent.Get("worldEvents");
                        array.arraySize++;
                        array.LastElement().objectReferenceValue = asset;
                }

                public static bool DeleteScriptableObject (ScriptableObject scriptableObject)
                {
                        if (scriptableObject == null)
                        {
                                return false;
                        }

                        string assetPath = AssetDatabase.GetAssetPath(scriptableObject);
                        if (string.IsNullOrEmpty(assetPath))
                        {
                                Debug.LogWarning("Could not find asset path for ScriptableObject.");
                                return false;
                        }

                        AssetDatabase.DeleteAsset(assetPath);
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                        return true;
                }

                private void CreateTag (string newTag)
                {
                        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
                        SerializedProperty tagsProp = tagManager.FindProperty("tags");
                        bool found = false;
                        for (int i = 0; i < tagsProp.arraySize; i++)
                        {
                                SerializedProperty t = tagsProp.GetArrayElementAtIndex(i);
                                if (t.stringValue.Equals(newTag))
                                { found = true; break; }
                        }
                        if (!found)
                        {
                                tagsProp.InsertArrayElementAtIndex(0);
                                SerializedProperty n = tagsProp.GetArrayElementAtIndex(0);
                                n.stringValue = newTag;
                        }
                        tagManager.ApplyModifiedProperties();
                }

                private void CreateLayer (string layerName)
                {
                        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
                        SerializedProperty layersProp = tagManager.FindProperty("layers");
                        if (!PropertyExists(layersProp, 0, 31, layerName))
                        {
                                SerializedProperty sp;
                                for (int i = 8, j = 31; i < j; i++)
                                {
                                        sp = layersProp.GetArrayElementAtIndex(i);
                                        if (sp.stringValue == "")
                                        {
                                                sp.stringValue = layerName;
                                                tagManager.ApplyModifiedProperties();
                                                return;
                                        }
                                }
                        }
                }

                private bool PropertyExists (SerializedProperty property, int start, int end, string value)
                {
                        for (int i = start; i < end; i++)
                        {
                                SerializedProperty t = property.GetArrayElementAtIndex(i);
                                if (t.stringValue.Equals(value))
                                {
                                        return true;
                                }
                        }
                        return false;
                }

        }
}
