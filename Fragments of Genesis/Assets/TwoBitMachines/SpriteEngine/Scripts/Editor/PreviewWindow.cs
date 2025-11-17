using System.Collections.Generic;
using TwoBitMachines.Editors;
using UnityEditor;
using UnityEngine;

namespace TwoBitMachines.TwoBitSprite.Editors
{
        public static class PreviewWindow
        {
                public static void Display (Editor editor, SerializedObject parent, ref Sprite currentSprite, List<SpritePacket> sprites, int spriteIndex)
                {
                        if (!parent.Bool("previewArea"))
                        {
                                currentSprite = RetrieveSpriteToPreview(editor, sprites, spriteIndex, new ScrollWindowInfo(parent));
                                return;
                        }

                        Rect mainPreviewWindow = Layout.CreateRect(Layout.longInfoWidth, Mathf.Min(Layout.infoWidth, 350), offsetX: -11);

                        ScrollWindowInfo scrollInfo = new ScrollWindowInfo(parent);
                        Sprite sprite = currentSprite = RetrieveSpriteToPreview(editor, sprites, spriteIndex, scrollInfo);
                        SpriteWindowInfo spriteInfo = new SpriteWindowInfo(sprite, mainPreviewWindow, scrollInfo.zoom.floatValue);

                        MouseScrollEvents(mainPreviewWindow, scrollInfo, spriteInfo);
                        DisplaySpriteInWindow(mainPreviewWindow, scrollInfo, spriteInfo);

                }

                private static Sprite RetrieveSpriteToPreview (Editor editor, List<SpritePacket> sprites, int spriteIndex, ScrollWindowInfo scrollInfo)
                {
                        if (scrollInfo.resetPlayFrame.ReadBool())
                        {
                                scrollInfo.timer.floatValue = 0;
                                scrollInfo.frame.intValue = 0;
                        }

                        float timer = scrollInfo.timer.floatValue;
                        int frame = scrollInfo.frame.intValue;
                        Sprite sprite = null;
                        for (int i = 0; i < sprites.Count; i++)
                        {
                                if (i != spriteIndex)
                                        continue;
                                List<Frame> frameArray = sprites[i].frame;

                                if (!scrollInfo.playInInspector.boolValue) //                      if sprite is currently not playing in inspector
                                {
                                        int index = sprites[i].frameIndex;
                                        if (index >= 0 && index < frameArray.Count)
                                                sprite = frameArray[index].sprite;
                                }
                                else if (frameArray.Count > 0) //                                  if play in inspector is true, then play sprite animation
                                {
                                        frame = Mathf.Clamp(frame, 0, frameArray.Count - 1);
                                        if (TwoBitMachines.Clock.TimerEditor(ref timer, frameArray[frame].rate))
                                                frame = ++frame >= frameArray.Count ? 0 : frame;
                                        sprite = frameArray[frame].sprite;
                                        editor.Repaint();
                                }
                        }

                        scrollInfo.timer.floatValue = timer;
                        scrollInfo.frame.intValue = frame;
                        return sprite;
                }

                #region Sprite Engine Extra
                public static void Display (SpriteEngineExtraEditor editor, SerializedObject parent, SpriteEngineExtra main)
                {
                        Rect mainPreviewWindow = Layout.CreateRect(Layout.longInfoWidth, Mathf.Min(Layout.infoWidth, 350), offsetX: -11);

                        ScrollWindowInfo scrollInfo = new ScrollWindowInfo(parent);
                        Sprite sprite = editor.currentSprite = RetrieveSpriteToPreview(editor, main, scrollInfo);
                        SpriteWindowInfo spriteInfo = new SpriteWindowInfo(sprite, mainPreviewWindow, scrollInfo.zoom.floatValue);

                        MouseScrollEvents(mainPreviewWindow, scrollInfo, spriteInfo);
                        DisplaySpriteInWindow(mainPreviewWindow, scrollInfo, spriteInfo);
                }

                private static Sprite RetrieveSpriteToPreview (SpriteEngineExtraEditor editor, SpriteEngineExtra main, ScrollWindowInfo scrollInfo)
                {
                        if (scrollInfo.resetPlayFrame.ReadBool())
                        {
                                scrollInfo.timer.floatValue = 0;
                                scrollInfo.frame.intValue = 0;
                        }

                        float timer = scrollInfo.timer.floatValue;
                        int frame = scrollInfo.frame.intValue;
                        Sprite sprite = null;
                        for (int i = 0; i < main.sprites.Count; i++)
                        {
                                if (i != main.spriteIndex)
                                        continue;
                                List<FrameExtra> frameArray = main.sprites[i].frame;

                                if (!scrollInfo.playInInspector.boolValue) //                      if sprite is currently not playing in inspector
                                {
                                        int index = main.sprites[i].frameIndex;
                                        if (index >= 0 && index < frameArray.Count)
                                                sprite = frameArray[index].sprite;
                                }
                                else if (frameArray.Count > 0) //                                  if play in inspector is true, then play sprite animation
                                {
                                        frame = Mathf.Clamp(frame, 0, frameArray.Count - 1);
                                        if (TwoBitMachines.Clock.TimerEditor(ref timer, frameArray[frame].rate))
                                                frame = ++frame >= frameArray.Count ? 0 : frame;
                                        sprite = frameArray[frame].sprite;
                                        editor.Repaint();
                                }
                        }

                        scrollInfo.timer.floatValue = timer;
                        scrollInfo.frame.intValue = frame;
                        return sprite;
                }
                #endregion

                private static void MouseScrollEvents (Rect viewRect, ScrollWindowInfo scrollInfo, SpriteWindowInfo spriteInfo)
                {
                        if (viewRect.ContainsMouseRightDown()) //                           reset sprite position and zoom level with right click
                        {
                                scrollInfo.scrollPosition.vector2Value = Vector2.zero;
                                scrollInfo.offsetPosition.vector2Value = Vector2.zero;
                                scrollInfo.zoom.floatValue = 1;
                        }
                        if (viewRect.ContainsScrollWheel()) //                              zoom sprite
                        {
                                float scale = spriteInfo.scale;
                                float baseScale = spriteInfo.baseScale;
                                Vector2 spriteSize = spriteInfo.spriteSize;
                                scrollInfo.zoom.floatValue += Event.current.delta.y < 0 ? 0.25f : -0.25f;
                                scrollInfo.zoom.floatValue = Mathf.Clamp(scrollInfo.zoom.floatValue, 0.25f, 10f);
                                float newScale = baseScale * scrollInfo.zoom.floatValue;
                                scrollInfo.offsetPosition.vector2Value += new Vector2((scale - newScale) * spriteSize.x, (scale - newScale) * spriteSize.y) / 2f; // adjust texture position for zoom
                        }
                        if (viewRect.ContainsScrollWheelDrag()) //                           move sprite inside window
                        {
                                scrollInfo.offsetPosition.vector2Value += Event.current.delta;
                        }
                }

                private static void DisplaySpriteInWindow (Rect viewRect, ScrollWindowInfo scrollInfo, SpriteWindowInfo spriteInfo)
                {
                        Vector2 offset = ScrollView.Begin(viewRect, scrollInfo.scrollPosition, spriteInfo.spritePosition, scrollInfo.color.colorValue);
                        {
                                float baseOffsetX = Mathf.Abs(viewRect.width - spriteInfo.spriteSize.x * spriteInfo.baseScale) / 2f;
                                float baseOffsetY = Mathf.Abs(viewRect.height - spriteInfo.spriteSize.y * spriteInfo.baseScale) / 2f;
                                spriteInfo.spritePosition.x = scrollInfo.offsetPosition.vector2Value.x - offset.x + baseOffsetX;
                                spriteInfo.spritePosition.y = scrollInfo.offsetPosition.vector2Value.y - offset.y + baseOffsetY;
                                if (spriteInfo.spriteTexture != null)
                                        GUI.DrawTextureWithTexCoords(spriteInfo.spritePosition, spriteInfo.spriteTexture, spriteInfo.spriteRect);
                        }
                        ScrollView.End();
                        Rect colorRect = Layout.GetLastRect(width: 15, height: 13, offsetX: -11, offsetY: 0); //place color field on top left corner of window
                        scrollInfo.color.colorValue = EditorGUI.ColorField(colorRect, GUIContent.none, scrollInfo.color.colorValue, false, true, false);
                }

        }

        public struct SpriteWindowInfo
        {
                public Texture2D spriteTexture;
                public Vector2 spriteSize;
                public Rect spritePosition;
                public Rect spriteRect;
                public float baseScale;
                public float scale;

                public SpriteWindowInfo (Sprite sprite, Rect viewRect, float zoomLevel)
                {
                        spriteTexture = null;
                        spriteRect = viewRect;
                        spriteSize = Vector2.one;
                        Util.ConfigureSpriteDimensions(sprite, ref spriteRect, ref spriteTexture, ref spriteSize);

                        // Initial sprite zoom level
                        baseScale = Mathf.Min((viewRect.height / spriteSize.y), (viewRect.width / spriteSize.x));
                        scale = baseScale * zoomLevel;
                        spritePosition = new Rect(viewRect) { width = scale * spriteSize.x, height = scale * spriteSize.y };
                }

        }

        public struct ScrollWindowInfo
        {
                public SerializedProperty playInInspector;
                public SerializedProperty spriteIndex;
                public SerializedProperty scrollPosition;
                public SerializedProperty offsetPosition;
                public SerializedProperty resetPlayFrame;
                public SerializedProperty frame;
                public SerializedProperty timer;
                public SerializedProperty color;
                public SerializedProperty zoom;

                public ScrollWindowInfo (SerializedObject parent)
                {
                        playInInspector = parent.Get("playInInspector");
                        spriteIndex = parent.Get("spriteIndex");
                        scrollPosition = parent.Get("scrollPosition");
                        offsetPosition = parent.Get("offsetPosition");
                        resetPlayFrame = parent.Get("resetPlayFrame");
                        frame = parent.Get("frameCounter");
                        timer = parent.Get("frameTimer");
                        color = parent.Get("color");
                        zoom = parent.Get("zoom");
                }
        }
}
