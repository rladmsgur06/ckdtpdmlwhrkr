using TwoBitMachines.Editors;
using TwoBitMachines.FlareEngine.Interactables;
using UnityEditor;

namespace TwoBitMachines.FlareEngine.Editors
{
        [CanEditMultipleObjects, CustomEditor(typeof(FoliageSway))]
        public class FoliageSwayEditor : UnityEditor.Editor
        {
                private FoliageSway main;
                private SerializedObject parent;

                private void OnEnable ()
                {
                        parent = serializedObject;
                        main = target as FoliageSway;
                        Layout.Initialize();
                }

                public override void OnInspectorGUI ()
                {
                        Layout.Update();
                        Layout.VerticalSpacing(10);
                        parent.Update();
                        {
                                FoldOut.Box(2 , Tint.Orange);
                                {
                                        parent.Field("Orientation" , "direction");
                                        parent.FieldToggle("Is Random" , "isRandom");
                                }
                                Layout.VerticalSpacing(5);

                                if (parent.Bool("isRandom"))
                                {
                                        FoldOut.Box(3 , FoldOut.boxColor);
                                        {
                                                parent.FieldDouble("Amplitude" , "amplitudeMin" , "amplitudeMax");
                                                Labels.FieldDoubleText("Min" , "Max");
                                                parent.FieldDouble("Frequency" , "frequencyMin" , "frequencyMax");
                                                Labels.FieldDoubleText("Min" , "Max");
                                                parent.FieldDouble("Speed" , "speedMin" , "speedMax");
                                                Labels.FieldDoubleText("Min" , "Max");
                                        }
                                        Layout.VerticalSpacing(5);
                                }
                                else
                                {
                                        FoldOut.Box(3 , FoldOut.boxColor);
                                        {
                                                parent.Field("Amplitude" , "amplitudeMin");
                                                parent.Field("Frequency" , "frequencyMin");
                                                parent.Field("Speed" , "speedMin");
                                        }
                                        Layout.VerticalSpacing(5);
                                }

                        }
                        parent.ApplyModifiedProperties();
                        Layout.VerticalSpacing(10);
                }
        }
}
