#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace TwoBitMachines.UIElement.Editor
{
        public static class Icon
        {
                public static List<Texture2D> icon = new List<Texture2D>();
                public static bool initialized;

                public static Texture2D Get (string name)
                {
                        return icon.GetIcon(name);
                }

                public static void Retrieve ()
                {
                        if (initialized)
                        {
                                return;
                        }
                        initialized = true;
                        TwoBitMachines.Editors.EditorTools.LoadGUI("TwoBitMachines", "/Tools/Icons", icon);
                }
        }

        public static class Tint
        {
                public const float activeTint = 1.15f;
                public const float hoverTint = 1.065f;

                public static Color BoxGrey = new Color32(170, 170, 170, 255);
                public static Color BoxLightGrey = new Color32(190, 190, 190, 255);
                public static Color BoxDarkGrey = new Color32(155, 155, 155, 255);

                public static Color BoxLight = new Color32(185, 185, 185, 255);
                public static Color BoxThree = new Color32(125, 125, 125, 255);
                public static Color BoxTwo = new Color32(111, 111, 111, 255);
                public static Color Button = new Color32(188, 188, 188, 255);
                public static Color Clear = new Color32(0, 0, 0, 0);
                public static Color Blue = new Color32(4, 184, 236, 255);
                public static Color LightBlue = new Color32(83, 238, 255, 255);
                public static Color Blue150 = new Color32(4, 184, 236, 150);
                public static Color Blue50 = new Color32(4, 184, 236, 50);
                public static Color Pink = new Color32(248, 90, 157, 255);
                public static Color Purple = new Color32(248, 94, 244, 255);
                public static Color PurpleDark = new Color32(164, 106, 227, 255);
                public static Color PastelGreen = new Color32(143, 255, 89, 255);
                public static Color Brown = new Color32(156, 129, 111, 255);
                public static Color Green = new Color32(90, 215, 90, 255);
                public static Color Orange = new Color32(244, 158, 5, 255);
                public static Color DarkOrange = new Color32(229, 128, 10, 255);
                public static Color EditClosed = new Color32(94, 94, 94, 255);
                public static Color EditOpen = new Color32(32, 191, 255, 255);
                public static Color White = new Color32(255, 255, 255, 255);
                public static Color WarmWhite = new Color32(255, 250, 240, 255);
                public static Color WarmWhiteB = new Color32(252, 252, 252, 255);
                public static Color WarmGrey = new Color32(235, 235, 235, 255);
                public static Color WarmGreyB = new Color32(238, 238, 238, 255);
                public static Color Delete = new Color32(255, 97, 97, 255);
                public static Color DeleteA = new Color32(255, 97, 97, 175);
                public static Color Selected = new Color32(114, 215, 253, 225);
                public static Color SelectedA = new Color32(114, 215, 253, 100);
                public static Color On { get { return new Color32(77, 244, 99, 255); } }
                public static Color OnA { get { return new Color32(77, 244, 99, 100); } }
                public static Color Off = new Color32(152, 152, 152, 255);
                public static Color Grey200 = new Color32(152, 152, 152, 200);
                public static Color Grey185 = new Color32(152, 152, 152, 185);
                public static Color Grey175 = new Color32(152, 152, 152, 175);
                public static Color Grey150 = new Color32(152, 152, 152, 150);
                public static Color Grey100 = new Color32(152, 152, 152, 100);
                public static Color Grey75 = new Color32(152, 152, 152, 75);
                public static Color Grey50 = new Color32(152, 152, 152, 50);
                public static Color Grey25 = new Color32(152, 152, 152, 25);
                public static Color Grey35 = new Color32(152, 152, 152, 35);
                public static Color Grey = new Color32(180, 180, 180, 255);
                public static Color GreySolid200 = new Color32(200, 200, 200, 255);
                public static Color normal = new Color32(90, 88, 93, 255);

                // new
                public static Color WarmGrey225 = new Color32(235, 235, 235, 225);
                public static Color PastelGreen100 = new Color32(143, 255, 89, 100);
                public static Color Delete180 = new Color32(255, 97, 97, 180);
                public static Color Delete100 = new Color32(255, 97, 97, 100);
                public static Color HardDark = new Color32(49, 57, 49, 255);
                public static Color SoftDark = new Color32(90, 89, 93, 255);
                public static Color SoftDarkA = new Color32(68, 68, 82, 75);
                public static Color SoftDark50 = new Color32(68, 68, 82, 50);
                public static Color SoftDark100 = new Color32(68, 68, 82, 100);
                public static Color SoftDark150 = new Color32(68, 68, 82, 150);
                public static Color SoftDark200 = new Color32(68, 68, 82, 200);
                public static Color SoftDark225 = new Color32(68, 68, 82, 225);
                public static Color SoftDark240 = new Color32(68, 68, 82, 240);
                public static Color WhiteOpacity10 = new Color32(255, 255, 255, 10);
                public static Color WhiteOpacity25 = new Color32(255, 255, 255, 25);
                public static Color WhiteOpacity50 = new Color32(255, 255, 255, 50);
                public static Color WhiteOpacity75 = new Color32(255, 255, 255, 75);
                public static Color WhiteOpacity100 = new Color32(255, 255, 255, 100);
                public static Color WhiteOpacity180 = new Color32(255, 255, 255, 180);
                public static Color WhiteOpacity240 = new Color32(255, 255, 255, 240);
                public static Color LightGrey = new Color32(202, 200, 200, 255);
                public static Color EventColor = new Color32(228, 229, 228, 255);
                public static Color AlwaysState = new Color32(123, 133, 215, 255);
                //Color: #f85a9d
        }

        public static class ElementTools
        {
                private static StyleSheet styleSheetProperty;

                public static StyleSheet styleSheet
                {
                        get
                        {
                                if (styleSheetProperty == null)
                                {
                                        styleSheetProperty = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/TwoBitMachines/Tools/UI_Elements/Style/Style.uss");
                                }
                                return styleSheetProperty;
                        }
                }

                public static VisualElement InitializeRoot ()
                {
                        VisualElement root = new VisualElement();
                        root.style.marginTop = 10;
                        root.styleSheets.Add(ElementTools.styleSheet);
                        Icon.Retrieve();
                        return root;
                }

                public static T IconButton<T> (string icon) where T : VisualElement, new()
                {
                        T element = new T();
                        element.AddToClassList("clear");
                        element.AddToClassList("button-small");

                        element.style.flexGrow = 0;
                        element.style.flexShrink = 0;
                        element.style.marginRight = 5f;
                        element.SetImage(icon);
                        return element;
                }


                public static bool ContainsPosition (this VisualElement element, Vector2 position)
                {
                        Vector2 s = element.worldBound.size;
                        Vector2 p = element.worldBound.position;
                        return position.x >= p.x && position.x <= p.x + s.x && position.y >= p.y && position.y <= p.y + s.y;
                }

                public static void Clamp (this SerializedProperty element, float min, float max)
                {
                        element.floatValue = Mathf.Clamp(element.floatValue, min, max);
                }

                public static VisualElement SetImage (this VisualElement element, string icon, Color? color = null, float scale = 1f)
                {
                        return element.SetImage(Icon.Get(icon), color, scale);
                }

                public static VisualElement SetImage (this VisualElement element, Texture2D icon, Color? color = null, float scale = 1f)
                {
                        Texture2D texture = icon;
                        element.style.width = texture.width * scale;
                        element.style.height = texture.height * scale;
                        element.style.backgroundImage = texture;
                        if (color != null && color.HasValue)
                        {
                                element.style.unityBackgroundImageTintColor = color.Value;
                        }
                        return element;
                }

                public static VisualElement SetImage (this VisualElement element, Texture2D icon, int size, Color? color = null)
                {
                        Texture2D texture = icon;
                        element.style.width = size;
                        element.style.height = size;
                        element.style.backgroundImage = texture;
                        if (color != null && color.HasValue)
                        {
                                element.style.unityBackgroundImageTintColor = color.Value;
                        }
                        return element;
                }

                public static void CreateNameList (this SerializedProperty array, List<string> names)
                {
                        names.Clear();
                        for (int i = 0; i < array.arraySize; i++)
                        {
                                names.Add(array.GetArrayElementAtIndex(i).FindPropertyRelative("name").stringValue);
                        }
                }

                public static Color Default (this Color color)
                {
                        return color == default ? Tint.WarmGrey : color;
                }
        }

        public static class Values
        {
                public static SerializedProperty Get (this SerializedObject obj, string field)
                {
                        return obj.FindProperty(field);
                }

                public static SerializedProperty Get (this SerializedProperty obj, string field)
                {
                        return obj.FindPropertyRelative(field);
                }

                public static SerializedProperty Get (this System.Object obj, string field)
                {
                        if (obj is SerializedObject serializedObject)
                        {
                                return serializedObject.FindProperty(field);
                        }
                        else if (obj is SerializedProperty serializedProperty)
                        {
                                return serializedProperty.FindPropertyRelative(field);
                        }
                        return null;
                }

                public static void ApplyProperties (this SerializedObject obj)
                {
                        obj.ApplyModifiedProperties();
                }

                public static string PropertyPath (this System.Object obj)
                {
                        if (obj is SerializedProperty serializedProperty)
                        {
                                return serializedProperty.propertyPath;
                        }
                        return "";
                }

                public static bool Toggle (this System.Object obj, string field)
                {
                        SerializedProperty property = obj.Get(field);
                        property.boolValue = !property.boolValue;
                        return property.boolValue;
                }

                public static UnityEngine.Object Object (this System.Object obj, string field)
                {
                        return obj.Get(field).objectReferenceValue;
                }

                public static bool Bool (this System.Object property, string field)
                {
                        return property.Get(field).boolValue;
                }

                public static bool ReadBool (this System.Object property, string field)
                {
                        SerializedProperty boolProperty = property.Get(field);
                        if (property == null)
                                return false;
                        bool state = boolProperty.boolValue;
                        boolProperty.boolValue = false;
                        return state;
                }

                public static bool ReadBool (this System.Object property)
                {
                        SerializedProperty boolProperty = property as SerializedProperty;
                        if (property == null)
                                return false;
                        bool state = boolProperty.boolValue;
                        boolProperty.boolValue = false;
                        return state;
                }

                public static bool SetTrue (this System.Object property, string field)
                {
                        return property.Get(field).boolValue = true;
                }

                public static bool SetFalse (this System.Object property, string field)
                {
                        return property.Get(field).boolValue = false;
                }

                public static Sprite Sprite (this System.Object property, string field)
                {
                        return property.Get(field).objectReferenceValue as Sprite;
                }

                public static float Float (this System.Object property, string field)
                {
                        return property.Get(field).floatValue;
                }

                public static int Int (this System.Object property, string field)
                {
                        return property.Get(field).intValue;
                }

                public static int Enum (this System.Object property, string field)
                {
                        return property.Get(field).enumValueIndex;
                }

                public static string String (this System.Object property, string field)
                {
                        return property.Get(field).stringValue;
                }

                public static SerializedProperty CreateNewElement (this SerializedProperty array)
                {
                        array.arraySize++;
                        SerializedProperty element = array.GetArrayElementAtIndex(array.arraySize - 1);
                        TwoBitMachines.Editors.EditorTools.ClearProperty(element);
                        return element;
                }

                public static void Delete (this SerializedProperty array, int index, bool applyProperties = false)
                {
                        if (array.arraySize == 0)
                                return;
                        array.MoveArrayElement(index, array.arraySize - 1);
                        array.arraySize--;
                        if (applyProperties)
                        {
                                array.serializedObject.ApplyProperties();
                        }
                }

                public static SerializedProperty Element (this SerializedProperty array, int index)
                {
                        return array.GetArrayElementAtIndex(index);
                }

                public static void Insert (this SerializedProperty array, int index, bool applyProperties = false)
                {
                        array.InsertArrayElementAtIndex(index);
                        if (applyProperties)
                        {
                                array.serializedObject.ApplyProperties();
                        }
                }

                public static void Sort (this SerializedProperty array, string name = "name")
                {
                        int arraySize = array.arraySize;
                        for (int i = 0; i < arraySize; i++)
                        {
                                for (int j = i + 1; j < arraySize; j++)
                                {
                                        SerializedProperty element1 = array.GetArrayElementAtIndex(i);
                                        SerializedProperty element2 = array.GetArrayElementAtIndex(j);
                                        if (element1.String(name).CompareTo(element2.String(name)) > 0)
                                        {
                                                array.MoveArrayElement(j, i);
                                        }
                                }
                        }
                }

                public static SerializedProperty IncIfZero (this SerializedProperty array)
                {
                        if (array.arraySize == 0)
                                array.arraySize++;
                        return array;
                }

                public static SerializedProperty LastElement (this SerializedProperty array)
                {
                        return array.GetArrayElementAtIndex(array.arraySize - 1);
                }

                public static int LastIndex (this SerializedProperty array)
                {
                        return array.arraySize - 1;
                }

                public static bool Visible (this VisualElement element)
                {
                        return element.style.display == DisplayStyle.Flex;
                }

                public static void Visible (this VisualElement element, bool value)
                {
                        element.style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
                }

                public static void ToggleVisibility (this VisualElement element)
                {
                        element.style.display = element.Visible() ? DisplayStyle.None : DisplayStyle.Flex;
                }

                public static void Tooltip (this VisualElement element, string tooltip)
                {
                        if (tooltip != "")
                        {
                                element.tooltip = tooltip;
                        }
                }
        }


        public static class Template
        {
                public static List<UnityEngine.Object> newObjs = new List<UnityEngine.Object>();

                public static Label HelperText (this VisualElement parent, string label, float spacing)
                {
                        Label labelField = new Label(label);
                        labelField.style.left = -spacing;
                        labelField.style.top = -16;
                        labelField.style.fontSize = 9;
                        labelField.style.marginBottom = -12;
                        labelField.style.alignSelf = Align.Center;
                        labelField.style.marginLeft = StyleKeyword.Auto;
                        parent.Add(labelField);
                        return labelField;
                }

                public static void Space (this VisualElement container, int height = 1)
                {
                        VisualElement space = new VisualElement();
                        space.style.height = height;
                        container.Add(space);
                }

                public static Slider Slider (float min, float max, Color color, int bottomSpace = 1)
                {
                        Slider slider = new Slider();
                        slider.AddToClassList("box");
                        slider.AddToClassList("mySlider");
                        slider.style.backgroundImage = Icon.Get("HeaderMiddle");
                        slider.style.unityBackgroundImageTintColor = color;
                        slider.style.marginBottom = bottomSpace;
                        slider.lowValue = min;
                        slider.highValue = max;
                        return slider;
                }

                public static Button LargetButton (VisualElement parent, string label, Color buttonColor, Color labelColor, int fontSize = 12, string tooltip = "", bool bold = true)
                {
                        Button button = new Button();
                        button.text = label;
                        button.style.color = labelColor;
                        button.style.fontSize = fontSize;
                        button.style.unityBackgroundImageTintColor = buttonColor;
                        button.style.marginBottom = 1;

                        button.AddToClassList("box");
                        button.AddToClassList("image-button");
                        if (bold)
                        {
                                button.AddToClassList("label-bold");
                        }
                        if (tooltip != "")
                        {
                                button.tooltip = tooltip;
                        }
                        parent.Add(button);
                        return button;
                }

                public static VisualElement Button (VisualElement parent, string background, string icon, Color color, int flexGrow = 1, int height = 0, string tooltip = "")
                {
                        VisualElement element = new VisualElement();
                        element.AddToClassList("box");
                        element.style.backgroundImage = Icon.Get(background);
                        element.style.flexDirection = FlexDirection.Row;
                        element.style.justifyContent = Justify.Center;
                        element.style.alignItems = Align.Center;
                        element.style.flexGrow = flexGrow;
                        element.style.unityBackgroundImageTintColor = color;
                        if (height != 0)
                        {
                                element.style.height = height;
                        }

                        VisualElement image = new VisualElement();
                        image.SetImage(icon);
                        element.Add(image);
                        parent.Add(element);

                        element.RegisterCallback<MouseDownEvent>(evt =>
                        {
                                element.style.unityBackgroundImageTintColor = color * Tint.activeTint;

                        });
                        element.RegisterCallback<MouseOverEvent>(evt =>
                        {
                                element.style.unityBackgroundImageTintColor = color * Tint.hoverTint;
                        });
                        element.RegisterCallback<MouseOutEvent>(evt =>
                        {
                                element.style.unityBackgroundImageTintColor = color;
                        });
                        return element;
                }

                public static Button InputAndButton (VisualElement parent, string label, string icon, Color color, out TextField text, string name = "Name", int height = 25, string tooltip = "")
                {
                        VisualElement element = new VisualElement();
                        element.AddToClassList("box");
                        element.AddToClassList("image-box");

                        element.style.unityBackgroundImageTintColor = color;
                        element.style.flexDirection = FlexDirection.Row;
                        element.style.alignItems = Align.Stretch;
                        element.style.height = height;
                        element.style.marginBottom = 1;

                        text = new TextField(label);
                        text.value = name;
                        text.style.flexGrow = 1;
                        text.style.unityTextAlign = TextAnchor.MiddleLeft;
                        element.Add(text);

                        Button button = ElementTools.IconButton<Button>(icon);
                        button.style.alignSelf = Align.Center;

                        element.Add(button);
                        parent.Add(element);
                        return button;
                }

                public static VisualElement DropArea<T, U> (VisualElement container, int height, string background, Color color, int bottomSpace = 1)
                {
                        VisualElement dropArea = new VisualElement();
                        dropArea.AddToClassList("box");
                        dropArea.style.flexGrow = 1;
                        dropArea.style.marginBottom = 1;
                        dropArea.style.height = height;
                        dropArea.style.alignItems = Align.Center;
                        dropArea.style.justifyContent = Justify.Center;
                        dropArea.style.backgroundImage = Icon.Get(background);
                        dropArea.style.unityBackgroundImageTintColor = color;
                        DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
                        container.Add(dropArea);

                        dropArea.RegisterCallback<MouseOverEvent>(evt =>
                        {
                                if (DragAndDrop.objectReferences.Length > 0)
                                {
                                        dropArea.style.unityBackgroundImageTintColor = color * Tint.BoxDarkGrey;
                                }
                        });
                        dropArea.RegisterCallback<MouseOutEvent>(evt =>
                        {
                                dropArea.style.unityBackgroundImageTintColor = color;
                        });
                        dropArea.RegisterCallback<DragUpdatedEvent>(evt =>
                        {
                                DragAndDrop.visualMode = DragAndDrop.objectReferences.Length > 0 ? DragAndDropVisualMode.Link : DragAndDropVisualMode.None;
                        });
                        dropArea.RegisterCallback<DragExitedEvent>(evt =>
                        {
                                DragAndDrop.visualMode = DragAndDropVisualMode.None;
                        });
                        dropArea.RegisterCallback<DragPerformEvent>(evt =>
                        {
                                DragAndDrop.AcceptDrag();
                                newObjs.Clear();
                                foreach (var obj in DragAndDrop.objectReferences)
                                {
                                        if (obj is T || obj is U)
                                        {
                                                newObjs.Add(obj);
                                        }
                                }
                        });
                        return dropArea;
                }

                public static VisualElement DropAreaMessage (this VisualElement container, string message, Color color, bool bold = true, int fontSize = 12)
                {
                        Label label = new Label();
                        label.text = message;
                        label.style.fontSize = fontSize;
                        label.style.color = color;
                        if (bold)
                        {
                                label.AddToClassList("label-bold");
                        }
                        container.Add(label);
                        return container;
                }

                public static VisualElement DropAreaIcon (this VisualElement container, string icon, Color color, float scale)
                {
                        Image image = new Image();
                        image.SetImage(icon, color, scale);
                        container.Add(image);
                        return container;
                }

                public static BindableElement HorizontalContainer (VisualElement parent)
                {
                        BindableElement container = new BindableElement();
                        container.style.flexDirection = FlexDirection.Row;
                        parent.Add(container);
                        return container;
                }

                public static PropertyField Field (VisualElement parent, string field, int marginRight = 0, float weight = 1f)
                {
                        PropertyField property = new PropertyField();
                        property.AddToClassList("hide-label");
                        property.AddToClassList("clear");
                        property.bindingPath = field;
                        property.style.flexBasis = 100 * weight;
                        property.style.flexGrow = 1f;
                        property.style.marginRight = marginRight;
                        parent.Add(property);
                        return property;
                }

                public static PropertyField Field (VisualElement parent, SerializedProperty field, int marginRight = 0, float weight = 1f)
                {
                        PropertyField property = new PropertyField();
                        property.AddToClassList("hide-label");
                        property.AddToClassList("clear");
                        property.BindProperty(field);
                        property.style.flexBasis = 100 * weight;
                        property.style.flexGrow = 1f;
                        property.style.marginRight = marginRight;
                        parent.Add(property);
                        return property;
                }

                public static BindableElement Header (VisualElement container, Color color, int height = 23, int space = 2, string backgroundImage = "Header")
                {
                        BindableElement header = new BindableElement();
                        header.AddToClassList("box");
                        header.style.backgroundImage = Icon.Get(backgroundImage);
                        header.style.unityBackgroundImageTintColor = color;
                        header.style.flexDirection = FlexDirection.Row;
                        header.style.alignItems = Align.Center;
                        header.style.paddingLeft = space;
                        header.style.height = height;
                        header.style.marginLeft = 0;
                        header.style.marginTop = 0;
                        if (container != null)
                                container.Add(header);
                        return header;
                }

                public static Label Label (VisualElement parent, string label)
                {
                        Label labelField = new Label(label);
                        labelField.AddToClassList("label-fixed-width");
                        labelField.style.flexGrow = 1;
                        labelField.style.paddingLeft = 3;
                        labelField.style.alignSelf = Align.Center;
                        parent.Add(labelField);
                        return labelField;
                }


                public static Toggle Toggle (VisualElement parent, string field)
                {
                        Toggle toggle = new Toggle();
                        toggle.style.flexShrink = 0;
                        toggle.bindingPath = field;
                        toggle.style.marginLeft = StyleKeyword.Auto;
                        parent.Add(toggle);
                        return toggle;
                }

        }

}
#endif


// public Unit Button (string field, string icon, string tooltip = "")
// {
//         // Button button = ElementTools.IconElement<Button>(icon);  // should work

//         // if (tooltip != "")
//         // {
//         //         button.tooltip = tooltip;
//         // }
//         // button.clicked += () =>
//         // {
//         //         property.Toggle(field);
//         //         ApplyProperties();
//         // };
//         // header.Add(button);
//         // return this;
//         return this;
// }
