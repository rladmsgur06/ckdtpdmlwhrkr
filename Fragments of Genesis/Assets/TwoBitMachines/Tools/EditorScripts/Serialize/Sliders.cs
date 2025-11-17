#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace TwoBitMachines.Editors
{
        public static class Sliders
        {
                public static void Slider (this SerializedObject property, string title, string field, float min = 0, float max = 1, float round = 0f)
                {
                        SerializedProperty currentField = property.Get(field);
                        Rect rect = Layout.CreateRectField();
                        {
                                Labels.Label(title, rect.Adjust(0, Layout.labelWidth));
                                rect = rect.Adjust(Layout.labelWidth, Layout.contentWidth);
                                currentField.floatValue = EditorGUI.Slider(rect, currentField.floatValue, min, max);
                                if (round != 0)
                                {
                                        currentField.floatValue = Compute.Round(currentField.floatValue, round);
                                }
                        }
                }

                public static void Slider (this SerializedProperty property, string title, string field, float min = 0, float max = 1)
                {
                        SerializedProperty currentField = property.Get(field);
                        Rect rect = Layout.CreateRectField();
                        {
                                Labels.Label(title, rect.Adjust(0, Layout.labelWidth));
                                rect = rect.Adjust(Layout.labelWidth, Layout.contentWidth);
                                currentField.floatValue = EditorGUI.Slider(rect, currentField.floatValue, min, max);
                        }
                }
        }
}
#endif
