#if UNITY_EDITOR
using System;
using System.Data;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TwoBitMachines.Editors
{
        #region foldout
        public class Bar
        {
                public static Rect barStart;
                public static Rect barEnd;
                public static float startX;
                public static float startXOrigin;
                public static int padding = 3;
                public static int tabButtonWidth = 1;
                public static int extraTabWidth = 0;
                public static bool isFirstTab = false;
                public static SerializedProperty property;
                public static SerializedObject objProperty;

                public static Rect oldBarStart;
                public static Rect oldBarEnd;
                public static float oldStartX;
                public static float oldStartXOrigin;

                public static void Remember ()
                {
                        oldBarStart = barStart;
                        oldBarEnd = barEnd;
                        oldStartX = startX;
                        oldStartXOrigin = startXOrigin;
                }

                public static void ResetToOld ()
                {
                        barStart = oldBarStart;
                        barEnd = oldBarEnd;
                        startX = oldStartX;
                        startXOrigin = oldStartXOrigin;
                }

                public bool C (bool origin = false)
                {
                        Rect openRect = new Rect(barStart) { x = origin ? startXOrigin : startX };
                        return Event.current.type == EventType.MouseDown && openRect.Contains(Event.current.mousePosition);
                }

                public Bar SU (Texture2D texture, Color color, int height, bool topSpace = true, int offsetY = 0)
                {
                        Setup(texture, color, space: topSpace, height: height, offsetY: offsetY);
                        return this;
                }

                public Bar Label (string label, Color? color = null, bool bold = true, int fontSize = 12, bool execute = true)
                {
                        if (execute == false)
                                return this;
                        Label(label, color ?? Color.white, bold: bold);
                        return this;
                }

                public Bar LabelRight (string label, Color? color = null, bool bold = true, int fontSize = 12, int offsetX = 0, bool execute = true)
                {
                        if (execute == false)
                                return this;
                        LabelRight(label, color ?? Color.white, offsetX: offsetX, bold: bold);
                        return this;
                }

                public Bar LabelAndEdit (string label, string edit, Color? color = null, bool bold = true, int fontSize = 12, int width = 150, bool execute = true)
                {
                        if (execute == false)
                                return this;

                        Rect area = barStart;
                        area.width = 35;

                        if (area.ContainsMouseDown() || (property.Bool(edit) && Event.current.isKey && Event.current.keyCode == KeyCode.Return))
                        {
                                property.Toggle(edit);
                        }
                        area.width = width;
                        if (Mouse.down && !area.ContainsMouse())
                        {
                                property.SetFalse(edit);
                        }
                        if (!property.Bool(edit))
                        {
                                Label(property.String(label), color ?? Color.white, bold: bold);
                        }
                        else
                        {
                                Rect fieldWidth = new Rect(barStart) { x = barStart.x - 1, y = barEnd.y + 3, width = width, height = Layout.rectFieldHeight - 1 };
                                EditorGUI.PropertyField(fieldWidth, property.Get(label), GUIContent.none);
                        }
                        return this;
                }

                public Bar ToggleButton (string button, string iconOn, string iconOff, string toolTip = "", bool execute = true)
                {
                        if (execute == false)
                                return this;
                        bool value = property.Bool(button);
                        if (value && ButtonRight(iconOn, Tint.White, toolTip: toolTip))
                                property.Toggle(button);
                        if (!value && ButtonRight(iconOff, Tint.White, toolTip: toolTip))
                                property.Toggle(button);
                        return this;
                }

                public Bar SL (int space = 8)
                {
                        SpaceLeft(space);
                        return this;
                }

                public Bar BlockRight (float width, Color color, float offsetX = 0)
                {
                        BlockRightStatic(width, color, offsetX);
                        return this;
                }

                public Bar SR (int space = 8, bool execute = true)
                {
                        if (execute == false)
                                return this;
                        SpaceRight(space);
                        return this;
                }

                public bool BBR (string icon = "Add", Color? color = null)
                {
                        return ButtonRight(icon, color ?? Color.white);
                }

                public Bar BR (string field = "add", string icon = "Add", Color? on = null, Color? off = null, bool execute = true)
                {
                        if (execute == false)
                                return this;
                        ButtonRight(Get(field), icon, on ?? Color.white, off ?? Color.white);
                        return this;
                }

                public Bar RightTab (string field, string icon, int index, Color on, Color off, string toolTip = "", bool execute = true)
                {
                        if (execute == false)
                                return this;

                        SerializedProperty tab = Get(field);

                        if (ButtonRight(icon, tab.intValue == index ? on : off, toolTip: toolTip))
                        {
                                tab.intValue = tab.intValue == index ? -1 : index;
                        }
                        return this;
                }

                public Bar RightButton (string field = "add", string icon = "Add", string toolTip = "", Color? on = null, Color? off = null, bool execute = true)
                {
                        if (execute == false)
                                return this;
                        ButtonRight(Get(field), icon, on ?? Color.white, off ?? Color.white, toolTip: toolTip);
                        return this;
                }

                public Bar BRB (string field = "add", string icon = "Add", string background = "BackgroundLight", Color? on = null, Color? off = null, bool execute = true)
                {
                        if (execute == false)
                                return this;
                        ButtonRight(Get(field), icon, on ?? Color.white, off ?? Color.white);
                        return this;
                }

                public Bar BRE (string enable = "enable", string icon = "Open")
                {
                        ButtonRight(Get(enable), icon, Tint.PastelGreen, Tint.WarmWhite);
                        return this;
                }

                public Bar BRE (string enable, string icon, Color on, Color off)
                {
                        ButtonRight(Get(enable), icon, on, off);
                        return this;
                }

                public Bar BL (string field = "enable", string icon = "LeftButton", Color? on = null, Color? off = null)
                {
                        ButtonLeft(Get(field), icon, on ?? Color.white, off ?? Color.white);
                        return this;
                }

                public Bar LF (string field, int width, int shortenHeight = 0, int yOffset = 3, int extraPad = 0, bool execute = true)
                {
                        if (execute == false)
                                return this;
                        LeftField(Get(field), width, shortenHeight, yOffset, extraPad);
                        return this;
                }

                public Bar LF (int width, int shortenHeight = 0, int yOffset = 3, int extraPad = 0, bool execute = true)
                {
                        if (execute == false)
                                return this;
                        LeftField(property, width, shortenHeight, yOffset, extraPad);
                        return this;
                }

                public Bar LDL (string field, int width, string[] list) // left drop down list
                {
                        Rect rect = new Rect(barStart) { x = barStart.x, width = width, y = barStart.y + 3 };
                        property.DropList(rect, list, field);
                        SpaceLeft(width);
                        return this;
                }

                public Bar RF (string field, int width, int shortenHeight = 0, int yOffset = 0, int extraPad = 0, bool execute = true)
                {
                        if (execute == false)
                                return this;
                        RightField(Get(field), width, shortenHeight, yOffset, extraPad);
                        return this;
                }

                public bool FoldOut (string foldOut = "foldOut")
                {
                        return FoldOpen(Get(foldOut));
                }

                public Bar Sprite (Sprite sprite, int size = 10, int yOffset = 0, bool execute = true)
                {
                        if (execute == false)
                                return this;
                        ShowSprite(sprite, size, yOffset);
                        return this;
                }
                public static void SpaceLeft (int space = 5)
                {
                        barStart.x += space;
                        startX += space;
                }

                public static void SpaceRight (int space = 5)
                {
                        barEnd.x -= space;
                }

                public Bar Grip (SerializedObject parent, SerializedProperty array, int index, int space = 25, Color? color = null)
                {
                        ListReorder.Grip(parent, array, barStart.CenterRectHeight(), index, color ?? Color.white);
                        SL(space);
                        return this;
                }

                public Bar GripSpecify (SerializedObject parent, SerializedProperty array, int index, int space = 25, string signalIndex = "", string active = "", Color? color = null)
                {
                        ListReorder.Grip(parent, array, barStart.CenterRectHeight(), index, color ?? Color.white, signalIndex, active);
                        SL(space);
                        return this;
                }

                public Bar Grip (SerializedProperty parent, SerializedProperty array, int index, int space = 25, Color? color = null)
                {
                        ListReorder.Grip(parent, array, barStart.CenterRectHeight(), index, color ?? Color.white);
                        SL(space);
                        return this;
                }

                public Bar G (SerializedProperty parent, SerializedProperty array, int index, int space = 25, Color? color = null)
                {
                        ListReorder.Grip(parent, array, barStart.CenterRectHeight(), index, color ?? Color.white);
                        SL(space);
                        return this;
                }

                public static void RightField (SerializedProperty property, int width, int shortenHeight = 0, int offsetY = 0, int extraPad = 0, bool execute = true)
                {
                        if (!execute)
                                return;
                        Rect fieldWidth = new Rect(barEnd) { x = barEnd.x - width - padding - extraPad, y = barEnd.y + offsetY, width = width + padding, height = barEnd.height + shortenHeight };
                        EditorGUI.PropertyField(fieldWidth, property, GUIContent.none);
                        SpaceRight(width + (padding + extraPad) + 1);
                }

                public static void LeftField (SerializedProperty property, int width, int shortenHeight = 0, int offsetY = 0, int extraPad = 0, bool execute = true)
                {
                        if (!execute)
                                return;
                        Rect fieldWidth = new Rect(barStart) { x = barStart.x + extraPad, y = barEnd.y + offsetY, width = width + padding, height = barEnd.height + shortenHeight };
                        EditorGUI.PropertyField(fieldWidth, property, GUIContent.none);
                        SpaceLeft(width + (padding + extraPad) + 1);
                }

                public static void ShowSprite (Sprite sprite, int size = 10, int offsetY = 0, bool execute = true)
                {
                        if (!execute || sprite == null)
                                return;

                        // GUI.DrawTextureWithTexCoords (rect, sprite.texture, sprite.rect);

                        Rect spriteRect = sprite.rect;
                        Texture2D tex = sprite.texture;
                        Rect rect = new Rect(barStart) { x = barStart.x, y = barEnd.y + offsetY + 3, width = size, height = size };
                        GUI.DrawTextureWithTexCoords(rect, tex, new Rect(spriteRect.x / tex.width, spriteRect.y / tex.height, spriteRect.width / tex.width, spriteRect.height / tex.height));

                        SpaceLeft((int) size + (padding) + 1);
                }

                public static bool FoldOpen (SerializedProperty property, bool selected = true)
                {
                        Rect openRect = new Rect(barStart) { x = startX };
                        if (Event.current.type == EventType.MouseDown && openRect.Contains(Event.current.mousePosition))
                        {
                                if (!selected && property.boolValue)
                                        property.boolValue = false; // keep open
                                property.boolValue = !property.boolValue;
                                GUI.changed = true;
                                Event.current.Use();
                        }
                        return property.boolValue;
                }

                public static void Setup (Texture2D texture, Color barColor, bool space = true, int height = 23, float xAdjust = 0, float offsetY = 0)
                {
                        if (space)
                                Layout.VerticalSpacing(1);
                        barStart = Layout.CreateRectAndDraw(width: Layout.longInfoWidth - xAdjust, height: height, offsetX: -11 + xAdjust, offsetY: offsetY, texture: texture, color: barColor);
                        barEnd = new Rect(barStart) { x = barStart.x + barStart.width - 6 };
                        startX = startXOrigin = barStart.x;
                }

                public static void SetupTabBar (int buttons, int height = 23)
                {
                        Layout.VerticalSpacing(1);
                        barStart = Layout.CreateRect(width: Layout.longInfoWidth, height: height, offsetX: -11);
                        tabButtonWidth = Mathf.RoundToInt((float) Layout.longInfoWidth / (float) buttons);
                        extraTabWidth = Mathf.RoundToInt(Layout.longInfoWidth - (tabButtonWidth * buttons));
                        isFirstTab = true;
                        startX = startXOrigin = barStart.x;
                        Layout.Pushclear();
                }

                private static int ExtraTabWidth ()
                {
                        int extraWidth = isFirstTab ? extraTabWidth : 0;
                        isFirstTab = false;
                        return extraWidth;
                }

                public static bool TabButton (SerializedProperty index, int value, string icon, string background, Color colorOn, Color colorOff, string toolTip = "")
                {
                        if (barStart.Push(tabButtonWidth + ExtraTabWidth()).Button(Icon.Get(icon), Icon.Get(background), index.intValue == value ? colorOn : colorOff))
                        {
                                index.intValue = index.intValue == value ? -1 : value;
                        }
                        if (toolTip != "")
                                GUI.Label(barStart, new GUIContent("", toolTip));
                        return index.intValue == value;
                }

                public static bool TabButton (string icon, string background, Color color, bool isSelected, string toolTip = "", Color? iconColor = null)
                {
                        if (barStart.Push(tabButtonWidth + ExtraTabWidth()).Button(Icon.Get(icon), Icon.Get(background), color, iconColor: iconColor, isSelected: isSelected))
                        {
                                return true;
                        }
                        if (toolTip != "")
                                GUI.Label(barStart, new GUIContent("", toolTip));
                        return false;
                }

                public static void Label (string label, Color color, int fontSize = 12, bool bold = true)
                {
                        Labels.textStyle.padding = Labels.rectZero;
                        Labels.textStyle.fontSize = (int) fontSize;
                        Labels.textStyle.normal.textColor = color;
                        Labels.textStyle.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;

                        Vector2 size = Labels.textStyle.CalcSize(new GUIContent(label));
                        float offsetY = Mathf.Abs(barStart.height - size.y) * 0.5f;
                        size.x = Mathf.Clamp(size.x, 0, Layout.infoWidth - 15);
                        Rect labelRect = new Rect(barStart) { y = barStart.y + offsetY, width = size.x };

                        Labels.textStyle.clipping = TextClipping.Clip;
                        GUI.Label(labelRect, label, Labels.textStyle);
                        barStart.x += size.x + 5;
                        Labels.ResetTextStyle();
                }

                public static void LabelRight (string label, Color color, int fontSize = 12, int offsetX = 0, bool bold = true)
                {
                        Labels.textStyle.padding = Labels.rectZero;
                        Labels.textStyle.fontSize = (int) fontSize;
                        Labels.textStyle.normal.textColor = color;
                        Labels.textStyle.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;

                        Vector2 size = Labels.textStyle.CalcSize(new GUIContent(label));
                        float offsetY = Mathf.Abs(barEnd.height - size.y) * 0.5f;
                        size.x = Mathf.Clamp(size.x, 0, Layout.infoWidth - 15);
                        Rect labelRect = new Rect(barEnd) { x = barEnd.x - (size.x + 2) + offsetX, y = barEnd.y + offsetY, width = size.x };

                        Labels.textStyle.clipping = TextClipping.Clip;
                        GUI.Label(labelRect, label, Labels.textStyle);
                        Labels.ResetTextStyle();
                }

                public static void BarField (SerializedProperty property, string field, float percent)
                {
                        float width = (barEnd.x - startX) * percent;
                        Rect rectField = new Rect(barStart) { width = width };
                        property.Get(field).Field(rectField);
                        SpaceLeft((int) width);
                }

                public static bool ButtonRight (SerializedProperty property, string icon, Color on, Color off, int extraPad = 0, bool execute = true, string toolTip = "")
                {
                        if (!execute)
                                return false;

                        Texture2D texture = Icon.Get(icon);
                        Rect button = new Rect(barEnd) { x = barEnd.x - texture.width - (padding + extraPad), width = texture.width + (padding + extraPad) * 2 };
                        if (toolTip != "")
                                GUI.Label(button, new GUIContent("", toolTip));
                        button.Toggle(property, texture, on, off);
                        SpaceRight(texture.width + (padding + extraPad) * 2);
                        return property.boolValue;
                }

                public static void BlockRightStatic (float width, Color color, float offsetX = 0)
                {
                        Rect block = new Rect(barEnd) { x = barEnd.x - width + offsetX, y = barEnd.y, width = width, height = barEnd.height - 2 };
                        block.DrawRect(color);
                }

                public static bool ButtonRight (SerializedProperty property, string icon, string background, Color on, Color off, int extraPad = 0, bool execute = true)
                {
                        if (!execute)
                                return false;
                        Texture2D texture = Icon.Get(icon);
                        Rect button = new Rect(barEnd) { x = barEnd.x - texture.width - (padding + extraPad), width = texture.width + (padding + extraPad) * 2 };

                        if (button.Button(texture, Icon.Get(background), on))
                                property.Toggle();
                        SpaceRight(texture.width + (padding + extraPad) * 2);
                        return property.boolValue;
                }

                public static bool ButtonRight (string icon, Color color, int extraPad = 0, bool execute = true, string toolTip = "")
                {
                        if (!execute)
                                return false;
                        Texture2D texture = Icon.Get(icon);
                        Rect button = new Rect(barEnd) { x = barEnd.x - texture.width - (padding + extraPad), width = texture.width + (padding + extraPad) * 2 };
                        if (toolTip != "")
                                GUI.Label(button, new GUIContent("", toolTip));
                        SpaceRight(texture.width + (padding + extraPad) * 2);
                        return button.Button(texture, color, center: true);
                }

                public static bool ButtonLeft (SerializedProperty property, string icon, Color on, Color off, int extraPad = 0, bool execute = true)
                {
                        if (!execute)
                                return false;
                        Texture2D texture = Icon.Get(icon);
                        Rect button = new Rect(barStart) { x = barStart.x, width = texture.width + (padding + extraPad) * 2 };

                        button.Toggle(property, texture, on, off);
                        SpaceLeft(texture.width + (padding + extraPad) * 2);
                        return property.boolValue;
                }

                public static SerializedProperty Get (string field)
                {
                        return property != null ? property.Get(field) : objProperty != null ? objProperty.Get(field) : null;
                }

        }

        public static class FoldOut
        {
                public static GUIStyle boldStyle => EditorGUIUtility.isProSkin ? EditorStyles.whiteBoldLabel : EditorStyles.label;
                public static Color backgroundColor => EditorGUIUtility.isProSkin ? Tint.Box : Color.white;
                public static Color titleColor => EditorGUIUtility.isProSkin ? Color.white : Color.black;
                public static Color boxColorLight => EditorGUIUtility.isProSkin ? Tint.BoxTwo : Tint.Box;
                public static Color boxColor => EditorGUIUtility.isProSkin ? Tint.SoftDark : Tint.Box;
                public static bool titleBold => EditorGUIUtility.isProSkin;
                public static Bar bar = new Bar();
                public static Texture2D background;
                public const int h = 23; // default height for bars

                public static void Initialize ()
                {
                        background = Icon.Get("BackgroundLight128x128");
                }

                public static Bar Bar (SerializedProperty newProperty, Color? color = null, int space = 8, int height = h)
                {
                        Editors.Bar.property = newProperty;
                        Editors.Bar.objProperty = null;
                        bar.SU(Editors.FoldOut.background, color ?? Tint.Blue, height);
                        bar.SL(space);
                        return bar;
                }

                public static Bar BarOffsetY (SerializedProperty newProperty, Color? color = null, int offsetY = 0, int space = 8, int height = h)
                {
                        Editors.Bar.property = newProperty;
                        Editors.Bar.objProperty = null;
                        bar.SU(Editors.FoldOut.background, color ?? Tint.Blue, height, offsetY: offsetY);
                        bar.SL(space);
                        return bar;
                }

                public static Bar Bar (SerializedObject newProperty, Texture2D texture, Color? color = null, int space = 8, int height = h, bool topSpace = true)
                {
                        Editors.Bar.objProperty = newProperty;
                        Editors.Bar.property = null;
                        bar.SU(texture, color ?? Tint.Blue, height, topSpace);
                        bar.SL(space);
                        return bar;
                }

                public static Bar Bar (SerializedObject newProperty, Color? color = null, int space = 8, int height = h)
                {
                        Editors.Bar.objProperty = newProperty;
                        Editors.Bar.property = null;
                        bar.SU(Editors.FoldOut.background, color ?? Tint.Blue, height);
                        bar.SL(space);
                        return bar;
                }

                public static Bar Bar (Color? color = null, int space = 8, int height = h)
                {
                        bar.SU(Editors.FoldOut.background, color ?? Tint.Blue, height);
                        bar.SL(space);
                        return bar;
                }

                public static void TabBarString (Texture2D texture, Color barColor, Color selected, string[] names, SerializedProperty index, LabelType type, int height = h)
                {
                        if (names.Length == 0)
                                return;

                        Layout.VerticalSpacing(1);
                        Rect rect = Layout.CreateRect(width: Layout.longInfoWidth, height: height, offsetX: -11);
                        float width = (float) Layout.longInfoWidth / (float) names.Length;

                        Layout.Pushclear();
                        for (int i = 0; i < names.Length; i++)
                        {
                                rect.Push(width);
                                if (rect.Button(names[i], texture, index.intValue == i ? selected : barColor, type, false))
                                {
                                        index.intValue = i;
                                }
                        }
                }

                public static int TabBarString (Color barColor, Color selected, string[] names, SerializedProperty index, LabelType type, int height = h)
                {
                        if (names.Length == 0)
                                return 0;

                        Rect rect = Layout.CreateRect(width: Layout.longInfoWidth, height: height, offsetX: -11);
                        int width = (int) ((float) rect.width / (float) names.Length);
                        int extra = Layout.longInfoWidth - (width * names.Length);
                        Layout.Pushclear();
                        for (int i = 0; i < names.Length; i++)
                        {
                                int realWidth = i == names.Length - 1 ? width + extra : width;
                                rect.Push(realWidth);
                                Texture2D texture = i == 0 ? Icon.Get("LeftBar") : i == names.Length - 1 ? Icon.Get("RightBar") : Icon.Get("MiddleBar");
                                if (rect.Button(names[i], texture, index.intValue == i ? selected : barColor, type, false))
                                {
                                        index.intValue = i;
                                }
                        }
                        return index.intValue;
                }

                public static bool LargeButton (string title, Color barColor, Color labelColor, Texture2D texture, int minusWidth = 0, int height = h)
                {
                        Layout.VerticalSpacing(1);
                        Rect barStart = Layout.CreateRect(width: Layout.longInfoWidth - minusWidth, height: height, offsetX: -11);
                        Rect button = new Rect(barStart);

                        Labels.textStyle.padding = Labels.rectZero;
                        Labels.textStyle.fontSize = 12;
                        Labels.textStyle.normal.textColor = labelColor;
                        Labels.textStyle.fontStyle = FontStyle.Bold;
                        Vector2 labelSize = Labels.textStyle.CalcSize(new GUIContent(title));

                        bool open = button.Button(texture, barColor);
                        GUI.Label(barStart.CenterRectContent(labelSize), title, Labels.textStyle);
                        Labels.ResetTextStyle();
                        return open;
                }

                public static void Box (int members, Color color, int extraHeight = 0, int offsetY = 0, int shortenX = 0)
                {
                        Layout.VerticalSpacing(1);
                        Layout.VerticalSpacing(0);
                        int height = (Layout.rectFieldHeight) * members + extraHeight + 10; // 10 is padding
                        Layout.GetLastRectDraw(Layout.longInfoWidth - shortenX, height, -11 + shortenX * 0.5f, offsetY, background, color);
                        Layout.VerticalSpacing(5);
                }

                public static void BoxSingle (int members, Color color, int extraHeight = 0, int offsetY = 0)
                {
                        Layout.VerticalSpacing(1);
                        Layout.VerticalSpacing(0);
                        int height = (Layout.rectFieldHeight) * members + extraHeight + 4; // 4 is padding
                        Layout.GetLastRectDraw(Layout.longInfoWidth, height, -11, offsetY, background, color);
                        Layout.VerticalSpacing(2);
                }

                public static void Box (Texture2D texture, int members, Color color, int extraHeight = 0, int offsetY = 0)
                {
                        Layout.VerticalSpacing(1);
                        Layout.VerticalSpacing(0);
                        int height = (Layout.rectFieldHeight) * members + extraHeight + 10; // 10 is padding
                        Layout.GetLastRectDraw(Layout.longInfoWidth, height, -11, offsetY, texture, color);
                        //Layout.VerticalSpacing (5);
                }

                public static bool FoldOutButton (SerializedProperty property, int offsetY = 0, string toolTip = "Events")
                {
                        Rect eventButton = Layout.CreateRect(10, 0, 0, -5 + offsetY);
                        eventButton.height = 12;
                        GUI.Label(eventButton, new GUIContent("", toolTip));
                        bool open = eventButton.Toggle(property, Icon.Get("TriangleBottom"), Tint.WarmWhite, Tint.PastelGreen);
                        Layout.VerticalSpacing(8 + offsetY);
                        return open;
                }

                public static bool FoldOutLeftButton (SerializedProperty property, Color color, int offsetX = 0, int offsetY = 0, int width = 10, int height = 20, string toolTip = "Options")
                {
                        Rect eventButton = Layout.GetLastRect(width, height, -width + offsetX, offsetY);
                        if (toolTip != "")
                                GUI.Label(eventButton, new GUIContent("", toolTip));
                        return eventButton.Toggle(property, Icon.Get("LeftButton"), color, Tint.White, center: false);
                }

                public static bool FoldOutButton (SerializedProperty property, string title, string message = "Events")
                {
                        Rect eventButton = Layout.CreateRect(10, 19, -6, 0);
                        //  eventButton.height = 11;
                        GUI.Label(eventButton, new GUIContent("", message));
                        bool open = eventButton.Toggle(property, property.boolValue ? Icon.Get("ArrowDown") : Icon.Get("ArrowRight"), Tint.WarmWhite, Tint.WarmWhite);
                        Labels.Label(title, eventButton.Adjust(12, 100));
                        Layout.VerticalSpacing(5);
                        return open;
                }

                public static bool FoldOutBoxButton (SerializedProperty property, string title, Color color, string toolTip = "")
                {
                        BoxSingle(1, color);
                        Rect eventButton = Layout.CreateRect(10, 19, -6, 0);
                        //  eventButton.height = 11;
                        if (toolTip != "")
                                GUI.Label(eventButton, new GUIContent("", toolTip));
                        bool open = eventButton.Toggle(property, property.boolValue ? Icon.Get("ArrowDown") : Icon.Get("ArrowRight"), Tint.WarmWhite, Tint.WarmWhite);
                        Labels.Label(title, eventButton.Adjust(12, 100));
                        Layout.VerticalSpacing(2);
                        return open;
                }

                public static bool CornerButton (Color color, int offsetX = 0, int offsetY = 0, string icon = "Add", bool ySpace = true)
                {
                        if (ySpace)
                                Layout.VerticalSpacing();
                        Rect rect = Layout.CreateRect(21, 21);
                        Rect button = new Rect(rect) { x = rect.x + Layout.longInfoWidth - 32 - offsetX, y = rect.y - offsetY };
                        return button.Button(Icon.Get(icon), Icon.Get("BackgroundLight"), color);
                }

                public static void CornerButtonLR (this SerializedProperty toggle, int offsetX = 0, int offsetY = 0, string icon = "Add", string tooltip = "") // last rect
                {
                        Layout.VerticalSpacing();
                        Rect rect = Layout.GetLastRect(23, 23);
                        Rect button = new Rect(rect) { x = rect.x + Layout.longInfoWidth - 34 - offsetX, y = rect.y - 23 - offsetY };
                        if (button.Button(Icon.Get(icon), Icon.Get("BackgroundLight"), toggle.boolValue ? Tint.SoftDark : Tint.Box, toolTip: tooltip))
                                toggle.Toggle();
                }

                public static bool CornerButtonLR (Color color, int offsetX = 0, int offsetY = 0, string icon = "Add", string tooltip = "") // last rect
                {
                        Layout.VerticalSpacing();
                        Rect rect = Layout.GetLastRect(23, 23);
                        Rect button = new Rect(rect) { x = rect.x + Layout.longInfoWidth - 34 - offsetX, y = rect.y - 23 - offsetY };
                        return button.Button(Icon.Get(icon), Icon.Get("BackgroundLight"), color, toolTip: tooltip);
                }

                public static bool DropDownMenu (List<string> names, SerializedProperty shift, SerializedProperty index, Texture2D[] iconArray = null, int itemLimit = 5)
                {
                        for (int i = 0; i < itemLimit; i++)
                        {
                                int trueIndex = Mathf.Clamp(i + (int) shift.floatValue, 0, names.Count);
                                Rect background = Layout.CreateRectAndDraw(Layout.longInfoWidth - 10, 22, offsetX: -11, texture: Texture2D.whiteTexture, color: Color.white);

                                if (trueIndex < names.Count)
                                {
                                        if (background.ContainsMouseUp())
                                        { index.intValue = trueIndex; return true; }
                                        if (background.ContainsMouse())
                                                background.DrawRect(color: Tint.SoftDark50);
                                        GUI.Label(new Rect(background) { x = background.x + 25 }, names[trueIndex], Editors.FoldOut.boldStyle); // display item name

                                        if (iconArray == null && index != null && trueIndex == index.intValue)
                                        {
                                                Skin.TextureCentered(new Rect(background) { width = 20 }, Icon.Get("CheckMark"), new Vector2(11, 11), Tint.SoftDark);
                                        }
                                        if (iconArray != null && trueIndex < iconArray.Length)
                                        {
                                                Skin.TextureCentered(new Rect(background) { width = 20 }, iconArray[trueIndex], new Vector2(13, 13), Tint.White);
                                        }
                                }
                                if (background.ContainsScrollWheel())
                                {
                                        float scrollValue = Mathf.Abs(Event.current.delta.y) > 3f ? 2f : 1f;
                                        shift.floatValue += Event.current.delta.y > 0 ? scrollValue : -scrollValue;
                                        shift.floatValue = Mathf.Clamp(shift.floatValue, 0, names.Count - itemLimit);
                                }
                        }
                        // reposition shift based on scrollbar position
                        float extraItems = names.Count - itemLimit;
                        float position = (float) shift.floatValue / extraItems;
                        shift.floatValue = Mathf.Clamp(extraItems * Editors.FoldOut.ScrollBar(itemLimit, 22f, names.Count, position), 0, extraItems);
                        if (names.Count <= itemLimit)
                                shift.floatValue = 0;
                        return false;
                }

                public static float ScrollBar (float items, float itemHeight, float totalCount, float percentPosition)
                {
                        float menuHeight = items * itemHeight;
                        float barHeight = (items / (totalCount == 0 ? 1f : totalCount)) * menuHeight;
                        barHeight = Mathf.Clamp(barHeight, itemHeight, menuHeight);
                        Rect vertBar = Layout.GetLastRect(10, menuHeight, offsetX: Layout.infoWidth - 5, offsetY: -menuHeight + itemHeight); // we are recreating menu from last rect
                        Rect scrollBar = new Rect(vertBar) { width = vertBar.width, height = barHeight, y = vertBar.y + (menuHeight - barHeight) * percentPosition };
                        vertBar.DrawTexture(Texture2D.whiteTexture, Tint.SoftDark50);
                        scrollBar.DrawTexture(Texture2D.whiteTexture, Tint.SoftDark50);
                        return Mouse.MouseDrag(false) ? ((Event.current.mousePosition.y) - vertBar.y) / menuHeight : percentPosition;
                }

                ///
                public static SerializedProperty Get (this System.Object obj, string field)
                {
                        if (obj is SerializedProperty property)
                        {
                                return property.FindPropertyRelative(field);
                        }
                        else if (obj is SerializedObject propertyObj)
                        {
                                return propertyObj.FindProperty(field);
                        }
                        return null;
                }

                public static int Enum (this System.Object obj, string field)
                {
                        return obj.Get(field).enumValueIndex;
                }

                public static bool Bool (this System.Object obj, string field)
                {
                        return obj.Get(field).boolValue;
                }

                public static int Int (this System.Object obj, string field)
                {
                        return obj.Get(field).intValue;
                }

                public static string String (this System.Object obj, string field)
                {
                        return obj.Get(field).stringValue;
                }

                public static SerializedObject SerializedObjet (this System.Object obj)
                {
                        if (obj is SerializedProperty ser)
                        {
                                return ser.serializedObject;
                        }
                        if (obj is SerializedObject serObj)
                        {
                                return serObj;
                        }
                        return null;
                }

                public static bool Toggle (this System.Object obj, string field)
                {
                        SerializedProperty property = obj.Get(field);
                        property.boolValue = !property.boolValue;
                        return property.boolValue;
                }
        }
        #endregion

        #region Block Handler

        public delegate void NormalCallback ();
        public delegate void IntCallback (int index);
        public delegate void IntDoubleCallback (int a, int b);
        public delegate void PropertyCallback (SerializedProperty property);

        public struct Header
        {
                public System.Object property;
                public static List<IBlockChild> child = new List<IBlockChild>();
                public static List<string> signal = new List<string>();
                public static int stretchChildren;

                public Header (System.Object property)
                {
                        this.property = property;
                        child.Clear();
                        signal.Clear();
                }

                public void Add (IBlockChild newChild)
                {
                        child.Add(newChild);
                }

                public bool Calculate ()
                {
                        Rect rect = GetMainRect();
                        bool hasFold = false;
                        bool isOpen = IsOpen(ref hasFold);

                        float totalWidth = rect.width;
                        float fixedWidth = FixedWidth(isOpen, hasFold);
                        float stretchWidth = totalWidth - fixedWidth;
                        stretchChildren = StretchableChildren();

                        if (stretchChildren > 0)
                        {
                                stretchWidth = stretchWidth / (float) stretchChildren;
                        }

                        float startPosition = rect.x;
                        for (int i = 0; i < child.Count; i++)
                        {
                                startPosition = child[i].Set(property, startPosition, rect.y, rect.height, stretchWidth, isOpen, hasFold);
                        }
                        return isOpen || !hasFold;
                }

                public Rect GetMainRect ()
                {
                        for (int i = 0; i < child.Count; i++)
                        {
                                if (child[i] is BlockStyle style)
                                {
                                        return style.rect;
                                }
                                else if (child[i] is BlockStyleRect styleRect)
                                {
                                        return styleRect.rect;
                                }
                        }
                        return Block.Rect(18);
                }

                public int FixedWidth (bool isOpen, bool hasFold)
                {
                        int totalWidth = 0;
                        for (int i = 0; i < child.Count; i++)
                        {
                                if (child[i].blockType == BlockType.Fixed && (!child[i].hidden || isOpen || !hasFold))
                                {
                                        totalWidth += child[i].fixedWidth;
                                }
                        }
                        return totalWidth;
                }

                public int StretchableChildren ()
                {
                        int total = 0;
                        for (int i = 0; i < child.Count; i++)
                        {
                                if (child[i].blockType == BlockType.Stretch)
                                {
                                        total++;
                                }
                        }
                        return total;
                }

                public bool IsOpen (ref bool hasFold)
                {
                        for (int i = 0; i < child.Count; i++)
                        {
                                if (child[i] is BlockFold fold)
                                {
                                        hasFold = true;
                                        return property.Bool(fold.foldOut);
                                }
                        }
                        return false;
                }

                public static bool SignalActive (string name)
                {
                        if (signal.Contains(name))
                        {
                                signal.Remove(name);
                                return true;
                        }
                        return false;
                }
        }

        public struct BlockStyle : IBlockChild
        {
                public Rect rect;
                public BlockType blockType => BlockType.None;
                public int fixedWidth => 0;
                public bool hidden => false;

                public BlockStyle (Color color, string background = "Header", int height = 23, int leftSpace = 8, bool noGap = false, int bottomSpace = 1, int shiftX = 0)
                {
                        rect = Block.BasicRect(width: Layout.longInfoWidth - shiftX, height: height, offsetX: -11 + shiftX, noGap: noGap, bottomSpace: bottomSpace, texture: Icon.Get(background), color: color);
                        rect.width = rect.width - leftSpace;
                        rect.x = rect.x + leftSpace;
                }

                public BlockStyle (Color color, int height = 23, int leftSpace = 8, bool noGap = false, int bottomSpace = 1, int shiftX = 0)
                {
                        rect = Block.BasicRectPlain(width: Layout.longInfoWidth - shiftX, height: height, offsetX: -11 + shiftX, noGap: noGap, bottomSpace: bottomSpace, color: color);
                        rect.width = rect.width - leftSpace;
                        rect.x = rect.x + leftSpace;
                }

                public float Set (System.Object property, float x, float y, float parentHeight, float stretchWidth, bool isOpen, bool hasFold)
                {
                        return x;
                }
        }

        public struct BlockStyleRect : IBlockChild
        {
                public Rect rect;
                public BlockType blockType => BlockType.None;
                public int fixedWidth => 0;
                public bool hidden => false;

                public BlockStyleRect (Color color, string background = "Header", bool selection = false, int height = 23, int leftSpace = 8, int bottomSpace = 1)
                {
                        rect = Block.RectField(height);
                        color = selection && rect.ContainsMouse() ? Tint.Green * Tint.WhiteOpacity180 : color;
                        Skin.Draw(rect, color, Icon.Get(background));
                        rect.width = rect.width - leftSpace;
                        rect.x = rect.x + leftSpace;
                        Block.boxRect.height = height;
                        Block.boxRect.y += bottomSpace;
                }

                public float Set (System.Object property, float x, float y, float parentHeight, float stretchWidth, bool isOpen, bool hasFold)
                {
                        return x;
                }
        }

        public struct BlockGrip : IBlockChild
        {
                public int index;
                public Editor editor;
                public Texture2D iconTexture;
                public SerializedProperty array;
                public bool upperCorner;

                public BlockType blockType => BlockType.Fixed;
                public int fixedWidth => iconTexture.width;
                public bool hidden => false;

                public static bool active;
                public static string path;
                public static int source;
                public static int sourceUsed;
                public static int destinationUsed;

                public BlockGrip (Editor editor, SerializedProperty array, int index, bool upperCorner = false)
                {
                        this.editor = editor;
                        this.array = array;
                        this.index = index;
                        this.upperCorner = upperCorner;
                        iconTexture = upperCorner ? Icon.Get("GripCorner") : Icon.Get("Grip");
                }

                public BlockGrip Set (float x, float y, int offsetY = 5, float parentHeight = 18)
                {
                        Set(null, x, y - offsetY, parentHeight, 0, true, false);
                        return this;
                }

                public float Set (System.Object property, float x, float y, float parentHeight, float stretchWidth, bool isOpen, bool hasFold)
                {
                        if (active && Mouse.anyUp)
                        {
                                active = false;
                                editor.Repaint();
                        }
                        Rect rect;
                        int offset = 2;
                        if (upperCorner)
                        {
                                offset = 7;
                                rect = new Rect(x - offset, y, iconTexture.width, iconTexture.height);
                        }
                        else
                        {
                                rect = new Rect(x - 2, y + parentHeight * 0.5f - iconTexture.height * 0.5f, iconTexture.width, iconTexture.height);
                        }
                        Color color = active && array != null && source == index && path == array.propertyPath ? Tint.Box : Tint.White;
                        Skin.Draw(rect, color, iconTexture);

                        Rect detectRect = new Rect(rect) { y = y, height = parentHeight };
                        if (detectRect.ContainsMouseDown())
                        {
                                active = true;
                                source = index;
                                path = array.propertyPath;
                        }
                        detectRect.x = 0;
                        detectRect.width = Layout.maxWidth;
                        if (active && array != null && source != index && detectRect.ContainsMouse() && path == array.propertyPath)
                        {
                                array.MoveArrayElement(source, index);
                                sourceUsed = source;
                                destinationUsed = index;
                                source = index;
                                Header.signal.Add("GripUsed");
                                editor.Repaint();
                        }
                        return x + rect.width - offset;
                }
        }

        public struct BlockImage : IBlockChild
        {
                public Texture2D iconTexture;
                public Color color;

                public BlockType blockType => BlockType.Fixed;
                public int fixedWidth => iconTexture.width;
                public bool hidden => false;

                public BlockImage (string icon, Color? color = null)
                {
                        iconTexture = Icon.Get(icon);
                        this.color = color == null ? Tint.White : color.Value;
                }

                public float Set (System.Object property, float x, float y, float parentHeight, float stretchWidth, bool isOpen, bool hasFold)
                {
                        Rect rect = new Rect(x, y + parentHeight * 0.5f - iconTexture.height * 0.5f, iconTexture.width, iconTexture.height);
                        Skin.Draw(rect, color, iconTexture);
                        return x + rect.width;
                }
        }

        public struct BlockImageTexture2D : IBlockChild
        {
                public Texture2D iconTexture;
                public Color color;
                public int size;

                public int height => size == 0 ? iconTexture.height : size;
                public BlockType blockType => BlockType.Fixed;
                public int fixedWidth => size;
                public bool hidden => false;

                public BlockImageTexture2D (Texture2D iconTexture, int size = 0, Color? color = null)
                {
                        this.size = size == 0 ? iconTexture.width : size;
                        this.iconTexture = iconTexture;
                        this.color = color == null ? Tint.White : color.Value;
                }

                public float Set (System.Object property, float x, float y, float parentHeight, float stretchWidth, bool isOpen, bool hasFold)
                {
                        Rect rect = new Rect(x, y + parentHeight * 0.5f - height * 0.5f, size, height);
                        Skin.DrawTexture(rect, iconTexture, color);
                        return x + rect.width;
                }
        }

        public struct BlockButton : IBlockChild
        {
                public Texture2D iconTexture;
                public string ID;
                public string tooltip;
                public bool hide;
                public Color color;

                public BlockType blockType => BlockType.Fixed;
                public int fixedWidth => iconTexture.width;
                public bool hidden => hide;

                public BlockButton (string icon, Color? color = null, string ID = "", string tooltip = "", bool hide = true)
                {
                        iconTexture = Icon.Get(icon);
                        this.hide = hide;
                        this.tooltip = tooltip;
                        this.ID = ID == "" ? icon : ID;
                        this.color = color == null ? Tint.White : color.Value;
                }

                public float Set (System.Object property, float x, float y, float parentHeight, float stretchWidth, bool isOpen, bool hasFold)
                {
                        if (!isOpen && hide && hasFold)
                        {
                                return x;
                        }
                        Rect rect = new Rect(x, y + parentHeight * 0.5f - iconTexture.height * 0.5f, iconTexture.width, iconTexture.height);
                        if (rect.Button(iconTexture, color))
                        {
                                Header.signal.Add(ID);
                        }
                        if (tooltip != "")
                        {
                                Block.Tooltip(rect, tooltip);
                        }

                        return x + rect.width;
                }
        }

        public struct BlockDropList : IBlockChild
        {
                public string field;
                public List<string> names;

                public BlockType blockType => BlockType.Stretch;
                public int fixedWidth => 0;
                public bool hidden => false;

                public BlockDropList (string field, List<string> names)
                {
                        this.field = field;
                        this.names = names;
                }

                public float Set (System.Object property, float x, float y, float parentHeight, float stretchWidth, bool isOpen, bool hasFold)
                {
                        Rect rect = new Rect(x, y + 3, stretchWidth, parentHeight - 6);
                        property.DropList(rect, field, names);
                        return x + rect.width;
                }
        }

        public struct BlockToggle : IBlockChild
        {
                public bool hide;
                public string field;
                public Color colorOn;
                public Color colorOff;
                public Texture2D iconTexture;

                public BlockType blockType => BlockType.Fixed;
                public int fixedWidth => iconTexture.width;
                public bool hidden => hide;

                public BlockToggle (string field, string icon, Color colorOn, Color colorOff, bool hide)
                {
                        this.hide = hide;
                        this.field = field;
                        this.colorOn = colorOn;
                        this.colorOff = colorOff;
                        iconTexture = Icon.Get(icon);
                }

                public float Set (System.Object property, float x, float y, float parentHeight, float stretchWidth, bool isOpen, bool hasFold)
                {
                        if (!isOpen && hide && hasFold)
                        {
                                return x;
                        }
                        Rect rect = new Rect(x, y + parentHeight * 0.5f - (iconTexture.height + 10) * 0.5f, iconTexture.width, (iconTexture.height + 10)); // add 10 make detection area bigger
                        rect.Toggle(property.Get(field), iconTexture, colorOn, colorOff);
                        return x + rect.width;
                }
        }

        public struct BlockTabButton : IBlockChild
        {
                public bool hide;
                public int index;
                public string field;
                public string tooltip;
                public Color colorOn;
                public Color colorOff;
                public Texture2D iconTexture;

                public BlockType blockType => BlockType.Fixed;
                public int fixedWidth => iconTexture.width;
                public bool hidden => hide;

                public BlockTabButton (string field, int index, string icon, string tooltip, Color colorOn, Color colorOff, bool hide)
                {
                        this.hide = hide;
                        this.index = index;
                        this.field = field;
                        this.tooltip = tooltip;
                        this.colorOn = colorOn;
                        this.colorOff = colorOff;
                        iconTexture = Icon.Get(icon);
                }

                public float Set (System.Object property, float x, float y, float parentHeight, float stretchWidth, bool isOpen, bool hasFold)
                {
                        if (!isOpen && hide && hasFold)
                        {
                                return x;
                        }
                        Rect rect = new Rect(x, y + parentHeight * 0.5f - iconTexture.height * 0.5f, iconTexture.width, iconTexture.height);
                        bool on = property.Int(field) == index;
                        if (rect.Button(iconTexture, on ? colorOn : colorOff, toolTip: tooltip))
                        {
                                property.Get(field).intValue = property.Int(field) == index ? -1 : index;
                        }
                        return x + rect.width;
                }
        }

        public struct BlockEnable : IBlockChild
        {
                public string icon;
                public string field;
                public Texture2D iconTexture;

                public BlockType blockType => BlockType.Fixed;
                public int fixedWidth => iconTexture.width;
                public bool hidden => false;

                public BlockEnable (string field, string icon = "Open")
                {
                        this.field = field;
                        this.icon = icon;
                        iconTexture = Icon.Get(icon);
                }

                public float Set (System.Object property, float x, float y, float parentHeight, float stretchWidth, bool isOpen, bool hasFold)
                {
                        Rect rect = new Rect(x, y + parentHeight * 0.5f - (iconTexture.height + 10) * 0.5f, iconTexture.width, (iconTexture.height + 10));
                        rect.Toggle(property.Get(field), iconTexture, Tint.PastelGreen, Tint.White);
                        GUI.enabled = isOpen ? property.Bool(field) : GUI.enabled;
                        return x + rect.width;
                }
        }

        public struct BlockStretch : IBlockChild
        {
                public BlockType blockType => BlockType.Stretch;
                public int fixedWidth => 0;
                public bool hidden => false;

                public float Set (System.Object property, float x, float y, float parentHeight, float stretchWidth, bool isOpen, bool hasFold)
                {
                        return x + stretchWidth;
                }
        }

        public struct BlockSpace : IBlockChild
        {
                public int space;
                public BlockType blockType => BlockType.Fixed;
                public int fixedWidth => space;
                public bool hidden => false;

                public BlockSpace (int space)
                {
                        this.space = space;
                }

                public float Set (System.Object property, float x, float y, float parentHeight, float stretchWidth, bool isOpen, bool hasFold)
                {
                        return x + space;
                }
        }

        public struct BlockField : IBlockChild
        {
                public string field;
                public float weight;
                public float offsetY;
                public bool invert;
                public bool hide;
                public BlockType blockType => BlockType.Stretch;
                public int fixedWidth => 0;
                public bool hidden => false;

                public BlockField (string field, float weight = 1f, bool invert = false, int offsetY = 0, bool hide = false)
                {
                        this.field = field;
                        this.weight = weight;
                        this.offsetY = offsetY;
                        this.invert = invert;
                        this.hide = hide;
                }

                public float Set (System.Object property, float x, float y, float parentHeight, float stretchWidth, bool isOpen, bool hasFold)
                {
                        Rect rect = new Rect() { x = x, y = y + 2 + offsetY, width = stretchWidth * Header.stretchChildren * weight, height = parentHeight - 4 };
                        if (!isOpen && hide && hasFold)
                        {
                                return x + rect.width;
                        }

                        if (invert)
                        {
                                SerializedProperty number = property.Get(field);
                                number.floatValue = 1f / EditorGUI.FloatField(rect, 1f / number.floatValue);
                        }
                        else
                        {
                                EditorGUI.PropertyField(rect, property.Get(field), GUIContent.none);
                        }
                        return x + rect.width;
                }
        }

        public struct BlockFieldRaw : IBlockChild
        {
                public float weight;
                public float offsetY;
                public SerializedProperty property;

                public BlockType blockType => BlockType.Stretch;
                public int fixedWidth => 0;
                public bool hidden => false;

                public BlockFieldRaw (SerializedProperty property, float weight = 1f, int offsetY = 0)
                {
                        this.property = property;
                        this.weight = weight;
                        this.offsetY = offsetY;
                }

                public float Set (System.Object property, float x, float y, float parentHeight, float stretchWidth, bool isOpen, bool hasFold)
                {
                        Rect rect = new Rect() { x = x, y = y + 2 + offsetY, width = stretchWidth * Header.stretchChildren * weight, height = parentHeight - 4 };
                        EditorGUI.PropertyField(rect, this.property, GUIContent.none);
                        return x + rect.width;
                }
        }

        public struct BlockFold : IBlockChild
        {
                public string label;
                public string foldOut;
                public Color color;
                public bool bold;
                public BlockType blockType => BlockType.Stretch;
                public int fixedWidth => 0;
                public bool hidden => false;

                public BlockFold (string label, string foldOut = "foldOut", bool bold = false, Color? color = null)
                {
                        this.label = label;
                        this.foldOut = foldOut;
                        this.bold = bold;
                        this.color = color != null ? color.Value : Color.black;
                }

                public float Set (System.Object property, float x, float y, float parentHeight, float stretchWidth, bool isOpen, bool hasFold)
                {
                        Labels.textStyle.padding = Labels.rectZero;
                        Labels.textStyle.fontSize = (int) 12;
                        Labels.textStyle.normal.textColor = color;
                        Labels.textStyle.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
                        Labels.textStyle.clipping = TextClipping.Clip;
                        Rect rect = new Rect(x, y + parentHeight * 0.5f - 8, stretchWidth, parentHeight);

                        GUI.Label(rect, label, Labels.textStyle);
                        Labels.ResetTextStyle();
                        rect.y = y;
                        if (rect.ContainsMouseDown())
                        {
                                property.Toggle(foldOut);
                        }
                        return x + stretchWidth;
                }
        }

        public struct BlockLabel : IBlockChild
        {
                public string label;
                public int size;
                public Color color;
                public bool bold;
                public BlockType blockType => BlockType.Stretch;
                public int fixedWidth => 0;
                public bool hidden => false;

                public BlockLabel (string label, int size, bool bold = false, Color? color = null)
                {
                        this.label = label;
                        this.size = size;
                        this.bold = bold;
                        this.color = color != null ? color.Value : Color.black;
                }

                public float Set (System.Object property, float x, float y, float parentHeight, float stretchWidth, bool isOpen, bool hasFold)
                {
                        Labels.textStyle.padding = Labels.rectZero;
                        Labels.textStyle.fontSize = size;
                        Labels.textStyle.normal.textColor = color;
                        Labels.textStyle.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
                        Labels.textStyle.clipping = TextClipping.Clip;
                        Rect rect = new Rect(x, y + parentHeight * 0.5f - 8, stretchWidth, parentHeight);
                        GUI.Label(rect, label, Labels.textStyle);
                        return x + stretchWidth;
                }
        }

        public enum BlockType
        {
                Fixed,
                Stretch,
                None
        }

        public interface IBlockChild
        {
                public float Set (System.Object property, float x, float y, float parentHeight, float stretchWidth, bool isOpen, bool hasFold);
                BlockType blockType { get; }
                int fixedWidth { get; }
                bool hidden { get; }
        }

        public static class evt
        {
                public static int filter = 0;
                public static int scrollDelta => Event.current.delta.y > 0 ? 1 : -1;
                public static bool mouseUp => Event.current.type == EventType.MouseUp;
                public static bool mouseDrag => Event.current.type == EventType.MouseDrag && Event.current.button == 0;
                public static Vector2 mouse => Event.current.mousePosition;
                public static bool isLayout => Event.current.type == EventType.Layout;
                public static bool downArrow => Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.DownArrow;
                public static bool upArrow => Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.UpArrow;

        }

        public static class Drawing
        {


                public static Rect DrawBasicRect (this Rect rect, Color color)
                {
                        EditorGUI.DrawRect(rect, color);
                        return rect;
                }

                public static float RowTint (this int number, float tintA, float tintB)
                {
                        return number % 2 == 0 ? tintA : tintB;
                }
        }

        public static class Block
        {
                public static Texture2D background;
                public static Rect boxRect = new Rect();
                public static Rect boxRectOrigin = new Rect();
                // helpers
                //
                public static Color hardDarkToggle => EditorGUIUtility.isProSkin ? Tint.WarmWhite : Tint.HardDark;

                //  public static Color hardDarkToggle => EditorGUIUtility.isProSkin ? Tint.WarmWhite : Tint.HardDark;

                //
                public static void Initialize ()
                {
                        background = Icon.Get("BackgroundLight128x128");
                }

                public static Header Header (System.Object property = null)
                {
                        return new Header(property);
                }

                public static CatalogList CatalogList (int maxItemsInWindow, int itemHeight, bool scrollBar = true, bool handleColorGrey = true)
                {
                        return new CatalogList(maxItemsInWindow, itemHeight, scrollBar, handleColorGrey);
                }

                public static CatalogProperty CatalogProperty (int maxItemsInWindow, int itemHeight, bool scrollBar = true, bool handleColorGrey = true)
                {
                        return new CatalogProperty(maxItemsInWindow, itemHeight, scrollBar, handleColorGrey);
                }


                public static Rect Rect (int height, int bottomSpace = 1, int shiftX = 0)
                {
                        boxRect = boxRectOrigin = Layout.Set(GUILayoutUtility.GetRect(0, height + bottomSpace), Layout.longInfoWidth - shiftX, height, -11 + shiftX, 0);
                        return boxRect;
                }

                public static Rect BasicRect (float width, float height, float offsetX = 0, bool noGap = false, float bottomSpace = 0, Texture2D texture = default(Texture2D), Color color = default(Color))
                {
                        int offset = noGap ? 2 : 0;
                        // creating a gui rect that is near the same size as layout maxwidth will trigger a horizontal bar :( but this only happens if it's inside a guilayout area
                        // to avoid all issues in any scenario, create gui with an initial witdh of zero, then reset to desired size. this seems to fix the problem.
                        boxRect = boxRectOrigin = Layout.Set(GUILayoutUtility.GetRect(0, height - offset + bottomSpace), width, height, offsetX, -offset);
                        Skin.Draw(boxRect, color == Color.clear ? Color.white : color, texture);
                        return boxRect;
                }

                public static Rect BasicRectPlain (float width, float height, float offsetX = 0, bool noGap = false, float bottomSpace = 0, Color color = default(Color))
                {
                        int offset = noGap ? 2 : 0;
                        // creating a gui rect that is near the same size as layout maxwidth will trigger a horizontal bar :( but this only happens if it's inside a guilayout area
                        // to avoid all issues in any scenario, create gui with an initial witdh of zero, then reset to desired size. this seems to fix the problem.
                        boxRect = boxRectOrigin = Layout.Set(GUILayoutUtility.GetRect(0, height - offset + bottomSpace), width, height, offsetX, -offset);
                        EditorGUI.DrawRect(boxRect, color == Color.clear ? Color.white : color);
                        return boxRect;
                }

                public static Rect Box (int members, Color color, bool noGap = false, int bottomSpace = 1, int extraHeight = 0, int padding = 10)
                {
                        int offset = noGap ? 2 : 0;
                        int height = (Layout.rectFieldHeight) * members + extraHeight + padding;

                        boxRect = boxRectOrigin = Layout.Set(GUILayoutUtility.GetRect(0, height - offset + bottomSpace), Layout.longInfoWidth, height, -11, -offset);
                        Skin.Draw(boxRect, color == Color.clear ? Color.white : color, background);

                        boxRect.y += 5;
                        boxRect.x += 7;
                        return boxRect;
                }

                public static Rect BoxPlain (int members, Color color, bool noGap = false, int bottomSpace = 1, int extraHeight = 0, int padding = 10, int shiftX = 0)
                {
                        int offset = noGap ? 2 : 0;
                        int height = (Layout.rectFieldHeight) * members + extraHeight + padding;

                        boxRect = boxRectOrigin = Layout.Set(GUILayoutUtility.GetRect(0, height - offset + bottomSpace), Layout.longInfoWidth, height, -11, -offset);
                        boxRect.x += shiftX;
                        boxRect.width -= shiftX;
                        EditorGUI.DrawRect(boxRect, color == Color.clear ? Color.white : color);

                        boxRect.y += 5;
                        boxRect.x += 7;
                        return boxRect;
                }

                public static void Tooltip (this Rect rect, string tooltip)
                {
                        GUI.Label(rect, new GUIContent("", tooltip));
                }

                public static bool ExtraFoldout (System.Object property, string field, string tooltip = "Events")
                {
                        Rect eventButton = new Rect(boxRectOrigin) { x = boxRectOrigin.x + 10, y = boxRectOrigin.y + boxRectOrigin.height - 13, width = 12, height = 12 };
                        if (tooltip != "")
                        {
                                GUI.Label(eventButton, new GUIContent("", tooltip));
                        }
                        return eventButton.Toggle(property.Get(field), Icon.Get("TriangleBottom"), Tint.WarmWhite, Tint.PastelGreen);
                }

                public static Rect BoxArray (SerializedProperty array, Color color, int height, bool noGap, int bottomSpace, string tooltip, IntDoubleCallback callback)
                {
                        int offset = noGap ? 2 : 0;
                        int totalHeight = array.arraySize == 0 ? height : height * array.arraySize;

                        boxRect = boxRectOrigin = Layout.Set(GUILayoutUtility.GetRect(0, totalHeight - offset + bottomSpace), Layout.longInfoWidth, totalHeight, -11, -offset);
                        Skin.Draw(boxRect, color == Color.clear ? Color.white : color, background);

                        if (tooltip != "")
                        {
                                GUI.Label(boxRect, new GUIContent("", tooltip));
                        }
                        if (array.arraySize == 0)
                        {
                                Rect rect = new Rect(boxRect) { x = boxRect.width - 12, y = boxRect.y + 6, width = 11, height = 11 }; //x, y + parentHeight * 0.5f - iconTexture.height * 0.5f, iconTexture.width, iconTexture.height);
                                if (rect.Button(Icon.Get("Add"), Tint.White))
                                {
                                        array.arraySize++;
                                }
                        }
                        for (int i = 0; i < array.arraySize; i++)
                        {
                                callback.Invoke(height, i);
                        }
                        return boxRect;
                }

                public static BlockGrip CornerGrip (Editor editor, SerializedProperty array, int index, float x = 0, float y = 0, float parentHeight = 18)
                {
                        return new BlockGrip(editor, array, index, true).Set(x == 0 ? Block.boxRect.x : x, y == 0 ? Block.boxRect.y : y);
                }

                public static Rect Single (Color color, bool noGap = false, int bottomSpace = 1)
                {
                        int yOffset = noGap ? 2 : 0;
                        int height = 23;
                        boxRect = boxRectOrigin = Layout.Set(GUILayoutUtility.GetRect(0, height - yOffset + bottomSpace), Layout.longInfoWidth, height, -11, -yOffset);
                        Skin.Draw(boxRect, color == Color.clear ? Color.white : color, background);
                        boxRect.y += 2;
                        boxRect.x += 7;
                        boxRect.height -= 4;
                        return boxRect;
                }

                public static void HelperText (string title, float rightSpacing = 2, bool execute = true, int yOffset = 2)
                {
                        if (!execute)
                                return;
                        Rect rect = HelperField();
                        rect.y += yOffset;
                        Labels.ResetTextStyle();
                        float title1Width = Labels.textStyle.CalcSize(new GUIContent(title)).x + 15;
                        EditorGUI.LabelField(rect.Adjust(rect.width - title1Width - rightSpacing, rect.width), title, Labels.textStyle);
                }

                public static bool InputAndButtonBox (string title, string icon, Color color, ref string name, int buttonWidth = 15)
                {
                        Rect rect = Single(color, false, 1);
                        Rect rectLabel = rect;
                        Labels.Label(rectLabel.Adjust(0, Layout.labelWidth, offsetY: 2), title, Tint.White);
                        name = EditorGUI.TextField(rect.Adjust(Layout.labelWidth, Layout.contentWidth - buttonWidth + 2), name);
                        return Buttons.Button(rect.Adjust(Layout.contentWidth - buttonWidth + 6, buttonWidth), Icon.Get(icon), Tint.White, center: true);
                }

                public static Rect RectField (int height = 0)
                {
                        float realHeight = height == 0 ? Layout.rectFieldHeight : height;
                        Rect rect = new Rect(Block.boxRect) { height = realHeight };
                        Block.boxRect.y += realHeight;
                        return rect;
                }

                public static Rect HelperField (int height = 0)
                {
                        Rect rect = new Rect(Block.boxRect) { height = height == 0 ? Layout.rectFieldHeight : height }; // Layout.CreateRectField();
                        rect.y -= height == 0 ? Layout.rectFieldHeight : height;
                        return rect;
                }

                // fields

                public static void Field_ (this System.Object property, string title, string field, bool execute = true, bool bold = false)
                {
                        if (!execute)
                                return;
                        Rect rect = Block.RectField();
                        Labels.Label(title, rect.Adjust(0, Layout.labelWidth), bold);
                        rect.height -= 3;
                        EditorGUI.PropertyField(rect.Adjust(Layout.labelWidth, Layout.contentWidth), property.Get(field), GUIContent.none);
                }

                public static void Field_ (this System.Object property, string title, bool execute = true, bool bold = false)
                {
                        if (!execute)
                                return;
                        Rect rect = Block.RectField();
                        Labels.Label(title, rect.Adjust(0, Layout.labelWidth), bold);
                        rect.height -= 3;
                        EditorGUI.PropertyField(rect.Adjust(Layout.labelWidth, Layout.contentWidth), property as SerializedProperty, GUIContent.none);
                }

                public static void FieldAndEnum_ (this System.Object property, string title, string field, string enumA, bool execute = true)
                {
                        if (!execute)
                                return;
                        Rect rect = Block.RectField();
                        Labels.Label(title, rect.Adjust(0, Layout.labelWidth));
                        rect.height -= 3;
                        EditorGUI.PropertyField(rect.Adjust(Layout.labelWidth, Layout.contentWidth - Layout.boolWidth - 5), property.Get(field), GUIContent.none);
                        EditorGUI.PropertyField(rect.Adjust(Layout.contentWidth - Layout.boolWidth - 5, Layout.boolWidth + 5), property.Get(enumA), GUIContent.none);
                }

                public static void FieldDouble_ (this System.Object property, string title, string fieldA, string fieldB, bool execute = true, bool bold = false)
                {
                        if (!execute)
                                return;
                        Rect rect = Block.RectField();
                        Labels.Label(title, rect.Adjust(0, Layout.labelWidth), bold);
                        rect.height -= 3;
                        EditorGUI.PropertyField(rect.Adjust(Layout.labelWidth, (Layout.contentWidth / 2f)), property.Get(fieldA), GUIContent.none);
                        EditorGUI.PropertyField(rect.Adjust((Layout.contentWidth / 2f), (Layout.contentWidth / 2f)), property.Get(fieldB), GUIContent.none);
                }

                public static void FieldAndEnable_ (this System.Object property, string title, string field, string toggle, bool execute = true)
                {
                        if (!execute)
                                return;
                        Rect rect = Block.RectField();
                        Layout.BeginGUIEnable(property.Bool(toggle));
                        Labels.Label(title, rect.Adjust(0, Layout.labelWidth));
                        EditorGUI.PropertyField(rect.Adjust(Layout.labelWidth, Layout.contentWidth - Layout.boolWidth), property.Get(field), GUIContent.none);
                        Layout.EndGUIEnable(rect.Adjust(Layout.contentWidth - Layout.boolWidth + 1, Layout.boolWidth), property.Get(toggle));
                }

                public static void FieldToggleAndEnable_ (this System.Object property, string title, string field, int toggleOffset = 0)
                {
                        Rect rect = Block.RectField();
                        Layout.BeginGUIEnable(property.Bool(field));
                        Labels.Label(title, rect.Adjust(0, Block.boxRect.width));
                        Layout.EndGUIEnable();
                        EditorGUI.PropertyField(rect.Adjust(Layout.totalWidth - Layout.boolWidth - toggleOffset + 1, Layout.boolWidth), property.Get(field), GUIContent.none);
                }

                public static bool FieldAndButton_ (this System.Object property, string title, string field, string icon, int space = 0, int buttonWidth = 15, string toolTip = "")
                {
                        Rect rect = Block.RectField();
                        Labels.Label(title, rect.Adjust(space, Layout.labelWidth));
                        rect.height -= 3;
                        EditorGUI.PropertyField(rect.Adjust(Layout.labelWidth, Layout.contentWidth - buttonWidth), property.Get(field), GUIContent.none);
                        return Buttons.Button(rect.Adjust(Layout.contentWidth - buttonWidth + 3, buttonWidth), Icon.Get(icon), Tint.White, center: true, toolTip: toolTip);
                }

                public static void Slider_ (this System.Object property, string title, string field, float min = 0, float max = 1, float round = 0f)
                {
                        SerializedProperty currentField = property.Get(field);
                        Rect rect = Block.RectField();
                        Labels.Label(title, rect.Adjust(0, Layout.labelWidth));
                        rect = rect.Adjust(Layout.labelWidth, Layout.contentWidth);
                        currentField.floatValue = EditorGUI.Slider(rect, currentField.floatValue, min, max);
                        if (round != 0)
                        {
                                currentField.floatValue = Compute.Round(currentField.floatValue, round);
                        }
                }

                public static void MinMaxSlider_ (this System.Object property, string title, string fieldA, string fieldB, float min = 0, float max = 1)
                {
                        SerializedProperty minField = property.Get(fieldA);
                        SerializedProperty maxField = property.Get(fieldB);
                        Rect rect = Block.RectField();
                        Labels.Label(title, rect.Adjust(0, Layout.labelWidth));
                        rect = rect.Adjust(Layout.labelWidth, Layout.contentWidth);
                        float minOut = minField.floatValue;
                        float maxOut = maxField.floatValue;
                        EditorGUI.MinMaxSlider(rect, GUIContent.none, ref minOut, ref maxOut, min, max);
                        minField.floatValue = minOut;
                        maxField.floatValue = maxOut;
                }

                public static void SliderInt_ (this System.Object property, string title, string field, int min = 0, int max = 1)
                {
                        SerializedProperty currentField = property.Get(field);
                        Rect rect = Block.RectField();
                        Labels.Label(title, rect.Adjust(0, Layout.labelWidth));
                        rect = rect.Adjust(Layout.labelWidth, Layout.contentWidth);
                        currentField.intValue = EditorGUI.IntSlider(rect, currentField.intValue, min, max);
                }

                public static void DropDownList_ (this System.Object property, List<string> names, string title, string nameField, bool execute = true, float space = 0, int buttonWidth = 15)
                {
                        if (!execute)
                                return;
                        Rect rect = Block.RectField();
                        Labels.Label(title, rect.Adjust(space, Layout.labelWidth));
                        rect.height -= 2;
                        property.DropList(rect.Adjust(Layout.labelWidth, Layout.contentWidth), nameField, names);
                }

                public static void DropDownDoubleList_ (this System.Object property, List<string> names, string title, string nameFieldA, string nameFieldB, bool execute = true, float space = 0, int buttonWidth = 15)
                {
                        if (!execute)
                                return;
                        Rect rect = Block.RectField();
                        Labels.Label(title, rect.Adjust(space, Layout.labelWidth));
                        rect.height -= 2;
                        float width = (Layout.contentWidth) / 2f;
                        property.DropList(rect.Adjust(Layout.labelWidth, width - 1), nameFieldA, names);
                        property.DropList(rect.Adjust(width + 1, width), nameFieldB, names);
                }

                public static bool DropDownListAndButton_ (this System.Object property, List<string> names, string title, string nameField, string icon, bool execute = true, float space = 0, int buttonWidth = 15)
                {
                        if (!execute)
                                return false;
                        Rect rect = Block.RectField();
                        Labels.Label(title, rect.Adjust(space, Layout.labelWidth));
                        rect.height -= 2;
                        property.DropList(rect.Adjust(Layout.labelWidth, Layout.contentWidth - buttonWidth + 2), nameField, names);
                        return Buttons.Button(rect.Adjust(Layout.contentWidth - buttonWidth + 4, buttonWidth - 2), Icon.Get(icon), Tint.White, center: true);
                }

        }

        public static class BlockExtensions
        {
                public static Header MouseDown (this Header block, NormalCallback callback)
                {
                        if (Block.boxRect.ContainsMouseDown(false))
                        {
                                callback.Invoke();
                        }
                        return block;
                }

                public static Header MouseDown (this Header block, int heightOffset, NormalCallback callback)
                {
                        Rect rect = Block.boxRect;
                        rect.y -= heightOffset;
                        if (rect.ContainsMouseDown(false))
                        {
                                callback.Invoke();
                        }
                        return block;
                }

                public static Header Style (this Header block, Color color, string background = "Header", int leftSpace = 8, int height = 23, bool noGap = false, int bottomSpace = 1, int shiftX = 0)
                {
                        block.Add(new BlockStyle(color, background, height, leftSpace, noGap, bottomSpace, shiftX));
                        return block;
                }

                public static Header StylePlain (this Header block, Color color, int leftSpace = 8, int height = 23, bool noGap = false, int bottomSpace = 1, int shiftX = 0)
                {
                        block.Add(new BlockStyle(color, height, leftSpace, noGap, bottomSpace, shiftX));
                        return block;
                }

                public static Header BoxRect (this Header block, Color color, string background = "ItemLight", bool selection = false, int leftSpace = 8, int height = 18, int bottomSpace = 0)
                {
                        block.Add(new BlockStyleRect(color, background, selection, height, leftSpace, bottomSpace));
                        return block;
                }

                public static Header Image (this Header block, string icon, Color? color = null, bool execute = true)
                {
                        if (execute)
                        {
                                block.Add(new BlockImage(icon, color));
                                block.Add(new BlockSpace(6));
                        }
                        return block;
                }

                public static Header Image (this Header block, Texture2D icon, int size = 0, Color? color = null, bool execute = true)
                {
                        if (execute)
                        {
                                block.Add(new BlockImageTexture2D(icon, size, color));
                                block.Add(new BlockSpace(6));
                        }
                        return block;
                }

                public static Header Grip (this Header block, Editor editor, SerializedProperty array, int index, bool execute = true)
                {
                        if (execute)
                        {
                                block.Add(new BlockGrip(editor, array, index));
                                block.Add(new BlockSpace(6));
                        }
                        return block;
                }

                public static Header TabButton (this Header block, string field, int index, string icon, string tooltip, Color? colorOn = null, Color? colorOff = null, bool hide = true)
                {
                        block.Add(new BlockTabButton(field, index, icon, tooltip, colorOn == null ? Tint.Blue : colorOn.Value, colorOff == null ? Tint.White : colorOff.Value, hide));
                        block.Add(new BlockSpace(6));
                        return block;
                }

                public static Header Toggle (this Header block, string field, string icon = "Add", Color? colorOn = null, Color? colorOff = null, bool hide = true, bool execute = true)
                {
                        if (execute)
                        {
                                block.Add(new BlockToggle(field, icon, colorOn == null ? Tint.White : colorOn.Value, colorOff == null ? Tint.White : colorOff.Value, hide));
                                block.Add(new BlockSpace(6));
                        }
                        return block;
                }

                public static Header DropList (this Header block, string field, List<string> names, int spaceRight = 6)
                {
                        block.Add(new BlockDropList(field, names));
                        block.Add(new BlockSpace(spaceRight));
                        return block;
                }

                public static Header DropArrow (this Header block, string field = "foldOut", Color? colorOn = null, Color? colorOff = null, bool hide = false, bool execute = true)
                {
                        if (execute)
                        {
                                bool isOpen = block.property.Bool(field);
                                block.Add(new BlockToggle(field, isOpen ? "ArrowDown" : "ArrowRight", colorOn == null ? Tint.White : colorOn.Value, colorOff == null ? Tint.White : colorOff.Value, hide));
                                block.Add(new BlockSpace(5));
                        }
                        return block;
                }

                public static Header Button (this Header block, string icon = "Add", Color? color = null, string ID = "", string tooltip = "", bool hide = true, bool execute = true)
                {
                        if (!execute)
                                return block;
                        block.Add(new BlockButton(icon, color, ID, tooltip, hide));
                        block.Add(new BlockSpace(6));
                        return block;
                }
                public static Header ArrayButtons (this Header block)
                {
                        block.Button("xsAdd");
                        block.Button("xsMinus");
                        return block;
                }

                public static Header ReadArrayButtons (this Header block, SerializedProperty array, int index)
                {
                        if (Header.SignalActive("xsAdd"))
                        {
                                array.InsertArrayElementAtIndex(index);
                        }
                        if (Header.SignalActive("xsMinus"))
                        {
                                array.DeleteArrayElement(index);
                        }
                        return block;
                }

                public static Header HiddenButton (this Header block, string condition, string icon, Color? color = null, string ID = "", string tooltip = "", bool hide = true, bool execute = true)
                {
                        if (execute && block.property.Bool(condition))
                        {
                                block.Add(new BlockButton(icon, color, ID, tooltip, hide));
                                block.Add(new BlockSpace(6));
                        }
                        return block;
                }

                public static Header Enable (this Header block, string field, string icon = "Open")
                {
                        block.Add(new BlockEnable(field, icon));
                        block.Add(new BlockSpace(6));
                        return block;
                }

                public static Header Fold (this Header block, string label, string foldOut = "foldOut", bool bold = false, Color? color = null)
                {
                        block.Add(new BlockFold(label, foldOut, bold, color));
                        return block;
                }

                public static Header Label (this Header block, string label, int size = 12, bool bold = false, Color? color = null)
                {
                        block.Add(new BlockLabel(label, size, bold, color));
                        return block;
                }


                public static Header Field (this Header block, string field, float weight = 1f, bool invert = false, int rightSpace = 1, int yoffset = 0, bool hide = false, bool execute = true)
                {
                        if (execute)
                        {
                                block.Add(new BlockField(field, weight, invert, yoffset, hide));
                                block.Add(new BlockSpace(rightSpace));
                        }
                        return block;
                }

                public static Header Field (this Header block, SerializedProperty property, float weight = 1f, int rightSpace = 1, int yoffset = 0)
                {
                        block.Add(new BlockFieldRaw(property, weight, yoffset));
                        block.Add(new BlockSpace(rightSpace));
                        return block;
                }

                public static Header Space (this Header block, int space = 5, bool execute = true)
                {
                        if (execute)
                        {
                                block.Add(new BlockSpace(space));
                        }
                        return block;
                }

                public static bool Build (this Header block)
                {
                        return block.Calculate();
                }

                public static Header BuildGet (this Header block)
                {
                        block.Calculate();
                        return block;
                }
        }


        [System.Serializable]
        public struct CatalogProperty
        {
                [SerializeField] public int itemHeight;
                [SerializeField] public int maxItems;
                [SerializeField] public bool handleColorGrey;
                [SerializeField] public bool scrollBar;

                public static List<SerializedProperty> list = new List<SerializedProperty>();

                public CatalogProperty (int maxItemsInWindow, int itemHeight, bool scrollBar = true, bool handleColorGrey = true)
                {
                        list.Clear();
                        this.itemHeight = itemHeight;
                        this.maxItems = maxItemsInWindow;
                        this.scrollBar = scrollBar;
                        this.handleColorGrey = handleColorGrey;
                }

                public CatalogProperty Register (SerializedProperty array, bool canDelete = false)
                {
                        for (int i = 0; i < array.arraySize; i++)
                        {
                                SerializedProperty item = array.Element(i);
                                if (item == null)
                                {
                                        continue;
                                }
                                if (canDelete && item.Get("canDelete") != null)
                                {
                                        item.Get("canDelete").boolValue = canDelete;
                                }
                                list.Add(item);
                        }
                        return this;
                }

                public CatalogProperty Scroll (Editor editor, int scrollSpeed, SerializedProperty scrollIndex, PropertyCallback callback)
                {
                        Rect window = Block.Rect(maxItems * itemHeight, 0);
                        if (scrollBar)
                        {
                                Block.boxRect.width -= 10;
                                Scrollbar.Run(editor, Block.boxRect, maxItems, itemHeight, scrollSpeed, list.Count, scrollIndex, handleColorGrey);

                        }
                        if (window.ContainsScrollWheel())
                        {
                                scrollIndex.intValue = scrollIndex.intValue + evt.scrollDelta * scrollSpeed;
                        }
                        int mainIndex = scrollIndex.intValue = Mathf.Clamp(scrollIndex.intValue, 0, Mathf.Max(0, list.Count - maxItems));

                        for (int i = 0; i < maxItems; i++)
                        {
                                if (mainIndex < list.Count)
                                {
                                        callback.Invoke(list[mainIndex++]);
                                        continue;
                                }
                                break;
                        }
                        return this;
                }

                public CatalogProperty Search (Editor editor, string searchFilter, PropertyCallback callback)
                {
                        int size = 0;
                        for (int i = 0; i < list.Count; i++)
                        {
                                if (list[i].String("name").ToLower().Contains(searchFilter.ToLower()))
                                {
                                        size++;
                                }
                        }
                        Rect window = Block.Rect(size * itemHeight, 0);

                        for (int i = 0; i < list.Count; i++)
                        {
                                if (list[i].String("name").ToLower().Contains(searchFilter.ToLower()))
                                {
                                        callback.Invoke(list[i]);
                                }
                        }
                        return this;
                }

        }

        [System.Serializable]
        public struct CatalogList
        {
                [SerializeField] public int itemHeight;
                [SerializeField] public int maxItems;
                [SerializeField] public bool handleColorGrey;
                [SerializeField] public bool scrollBar;

                public CatalogList (int maxItemsInWindow, int itemHeight, bool scrollBar = true, bool handleColorGrey = true)
                {
                        this.itemHeight = itemHeight;
                        this.maxItems = maxItemsInWindow;
                        this.scrollBar = scrollBar;
                        this.handleColorGrey = handleColorGrey;
                }

                public CatalogList Scroll (Editor editor, int scrollSpeed, int size, SerializedProperty scrollIndex, IntDoubleCallback callback)
                {
                        int realSize = Mathf.Min(maxItems, size);
                        Rect window = Block.Rect(realSize * itemHeight, 0);
                        if (scrollBar)
                        {
                                Block.boxRect.width -= 10;
                                Scrollbar.Run(editor, Block.boxRect, maxItems, itemHeight, scrollSpeed, size, scrollIndex, handleColorGrey);
                        }
                        if (window.ContainsScrollWheel())
                        {
                                scrollIndex.intValue = scrollIndex.intValue + evt.scrollDelta * scrollSpeed;
                        }
                        int mainIndex = scrollIndex.intValue = Mathf.Clamp(scrollIndex.intValue, 0, Mathf.Max(0, size - maxItems));

                        for (int i = 0; i < maxItems; i++)
                        {
                                if (mainIndex < size)
                                {
                                        callback.Invoke(itemHeight, mainIndex++);
                                        continue;
                                }
                                break;
                        }
                        return this;
                }


        }

        public static class Scrollbar
        {
                public static int dragID;
                public static bool isDragging;
                public static float dragOffset;

                public static void Run (Editor editor, Rect rect, int maxItems, int itemHeight, int scrollSpeed, int size, SerializedProperty scrollIndex, bool colorGrey)
                {
                        // Scroll bar
                        Rect scrollBar = new Rect(rect) { x = rect.x + rect.width, width = 10 };
                        scrollBar.DrawBasicRect(Tint.BoxLight);

                        // Scroll Handle Height
                        int totalItems = size;
                        float currentItemsInWindow = Mathf.Min(maxItems, totalItems);
                        float percent = Mathf.Min(currentItemsInWindow / totalItems, 1f);
                        float handleHeight = Mathf.Clamp(percent * (currentItemsInWindow * itemHeight), Mathf.Min(3, currentItemsInWindow) * itemHeight, currentItemsInWindow * itemHeight);

                        // Scroll Handle Position Y
                        float handleArea = scrollBar.height - handleHeight;
                        float scrollableItems = Mathf.Max(totalItems - currentItemsInWindow, 1f);
                        percent = (float) scrollIndex.intValue / scrollableItems;
                        float handleY = Mathf.Clamp(percent * handleArea, 0, handleArea);

                        // Scroll Handle
                        Rect scrollHandle = new Rect(scrollBar) { y = scrollBar.y + handleY, height = handleHeight };
                        scrollHandle.DrawBasicRect(colorGrey ? Tint.Box : Tint.Delete * Tint.WhiteOpacity180);

                        // Scroll
                        if (isDragging && evt.mouseUp)
                        {
                                isDragging = false;
                        }
                        if (!isDragging && scrollHandle.ContainsMouseDown())
                        {
                                isDragging = true;
                                dragID = (int) scrollBar.y;
                                dragOffset = evt.mouse.y - scrollHandle.y;
                        }
                        if (isDragging && evt.mouseDrag && dragID == (int) scrollBar.y)
                        {
                                float y = Mathf.Clamp(evt.mouse.y - dragOffset - scrollBar.y, 0, handleArea);
                                scrollIndex.intValue = (int) Compute.Round(scrollableItems * (y / handleArea), 1f);
                                editor.Repaint();
                        }
                }
        }


        #endregion
}
#endif
