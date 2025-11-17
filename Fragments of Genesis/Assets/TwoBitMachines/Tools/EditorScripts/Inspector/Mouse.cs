#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace TwoBitMachines.Editors
{
        public static class Mouse
        {
                public static bool up = false;
                public static bool anyUp = false;
                public static bool alt = false;
                public static bool ctrl = false;
                public static bool down = false;
                public static bool middleDown = false;
                public static bool holding = false;
                public static bool dragging = false;
                public static bool middleDragging = false;
                public static bool scrolling = false;
                public static bool rightDown = false;
                public static bool rightHold = false;
                public static bool middleHold = false;
                public static bool wasMiddle = false;
                public static bool wasUp = false;
                public static bool wasDragging = false;
                public static Vector3 position = Vector2.zero;
                public static Vector3 previousPosition = Vector2.zero;
                public static Vector3 delta;
                public static bool moved = false;
                public static Vector2 mousePosition => position;

                public static void Update ()
                {
                        moved = false;
                        delta = Vector3.zero;
                        Event guiEvent = Event.current;
                        Vector3 p = position;
                        position = Event.current.mousePosition;
                        if (SceneView.currentDrawingSceneView != null && SceneView.currentDrawingSceneView.camera != null)
                        {
                                Camera camera = SceneView.currentDrawingSceneView.camera;
                                Vector3 screenPos = position;
                                screenPos.y = camera.pixelHeight - screenPos.y;

                                // Check if the screen position is within the camera's view frustum
                                if (screenPos.x >= 0 && screenPos.x <= camera.pixelWidth &&
                                    screenPos.y >= 0 && screenPos.y <= camera.pixelHeight)
                                {
                                        // Assign valid depth before conversion
                                        screenPos.z = Vector3.Distance(camera.transform.position, SceneView.currentDrawingSceneView.pivot);
                                        screenPos.z = Mathf.Clamp(screenPos.z, camera.nearClipPlane, camera.farClipPlane);

                                        previousPosition = p;
                                        position = camera.ScreenToWorldPoint(screenPos);

                                        if ((previousPosition - position).magnitude > 5)
                                                previousPosition = position;

                                        delta = position - previousPosition;
                                        if (delta != Vector3.zero)
                                                moved = true;
                                }
                                else
                                {
                                        // Mouse is outside valid screen bounds, keep last position
                                        position = p;
                                }
                                // previousPosition = p;
                                // position.y = SceneView.currentDrawingSceneView.camera.pixelHeight - position.y;
                                // position = SceneView.currentDrawingSceneView.camera.ScreenToWorldPoint(position);
                                // if ((previousPosition - position).magnitude > 5)
                                //         previousPosition = position;
                                // delta = position - previousPosition;
                                // if (delta != Vector3.zero)
                                //         moved = true;
                        }

                        wasMiddle = middleDown;
                        wasUp = anyUp;
                        wasDragging = dragging;

                        anyUp = guiEvent.type == EventType.MouseUp;
                        up = (guiEvent.type == EventType.MouseUp && guiEvent.button == 0);
                        down = (guiEvent.type == EventType.MouseDown && guiEvent.button == 0);
                        middleDown = (guiEvent.type == EventType.MouseDown && guiEvent.button == 2);
                        rightDown = (guiEvent.type == EventType.MouseDown && guiEvent.button == 1);
                        scrolling = (guiEvent.type == EventType.ScrollWheel && guiEvent.button == 2);
                        ctrl = (guiEvent.modifiers == EventModifiers.Control);
                        alt = (guiEvent.modifiers == EventModifiers.Alt);
                        if (rightDown)
                                rightHold = true;
                        if (middleDown)
                                middleHold = true;
                        if (down)
                                holding = true;
                        if (up)
                                dragging = false;
                        if (anyUp)
                                holding = middleHold = rightHold = dragging = middleDragging = false;

                        if (holding && guiEvent.type == EventType.MouseDrag)
                                dragging = true;
                        if (middleHold && guiEvent.type == EventType.MouseDrag)
                                middleDragging = true;
                }

                public static bool ContainsMouse (this Rect rect)
                {
                        return rect.Contains(Event.current.mousePosition);
                }

                public static bool ContainsMouseDown (this Rect rect, bool UseEvent = true)
                {
                        if (rect.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown && Event.current.button == 0)
                        {
                                if (UseEvent)
                                        Layout.UseEvent();
                                return true;
                        }
                        // Debug.Log("False " + rect.Contains(Event.current.mousePosition) + "   " + (Event.current.type == EventType.MouseDown));
                        return false;
                }

                public static bool ContainsMouseDrag (this Rect rect)
                {
                        if (rect.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDrag && Event.current.button == 0)
                        {
                                GUI.changed = true;
                                Event.current.Use();
                                return true;
                        }
                        return false;
                }

                public static bool ContainsMouseDrag (this Rect rect, bool useEvent = true)
                {
                        if (rect.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDrag && Event.current.button == 0)
                        {
                                if (useEvent)
                                        Layout.UseEvent();
                                return true;
                        }
                        return false;
                }

                public static bool ContainsMouseRightDown (this Rect rect, bool useEvent = true)
                {
                        if (rect.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown && Event.current.button == 1)
                        {
                                if (useEvent)
                                        Layout.UseEvent();
                                return true;
                        }
                        return false;
                }


                public static bool ContainsMouseMiddleDown (this Rect rect, bool useEvent = true)
                {
                        if (rect.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown && Event.current.button == 2)
                        {
                                if (useEvent)
                                        Layout.UseEvent();
                                return true;
                        }
                        return false;
                }

                public static bool ContainsScrollWheel (this Rect rect, bool useEvent = true)
                {
                        if (rect.Contains(Event.current.mousePosition) && Event.current.type == EventType.ScrollWheel)
                        {
                                GUI.changed = true;
                                if (useEvent)
                                        Event.current.Use();
                                return true;
                        }
                        return false;
                }

                public static bool ContainsScrollWheelDrag (this Rect rect)
                {
                        if (rect.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDrag && Event.current.button == 2)
                        {
                                GUI.changed = true;
                                Event.current.Use();
                                return true;
                        }
                        return false;
                }

                public static bool ContainsMouseUp (this Rect rect, bool UseEvent = true)
                {
                        if (rect.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseUp && Event.current.button == 0)
                        {
                                if (UseEvent)
                                        Layout.UseEvent();
                                return true;
                        }
                        return false;
                }

                public static int MouseDeltaSign ()
                {
                        return (int) Mathf.Sign(Event.current.delta.y);
                }

                public static bool MouseDown (bool useEvent = false)
                {
                        if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
                        {
                                if (useEvent)
                                        Layout.UseEvent();
                                return true;
                        }
                        return false;
                }

                public static bool MouseUp (bool useEvent = false)
                {
                        if (Event.current.type == EventType.MouseUp && Event.current.button == 0)
                        {
                                if (useEvent)
                                        Layout.UseEvent();
                                return true;
                        }
                        return false;
                }

                public static bool MouseMiddleDown (bool useEvent = false)
                {
                        if (Event.current.type == EventType.MouseDown && Event.current.button == 2)
                        {
                                if (useEvent)
                                        Layout.UseEvent();
                                return true;
                        }
                        return false;
                }

                public static bool MouseDrag (bool useEvent = true)
                {
                        if (Event.current.type == EventType.MouseDrag && Event.current.button == 0)
                        {
                                GUI.changed = true;
                                if (useEvent)
                                        Event.current.Use();
                                return true;
                        }
                        return false;
                }

                public static bool MouseRightDown ()
                {
                        return Event.current.type == EventType.MouseDown && Event.current.button == 1;
                }

                public static bool EventScrollWheel (bool useEvent = false)
                {
                        if (Event.current.type == EventType.ScrollWheel)
                        {
                                if (useEvent)
                                        Layout.UseEvent();
                                return true;
                        }
                        return false;
                }

        }

}
#endif
