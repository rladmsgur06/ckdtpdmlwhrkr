using TwoBitMachines.Editors;
using TwoBitMachines.FlareEngine.Interactables;
using UnityEditor;
using UnityEngine;

namespace TwoBitMachines.FlareEngine.Editors
{
        [CustomEditor(typeof(SpriteRendererColor))]
        [CanEditMultipleObjects]
        public class InterpolateColorEditor : UnityEditor.Editor
        {
                private SpriteRendererColor main;
                private SerializedObject so;

                private void OnEnable ()
                {
                        main = target as SpriteRendererColor;
                        so = serializedObject;
                        Layout.Initialize();
                }

                public override void OnInspectorGUI ()
                {
                        Layout.Update();
                        Layout.VerticalSpacing(10);
                        so.Update();

                        int type = so.Enum("colorType");
                        Block.Box(3, FoldOut.boxColor, noGap: true);
                        {
                                if (type == 0) // set 
                                {
                                        so.Field_("Type", "colorType");
                                        so.Field_("Color", "colorSet");
                                        so.Field_("Sprite Renderer", "rendererRef");
                                }
                                else if (type == 1) // interpolate
                                {
                                        so.Field_("Type", "colorType");
                                        so.Field_("Gradient", "color");
                                        so.Field_("Sprite Renderer", "rendererRef");
                                }
                                else // Flash
                                {
                                        so.Field_("Type", "colorType");
                                        so.FieldDouble_("Color", "colorSet", "speed");
                                        Block.HelperText("Speed");
                                        so.Field_("Sprite Renderer", "rendererRef");
                                }

                        }

                        so.ApplyModifiedProperties();
                        Layout.VerticalSpacing(10);
                }

        }
}
