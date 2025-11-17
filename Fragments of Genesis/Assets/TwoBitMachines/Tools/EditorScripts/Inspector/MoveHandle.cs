#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace TwoBitMachines.Editors
{
        public class MoveHandle
        {
                public static bool disable;
                public static bool blockSelect;
                public static bool whiteOutline;
                public static object clickedObject;
                public static int clickedIndex = -1;
                public static float firstSize = 0.7f;
                public static Vector2 initialOffset;
                public static Vector2Int intID = new Vector2Int(-1, -1);
                public static Color outline => whiteOutline ? Tint.WarmGrey * opacity : Tint.SoftDark * opacity;
                public static Color opacity => disable ? Tint.WhiteOpacity50 : Tint.White;// Tint.WhiteOpacity220;
                private static bool modifyRate => Time.realtimeSinceStartup >= selectedTimeStamp + 0.03F;
                public static float selectedTimeStamp;

                public static bool Move<T> (T objectInput, int id, Vector2 position, Color color, float handleSize, float snapSize, out Vector2 newPosition, HandleType type = HandleType.SolidCircle, bool isFirst = false)
                {
                        newPosition = position;
                        bool contains = clickedObject != null && clickedObject.Equals(objectInput) && clickedIndex == id;

                        switch (Event.current.type)
                        {
                                case EventType.MouseUp:
                                        if (clickedIndex != -1)
                                        {
                                                Event.current.Use();
                                                SceneView.RepaintAll();
                                        }
                                        clickedIndex = -1;
                                        clickedObject = null;
                                        break;
                                case EventType.MouseDown:
                                        if ((Mouse.mousePosition - position).sqrMagnitude <= handleSize * handleSize && !disable && !blockSelect)
                                        {
                                                initialOffset = Mouse.mousePosition - position;
                                                clickedObject = objectInput;
                                                clickedIndex = id;
                                                Event.current.Use();
                                                selectedTimeStamp = Time.realtimeSinceStartup;
                                        }
                                        break;
                                case EventType.Repaint:
                                        color = contains ? Color.yellow : color * opacity;
                                        DrawHandle(type, position, color, handleSize);
                                        if (isFirst)
                                        {
                                                SceneTools.Circle(position, handleSize * 0.35f, color * 0.7f);
                                        }
                                        break;
                                case EventType.MouseDrag:
                                        if (Event.current.button == 0 && contains && modifyRate)
                                        {
                                                newPosition = Compute.Round(Mouse.mousePosition - initialOffset, snapSize);
                                                Event.current.Use();
                                                selectedTimeStamp = Time.realtimeSinceStartup;
                                                return disable ? false : true;
                                        }
                                        break;
                        }
                        return false;
                }

                public static bool MoveID (int idA, int idB, Vector2 position, Color color, float handleSize, float snapSize, out Vector2 newPosition, HandleType type = HandleType.Circle, bool isFirst = false)
                {
                        newPosition = position;

                        Vector2Int controlID = new Vector2Int(idA, idB);

                        if (intID == controlID)
                        {
                                GUI.FocusControl(null);
                        }
                        switch (Event.current.type)
                        {
                                case EventType.MouseUp:
                                        if (intID == controlID)
                                        {
                                                intID = new Vector2Int(-1, -1);
                                                Event.current.Use();
                                                SceneView.RepaintAll();
                                        }
                                        break;
                                case EventType.MouseDown:
                                        if ((Mouse.mousePosition - position).sqrMagnitude <= handleSize * handleSize && !disable && !blockSelect)
                                        {
                                                intID = controlID;
                                                initialOffset = Mouse.mousePosition - position;
                                                Event.current.Use();
                                                selectedTimeStamp = Time.realtimeSinceStartup;
                                        }
                                        break;
                                case EventType.Repaint:
                                        color = intID == controlID ? Color.yellow : color * opacity;
                                        DrawHandle(type, position, color, handleSize);
                                        if (isFirst)
                                        {
                                                SceneTools.Circle(position, handleSize * 0.35f, color * 0.7f);
                                        }
                                        break;
                                case EventType.MouseDrag:
                                        if (intID == controlID && Event.current.button == 0 && modifyRate)
                                        {
                                                newPosition = Compute.Round(Mouse.mousePosition - initialOffset, snapSize);
                                                Event.current.Use();
                                                selectedTimeStamp = Time.realtimeSinceStartup;
                                                return disable ? false : true;
                                        }
                                        break;
                        }
                        return false;
                }

                public static bool Move (Vector2 position, float handleSize, out Vector2 newPosition, ref bool active, HandleType type = HandleType.None, bool isFirst = false)
                {
                        newPosition = position;
                        int controlID = GUIUtility.GetControlID(1400, FocusType.Passive);

                        if (GUIUtility.hotControl == controlID)
                        {
                                GUI.FocusControl(null);
                        }
                        switch (Event.current.GetTypeForControl(controlID))
                        {
                                case EventType.MouseUp:
                                        active = false;
                                        if (GUIUtility.hotControl == controlID)
                                        {
                                                GUIUtility.hotControl = 0;
                                                Event.current.Use();
                                                SceneView.RepaintAll();
                                        }
                                        break;
                                case EventType.MouseDown:
                                        if ((Mouse.mousePosition - position).sqrMagnitude <= handleSize * handleSize && !disable && !blockSelect)
                                        {
                                                active = true;
                                                GUIUtility.hotControl = controlID;
                                                initialOffset = Mouse.mousePosition - position;
                                                Event.current.Use();
                                                selectedTimeStamp = Time.realtimeSinceStartup;
                                        }
                                        break;
                                case EventType.MouseDrag:
                                        if (GUIUtility.hotControl == controlID && Event.current.button == 0 && modifyRate)
                                        {
                                                active = true;
                                                newPosition = Mouse.mousePosition - initialOffset;
                                                Event.current.Use();
                                                selectedTimeStamp = Time.realtimeSinceStartup;
                                                return disable ? false : true;
                                        }
                                        break;
                        }
                        return false;
                }

                public static bool MovePoint (ref Vector2 position, float handleSize = 1f, HandleType type = HandleType.None, bool isFirst = false)
                {
                        int controlID = GUIUtility.GetControlID(1400, FocusType.Passive);

                        if (GUIUtility.hotControl == controlID)
                        {
                                GUI.FocusControl(null);
                        }
                        switch (Event.current.GetTypeForControl(controlID))
                        {
                                case EventType.Repaint:
                                        Color32 color = Color.green;
                                        DrawHandle(type, position, color, handleSize);
                                        if (isFirst)
                                        {
                                                SceneTools.Circle(position, handleSize * 0.35f, color);
                                        }
                                        break;
                                case EventType.MouseUp:
                                        if (GUIUtility.hotControl == controlID)
                                        {
                                                GUIUtility.hotControl = 0;
                                                Event.current.Use();
                                                SceneView.RepaintAll();
                                        }
                                        break;
                                case EventType.MouseDown:
                                        Debug.Log("Here a " + Mouse.mousePosition + "   " + position + "   " + Event.current.mousePosition);
                                        if ((Mouse.mousePosition - position).sqrMagnitude <= handleSize * handleSize && !disable && !blockSelect)
                                        {
                                                Debug.Log("Down");
                                                GUIUtility.hotControl = controlID;
                                                initialOffset = Mouse.mousePosition - position;
                                                Event.current.Use();
                                                selectedTimeStamp = Time.realtimeSinceStartup;
                                        }
                                        break;
                                case EventType.MouseDrag:
                                        if (GUIUtility.hotControl == controlID && Event.current.button == 0 && modifyRate)
                                        {
                                                Debug.Log("Drag");
                                                position = Mouse.mousePosition - initialOffset;
                                                Event.current.Use();
                                                selectedTimeStamp = Time.realtimeSinceStartup;
                                                return disable ? false : true;
                                        }
                                        break;
                        }
                        return false;
                }

                public static bool Drag (bool pressed, ref bool active)
                {
                        int controlID = GUIUtility.GetControlID(1100, FocusType.Passive);

                        if (GUIUtility.hotControl == controlID)
                        {
                                GUI.FocusControl(null);
                        }
                        switch (Event.current.GetTypeForControl(controlID))
                        {
                                case EventType.MouseUp:
                                        active = false;
                                        if (GUIUtility.hotControl == controlID)
                                        {
                                                GUIUtility.hotControl = 0;
                                                Event.current.Use();
                                                SceneView.RepaintAll();
                                        }
                                        break;
                                case EventType.MouseDown:
                                        if (pressed && !disable && !blockSelect)
                                        {
                                                active = true;
                                                GUIUtility.hotControl = controlID;
                                                Event.current.Use();
                                        }
                                        break;
                                case EventType.MouseDrag:
                                        if (GUIUtility.hotControl == controlID && Event.current.button == 0)
                                        {
                                                active = true;
                                                Event.current.Use();
                                                return disable ? false : true;
                                        }
                                        break;
                        }
                        return false;
                }

                public static bool Drag (Rect rect)
                {
                        int controlID = GUIUtility.GetControlID(1200, FocusType.Passive);

                        if (GUIUtility.hotControl == controlID)
                        {
                                GUI.FocusControl(null);
                        }
                        switch (Event.current.GetTypeForControl(controlID))
                        {
                                case EventType.MouseUp:
                                        if (GUIUtility.hotControl == controlID)
                                        {
                                                GUIUtility.hotControl = 0;
                                                Event.current.Use();
                                                SceneView.RepaintAll();
                                        }
                                        break;
                                case EventType.MouseDown:
                                        if (rect.ContainsMouseDown() && !disable && !blockSelect)
                                        {
                                                GUIUtility.hotControl = controlID;
                                                Event.current.Use();
                                                return true;
                                        }
                                        break;
                                case EventType.MouseDrag:
                                        if (GUIUtility.hotControl == controlID && Event.current.button == 0)
                                        {
                                                Event.current.Use();
                                                return true;
                                        }
                                        break;
                        }
                        return false;
                }

                public static bool Drag (Vector2 position, Color color, float handleSize, HandleType type = HandleType.SolidCircle, bool isFirst = false) // works better if handle points have a fixed number
                {
                        int controlID = GUIUtility.GetControlID(1300, FocusType.Passive);

                        if (GUIUtility.hotControl == controlID)
                        {
                                GUI.FocusControl(null);
                        }
                        switch (Event.current.GetTypeForControl(controlID))
                        {
                                case EventType.Repaint:
                                        color = GUIUtility.hotControl == controlID ? Color.yellow : color * opacity;
                                        DrawHandle(type, position, color, handleSize);
                                        if (isFirst)
                                        {
                                                SceneTools.Circle(position, handleSize * firstSize, color * 0.7f);
                                        }
                                        break;
                                case EventType.MouseUp:
                                        if (GUIUtility.hotControl == controlID)
                                        {
                                                GUIUtility.hotControl = 0;
                                                Event.current.Use();
                                                SceneView.RepaintAll();
                                        }
                                        break;
                                case EventType.MouseDown:
                                        if ((Mouse.mousePosition - position).sqrMagnitude <= handleSize * handleSize && !disable)
                                        {
                                                GUIUtility.hotControl = controlID;
                                                initialOffset = Mouse.mousePosition - position;
                                                Event.current.Use();
                                        }
                                        break;
                                case EventType.MouseDrag:
                                        if (GUIUtility.hotControl == controlID && Event.current.button == 0)
                                        {
                                                Event.current.Use();
                                                return disable ? false : true;
                                        }
                                        break;
                        }
                        return false;
                }

                public static bool ButtonDown (Vector2 position, float handleSize)
                {
                        if (Event.current.type == EventType.MouseDown && (Mouse.mousePosition - position).sqrMagnitude <= handleSize * handleSize && !disable && !blockSelect)
                        {
                                Event.current.Use();
                                return MoveHandle.disable ? false : true;
                        }
                        return false;
                }

                public static bool ButtonDown (Vector2 position, Color color, float handleSize)
                {
                        if (Event.current.type == EventType.Repaint)
                        {
                                //  GLDraw.solidCircles.Add(new GLPoint(position, MoveHandle.opacity * color, size));
                                // GLDraw.circles.Add(new GLPoint(position, MoveHandle.outline, size));
                        }
                        if (Event.current.type == EventType.MouseDown && (Mouse.mousePosition - position).sqrMagnitude <= handleSize * handleSize && !disable && !blockSelect)
                        {
                                Event.current.Use();
                                return MoveHandle.disable ? false : true;
                        }
                        return false;
                }

                public static void DrawHandle (HandleType type, Vector2 position, Color color, float handleSize)
                {
                        // if (type == HandleType.SolidCircle)
                        {
                                SceneTools.Circle(position, handleSize, color);
                                // GLDraw.solidCircles.Add(new GLPoint(position, color, size));
                                //  GLDraw.circles.Add(new GLPoint(position, outline, size));
                        }
                        // else if (type == HandleType.Circle)
                        // {
                        //         GLDraw.circles.Add(new GLPoint(position, color, size));
                        // }
                        // else if (type == HandleType.Triangle)
                        // {
                        //         Vector2 top = position + Vector2.up * size;
                        //         Vector2 bottom = position + Vector2.down * size;
                        //         Vector2 right = Vector2.right * size;

                        //         GLDraw.triangles.Add(new GLPoint(top, bottom - right, bottom + right, color));
                        //         GLDraw.lines.Add(new GLPoint(top, bottom - right, outline));
                        //         GLDraw.lines.Add(new GLPoint(top, bottom + right, outline));
                        //         GLDraw.lines.Add(new GLPoint(bottom - right, bottom + right, outline));
                        // }
                        // else if (type == HandleType.Square)
                        // {
                        //         GLDraw.quads.Add(new GLPoint(position, color, size));
                        // }
                }
        }

        public enum HandleType
        {
                SolidCircle,
                Square,
                Circle,
                Triangle,
                None
        }
}
#endif
