#if UNITY_EDITOR
using System.Collections.Generic;
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace TwoBitMachines.UIElement.Editor
{
        public delegate void TabPressed ();

        public class Tab : BindableElement
        {
                [SerializeField] public TabPressed aTabWasPressed;
                [SerializeField] public int index = 0;

                public Tab (VisualElement container, int height = 23, int bottomSpace = 0)
                {
                        style.flexDirection = FlexDirection.Row;
                        style.height = 23;
                        style.marginBottom = bottomSpace;
                        container.Add(this);
                }

                public Button TabButton (string background, string icon, Color backgroundColor, Color iconColor, int tabIndex = -1, SerializedProperty toggle = null, string iconOff = "", string tooltip = "")
                {
                        Button button = new Button();
                        button.AddToClassList("box");
                        button.style.backgroundImage = Icon.Get(background);
                        button.style.flexDirection = FlexDirection.Row;
                        button.style.justifyContent = Justify.Center;
                        button.style.alignItems = Align.Center;
                        button.style.flexGrow = 1f;
                        button.style.unityBackgroundImageTintColor = backgroundColor;
                        button.Tooltip(tooltip);

                        VisualElement image = new VisualElement();
                        image.SetImage(icon);
                        image.style.unityBackgroundImageTintColor = iconColor;

                        button.Add(image);
                        Add(button);

                        if (toggle != null)
                        {
                                image.SetImage(toggle.boolValue ? icon : iconOff);
                        }

                        aTabWasPressed += () =>
                        {
                                button.style.unityBackgroundImageTintColor = index == tabIndex && tabIndex != -1 ? backgroundColor * Tint.activeTint : backgroundColor;
                        };
                        button.clicked += () =>
                        {
                                if (toggle != null)
                                {
                                        toggle.boolValue = !toggle.boolValue;
                                        image.SetImage(toggle.boolValue ? icon : iconOff);
                                        toggle.serializedObject.ApplyModifiedProperties();
                                }
                        };
                        button.RegisterCallback<MouseOverEvent>(evt =>
                        {
                                if (index != tabIndex || tabIndex == -1)
                                {
                                        button.style.unityBackgroundImageTintColor = backgroundColor * Tint.hoverTint;
                                }
                        });
                        button.RegisterCallback<MouseOutEvent>(evt =>
                        {
                                if (index != tabIndex || tabIndex == -1)
                                {
                                        button.style.unityBackgroundImageTintColor = backgroundColor;
                                }
                        });
                        return button;
                }

                public void TabWasPressed (int tabIndex)
                {
                        index = tabIndex;
                        aTabWasPressed.Invoke();
                }
        }

}
#endif
