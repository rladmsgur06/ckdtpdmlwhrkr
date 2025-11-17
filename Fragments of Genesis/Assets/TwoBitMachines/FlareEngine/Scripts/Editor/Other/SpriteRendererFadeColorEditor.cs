using TwoBitMachines.Editors;
using UnityEditor;

namespace TwoBitMachines.FlareEngine.Editors
{
        [CustomEditor(typeof(SpriteRendererFadeColor) , true)]
        [CanEditMultipleObjects]
        public class SpriteRendererFadeColorEditor : UnityEditor.Editor
        {
                private SpriteRendererFadeColor main;
                private SerializedObject parent;

                private void OnEnable ()
                {
                        main = target as SpriteRendererFadeColor;
                        parent = serializedObject;
                        Layout.Initialize();
                }

                public override void OnInspectorGUI ()
                {
                        Layout.Update();
                        Layout.VerticalSpacing(10);

                        parent.Update();
                        {
                                FoldOut.Box(2 , Tint.Blue);
                                {
                                        parent.Field("Renderer" , "rendererRef");
                                        parent.FieldDouble("Time" , "holdTime" , "fadeTime");
                                        Labels.FieldDoubleText("Hold" , "Fade");
                                }
                                Layout.VerticalSpacing(5);

                                FoldOut.Box(2 , Tint.Blue);
                                {
                                        parent.FieldToggle("Reverse" , "reverseFade");
                                        parent.FieldToggle("Off On Complete" , "deactivate");
                                }
                                Layout.VerticalSpacing(5);

                        }
                        parent.ApplyModifiedProperties();
                        Layout.VerticalSpacing(10);
                }

        }
}
