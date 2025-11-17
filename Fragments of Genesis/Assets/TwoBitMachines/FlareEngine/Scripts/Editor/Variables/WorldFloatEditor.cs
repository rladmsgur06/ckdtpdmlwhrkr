using TwoBitMachines.Editors;
using UnityEditor;
using UnityEngine;

namespace TwoBitMachines.FlareEngine.Editors
{
        [CustomEditor(typeof(WorldFloat) , true)]
        [CanEditMultipleObjects]
        public class WorldFloatEditor : UnityEditor.Editor
        {
                private WorldFloat main;
                private SerializedObject parent;

                private void OnEnable ()
                {
                        main = target as WorldFloat;
                        parent = serializedObject;
                        Layout.Initialize();
                }

                public override void OnInspectorGUI ()
                {
                        Layout.Update();
                        Layout.VerticalSpacing(10);
                        parent.Update();
                        {
                                if (FoldOut.Bar(parent , Tint.Blue).Label("Float:   " + parent.String("variableName")).FoldOut())
                                {
                                        FoldOut.Box(4 , FoldOut.boxColor , extraHeight: 3);
                                        if (parent.FieldAndButton("Name ID" , "variableName" , "Reset"))
                                        {
                                                parent.Get("variableName").stringValue = System.Guid.NewGuid().ToString();
                                        }
                                        parent.Field("Value" , "currentValue");
                                        parent.FieldDouble("Clamp" , "minValue" , "maxValue");
                                        Labels.FieldDoubleText("Min" , "Max" , rightSpacing: 2);
                                        parent.FieldToggle("Broadcast Value" , "broadcastValue");

                                        if (FoldOut.FoldOutButton(parent.Get("eventFoldOut")))
                                        {
                                                Color color = Tint.Orange;
                                                Fields.EventFoldOut(parent.Get("onMinValue") , parent.Get("minFoldOut") , "On Min Value" , color: color);
                                                Fields.EventFoldOut(parent.Get("onMaxValue") , parent.Get("maxFoldOut") , "On Max Value" , color: color);
                                                Fields.EventFoldOut(parent.Get("onValueChanged") , parent.Get("changedFoldOut") , "On Value Changed" , color: color);
                                                Fields.EventFoldOut(parent.Get("onValueIncreased") , parent.Get("increasedFoldOut") , "On Value Increased" , color: color);
                                                Fields.EventFoldOut(parent.Get("onValueDecreased") , parent.Get("decreasedFoldOut") , "On Value Decreased" , color: color);
                                                Fields.EventFoldOut(parent.Get("onLoadConditionTrue") , parent.Get("loadFoldOutTrue") , "On Load Condition True" , color: color);
                                                Fields.EventFoldOut(parent.Get("onLoadConditionFalse") , parent.Get("loadFoldOutFalse") , "On Load Condition False" , color: color);
                                                Fields.EventFoldOut(parent.Get("onSceneStart") , parent.Get("sceneStartFoldOut") , "On Scene Start" , color: color);
                                        }

                                        if (FoldOut.Bar(parent , FoldOut.boxColor).Label("Save" , FoldOut.titleColor , false).BRE("save").BBR("Delete"))
                                        {
                                                WorldManagerEditor.DeleteSavedData(main.variableName);
                                        }
                                        if (parent.Bool("save"))
                                        {
                                                FoldOut.Bar(parent , FoldOut.boxColor).Label("Save Manually Only" , FoldOut.titleColor , false).BRE("saveManually");
                                        }

                                        FoldOut.Bar(parent , FoldOut.boxColor).Label("Is Scriptable Object" , FoldOut.titleColor , false).BRE("isScriptableObject");

                                        if (parent.Bool("isScriptableObject"))
                                        {
                                                Layout.VerticalSpacing(1);
                                                if (FoldOut.LargeButton("Create Scriptable Object" , Tint.Blue , Tint.White , Icon.Get("BackgroundLight128x128")))
                                                {
                                                        CreateScriptableObject<WorldFloatSO>(parent , parent.String("variableName") , "Variables");
                                                }
                                                parent.Field("SO Reference" , "soReference");
                                        }
                                }
                                Layout.VerticalSpacing(1);
                        }
                        parent.ApplyModifiedProperties();
                        Layout.VerticalSpacing(10);

                }

                public static void CreateScriptableObject<T> (SerializedObject parent , string name , string folderName) where T : ScriptableObject
                {
                        string path = "Assets/TwoBitMachines/FlareEngine/AssetsFolder/" + folderName + "/" + name + ".asset";
                        T wf = AssetDatabase.LoadAssetAtPath(path , typeof(T)) as T;
                        if (wf != null)
                        {
                                if (parent != null)
                                        parent.Get("soReference").objectReferenceValue = wf;
                                Debug.LogWarning("Scriptable Object with name " + name + " already exists.");
                                return;
                        }

                        T asset = ScriptableObject.CreateInstance<T>();
                        AssetDatabase.CreateAsset(asset , path);
                        AssetDatabase.SaveAssets();
                        if (parent != null)
                                parent.Get("soReference").objectReferenceValue = asset;
                }
        }
}
