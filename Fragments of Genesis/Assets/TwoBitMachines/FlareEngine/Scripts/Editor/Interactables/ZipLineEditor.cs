using TwoBitMachines.Editors;
using TwoBitMachines.FlareEngine.Interactables;
using UnityEditor;
using UnityEngine;

namespace TwoBitMachines.FlareEngine.Editors
{
        [CustomEditor(typeof(Zipline))]
        public class ZipLineEditor : UnityEditor.Editor
        {
                private Zipline main;
                private SerializedObject parent;

                private void OnEnable ()
                {
                        main = target as Zipline;
                        parent = serializedObject;
                        Layout.Initialize();
                }

                public override void OnInspectorGUI ()
                {
                        Layout.Update();
                        Layout.VerticalSpacing(10);
                        parent.Update();

                        if (FoldOut.Bar(parent , Tint.Blue).Label("Zipline" , Color.white).FoldOut())
                        {
                                FoldOut.Box(8 , FoldOut.boxColor);
                                parent.Field("Lines" , "lines");
                                parent.Field("Gravity" , "gravity");
                                parent.Field("Up Friction" , "upFriction");
                                parent.Field("Bounce" , "bounce");
                                parent.Field("Stiffness" , "stiffness");
                                parent.Field("Line Renderer" , "line");
                                parent.FieldDouble("Area" , "areaHeight" , "areaOffset");
                                Labels.FieldDoubleText("Height" , "Offset");
                                parent.FieldToggle("Create On Awake" , "createOnAwake");
                                Layout.VerticalSpacing(5);
                        }

                        bool create = FoldOut.LargeButton("Create +" , Tint.Orange , Tint.White , Icon.Get("BackgroundLight") , minusWidth: 24);
                        FoldOut.CornerButtonLR(parent.Get("view") , icon: "EyeOpen");

                        parent.ApplyModifiedProperties();
                        Layout.VerticalSpacing(10);

                        if (create)
                        {
                                main.CreateZipLine();
                        }
                }

                void OnSceneGUI ()
                {
                        if (main == null)
                                return;
                        Mouse.Update();
                        parent.Update();

                        SerializedProperty endOffset = parent.Get("endOffset");
                        if (endOffset.vector3Value == Vector3.zero)
                                endOffset.vector3Value = new Vector3(0 , 1f);
                        Vector2 newPoint = SceneTools.MovePositionCircleHandle(main.transform.position + endOffset.vector3Value , Vector2.zero , Color.red , out bool changed , snap: 0.5f);
                        endOffset.vector3Value = newPoint - (Vector2) main.transform.position;

                        parent.ApplyModifiedProperties();
                        if (changed)
                                Repaint();
                }
        }
}
