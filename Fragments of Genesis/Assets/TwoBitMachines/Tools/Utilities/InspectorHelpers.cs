#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;

namespace TwoBitMachines.Editors
{
        public static class Icon
        {
                public static List<Texture2D> icon = new List<Texture2D>();

                public static Texture2D Get (string name)
                {
                        return icon.GetIcon(name);
                }
        }

        public static class Tint // Color "Manager"
        {
                public static Color Header = new Color32(111, 111, 111, 255);
                public static Color Box = new Color32(165, 165, 165, 255);
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
                public static Color SlateGrey = new Color32(112, 128, 144, 255);

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
                public static Color WhiteOpacity140 = new Color32(255, 255, 255, 140);
                public static Color WhiteOpacity180 = new Color32(255, 255, 255, 180);
                public static Color WhiteOpacity240 = new Color32(255, 255, 255, 240);
                public static Color LightGrey = new Color32(202, 200, 200, 255);
                public static Color EventColor = new Color32(228, 229, 228, 255);
                public static Color AlwaysState = new Color32(123, 133, 215, 255);
                // Color: #f85a9d
        }

        public static class Skin
        {
                public static Texture2D square;
                public static Texture2D round;
                public static GUIStyle style = new GUIStyle();

                public static void Set ()
                {
                        square = Icon.Get("Square");
                        round = Icon.Get("Round");
                        style.margin = new RectOffset(0, 0, 0, 0);
                        style.padding = new RectOffset(0, 0, 0, 0);
                        style.border = new RectOffset(7, 7, 7, 7);
                }

                public static void Draw (Rect rect, Color color, Texture2D texture)
                {
                        texture = texture == null ? Skin.square : texture;
                        Color previousColor = GUI.color;
                        GUI.color = color;
                        style.normal.background = texture;
                        style.hover.textColor = Color.red;
                        style.onHover.textColor = Color.red;
                        if (Event.current.type == EventType.Repaint)
                                style.Draw(rect, true, false, false, false);

                        GUI.color = previousColor;
                }

                public static Rect TextureCentered (this Rect rect, Texture2D texture, Vector2 textureSize, Color color, int shiftY = 0)
                {
                        Color previous = GUI.color;
                        GUI.color = color == Color.clear ? Color.white : color;
                        rect.CenterRectContent(textureSize);
                        rect.y += shiftY;
                        if (texture != null)
                                GUI.DrawTexture(rect, texture);
                        GUI.color = previous;
                        return rect;
                }

                public static void DrawTexture (this Rect rect, Texture2D texture, Color color, float xOffset = 0)
                {
                        Color previous = GUI.color;
                        rect.x += xOffset;
                        GUI.color = color == Color.clear ? Color.white : color;
                        if (texture != null)
                                GUI.DrawTexture(rect, texture);
                        GUI.color = previous;
                }

                public static void DrawSprite (this Rect rect, Sprite sprite, float xOffset = 0)
                {
                        if (sprite == null)
                                return;
                        rect.x += xOffset;
                        Rect spriteRect = sprite.rect;
                        Texture2D tex = sprite.texture;
                        GUI.DrawTextureWithTexCoords(rect, tex, new Rect(spriteRect.x / tex.width, spriteRect.y / tex.height, spriteRect.width / tex.width, spriteRect.height / tex.height));
                }

                public static Rect DrawRect (this Rect rect, Color color, Texture2D texture = null)
                {
                        Skin.Draw(rect, color, texture == null ? Texture2D.whiteTexture : texture);
                        return rect;
                }


        }

}
#endif
