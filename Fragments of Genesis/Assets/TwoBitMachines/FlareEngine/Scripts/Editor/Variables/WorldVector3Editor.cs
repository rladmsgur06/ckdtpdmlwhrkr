using TwoBitMachines.Editors;
using UnityEditor;
using UnityEngine;

namespace TwoBitMachines.FlareEngine.Editors
{
        [CustomEditor(typeof(WorldVector3) , true)]
        [CanEditMultipleObjects]
        public class WorldVector3Editor : UnityEditor.Editor
        {
                private WorldVector3 main;
                private SerializedObject parent;
                private GameObject objReference;

                private void OnEnable ()
                {
                        main = target as WorldVector3;
                        parent = serializedObject;
                        objReference = main.gameObject;
                        Layout.Initialize();
                }

                public override void OnInspectorGUI ()
                {
                        Layout.Update();
                        Layout.VerticalSpacing(10);
                        parent.Update();
                        {
                                if (FoldOut.Bar(parent , Tint.Blue).Label("Vector3:   " + parent.String("variableName")).FoldOut())
                                {
                                        FoldOut.Box(2 , FoldOut.boxColor , extraHeight: 3);
                                        if (parent.FieldAndButton("Name ID" , "variableName" , "Reset"))
                                        {
                                                parent.Get("variableName").stringValue = System.Guid.NewGuid().ToString();
                                        }
                                        parent.Field("Value" , "currentValue");

                                        bool eventOpen = FoldOut.FoldOutButton(parent.Get("eventFoldOut"));
                                        Fields.EventFoldOut(parent.Get("afterLoad") , parent.Get("loadFoldOut") , "After Load" , execute: eventOpen);

                                        if (FoldOut.Bar(parent , FoldOut.boxColor).Label("Save" , FoldOut.titleColor , false).BRE("save").BBR("Delete"))
                                        {
                                                WorldManagerEditor.DeleteSavedData(main.variableName);
                                        }

                                        FoldOut.Bar(parent , FoldOut.boxColor).Label("Is Scriptable Object" , FoldOut.titleColor , false).BRE("isScriptableObject");
                                        if (parent.Bool("isScriptableObject"))
                                        {
                                                Layout.VerticalSpacing(1);
                                                if (FoldOut.LargeButton("Create Scriptable Object" , Tint.Blue , Tint.White , Icon.Get("BackgroundLight128x128")))
                                                {
                                                        CreateScriptableObject(parent , parent.String("variableName"));
                                                }
                                                parent.Field("SO Reference" , "soReference");
                                        }
                                }
                                Layout.VerticalSpacing(1);
                        }
                        parent.ApplyModifiedProperties();
                        Layout.VerticalSpacing(10);

                }

                public static void CreateScriptableObject (SerializedObject parent , string name)
                {
                        string path = "Assets/TwoBitMachines/FlareEngine/AssetsFolder/Variables/" + name + ".asset";
                        WorldVector3SO wf = AssetDatabase.LoadAssetAtPath(path , typeof(WorldVector3SO)) as WorldVector3SO;
                        if (wf != null)
                        {
                                parent.Get("soReference").objectReferenceValue = wf;
                                Debug.LogWarning("Scriptable Object with name " + name + " already exists.");
                                return;
                        }

                        WorldVector3SO asset = ScriptableObject.CreateInstance<WorldVector3SO>();
                        AssetDatabase.CreateAsset(asset , path);
                        AssetDatabase.SaveAssets();
                        parent.Get("soReference").objectReferenceValue = asset;
                }
        }
}
