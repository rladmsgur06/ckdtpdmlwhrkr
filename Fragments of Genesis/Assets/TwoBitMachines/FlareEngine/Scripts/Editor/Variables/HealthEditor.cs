using TwoBitMachines.Editors;
using UnityEditor;
using UnityEngine;

namespace TwoBitMachines.FlareEngine.Editors
{
        [CustomEditor(typeof(Health) , true)]
        [CanEditMultipleObjects]
        public class HealthEditor : UnityEditor.Editor
        {
                private Health main;
                private SerializedObject parent;
                private GameObject objReference;

                private void OnEnable ()
                {
                        main = target as Health;
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
                                if (FoldOut.Bar(parent , Tint.Blue).Label("Health:   " + parent.String("variableName")).FoldOut())
                                {

                                        bool readOnly = parent.Bool("readImpactOnly");

                                        if (readOnly)
                                        {
                                                FoldOut.Box(2 , FoldOut.boxColor , extraHeight: 3);
                                                if (parent.FieldAndButton("Name ID" , "variableName" , "Reset"))
                                                {
                                                        parent.Get("variableName").stringValue = System.Guid.NewGuid().ToString();
                                                }
                                                parent.FieldToggleAndEnable("Read Impact Only" , "readImpactOnly");

                                                if (FoldOut.FoldOutButton(parent.Get("eventFoldOut")))
                                                {
                                                        Fields.EventFoldOutEffect(parent.Get("onImpact") , parent.Get("impactWE") , parent.Get("impactFoldOut") , "On Impact");
                                                }
                                        }
                                        else
                                        {
                                                FoldOut.Box(8 , FoldOut.boxColor , extraHeight: 3);
                                                if (parent.FieldAndButton("Name ID" , "variableName" , "Reset"))
                                                {
                                                        parent.Get("variableName").stringValue = System.Guid.NewGuid().ToString();
                                                }
                                                parent.Field("Value" , "currentValue");
                                                parent.FieldDouble("Clamp" , "minValue" , "maxValue");
                                                Labels.FieldDoubleText("Min" , "Max" , rightSpacing: 2);
                                                parent.Field("Recovery Time" , "recoveryTime");
                                                parent.Field("World Effect" , "worldEffect");
                                                parent.FieldAndEnable("Has Shield" , "shieldDirection" , "hasShield");
                                                Labels.FieldText("Direction" , rightSpacing: 18);
                                                parent.FieldToggleAndEnable("Broadcast Value" , "broadcastValue");
                                                parent.FieldToggleAndEnable("Read Impact Only" , "readImpactOnly");

                                                if (FoldOut.FoldOutButton(parent.Get("eventFoldOut")))
                                                {
                                                        Fields.EventFoldOut(parent.Get("onMinValue") , parent.Get("minFoldOut") , "On Min Value");
                                                        Fields.EventFoldOut(parent.Get("onMaxValue") , parent.Get("maxFoldOut") , "On Max Value");
                                                        Fields.EventFoldOut(parent.Get("onValueChanged") , parent.Get("changedFoldOut") , "On Value Changed");
                                                        Fields.EventFoldOut(parent.Get("onValueIncreased") , parent.Get("increasedFoldOut") , "On Value Increased");
                                                        Fields.EventFoldOut(parent.Get("onValueDecreased") , parent.Get("decreasedFoldOut") , "On Value Decreased");
                                                        Fields.EventFoldOut(parent.Get("onLoadConditionTrue") , parent.Get("loadFoldOutTrue") , "On Load Condition True");
                                                        Fields.EventFoldOut(parent.Get("onLoadConditionFalse") , parent.Get("loadFoldOutFalse") , "On Load Condition False");
                                                        Fields.EventFoldOut(parent.Get("onShield") , parent.Get("shieldFoldOut") , "On Shield Hit" , execute: parent.Bool("hasShield") , color: FoldOut.boxColorLight);
                                                }
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
