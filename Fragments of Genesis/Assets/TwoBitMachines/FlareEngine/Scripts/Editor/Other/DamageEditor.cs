using TwoBitMachines.Editors;
using UnityEditor;

namespace TwoBitMachines.FlareEngine.Editors
{
        [CustomEditor(typeof(Damage) , true)]
        [CanEditMultipleObjects]
        public class DamageEditor : UnityEditor.Editor
        {
                private Damage main;
                private SerializedObject parent;

                private void OnEnable ()
                {
                        main = target as Damage;
                        parent = serializedObject;
                        Layout.Initialize();
                }

                public override void OnInspectorGUI ()
                {
                        Layout.Update();
                        Layout.VerticalSpacing(10);
                        parent.Update();
                        {
                                FoldOut.Box(4 , Tint.Delete);
                                parent.Field("Layer" , "layer");
                                parent.Field("Direction" , "direction");
                                parent.Field("Amount" , "damage");
                                parent.Field("Force" , "force");
                                Layout.VerticalSpacing(5);
                        }
                        parent.ApplyModifiedProperties();
                        Layout.VerticalSpacing(10);
                }

        }
}
