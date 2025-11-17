using TwoBitMachines.Editors;
using UnityEditor;

namespace TwoBitMachines.FlareEngine.Editors
{
        [CustomEditor(typeof(WorldFloatHUD))]
        public class WorldFloatHUDEditor : UnityEditor.Editor
        {
                public WorldFloatHUD main;
                public SerializedObject parent;

                private void OnEnable ()
                {
                        main = target as WorldFloatHUD;
                        parent = serializedObject;
                        Layout.Initialize();

                        if (main.discreteSaveKey == "")
                        {
                                main.discreteSaveKey = System.Guid.NewGuid().ToString();
                        }
                }

                public override void OnInspectorGUI ()
                {
                        Layout.Update();
                        parent.Update();
                        {
                                FoldOut.Box(6 , Tint.Blue);
                                {
                                        parent.Field("Type" , "type");
                                        parent.Field("World Float" , "worldFloat");
                                        parent.Field("World FloatSO" , "worldFloatSO");
                                        parent.Field("Projectile" , "projectile");
                                        parent.Field("Firearms" , "fireArm");
                                        parent.Field("Tool" , "tool");
                                }
                                Layout.VerticalSpacing(5);

                                Fields.EventFoldOut(parent.Get("onValueChanged") , parent.Get("eventFoldOut") , "On Value Changed" , color: FoldOut.boxColor);

                                int type = parent.Enum("type");

                                if (type == 0)
                                {
                                        bool canIncrease = parent.Bool("canIncrease");
                                        int height = canIncrease ? 1 : 0;

                                        FoldOut.Box(3 + height , FoldOut.boxColor);
                                        {
                                                parent.Field("Sprite Full" , "iconFull");
                                                parent.Field("Sprite Empty" , "iconEmpty");
                                                parent.FieldAndEnable("Can Increase" , "startValue" , "canIncrease");
                                                Labels.FieldText("Start Value" , rightSpacing: 18);
                                                if (canIncrease)
                                                        parent.FieldToggle("Save Manually" , "saveDiscreteManually");
                                        }
                                        Layout.VerticalSpacing(5);

                                        FoldOut.Box(parent.Get("icons").arraySize , FoldOut.boxColor);
                                        {
                                                parent.Get("icons").FieldProperty("Image Icons");
                                        }
                                        Layout.VerticalSpacing(5);
                                }
                                if (type == 1)
                                {
                                        FoldOut.Box(2 , FoldOut.boxColor);
                                        {
                                                parent.Field("Image Bar" , "bar");
                                                parent.Field("Max Value" , "maxValue");
                                        }
                                        Layout.VerticalSpacing(5);
                                }
                                if (type == 2)
                                {
                                        FoldOut.Box(1 , FoldOut.boxColor);
                                        {
                                                parent.Field("TextMeshPro" , "textNumbers");
                                        }
                                        Layout.VerticalSpacing(5);
                                }
                        }
                        parent.ApplyModifiedProperties();
                }
        }
}
