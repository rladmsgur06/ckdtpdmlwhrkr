using TwoBitMachines.Editors;
using UnityEditor;

namespace TwoBitMachines.FlareEngine.Editors
{
        [CustomEditor(typeof(SaveMenu) , true)]
        public class SaveMenuEditor : UnityEditor.Editor
        {
                private SaveMenu main;
                private SerializedObject parent;

                private void OnEnable ()
                {
                        main = target as SaveMenu;
                        parent = serializedObject;
                        Layout.Initialize();
                }

                public override void OnInspectorGUI ()
                {
                        Layout.Update();
                        Layout.VerticalSpacing(10);
                        parent.Update();
                        {
                                FoldOut.Box(2 , Tint.PurpleDark);
                                parent.Field("Higlight" , "highlight");
                                parent.Field("Selected" , "selected");
                                Layout.VerticalSpacing(5);
                        }
                        parent.ApplyModifiedProperties();
                        Layout.VerticalSpacing(10);
                }

        }
}
