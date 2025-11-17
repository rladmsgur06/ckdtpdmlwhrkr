using System.Collections.Generic;
using TwoBitMachines.Editors;
using UnityEditor;
using UnityEngine;

namespace TwoBitMachines.FlareEngine.Editors
{
        [CustomEditor(typeof(WorldBool), true)]
        [CanEditMultipleObjects]
        public class WorldBoolEditor : UnityEditor.Editor
        {
                private WorldBool main;
                private SerializedObject so;
                private GameObject objReference;
                private List<string> sceneNames = new List<string>();

                private void OnEnable ()
                {
                        main = target as WorldBool;
                        so = serializedObject;
                        objReference = main.gameObject;
                        Layout.Initialize();

                        int sceneCount = Util.SceneCount();
                        sceneNames.Clear();
                        for (int i = 0; i < sceneCount; i++)
                        {
                                sceneNames.Add(Util.GetSceneName(i));
                        }
                }

                public override void OnInspectorGUI ()
                {
                        Layout.Update();
                        Layout.VerticalSpacing(10);
                        so.Update();

                        Block.Header(so).Style(Tint.Blue).Label("Bool:   " + so.String("variableName"), bold: true, color: Tint.White).Build();
                        Block.Box(3, FoldOut.boxColor, extraHeight: 3, noGap: true);
                        {
                                bool isSceneName = so.Bool("isSceneName");
                                if (isSceneName)
                                {
                                        so.DropDownList_(sceneNames, "Scene Name", "variableName");
                                }
                                else
                                {
                                        if (so.FieldAndButton_("Name ID", "variableName", "Reset"))
                                        {
                                                so.Get("variableName").stringValue = System.Guid.NewGuid().ToString();
                                        }
                                }
                                so.FieldToggleAndEnable_("Is Scene Name", "isSceneName");
                                so.FieldToggleAndEnable_("Bool Value", "currentValue");
                        }
                        if (Block.ExtraFoldout(so, "eventFoldOut"))
                        {
                                Fields.EventFoldOut(so.Get("onLoadConditionTrue"), so.Get("loadFoldOutTrue"), "On Load Condition True", color: Tint.BoxTwo, space: false);
                                Fields.EventFoldOut(so.Get("onLoadConditionFalse"), so.Get("loadFoldOutFalse"), "On Load Condition False", color: Tint.BoxTwo);
                                Layout.VerticalSpacing();
                        }
;
                        if (Block.Header(so).Style(FoldOut.boxColor).Fold("Save", "saveFoldOut").Button("Delete", hide: false).Enable("save").Build())
                        {
                                Block.Box(1, FoldOut.boxColor, noGap: true);
                                {
                                        so.FieldToggleAndEnable_("Save Manually Only", "saveManually");
                                }
                                GUI.enabled = true;
                        }
                        if (Header.SignalActive("Delete"))
                        {
                                WorldManagerEditor.DeleteSavedData(main.variableName);
                        }
                        if (Block.Header(so).Style(FoldOut.boxColor).Fold("Is Scriptable Object", "objFoldOut").Enable("isScriptableObject").Build())
                        {
                                Block.Box(1, FoldOut.boxColor, noGap: true);
                                {
                                        so.Field_("SO Reference", "soReference");
                                }
                                if (FoldOut.LargeButton("Create Scriptable Object", Tint.Blue, Tint.White, Icon.Get("BackgroundLight128x128")))
                                {
                                        CreateScriptableObject(so, so.String("variableName"));
                                }
                                GUI.enabled = true;
                        }

                        so.ApplyModifiedProperties();
                        Layout.VerticalSpacing(10);

                }

                public static void CreateScriptableObject (SerializedObject parent, string name)
                {
                        string path = "Assets/TwoBitMachines/FlareEngine/AssetsFolder/Variables/" + name + ".asset";
                        WorldBoolSO variable = AssetDatabase.LoadAssetAtPath(path, typeof(WorldBoolSO)) as WorldBoolSO;
                        if (variable != null)
                        {
                                parent.Get("soReference").objectReferenceValue = variable;
                                Debug.LogWarning("Scriptable Object with name " + name + " already exists.");
                                return;
                        }

                        WorldBoolSO asset = ScriptableObject.CreateInstance<WorldBoolSO>();
                        AssetDatabase.CreateAsset(asset, path);
                        AssetDatabase.SaveAssets();
                        parent.Get("soReference").objectReferenceValue = asset;
                }
        }
}
