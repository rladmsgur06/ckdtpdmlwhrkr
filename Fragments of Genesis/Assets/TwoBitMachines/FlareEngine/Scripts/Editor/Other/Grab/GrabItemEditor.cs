using TwoBitMachines.Editors;
using UnityEditor;

namespace TwoBitMachines.FlareEngine.Editors
{
        [CustomEditor(typeof(GrabItem))]
        [CanEditMultipleObjects]
        public class GrabItemEditor : UnityEditor.Editor
        {
                private GrabItem main;
                private SerializedObject parent;

                private void OnEnable ()
                {
                        main = target as GrabItem;
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
                                parent.FieldToggle("Deactivate On Grab", "deactivate");
                                Layout.VerticalSpacing(5);

                                Fields.EventFoldOut(parent.Get("onGrab"), parent.Get("foldOut"), "On Grab", color: Tint.Box);

                        }
                        parent.ApplyModifiedProperties();
                        Layout.VerticalSpacing(10);
                }

        }
}
