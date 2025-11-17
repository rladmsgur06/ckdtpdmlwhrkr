using System;
using System.Reflection;
using TwoBitMachines.Editors;
using UnityEditor;
using UnityEngine;

namespace TwoBitMachines.FlareEngine.Editors
{
        [CanEditMultipleObjects, CustomEditor(typeof(AudioSFXGroup))]
        public class AudioSFXGroupEditor : UnityEditor.Editor
        {
                private SerializedObject parent;
                public static string inputName = "Name";

                private void OnEnable ()
                {
                        parent = serializedObject;
                        Layout.Initialize();
                }

                public override void OnInspectorGUI ()
                {
                        Layout.Update();
                        Layout.VerticalSpacing(10);

                        parent.Update();
                        {
                                FoldOut.BoxSingle(3, Tint.SoftDark);
                                {
                                        parent.Field("Audio Manager", "audioManager");
                                        parent.Field("Time Rate", "timeRate");
                                        parent.FieldAndEnable("Attenuate", "distance", "attenuate");
                                }
                                Layout.VerticalSpacing(3);

                                SerializedProperty sfx = parent.Get("sfx");

                                for (int i = 0; i < sfx.arraySize; i++)
                                {
                                        SerializedProperty element = sfx.Element(i);
                                        SerializedProperty childList = element.Get("childList");

                                        Block.Header(element).Style(Tint.SoftDark)
                                        .Grip(this, sfx, i)
                                        .Field("clip", 0.8f)
                                        .Field("volume", 0.2f)
                                        .Space(4)
                                        .Button("Add", hide: false, tooltip: "Add audio variation")
                                        .Button("Play", hide: false)
                                        .Button("Red", hide: false)
                                        .Button("Delete", hide: false)
                                        .Build();

                                        element.Clamp("volume");

                                        if (Header.SignalActive("Delete"))
                                        {
                                                sfx.MoveArrayElement(i, sfx.arraySize - 1);
                                                sfx.arraySize--;
                                                break;
                                        }
                                        if (Header.SignalActive("Play"))
                                        {
                                                StopAllClips();
                                                PlayClip(element.Get("clip").objectReferenceValue as AudioClip);
                                        }
                                        if (Header.SignalActive("Red"))
                                        {
                                                StopAllClips();
                                        }
                                        if (Header.SignalActive("Add"))
                                        {
                                                childList.arraySize++;
                                                childList.LastElement().Get("volume").floatValue = 1f;
                                                childList.LastElement().Get("probability").floatValue = 0.5f;
                                        }

                                        for (int k = 0; k < childList.arraySize; k++)
                                        {
                                                SerializedProperty childElement = childList.Element(k);

                                                Block.Header(childElement).Style(Tint.BoxTwo, shiftX: 10)
                                                  .Grip(this, childList, k)
                                                  .Field("clip", 0.6f)
                                                  .Field("volume", 0.2f)
                                                  .Field("probability", 0.2f)
                                                  .Space(4)
                                                  .Button("Play", hide: false)
                                                  .Button("Red", hide: false)
                                                  .Button("Delete", hide: false)
                                                  .Build();

                                                childElement.Clamp("volume");

                                                if (Header.SignalActive("Delete"))
                                                {
                                                        childList.MoveArrayElement(k, childList.arraySize - 1);
                                                        childList.arraySize--;
                                                        break;
                                                }
                                                if (Header.SignalActive("Play"))
                                                {
                                                        StopAllClips();
                                                        PlayClip(childElement.Get("clip").objectReferenceValue as AudioClip);
                                                }
                                                if (Header.SignalActive("Red"))
                                                {
                                                        StopAllClips();
                                                }
                                        }
                                }

                                if (FoldOut.CornerButton(Tint.Blue))
                                {
                                        sfx.arraySize++;
                                        sfx.LastElement().Get("volume").floatValue = 1f;
                                }

                        }
                        parent.ApplyModifiedProperties();
                        Layout.VerticalSpacing(10);

                }

                //* https: //forum.unity.com/threads/way-to-play-audio-in-editor-using-an-editor-script.132042/
                public static void PlayClip (AudioClip clip, int startSample = 0, bool loop = false)
                {
                        if (clip == null)
                        {
                                return;
                        }
                        Assembly unityEditorAssembly = typeof(AudioImporter).Assembly;

                        Type audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");
                        MethodInfo method = audioUtilClass.GetMethod(
                                "PlayPreviewClip",
                                BindingFlags.Static | BindingFlags.Public,
                                null,
                                new Type[] { typeof(AudioClip), typeof(int), typeof(bool) },
                                null
                        );

                        method.Invoke(null, new object[] { clip, startSample, loop });
                }

                public static void StopAllClips ()
                {
                        Assembly unityEditorAssembly = typeof(AudioImporter).Assembly;

                        Type audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");
                        MethodInfo method = audioUtilClass.GetMethod(
                                "StopAllPreviewClips",
                                BindingFlags.Static | BindingFlags.Public,
                                null,
                                new Type[] { },
                                null
                        );
                        method.Invoke(null, new object[] { });
                }

        }
}
