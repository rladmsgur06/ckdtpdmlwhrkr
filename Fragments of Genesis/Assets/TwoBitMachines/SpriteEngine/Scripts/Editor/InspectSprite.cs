using TwoBitMachines.Editors;
using UnityEditor;
using UnityEngine;

namespace TwoBitMachines.TwoBitSprite.Editors
{
        public static class InspectSprite
        {
                public static void Display (Editor editor, SerializedObject parent, SerializedProperty frames, SerializedProperty frameIndex, SerializedProperty sprite, bool usingExtra = false)
                {
                        if (frames.arraySize > 0)
                        {
                                SliderGlobalRate(frames, sprite.Get("globalRate"));
                                DisplaySprites(editor, frames, frameIndex, sprite, usingExtra);
                        }
                }

                private static void SliderGlobalRate (SerializedProperty frames, SerializedProperty globalRate)
                {
                        if (Slider.Set(Tint.White, Tint.SoftDark, globalRate, min: 0.25f, max: 60f, noGap: true))
                        {
                                for (int i = 0; i < frames.arraySize; i++)
                                {
                                        frames.Element(i).Get("rate").floatValue = 1f / globalRate.floatValue;
                                }
                        }
                }

                private static void DisplaySprites (Editor editor, SerializedProperty frames, SerializedProperty frameIndex, SerializedProperty sprite, bool usingExtra)
                {

                        if (evt.downArrow || evt.upArrow)
                        {
                                int newIndex = frameIndex.intValue + (evt.downArrow ? 1 : -1);
                                frameIndex.intValue = Compute.WrapArrayIndex(newIndex, frames.arraySize);
                                Event.current.Use();
                        }

                        for (int i = 0; i < frames.arraySize; i++)
                        {
                                SerializedProperty frame = frames.Element(i);
                                SerializedProperty rate = Mouse.holding ? frame.Get("rate") : null;
                                float ratePrevious = Mouse.holding ? rate.floatValue : 0;

                                Block.Header(frame).Style(frameIndex.intValue == i ? Tint.SoftDark * 1.6f : Tint.SoftDark)
                                     .MouseDown(() => frameIndex.intValue = i)
                                     .Grip(editor, frames, i)
                                     .DropArrow("eventFoldOut")
                                     .Field("sprite", weight: 0.8f)
                                     .Field("rate", weight: 0.2f, invert: true, rightSpace: 5)
                                     .Button("xsAdd", Tint.On, hide: false)
                                     .Button("xsMinus", Tint.Delete, hide: false)
                                     .Build();

                                if (Mouse.holding && rate != null && rate.floatValue != ratePrevious)
                                {
                                        sprite.Get("globalRate").floatValue = Mathf.Clamp(rate.floatValue == 0 ? 0 : 1f / rate.floatValue, 0.25f, 60f);
                                        for (int j = 0; j < frames.arraySize; j++)
                                        {
                                                frames.Element(j).Get("rate").floatValue = rate.floatValue;
                                        }
                                }

                                if (Header.SignalActive("GripUsed"))
                                {
                                        frameIndex.intValue = i;
                                        SpriteProperty.ReorderNestedArrays(sprite.Get("property"), ListReorder.srcItemIndex, ListReorder.dstItemIndex);
                                }
                                if (frame.Bool("eventFoldOut"))
                                {
                                        if (usingExtra)
                                        {
                                                FrameEvents(frame);
                                        }
                                        else
                                        {
                                                Fields.EventField(frame.Get("onEnterFrame"));
                                        }
                                }
                                if (Header.SignalActive("xsMinus"))
                                {
                                        frames.DeleteArrayElement(frameIndex.intValue);
                                        SpriteProperty.DeleteNestedArrayElement(sprite.Get("property"), frameIndex.intValue);
                                        frameIndex.intValue = Mathf.Clamp(frameIndex.intValue, 0, frames.arraySize - 1);
                                        return;
                                }
                                if (Header.SignalActive("xsAdd"))
                                {
                                        frames.InsertArrayElementAtIndex(i);
                                        SpriteProperty.MatchArraySize(sprite.Get("property"), frames.arraySize);
                                        frameIndex.intValue = Mathf.Clamp(frameIndex.intValue, 0, frames.arraySize - 1);
                                        return;
                                }
                                if (Block.boxRect.ContainsScrollWheel(true))
                                {
                                        int newIndex = frameIndex.intValue + evt.scrollDelta;
                                        frameIndex.intValue = Compute.WrapArrayIndex(newIndex, frames.arraySize);
                                }
                        }
                }

                public static void FrameEvents (SerializedProperty frame)
                {
                        SerializedProperty array = frame.Get("events");
                        for (int i = 0; i < array.arraySize; i++)
                        {
                                SerializedProperty element = array.Element(i);
                                if (Fields.EventFoldOutFloat(element.Get("frameEvent"), element.Get("atPercent"), element.Get("eventFoldOut"), "       Frame Event", offsetX: 20, ySpace: false, color: Tint.SoftDark))
                                {
                                        array.DeleteArrayElement(i);
                                        break;
                                }
                                ListReorder.Grip(frame, array, Layout.GetLastRect(15, 15, offsetY: 2), i, Tint.Blue);
                        }

                        if (FoldOut.CornerButton(Tint.Blue, ySpace: false))
                        {
                                array.arraySize++;
                        }
                        Layout.VerticalSpacing();
                }

        }
}
