using TwoBitMachines.Editors;
using TwoBitMachines.FlareEngine.Interactables;
using UnityEditor;
using UnityEngine;

namespace TwoBitMachines.FlareEngine.Editors
{
        [CustomEditor(typeof(Friction))]
        public class FrictionEditor : UnityEditor.Editor
        {
                private Friction main;
                private SerializedObject parent;

                private void OnEnable ()
                {
                        main = target as Friction;
                        parent = serializedObject;
                        Layout.Initialize();
                }

                public override void OnInspectorGUI ()
                {
                        Layout.Update();
                        Layout.VerticalSpacing(10);

                        parent.Update();
                        if (FoldOut.Bar(parent, Tint.Blue).Label("Friction", Color.white).FoldOut())
                        {
                                int type = parent.Enum("type");
                                FoldOut.Box(2, FoldOut.boxColor, offsetY: -2);
                                parent.Field("Type", "type");
                                parent.Field("Friction", "friction", execute: type == 0);
                                parent.Field("Slide", "slideSpeed", execute: type == 1);
                                parent.Field("Auto Speed", "autoSpeed", execute: type == 2);
                                Layout.VerticalSpacing(3);
                        }
                        parent.ApplyModifiedProperties();
                        Layout.VerticalSpacing(10);
                }

        }
}
