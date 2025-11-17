#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace TwoBitMachines.Editors
{
        public static class Labels
        {
                public static GUIStyle labelStyle = new GUIStyle();
                public static GUIStyle textStyle = new GUIStyle();
                public static GUIStyle fieldTextStyle = new GUIStyle();
                public static RectOffset rectZero = new RectOffset(0, 0, 0, 0);
                public static bool useWhite = false;

                public static void InitializeStyle ()
                {
                        labelStyle.fontSize = 11;
                        labelStyle.fontStyle = FontStyle.Bold;
                        labelStyle.normal.textColor = Color.white;
                        labelStyle.padding = new RectOffset(0, 0, 0, 0);
                        labelStyle.margin = new RectOffset(0, 0, 0, 0);
                        labelStyle.padding.top += 3;
                        SetFieldTextStyle();
                        ResetTextStyle();
                }



                public static void Label (string title, Rect rect, bool bold = false, bool whiteStyle = false)
                {
                        EditorGUI.LabelField(rect, title, useWhite || whiteStyle ? labelStyle : bold ? EditorStyles.boldLabel : EditorStyles.label);
                }

                public static void Label (Rect rect, string title, Color color, float fontSize = 12, bool bold = true)
                {
                        Labels.textStyle.padding = Labels.rectZero;
                        Labels.textStyle.fontSize = (int) fontSize;
                        Labels.textStyle.normal.textColor = color;
                        Labels.textStyle.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;

                        Labels.textStyle.clipping = TextClipping.Clip;
                        EditorGUI.LabelField(rect, title, Labels.textStyle);
                        Labels.ResetTextStyle();
                }

                public static void Label (string title, Color color, float fontSize = 12, bool bold = true)
                {
                        Labels.textStyle.padding = Labels.rectZero;
                        Labels.textStyle.fontSize = (int) fontSize;
                        Labels.textStyle.normal.textColor = color;
                        Labels.textStyle.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;

                        Labels.textStyle.clipping = TextClipping.Clip;
                        EditorGUI.LabelField(Layout.CreateRectField(), title, Labels.textStyle);
                        Labels.ResetTextStyle();
                }

                public static void Label (string title, bool bold = false)
                {
                        EditorGUI.LabelField(Layout.CreateRectField(), title, bold ? EditorStyles.boldLabel : EditorStyles.label);
                }

                public static void LabelCenterWidth (string title, Rect rect, LabelType type, int yOffset = 0)
                {
                        Vector2 size = Vector2.one;
                        if (type == LabelType.White)
                                size = labelStyle.CalcSize(new GUIContent(title));
                        else if (type == LabelType.Bold)
                                size = EditorStyles.boldLabel.CalcSize(new GUIContent(title));
                        else
                                size = EditorStyles.label.CalcSize(new GUIContent(title));
                        rect.CenterRectContent(size);
                        rect.y += yOffset;
                        EditorGUI.LabelField(rect, title, type == LabelType.White ? labelStyle : type == LabelType.Bold ? EditorStyles.boldLabel : EditorStyles.label);
                }

                public static bool LabelAndButton (string title, string icon)
                {
                        Rect rect = Layout.CreateRectField();
                        int buttonWidth = 15;
                        Labels.Label(title, rect.Adjust(0, Layout.labelWidth + Layout.contentWidth - buttonWidth + 2));
                        return Buttons.Button(rect.Adjust(Layout.labelWidth + Layout.contentWidth - buttonWidth + 2, buttonWidth), Icon.Get(icon), Tint.White, center: true);
                }

                public static bool LabelDisplayAndButton (string title, string icon, int space = 0, int buttonWidth = 15)
                {
                        Rect rect = Layout.CreateRectField();
                        EditorGUI.LabelField(rect.Adjust(space, Layout.labelWidth), title, useWhite ? labelStyle : EditorStyles.label);
                        bool pressed = Buttons.Button(rect.Adjust(+Layout.labelWidth + Layout.contentWidth - buttonWidth - space, buttonWidth - 2), Icon.Get(icon), Tint.White, center: true);
                        return pressed;
                }

                public static void Centered (Rect rect, string label, Color color, int fontSize = 11, int shiftY = 0)
                {
                        GUIStyle style = new GUIStyle();
                        style.normal.textColor = color;
                        style.normal.background = null;
                        style.fontStyle = FontStyle.Bold;
                        style.fontSize = fontSize;
                        Vector2 labelSize = style.CalcSize(new GUIContent(label));
                        rect = rect.CenterRectContentForClipping(labelSize);
                        rect.y += shiftY;
                        EditorGUI.LabelField(rect, label, style);
                }

                public static void CenteredAndClip (Rect rect, string label, Color color, int fontSize = 11, int shiftY = 0)
                {
                        GUIStyle style = new GUIStyle();
                        style.normal.textColor = color;
                        style.normal.background = null;
                        style.fontStyle = FontStyle.Bold;
                        style.fontSize = fontSize;
                        style.clipping = TextClipping.Clip;
                        Vector2 labelSize = style.CalcSize(new GUIContent(label));

                        rect = rect.CenterRectContentForClipping(labelSize);
                        rect.y += shiftY;
                        EditorGUI.LabelField(rect, label, style);
                }

                public static void Display (Rect rect, string label, float offsetX = 0, float offsetY = 0, bool useTextStyle = false)
                {
                        rect.x += offsetX;
                        rect.y += offsetY;
                        if (useTextStyle)
                                EditorGUI.LabelField(rect, label, textStyle);
                        else
                                Label(label, rect);
                }

                public static void Display (string title, int space = 0)
                {
                        Rect rect = Layout.CreateRect(Layout.infoWidth, Layout.rectFieldHeight);
                        EditorGUI.LabelField(rect.Adjust(space, Layout.labelWidth), title, labelStyle);
                }

                public static void Display (string title, float width, int space, bool useTextStyle = false, bool bold = false)
                {
                        Rect rect = Layout.CreateRect(Layout.infoWidth, Layout.rectFieldHeight);
                        if (useTextStyle)
                                EditorGUI.LabelField(rect.Adjust(space, width), title, textStyle);
                        else
                                Label(title, rect.Adjust(space, width), bold);
                }

                public static void InfoBox (int height, string message, float offsetY = -1)
                {
                        Layout.VerticalSpacing();
                        Rect rect = Layout.CreateRect(Layout.longInfoWidth, height, offsetX: -11, offsetY: offsetY);
                        EditorGUI.HelpBox(rect, message, MessageType.Info);
                }

                public static void InfoBoxTop (int height, string message, int offsetY = 0, MessageType type = MessageType.Info)
                {
                        Rect rect = Layout.GetLastRect(Layout.longInfoWidth, height, offsetX: -11, offsetY: -height + offsetY);
                        rect.DrawRect(EditorGUIUtility.isProSkin ? Tint.SoftDark240 : Tint.WarmGrey225);
                        EditorGUI.HelpBox(rect, message, type);
                }

                public static void FieldText (string title1, float rightSpacing = 2, bool execute = true, int space = 0, int yOffset = 2)
                {
                        if (!execute)
                                return;
                        Rect rect = Layout.GetLastRect(Layout.infoWidth, Layout.rectFieldHeight);
                        rect.y += yOffset;
                        fieldTextStyle.normal.textColor = EditorGUIUtility.isProSkin ? Tint.WarmWhite : Tint.HardDark;
                        float title1Width = fieldTextStyle.CalcSize(new GUIContent(title1)).x + 4f;
                        EditorGUI.LabelField(rect.Adjust(rect.width - title1Width - rightSpacing, rect.width), title1, fieldTextStyle);
                }

                public static void FieldDoubleText (string title1, string title2, bool show1 = true, bool show2 = true, float rightSpacing = 2, bool execute = true, int space = 0)
                {
                        if (!execute || (!show1 && !show2))
                                return;
                        Rect rect = Layout.GetLastRect(Layout.infoWidth, Layout.rectFieldHeight);
                        //rect.y -= Layout.rectFieldHeight;
                        rect.y += 2;
                        Rect origin = new Rect(rect);
                        float width = (Layout.contentWidth - rightSpacing) / 2f;
                        fieldTextStyle.normal.textColor = EditorGUIUtility.isProSkin ? Tint.WarmWhite : Tint.HardDark;
                        if (show2)
                        {
                                float title2Width = fieldTextStyle.CalcSize(new GUIContent(title2)).x + 4f;
                                EditorGUI.LabelField(rect.Adjust(rect.width - title2Width - rightSpacing, width), title2, fieldTextStyle);
                        }
                        if (show1)
                        {
                                rect = origin;
                                float title1Width = fieldTextStyle.CalcSize(new GUIContent(title1)).x + 4f;
                                EditorGUI.LabelField(rect.Adjust(rect.width - title1Width - rightSpacing - width, width), title1, fieldTextStyle);
                        }
                }
                public static void FieldDoubleText (string title1, string title2, float spacingA, float spacingB, bool show1 = true, bool show2 = true, bool execute = true, int space = 0)
                {
                        if (!execute || (!show1 && !show2))
                                return;
                        Rect rect = Layout.GetLastRect(Layout.infoWidth, Layout.rectFieldHeight);
                        rect.y += 3;
                        Rect origin = new Rect(rect);
                        float width = (Layout.contentWidth - spacingA) / 2f;
                        fieldTextStyle.normal.textColor = EditorGUIUtility.isProSkin ? Tint.WarmWhite : Tint.HardDark;
                        if (show2)
                        {
                                float title2Width = fieldTextStyle.CalcSize(new GUIContent(title2)).x + 4f;
                                EditorGUI.LabelField(rect.Adjust(rect.width - title2Width - spacingA, width), title2, fieldTextStyle);
                        }
                        if (show1)
                        {
                                rect = origin;
                                float title1Width = fieldTextStyle.CalcSize(new GUIContent(title1)).x + 4f;
                                EditorGUI.LabelField(rect.Adjust(rect.width - title1Width - spacingA - spacingB - width, width), title1, fieldTextStyle);
                        }
                }

                public static void ResetTextStyle ()
                {
                        textStyle.fontSize = 8;
                        textStyle.fontStyle = FontStyle.Normal;
                        textStyle.normal.textColor = Tint.SoftDark;
                        textStyle.padding = rectZero;
                        textStyle.margin = rectZero;
                        textStyle.padding.top += 3;
                }

                public static void SetFieldTextStyle ()
                {
                        fieldTextStyle.fontSize = 8;
                        fieldTextStyle.fontStyle = FontStyle.Normal;
                        fieldTextStyle.normal.textColor = Tint.SoftDark;
                        fieldTextStyle.padding = rectZero;
                        fieldTextStyle.margin = rectZero;
                        fieldTextStyle.padding.top += 3;
                }

        }

        public enum LabelType
        {
                White,
                Bold,
                Normal
        }

}
#endif
