using UnityEditor;

namespace TwoBitMachines.Editors
{
        [CustomEditor(typeof(TextMeshProEffects))]
        public class TextMeshEffectEditor : UnityEditor.Editor
        {
                private TextMeshProEffects main;
                private SerializedObject parent;

                private void OnEnable ()
                {
                        main = target as TextMeshProEffects;
                        parent = serializedObject;
                        Layout.Initialize();
                }

                public override void OnInspectorGUI ()
                {
                        ShowType showType = main.showType;

                        Layout.Update();
                        Layout.VerticalSpacing(10);

                        parent.Update();

                        FoldOut.Box(2, Tint.PurpleDark);
                        {
                                parent.Field("Text Mesh", "textMesh");
                                parent.FieldToggle("Start On Enable", "startOnEnable");
                        }
                        Layout.VerticalSpacing(5);

                        FoldOut.Box(3, FoldOut.boxColor, extraHeight: 3);
                        {
                                parent.Slider("Typewriter Speed", "typewriterSpeed", 0.01f, 10f);
                                parent.Slider("Typewriter Wobble", "typewriterWobble", 0.01f, 5f);
                                parent.Field("Typewriter Fade", "typewriterFade");
                        }

                        if (FoldOut.FoldOutButton(parent.Get("eventsFoldOut")))
                        {
                                Fields.EventFoldOutEffectAndRate(parent.Get("onTyping"), parent.Get("worldEffect"), parent.Get("typingRate"), parent.Get("typingFoldOut"), "On Typing", color: FoldOut.boxColor);
                                Fields.EventFoldOut(parent.Get("onComplete"), parent.Get("completeFoldOut"), "On Complete");
                        }

                        if ((showType & ShowType.Wobble) != 0)
                        {
                                FoldOut.Box(2, FoldOut.boxColor);
                                {
                                        parent.Slider("Wobble Speed", "wobbleSpeed", 0.01f, 10f);
                                        parent.Field("Wobble Strength", "wobble");
                                }
                                Layout.VerticalSpacing(5);
                        }
                        if ((showType & ShowType.Wave) != 0)
                        {
                                FoldOut.Box(3, FoldOut.boxColor);
                                {
                                        parent.Slider("Wave Speed", "waveSpeed", 0.01f, 10f);
                                        parent.Field("Wave Strength", "waveStrength");
                                        parent.Field("Wave Phase", "wavePhase");
                                }
                                Layout.VerticalSpacing(5);
                        }
                        if ((showType & ShowType.WaveX) != 0)
                        {
                                FoldOut.Box(3, FoldOut.boxColor);
                                {
                                        parent.Slider("Wave Speed X", "waveSpeedX", 0.01f, 10f);
                                        parent.Field("Wave Strength X", "waveStrengthX");
                                        parent.Field("Wave Phase X", "wavePhaseX");
                                }
                                Layout.VerticalSpacing(5);
                        }
                        if ((showType & ShowType.Jitter) != 0)
                        {
                                FoldOut.Box(2, FoldOut.boxColor);
                                {
                                        parent.Slider("Jitter Rate", "jitterRate", 0.01f, 0.5f);
                                        parent.Field("Jitter Strength", "jitterStrength");
                                }
                                Layout.VerticalSpacing(5);
                        }
                        if ((showType & ShowType.Distortion) != 0)
                        {
                                FoldOut.Box(2, FoldOut.boxColor);
                                {
                                        parent.Slider("Distortion Rate", "distortionRate", 0.01f, 0.5f);
                                        parent.Field("Distortion Strength", "distortionStrength");
                                }
                                Layout.VerticalSpacing(5);
                        }

                        FoldOut.BoxSingle(1, Tint.BoxTwo);
                        {
                                parent.Field("Modify Extra", "showType");
                        }
                        Layout.VerticalSpacing(3);

                        parent.ApplyModifiedProperties();
                        Layout.VerticalSpacing(10);

                }

        }
}
