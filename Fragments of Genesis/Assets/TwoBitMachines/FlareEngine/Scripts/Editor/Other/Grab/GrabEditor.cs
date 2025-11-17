using TwoBitMachines.Editors;
using UnityEditor;

namespace TwoBitMachines.FlareEngine.Editors
{
        [CustomEditor(typeof(Grab))]
        [CanEditMultipleObjects]
        public class GrabEditor : UnityEditor.Editor
        {
                private Grab main;
                private SerializedObject parent;

                private void OnEnable ()
                {
                        main = target as Grab;
                        parent = serializedObject;
                        Layout.Initialize();
                }

                public override void OnInspectorGUI ()
                {
                        Layout.Update();
                        Layout.VerticalSpacing(10);
                        parent.Update();
                        {
                                FoldOut.Box(1, Tint.Orange);
                                parent.Field("Grab Layer", "layer");
                                Layout.VerticalSpacing(5);
                        }
                        parent.ApplyModifiedProperties();
                        Layout.VerticalSpacing(10);
                }

        }
}
