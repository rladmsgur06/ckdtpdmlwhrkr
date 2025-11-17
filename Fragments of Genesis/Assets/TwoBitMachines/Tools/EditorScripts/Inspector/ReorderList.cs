#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace TwoBitMachines.Editors
{
        public class ListReorder
        {
                public static bool isActive;
                public static bool canListSwap;
                public static bool canListSwapDifferentList;
                public static bool canExchangeItemsFromOtherLists;

                public static int srcListIndex;
                public static int dstListIndex;
                public static int srcItemIndex;
                public static int dstItemIndex;
                public static int listIndexRef;
                public static bool listSwapObj;
                public static float timeStart;
                public static SerializedProperty activeArray;

                public static bool canSwap => canListSwap || canListSwapDifferentList;

                public static bool Reorder (Rect rect, Texture2D icon, int itemIndex, SerializedProperty array, SerializedProperty index, SerializedProperty active, Color color)
                {
                        switch (Event.current.type)
                        {
                                case EventType.Repaint:
                                        if (icon != null)
                                                Skin.DrawTexture(rect, icon, index.intValue == itemIndex && active.boolValue ? Tint.Grey150 : color);
                                        break;
                                case EventType.MouseDown:
                                        if (rect.ContainsMouseDown(false))
                                        {
                                                timeStart = Time.time;
                                                active.boolValue = true;
                                                activeArray = array;
                                                isActive = true;
                                                GUI.FocusControl(null);
                                                srcListIndex = listIndexRef;
                                                index.intValue = itemIndex;
                                                srcItemIndex = itemIndex;
                                        }
                                        break;
                                case EventType.MouseUp:
                                        if (active.boolValue)
                                        {
                                                isActive = false;
                                                active.boolValue = false;
                                                Layout.UseEvent();
                                        }
                                        break;
                                case EventType.MouseDrag:
                                        rect.width = Layout.maxWidth;
                                        if (isActive && canExchangeItemsFromOtherLists && !active.boolValue && rect.ContainsMouse() && activeArray != null && array != null && activeArray.propertyPath != array.propertyPath)
                                        {
                                                if (array.arraySize > 0 && activeArray.arrayElementType == array.arrayElementType)
                                                {
                                                        listSwapObj = activeArray.serializedObject.targetObject;
                                                        dstListIndex = listIndexRef;
                                                        canListSwap = true;
                                                        isActive = false;
                                                        return true;
                                                }
                                                if (array.arraySize > 0 && activeArray.arrayElementType != array.arrayElementType)
                                                {
                                                        listSwapObj = activeArray.serializedObject.targetObject;
                                                        dstListIndex = listIndexRef;
                                                        canListSwapDifferentList = true;
                                                        isActive = false;
                                                        return true;
                                                }
                                        }
                                        else if (active.boolValue && rect.ContainsMouse())
                                        {
                                                if (itemIndex != index.intValue)
                                                {
                                                        srcItemIndex = index.intValue;
                                                        dstItemIndex = itemIndex;
                                                        array.MoveArrayElement(index.intValue, itemIndex);
                                                        index.intValue = itemIndex;
                                                        return true;
                                                }
                                                Layout.UseEvent();
                                        }
                                        break;
                        }
                        return false;
                }

                public static bool Grip (SerializedObject property, SerializedProperty array, Rect rect, int index, Color gripColor, string signalIndex = "signalIndex", string active = "active", int yOffset = 0)
                {
                        return Grip(property.Get(signalIndex), property.Get(active), array, rect, index, gripColor, 0, yOffset);
                }

                public static bool Grip (SerializedProperty property, SerializedProperty array, Rect rect, int index, Color gripColor, string signalIndex = "signalIndex", string active = "active", int yOffset = 0)
                {
                        return Grip(property.Get(signalIndex), property.Get(active), array, rect, index, gripColor, 0, yOffset);
                }

                public static bool Grip (SerializedProperty signalIndex, SerializedProperty active, SerializedProperty array, Rect rect, int index, Color gripColor, float xOffset = 0, int yOffset = 0)
                {
                        if (signalIndex == null || active == null)
                                return false;
                        Rect grip = new Rect(rect) { x = 12 + xOffset, y = rect.y + 2 + yOffset, width = 12, height = 12 };
                        return Reorder(grip, Icon.Get("Grip"), index, array, signalIndex, active, gripColor);
                }

                public static bool GripRaw (SerializedObject property, SerializedProperty array, Rect rect, int index, string signalIndex = "signalIndex", string active = "active")
                {
                        return GripRaw(property.Get(signalIndex), property.Get(active), array, rect, index);
                }

                public static bool GripRaw (SerializedProperty signalIndex, SerializedProperty active, SerializedProperty array, Rect rect, int index)
                {
                        if (signalIndex == null || active == null)
                                return false;
                        return Reorder(rect, null, index, array, signalIndex, active, Color.clear);
                }
        }

}
#endif
