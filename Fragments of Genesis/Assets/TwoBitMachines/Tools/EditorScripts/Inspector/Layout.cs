#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace TwoBitMachines.Editors
{
        public static class Layout
        {
                public static int shorten = 0;
                // public static int maxWidth => Screen.width - (25 + shorten);
                public static int maxWidth => (int) EditorGUIUtility.currentViewWidth - (25 + shorten);
                public static int infoWidth => maxWidth - 12;
                public static int longInfoWidth => infoWidth + 16;
                public static float labelWidth;
                public static float contentWidth;
                public static float buttonWidth = 15f;
                public static float boolWidth = 15f;
                public static int rectFieldHeight = 19; //8;
                public static int rectVertSpace = 2;
                public static bool previousGUIEnabled = false;
                public static float rectTempWidth = 0;
                public static float totalWidth => labelWidth + contentWidth;
                public static float half => contentWidth * 0.5f;
                public static float thrice => contentWidth * 0.333f;
                public static float quarter => contentWidth * 0.25f;
                public static float quint => contentWidth * 0.2f;
                private static bool initiated = false;

                public static void Initialize (Editor editor = null)
                {
                        if (initiated)
                        {
                                return; // save load time
                        }
                        EditorTools.LoadGUI("TwoBitMachines", "/Tools/Icons", Icon.icon);
                        Skin.Set();
                        Labels.InitializeStyle();
                        FoldOut.Initialize();
                        Block.Initialize();
                        initiated = true;
                }

                public static void Update (float labelWidthPercent = 0.45f)
                {
                        labelWidthPercent = Mathf.Clamp01(labelWidthPercent);
                        labelWidth = infoWidth * labelWidthPercent;
                        contentWidth = infoWidth * (1 - labelWidthPercent);
                        Labels.useWhite = EditorGUIUtility.isProSkin;
                        Mouse.Update();
                }

                public static void BeginGUIEnable (bool toggleState)
                {
                        previousGUIEnabled = GUI.enabled;
                        GUI.enabled = toggleState && previousGUIEnabled;
                }
                public static void EndGUIEnable (Rect rect = default(Rect), SerializedProperty toggle = null)
                {
                        GUI.enabled = previousGUIEnabled;
                        if (toggle != null)
                                EditorGUI.PropertyField(rect, toggle, GUIContent.none);
                }
                public static void UseEvent ()
                {
                        GUI.changed = true;
                        Event.current.Use();
                }

                #region Get Rects


                // public static Rect Set (Rect rect, float width, float height, float offsetX, float offsetY)
                // {
                //         rect.width = width;
                //         rect.height = height;
                //         rect.x += offsetX;
                //         rect.y += offsetY;
                //         return rect;
                // }

                public static Rect CreateRect (float width, float height, float offsetX = 0, float offsetY = 0)
                {
                        return Set(GUILayoutUtility.GetRect(0, height), width, height, offsetX, offsetY);
                }

                public static Rect CreateRectAndDraw (float width, float height, float offsetX = 0, float offsetY = 0, Texture2D texture = default(Texture2D), Color color = default(Color))
                {
                        // creating a gui rect that is near the same size as layout maxwidth will trigger a horizontal bar :( but this only happens if it's inside a guilayout area
                        // to avoid all issues in any scenario, create gui with an initial witdh of zero, then reset to desired size. this seems to fix the problem.
                        Rect rect = Set(GUILayoutUtility.GetRect(0, height), width, height, offsetX, offsetY);
                        Skin.Draw(rect, color == Color.clear ? Color.white : color, texture);
                        return rect;
                }

                public static Rect CreateBoxRect (float width, float height, float offsetX = 0, float bottomSpace = 0, Texture2D texture = default(Texture2D), Color color = default(Color))
                {
                        // creating a gui rect that is near the same size as layout maxwidth will trigger a horizontal bar :( but this only happens if it's inside a guilayout area
                        // to avoid all issues in any scenario, create gui with an initial witdh of zero, then reset to desired size. this seems to fix the problem.
                        Rect rect = Set(GUILayoutUtility.GetRect(0, height + bottomSpace), width, height, offsetX, 0);
                        Skin.Draw(rect, color == Color.clear ? Color.white : color, texture);
                        return rect;
                }

                public static Rect CreateRectField (float offsetX = 0, float offsetY = 0)
                {
                        Rect rect = Set(GUILayoutUtility.GetRect(0, rectFieldHeight), infoWidth, rectFieldHeight, offsetX - 3, offsetY);
                        rect.height -= 2;
                        rect.y += 1;
                        return rect;
                }

                public static Rect GetLastRect (float width, float height, float offsetX = 0, float offsetY = 0)
                {
                        return Set(GUILayoutUtility.GetLastRect(), width, height, offsetX, offsetY);
                }

                public static Rect GetLastRectDraw (float width, float height, float offsetX = 0, float offsetY = 0, Texture2D texture = default(Texture2D), Color color = default(Color))
                {
                        Rect rect = Set(GUILayoutUtility.GetLastRect(), width, height, offsetX, offsetY);
                        Skin.Draw(rect, color == Color.clear ? Color.white : color, texture);
                        return rect;
                }

                public static Rect Box (Color color, int height = FoldOut.h, int offsetY = 0)
                {
                        VerticalSpacing(1);
                        Rect rect = CreateRectAndDraw(longInfoWidth, height, -11, offsetY, Icon.Get("BackgroundLight128x128"), color);
                        return rect.CenterRectHeight(rectFieldHeight);
                }

                public static Rect CenterRectContent (this ref Rect rect, Vector2 contentSize)
                {
                        rect.x += (rect.width / 2f) - (contentSize.x / 2f);
                        rect.y += (rect.height / 2f) - (contentSize.y / 2f);
                        rect.width = contentSize.x;
                        rect.height = contentSize.y;
                        return rect;
                }

                public static Rect CenterRectContentForClipping (this ref Rect rect, Vector2 contentSize)
                {
                        float moveX = (rect.width / 2f) - (contentSize.x / 2f);
                        float moveY = (rect.height / 2f) - (contentSize.y / 2f);
                        float x = rect.x;
                        float y = rect.y;
                        float w = rect.width;
                        float h = rect.height;
                        rect.x += moveX;
                        rect.y += moveY;
                        if (rect.x < x)
                                rect.x = x;
                        if (rect.y < y)
                                rect.y = y;
                        rect.width -= moveX;
                        rect.height -= moveY;
                        if (rect.width < 0 || rect.width > w)
                                rect.width = w;
                        if (rect.height < 0 || rect.height > h)
                                rect.height = h;
                        return rect;
                }

                public static Rect CenterRectHeight (this ref Rect rect, float height = 17)
                {
                        rect.y += (rect.height / 2f) - (height / 2f);
                        rect.height = height;
                        return rect;
                }

                public static Rect Set (Rect rect, float width, float height, float offsetX, float offsetY)
                {
                        rect.width = width;
                        rect.height = height;
                        rect.x += offsetX;
                        rect.y += offsetY;
                        return rect;
                }

                public static Rect Offset (this ref Rect rect, float offsetX, float offsetY, float addWidth, float addHeight)
                {
                        return rect = Set(rect, rect.width + addWidth, rect.height + addHeight, offsetX, offsetY);
                }

                public static Rect Adjust (this ref Rect rect, float offsetX, float width, float offsetY = 0)
                {
                        return rect = Set(rect, width, rect.height, offsetX, offsetY);
                }

                public static void Pushclear ()
                {
                        rectTempWidth = 0;
                }

                public static Rect Push (this ref Rect rect, float width = 15f)
                {
                        float offsetX = rectTempWidth;
                        return rect = Set(rect, rectTempWidth = width, rect.height, offsetX, 0);
                }

                public static Rect OffsetX (this ref Rect rect, float offsetX)
                {
                        return rect = Set(rect, rect.width, rect.height, offsetX, 0);
                }

                public static void VerticalSpacing (int height = 1)
                {
                        GUILayoutUtility.GetRect(0, height);
                }

                #endregion
        }

}
#endif
