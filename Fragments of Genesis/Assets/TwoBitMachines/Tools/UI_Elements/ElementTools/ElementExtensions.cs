#if UNITY_EDITOR
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using TwoBitMachines.Editors;

namespace TwoBitMachines.Editors
{
        public static class ElementExtensions
        {
                #region Add
                /// <summary> Add an element to this element's contentContainer.
                /// <para/> Add
                ///</summary>
                /// <param name ="target"> Target VisualElement </param>
                /// <param name ="child"> Visual Element to add </param>
                public static T AddChild<T> (this T target , VisualElement child) where T : VisualElement
                {
                        target.Add(child);
                        return target;
                }
                #endregion

                #region Bind Property
                /// <summary> Bind a serialized object to the visual element
                /// <para/> BindProperty
                ///</summary>
                /// <param name ="target"> Target VisualElement </param>
                /// <param name ="bindObject"> Object to bind </param>
                public static T Bind<T> (this T target , SerializedObject bindObject) where T : IBindable
                {
                        target.BindProperty(bindObject);
                        return target;
                }

                /// <summary> Bind a serialized property to the visual element
                /// <para/> BindProperty
                ///</summary>
                /// <param name ="target"> Target VisualElement </param>
                /// <param name ="bindObject"> Object to bind </param>
                public static T Bind<T> (this T target , SerializedProperty bindObject) where T : IBindable
                {
                        target.BindProperty(bindObject);
                        return target;
                }
                #endregion

                #region Tooltip
                /// <summary> Display tool tip information after user hovers over the element
                /// <para/> tooltip
                ///</summary>
                /// <param name ="target"> Target VisualElement </param>
                /// <param name ="value"> Tooltip string </param>
                public static T ToolTip<T> (this T target , string value) where T : VisualElement
                {
                        target.tooltip = value;
                        return target;
                }

                /// <summary> Retrieve the tooltip string from this visual element
                /// <para/> tooltip
                ///</summary>
                /// <param name ="target"> Target VisualElement </param>
                public static string ToolTip<T> (this T target) where T : VisualElement
                {
                        return target.tooltip;
                }
                #endregion

                #region Style Flex
                /// <summary> Initial main size of a flex item, on the main flex axis. The final layout might be smaller or larger, according to the flex shrinking and growing determined by the flex property.
                /// <para/> style.flexBasis
                ///</summary>
                /// <param name ="target"> Target VisualElement </param>
                /// <param name ="value"> Value </param>
                public static T FlexBasis<T> (this T target , int value) where T : VisualElement
                {
                        target.style.flexBasis = value;
                        return target;
                }

                /// <summary> Direction of the main axis to layout children in a container.
                /// <para/> style.flexDirection
                ///</summary>
                /// <param name ="target"> Target VisualElement </param>
                /// <param name ="direction"> Flex Direction </param>
                public static T FlexDirection<T> (this T target , FlexDirection direction) where T : VisualElement
                {
                        target.style.flexDirection = direction;
                        return target;
                }

                /// <summary> Specifies how much the item will grow relative to the rest of the flexible items inside the same container.
                /// <para/> style.flexGrow
                ///</summary>
                /// <param name ="target"> Target VisualElement </param>
                /// <param name ="value"> Value </param>
                public static T FlexGrow<T> (this T target , int value) where T : VisualElement
                {
                        target.style.flexGrow = value;
                        return target;
                }

                /// <summary> Specifies how the item will shrink relative to the rest of the flexible items inside the same container.
                /// <para/> style.flexShrink
                ///</summary>
                /// <param name ="target"> Target VisualElement </param>
                /// <param name ="value"> Value </param>
                public static T FlexShrink<T> (this T target , int value) where T : VisualElement
                {
                        target.style.flexShrink = value;
                        return target;
                }

                #endregion
        }
}
#endif
