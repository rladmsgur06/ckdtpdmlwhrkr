#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace TwoBitMachines.Editors
{
        public static class Lists
        {
                public static string[] ints;

                public static void DropList (this System.Object property, Rect rect, string nameField, List<string> names, Color? color = null, int yoffset = 0)
                {
                        if (rect.ButtonDropdown(property.String(nameField), Icon.Get("ItemLight"), color == null ? Tint.WarmGrey : color.Value, LabelType.Normal, false, yoffset))
                        {
                                string name = property.String(nameField);
                                GenericMenu menu = new GenericMenu();
                                for (int i = 0; i < names.Count; i++)
                                {
                                        string itemName = names[i];
                                        menu.AddItem(new GUIContent(itemName), itemName == name, () =>
                                        {
                                                SerializedObject so = property.SerializedObjet();
                                                so.Update();
                                                property.Get(nameField).stringValue = itemName;
                                                so.ApplyModifiedProperties();
                                        });
                                }
                                menu.ShowAsContext();
                        }
                }

                public static void DropList (this SerializedProperty property, Rect rect, string[] names, string stringField)
                {
                        SerializedProperty name = property.Get(stringField);
                        int index = 0;
                        string stringName = name.stringValue;
                        for (int i = 0; i < names.Length; i++)
                        {
                                if (names[i] == stringName)
                                {
                                        index = i;
                                }
                        }
                        index = names.Length == 0 ? 0 : EditorGUI.Popup(rect, index, names);
                        name.stringValue = names.Length == 0 ? "Empty" : names[index];
                }

                public static void DropListInts (this SerializedProperty property, Rect rect, int length, string indexField)
                {
                        SerializedProperty index = property.Get(indexField);

                        if (ints == null || ints.Length != length)
                        {
                                ints = new string[length];
                                for (int i = 0; i < length; i++)
                                {
                                        ints[i] = i.ToString();
                                }
                        }

                        int tempIndex = index.intValue;
                        tempIndex = length == 0 ? 0 : EditorGUI.Popup(rect, tempIndex, ints);
                        index.intValue = tempIndex;
                }

                public static void DropListRaw (this SerializedProperty property, Rect rect, string[] names)
                {
                        string stringName = property.stringValue;
                        int index = 0;
                        for (int i = 0; i < names.Length; i++)
                        {
                                if (names[i] == stringName)
                                {
                                        index = i;
                                }
                        }
                        index = EditorGUI.Popup(rect, index, names);
                        property.stringValue = names[index];
                }

                public static void DropList (this SerializedObject property, Rect rect, string[] names, string stringField)
                {
                        SerializedProperty name = property.Get(stringField);
                        string stringName = name.stringValue;
                        int index = 0;
                        for (int i = 0; i < names.Length; i++)
                        {
                                if (names[i] == stringName)
                                {
                                        index = i;
                                }
                        }
                        index = EditorGUI.Popup(rect, index, names);
                        name.stringValue = names[index];
                }

                public static void DropList (this SerializedObject property, Rect rect, string[] names, SerializedProperty stringField)
                {
                        string stringName = stringField.stringValue;
                        int index = 0;
                        for (int i = 0; i < names.Length; i++)
                                if (names[i] == stringName)
                                        index = i;
                        index = EditorGUI.Popup(rect, index, names);
                        stringField.stringValue = names[index];
                }

                public static void DropDownList (this SerializedObject property, string[] names, string title, string stringField, bool execute = true, float space = 0)
                {
                        if (!execute || names.Length == 0)
                                return;
                        Rect rect = Layout.CreateRectField();
                        Labels.Label(title, rect.Adjust(space, Layout.labelWidth));
                        property.DropList(rect.Adjust(Layout.labelWidth, Layout.contentWidth), names, stringField);
                }

                public static void DropDownList (this SerializedObject property, string[] names, string title, SerializedProperty stringField, bool execute = true, float space = 0)
                {
                        if (!execute || names.Length == 0)
                                return;
                        Rect rect = Layout.CreateRectField();
                        Labels.Label(title, rect.Adjust(space, Layout.labelWidth));
                        property.DropList(rect.Adjust(Layout.labelWidth, Layout.contentWidth), names, stringField);
                }

                public static void DropDownList (this SerializedProperty property, string[] names, string title, string stringField, bool execute = true, float space = 0, float shorten = 0)
                {
                        if (!execute || names.Length == 0)
                                return;
                        Rect rect = Layout.CreateRectField();
                        Labels.Label(title, rect.Adjust(space, Layout.labelWidth));
                        property.DropList(rect.Adjust(Layout.labelWidth, Layout.contentWidth - shorten), names, stringField);
                }

                public static void DropDownListAndField (this SerializedObject property, string[] names, string title, string listField, string normalField, bool execute = true, float space = 0)
                {
                        if (!execute || names.Length == 0)
                                return;
                        Rect rect = Layout.CreateRectField();
                        EditorGUI.LabelField(rect.Adjust(space, Layout.labelWidth), title, EditorStyles.label);
                        property.DropList(rect.Adjust(Layout.labelWidth, Layout.contentWidth / 2f - 1), names, listField);
                        EditorGUI.PropertyField(rect.Adjust(Layout.contentWidth / 2f, Layout.contentWidth / 2f), property.Get(normalField), GUIContent.none);
                }

                public static void DropDownFieldAndList (this SerializedObject property, string[] names, string title, string normalField, string listField, bool execute = true, float space = 0)
                {
                        if (!execute || names.Length == 0)
                                return;
                        Rect rect = Layout.CreateRectField();
                        EditorGUI.LabelField(rect.Adjust(space, Layout.labelWidth), title, EditorStyles.label);
                        EditorGUI.PropertyField(rect.Adjust(Layout.labelWidth, Layout.contentWidth / 2f - 1), property.Get(normalField), GUIContent.none);
                        property.DropList(rect.Adjust(Layout.contentWidth / 2f, Layout.contentWidth / 2f), names, listField);
                }

                public static void DropDownListAndButton (this SerializedProperty property, string[] names, string title, string stringField, string button, string icon, bool execute = true, float space = 0, int buttonWidth = 15)
                {
                        if (!execute || names.Length == 0)
                                return;
                        Rect rect = Layout.CreateRectField();
                        Labels.Label(title, rect.Adjust(space, Layout.labelWidth));
                        property.DropList(rect.Adjust(Layout.labelWidth, Layout.contentWidth - buttonWidth + 2), names, stringField);
                        property.Get(button).boolValue = Buttons.Button(rect.Adjust(Layout.contentWidth - buttonWidth + 4, buttonWidth - 2), Icon.Get(icon), Tint.White, center: true);
                }

                public static bool DropDownListAndButton (this SerializedProperty property, string[] names, string title, string stringField, string icon, bool execute = true, float space = 0, int buttonWidth = 15)
                {
                        if (!execute || names.Length == 0)
                                return false;
                        Rect rect = Layout.CreateRectField();
                        Labels.Label(title, rect.Adjust(space, Layout.labelWidth));
                        property.DropList(rect.Adjust(Layout.labelWidth, Layout.contentWidth - buttonWidth + 2), names, stringField);
                        return Buttons.Button(rect.Adjust(Layout.contentWidth - buttonWidth + 4, buttonWidth - 2), Icon.Get(icon), Tint.White, center: true);
                }

                public static bool DropDownListRawAndButton (this SerializedProperty property, string[] names, string title, string icon, bool execute = true, float space = 0, int buttonWidth = 15)
                {
                        if (!execute || names.Length == 0)
                                return false;
                        Rect rect = Layout.CreateRectField();
                        Labels.Label(title, rect.Adjust(space, Layout.labelWidth));
                        property.DropListRaw(rect.Adjust(Layout.labelWidth, Layout.contentWidth - buttonWidth + 2), names);
                        return Buttons.Button(rect.Adjust(Layout.contentWidth - buttonWidth + 4, buttonWidth - 2), Icon.Get(icon), Tint.White, center: true);
                }

                public static void DropDownListAndEnable (this SerializedProperty property, string[] names, string title, string stringField, string toggle)
                {
                        if (names.Length == 0)
                                return;
                        Rect rect = Layout.CreateRectField();
                        Layout.BeginGUIEnable(property.Bool(toggle));
                        Labels.Label(title, rect.Adjust(0, Layout.labelWidth));
                        property.DropList(rect.Adjust(Layout.labelWidth, Layout.contentWidth - Layout.boolWidth), names, stringField);
                        Layout.EndGUIEnable(rect.Adjust(Layout.contentWidth - Layout.boolWidth, Layout.boolWidth), property.Get(toggle));
                }
                public static void DropDownListAndEnable (this SerializedObject property, string[] names, string title, string stringField, string toggle)
                {
                        if (names.Length == 0)
                                return;
                        Rect rect = Layout.CreateRectField();
                        Layout.BeginGUIEnable(property.Bool(toggle));
                        Labels.Label(title, rect.Adjust(0, Layout.labelWidth));
                        property.DropList(rect.Adjust(Layout.labelWidth, Layout.contentWidth - Layout.boolWidth), names, stringField);
                        Layout.EndGUIEnable(rect.Adjust(Layout.contentWidth - Layout.boolWidth, Layout.boolWidth), property.Get(toggle));
                }

                public static void DropDownDoubleList (this SerializedProperty property, string[] names, string title, string stringField1, string stringField2, bool execute = true)
                {
                        if (names.Length == 0 || !execute)
                                return;
                        Rect rect = Layout.CreateRectField();
                        Labels.Label(title, rect.Adjust(0, Layout.labelWidth));
                        float width = (Layout.contentWidth) / 2f;
                        property.DropList(rect.Adjust(Layout.labelWidth, width), names, stringField1);
                        property.DropList(rect.Adjust(width, width), names, stringField2);
                }

                public static void DropDownDoubleList (this SerializedObject property, string[] names, string title, string stringField1, string stringField2)
                {
                        if (names.Length == 0)
                                return;
                        Rect rect = Layout.CreateRectField();
                        Labels.Label(title, rect.Adjust(0, Layout.labelWidth));
                        float width = (Layout.contentWidth) / 2f;
                        property.DropList(rect.Adjust(Layout.labelWidth, width), names, stringField1);
                        property.DropList(rect.Adjust(width, width), names, stringField2);
                }

                public static void DropDownDoubleListAndEnable (this SerializedProperty property, string[] names, string title, string stringField1, string stringField2, string toggle)
                {
                        if (names.Length == 0)
                                return;
                        Rect rect = Layout.CreateRectField();
                        Layout.BeginGUIEnable(property.Bool(toggle));
                        Labels.Label(title, rect.Adjust(0, Layout.labelWidth));
                        float width = (Layout.contentWidth - Layout.boolWidth) / 2f;
                        property.DropList(rect.Adjust(Layout.labelWidth, width), names, stringField1);
                        property.DropList(rect.Adjust(width, width), names, stringField2);
                        Layout.EndGUIEnable(rect.Adjust(width, Layout.boolWidth), property.Get(toggle));
                }

                public static void DropDownDoubleListAndEnable (this SerializedObject property, string[] names, string title, string stringField1, string stringField2, string toggle)
                {
                        if (names.Length == 0)
                                return;
                        Rect rect = Layout.CreateRectField();
                        Layout.BeginGUIEnable(property.Bool(toggle));
                        Labels.Label(title, rect.Adjust(0, Layout.labelWidth));
                        float width = (Layout.contentWidth - Layout.boolWidth) / 2f;
                        property.DropList(rect.Adjust(Layout.labelWidth, width), names, stringField1);
                        property.DropList(rect.Adjust(width, width), names, stringField2);
                        Layout.EndGUIEnable(rect.Adjust(width, Layout.boolWidth), property.Get(toggle));
                }

                public static void DropDownListAndDoubleButton (this SerializedProperty property, string[] names, string title, string stringField, string icon, string icon2, ref bool button1, ref bool button2, bool execute = true, float space = 0)
                {
                        if (!execute || names.Length == 0)
                                return;
                        Rect rect = Layout.CreateRectField();
                        int buttonWidth = 15;
                        Labels.Label(title, rect.Adjust(0, Layout.labelWidth));
                        property.DropList(rect.Adjust(Layout.labelWidth, Layout.contentWidth - (2f * buttonWidth) + 2), names, stringField);
                        button1 = Buttons.Button(rect.Adjust(Layout.contentWidth - buttonWidth * 2 + 4, buttonWidth), Icon.Get(icon), Tint.White, center: true);
                        button2 = Buttons.Button(rect.Adjust(buttonWidth, buttonWidth - 2), Icon.Get(icon2), Tint.White, center: true);
                }

                public static void DropDownListIntsAndDoubleButton (this SerializedProperty property, string title, int length, string indexField, string icon, string icon2, out bool button1, out bool button2, float space = 0)
                {
                        Rect rect = Layout.CreateRectField();
                        int buttonWidth = 15;
                        Labels.Label(title, rect.Adjust(0, Layout.labelWidth));
                        property.DropListInts(rect.Adjust(Layout.labelWidth, Layout.contentWidth - (2f * buttonWidth) + 2), length, indexField);
                        button1 = Buttons.Button(rect.Adjust(Layout.contentWidth - buttonWidth * 2 + 4, buttonWidth), Icon.Get(icon), Tint.White, center: true);
                        button2 = Buttons.Button(rect.Adjust(buttonWidth, buttonWidth - 2), Icon.Get(icon2), Tint.White, center: true);
                }

                public static void DropDownListAndDoubleButton (this SerializedObject property, string[] names, string title, string stringField, string icon, string icon2, ref bool button1, ref bool button2, bool execute = true, float space = 0)
                {
                        if (!execute || names.Length == 0)
                                return;
                        Rect rect = Layout.CreateRectField();
                        int buttonWidth = 15;
                        Labels.Label(title, rect.Adjust(0, Layout.labelWidth));
                        property.DropList(rect.Adjust(Layout.labelWidth, Layout.contentWidth - (2f * buttonWidth) + 2), names, stringField);
                        button1 = Buttons.Button(rect.Adjust(Layout.contentWidth - buttonWidth * 2 + 4, buttonWidth), Icon.Get(icon), Tint.White, center: true);
                        button2 = Buttons.Button(rect.Adjust(buttonWidth, buttonWidth - 2), Icon.Get(icon2), Tint.White, center: true);
                }

                public static bool DropDownListAndButton (this SerializedObject property, string[] names, string title, string stringField, string icon)
                {
                        if (names.Length == 0)
                                return false;
                        bool open = false;
                        Rect rect = Layout.CreateRectField();
                        int buttonWidth = 15;
                        Labels.Label(title, rect.Adjust(0, Layout.labelWidth));
                        property.DropList(rect.Adjust(Layout.labelWidth, Layout.contentWidth - buttonWidth + 2), names, stringField);
                        open = Buttons.Button(rect.Adjust(Layout.contentWidth - buttonWidth + 4, buttonWidth), Icon.Get(icon), Tint.White, center: true);
                        return open;
                }

        }

}
#endif
