#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace TwoBitMachines.Editors
{
        public static class Buttons
        {
                public static bool Toggle (this Rect rect, SerializedProperty property, Texture2D icon, Color on, Color off, bool center = true)
                {
                        if (ButtonRect.Get(rect, icon, property.boolValue ? on : off, isSelected: false, center: center))
                        {
                                property.Toggle();
                        }
                        return property.boolValue;
                }

                public static bool Button (this Rect rect, Texture2D icon, Texture2D background, Color color, Color? iconColor = null, bool isSelected = false, bool center = false, string toolTip = "")
                {
                        bool button = ButtonRect.Get(rect, background, color, isSelected, center, toolTip);
                        rect.CenterRectContent(new Vector2(icon.width, icon.height));
                        Skin.DrawTexture(rect, icon, iconColor == null ? Tint.White : iconColor.Value);
                        return button;
                }

                public static bool Button (this Rect rect, string name, Texture2D background, Color color, LabelType type, bool isSelected, bool center = false)
                {
                        bool button = ButtonRect.Get(rect, background, color, isSelected, center);
                        Labels.LabelCenterWidth(name, rect, type);
                        return button;
                }

                public static bool ButtonDropdown (this Rect rect, string name, Texture2D background, Color color, LabelType type, bool isSelected, int yoffset = 0)
                {
                        bool button = ButtonRect.Get(rect, background, color, isSelected, false);
                        Rect rectLabel = new Rect(rect) { x = rect.x + 6, y = rect.y - 2 + yoffset, width = rect.width - 8 };
                        Labels.ResetTextStyle();
                        Labels.textStyle.fontSize = 12;
                        Labels.textStyle.normal.textColor = Color.black;
                        Labels.textStyle.clipping = TextClipping.Clip;
                        GUI.Label(rectLabel, name, Labels.textStyle);
                        Labels.ResetTextStyle();
                        return button;
                }

                public static bool Button (this Rect rect, Texture2D icon, Color color, bool isSelected = false, bool center = false, string toolTip = "")
                {
                        return ButtonRect.Get(rect, icon, color, isSelected, center, toolTip);
                }
        }

}
#endif
