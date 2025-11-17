using TwoBitMachines.Editors;
using UnityEditor;

namespace TwoBitMachines.FlareEngine.Editors
{
        [CustomEditor(typeof(SpriteTrail))]
        [CanEditMultipleObjects]
        public class SpriteTrailEditor : UnityEditor.Editor
        {
                private SpriteTrail main;
                private SerializedObject parent;

                private void OnEnable ()
                {
                        main = target as SpriteTrail;
                        parent = serializedObject;
                        Layout.Initialize();
                }

                public override void OnInspectorGUI ()
                {
                        Layout.Update();
                        Layout.VerticalSpacing(10);

                        parent.Update();


                        FoldOut.Box(4 , Tint.Box);
                        {
                                parent.Field("Effect Time" , "effectTime");
                                parent.Field("Spawn Rate" , "spawnRate");
                                parent.Field("Gradient" , "gradient");
                                parent.Field("Template" , "template");
                        }
                        Layout.VerticalSpacing(5);

                        parent.ApplyModifiedProperties();
                        Layout.VerticalSpacing(10);
                }

        }
}
