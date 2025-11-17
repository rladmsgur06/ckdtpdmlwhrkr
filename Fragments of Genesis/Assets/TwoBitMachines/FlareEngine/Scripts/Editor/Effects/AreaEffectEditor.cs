using TwoBitMachines.Editors;
using UnityEditor;
using UnityEngine;

namespace TwoBitMachines.FlareEngine.Editors
{
        [CustomEditor(typeof(AreaEffect))]
        [CanEditMultipleObjects]

        public class AreaEffectEditor : UnityEditor.Editor
        {
                private AreaEffect main;
                private SerializedObject parent;

                private void OnEnable ()
                {
                        main = target as AreaEffect;
                        parent = serializedObject;
                        Layout.Initialize();
                }

                public override void OnInspectorGUI ()
                {
                        Layout.Update();
                        Layout.VerticalSpacing(10);

                        parent.Update();

                        FoldOut.Box(3, Tint.Box);
                        {
                                parent.Field("Type", "type");
                                parent.Field("Controller", "controller");
                                parent.FieldDouble("Duration", "time", "rate");
                                Labels.FieldText("Rate");
                        }
                        Layout.VerticalSpacing(5);

                        int type = parent.Enum("type");

                        if (type == 0)
                        {
                                FoldOut.Box(1, Tint.Blue);
                                {
                                        parent.Field("Radius", "radius");
                                }
                                Layout.VerticalSpacing(5);
                        }
                        else if (type == 1)
                        {
                                FoldOut.Box(1, Tint.Blue);
                                {
                                        parent.Field("Size", "size");
                                }
                                Layout.VerticalSpacing(5);
                        }
                        else if (type == 2)
                        {
                                FoldOut.Box(5, Tint.Blue);
                                {
                                        parent.Field("Directions", "directions");
                                        parent.FieldDouble("Angle", "angle", "offset");
                                        Labels.FieldText("Offset");
                                        parent.FieldDouble("Speed", "speed", "gravity");
                                        Labels.FieldText("Gravity");
                                        parent.Clamp("gravity", 0, 2f);
                                        parent.FieldDouble("Sine Wave", "frequency", "amplitude");
                                        Labels.FieldDoubleText("Frequency", "Amplitude");
                                        parent.Field("Rotation Speed", "rotationSpeed");
                                }
                                Layout.VerticalSpacing(5);
                        }
                        else if (type == 3)
                        {
                                FoldOut.Box(1, Tint.Blue);
                                {
                                        parent.FieldToggleAndEnable("Off On Complete", "offOnComplete");
                                }
                                Layout.VerticalSpacing(5);

                                SerializedProperty arrayF = parent.Get("fixedPosition");
                                if (arrayF.arraySize == 0)
                                        arrayF.arraySize++;

                                FoldOut.Box(arrayF.arraySize, Tint.Blue);
                                {
                                        arrayF.FieldProperty("Fixed Position");
                                }
                                Layout.VerticalSpacing(5);
                        }


                        SerializedProperty array = parent.Get("effect");
                        if (array.arraySize == 0)
                                array.arraySize++;

                        FoldOut.Box(array.arraySize, Tint.Orange);
                        {
                                array.FieldProperty("Effect Name");
                        }
                        Layout.VerticalSpacing(5);



                        parent.ApplyModifiedProperties();
                        Layout.VerticalSpacing(10);
                }

                private void OnSceneGUI ()
                {
                        if (Application.isPlaying || main.type != AreaEffectType.Fixed)
                                return;

                        Vector2 startPoint = main.transform.position;
                        for (var i = 0; i < main.fixedPosition.Count; i++)
                        {
                                Vector2 position = startPoint + main.fixedPosition[i];
                                Vector2 newPosition = SceneTools.MovePositionCircleHandle(position, Vector2.zero, Color.green, out bool changed, handleSize: 0.5f);
                                main.fixedPosition[i] = Compute.Round(newPosition - startPoint, 0.25f);
                                if (changed)
                                        Repaint();
                        }


                        Draw.GLStart();
                        for (var i = 0; i < main.fixedPosition.Count - 1; i++)
                        {
                                Vector2 a = startPoint + main.fixedPosition[i];
                                Vector2 b = startPoint + main.fixedPosition[i + 1];
                                if (i == 0)
                                {
                                        Draw.GLLine(startPoint, a, Tint.Delete);
                                }
                                Draw.GLLine(a, b, Tint.Delete);
                        }
                        Draw.GLEnd();
                }
        }
}
