#if UNITY_EDITOR
using UnityEngine;

namespace TwoBitMachines.Editors
{
        public static class ButtonRect
        {
                public static bool Get (Rect rect, Texture2D texture, Color color, bool isSelected = false, bool center = false, string toolTip = "") // center Content
                {
                        bool pressed = false;
                        int controlID = GUIUtility.GetControlID(1003, FocusType.Passive);
                        if (GUIUtility.hotControl == controlID)
                        {
                                GUI.FocusControl(null);
                        }
                        switch (Event.current.GetTypeForControl(controlID))
                        {
                                case EventType.Repaint:
                                        if (toolTip != "")
                                        {
                                                GUI.Label(rect, new GUIContent("", toolTip));
                                        }
                                        if (GUIUtility.hotControl == controlID || isSelected)
                                        {
                                                color *= Tint.Grey;
                                        }
                                        if (center)
                                        {
                                                Skin.Draw(rect.CenterRectContent(new Vector2(texture.width, texture.height)), color, texture);
                                        }
                                        if (!center)
                                        {
                                                Skin.Draw(rect, color, texture);
                                        }
                                        break;
                                case EventType.MouseDown:
                                        if (rect.Contains(Event.current.mousePosition) && Event.current.button == 0)
                                        {
                                                GUIUtility.hotControl = controlID;
                                                Layout.UseEvent();
                                        }
                                        break;
                                case EventType.MouseUp:
                                        if (GUIUtility.hotControl == controlID)
                                        {
                                                if (rect.Contains(Event.current.mousePosition))
                                                {
                                                        pressed = true;
                                                }
                                                GUIUtility.hotControl = 0;
                                                Layout.UseEvent();
                                        }
                                        break;
                        }
                        return pressed;
                }
        }
}
#endif
