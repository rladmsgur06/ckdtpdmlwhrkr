using System;
using System.Reflection;
using TwoBitMachines.Editors;
using UnityEditor;
using UnityEngine;

namespace TwoBitMachines.FlareEngine.Editors
{
        [CustomEditor(typeof(AudioManager))]
        public class AudioManagerEditor : UnityEditor.Editor
        {
                private AudioManager main;
                private SerializedObject parent;
                private GameObject objReference;
                public static string inputName = "Name";

                private void OnEnable ()
                {
                        main = target as AudioManager;
                        parent = serializedObject;
                        objReference = main.gameObject;
                        Layout.Initialize();
                }

                public override void OnInspectorGUI ()
                {
                        Layout.Update();
                        Layout.VerticalSpacing(10);
                        parent.Update();

                        Block.Box(2, FoldOut.boxColor);
                        {
                                parent.Field_("Music", "music");
                                parent.Slider_("Master Volume", "musicMasterVolume");
                        }
                        Block.Box(2, FoldOut.boxColor);
                        {
                                parent.Field_("SFX", "sfx");
                                parent.Slider_("Master Volume", "sfxMasterVolume");
                        }

                        Block.Box(5, FoldOut.boxColor);
                        {
                                parent.Field_("AudioManagerSO", "audioManagerSO");
                                parent.Field_("Fade In Time", "fadeInTime");
                                parent.Field_("Fade Out Time", "fadeOutTime");
                                parent.FieldAndEnable_("Play On Start", "startMusic", "playOnStart");
                                parent.FieldToggleAndEnable_("Main Manager", "isMainManager");
                        }

                        SerializedProperty categories = parent.Get("categories");
                        if (Block.InputAndButtonBox("Audio Category", "Add", Tint.Blue, ref inputName))
                        {
                                categories.arraySize++;
                                categories.LastElement().Get("name").stringValue = inputName;
                                categories.LastElement().Get("audio").arraySize = 0;
                                categories.LastElement().Get("foldOut").boolValue = true;
                        }

                        for (int i = 0; i < categories.arraySize; i++)
                        {
                                SerializedProperty category = categories.Element(i);
                                SerializedProperty audio = category.Get("audio");

                                Header header = Block.Header(category).Style(Tint.Orange)
                                                     .Grip(this, categories, i)
                                                     .Fold(category.String("name"), bold: true, color: Tint.White)
                                                     .HiddenButton("deleteAsk", "Delete")
                                                     .Toggle("deleteAsk", "DeleteAsk")
                                                     .Button();
                                if (header.Build())
                                {
                                        if (Header.SignalActive("Add"))
                                        {
                                                audio.arraySize++;
                                                audio.LastElement().Get("volume").floatValue = 1f;
                                                break;
                                        }

                                        if (Header.SignalActive("Delete"))
                                        {
                                                categories.DeleteArrayElement(i);
                                                break;
                                        }

                                        for (int j = 0; j < audio.arraySize; j++)
                                        {
                                                SerializedProperty element = audio.Element(j);
                                                SerializedProperty childList = element.Get("childList");

                                                Block.Header(element).Style(Tint.SoftDark)
                                                     .Grip(this, audio, j)
                                                     .Field("clip", 0.65f)
                                                     .Field("volume", 0.17f)
                                                     .Field("type", 0.18f, yoffset: 1)
                                                     .Space(4)
                                                     .Button("Add", hide: false, tooltip: "Add audio variation")
                                                     .Button("Play", hide: false)
                                                     .Button("Red", hide: false)
                                                     .Button("Delete", hide: false)
                                                     .Build();

                                                element.Clamp("volume");

                                                if (Header.SignalActive("Delete"))
                                                {
                                                        audio.MoveArrayElement(j, audio.arraySize - 1);
                                                        audio.arraySize--;
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
                                }
                        }
                        parent.ApplyModifiedProperties();
                        Layout.VerticalSpacing(20);

                }
                //https: //forum.unity.com/threads/way-to-play-audio-in-editor-using-an-editor-script.132042/
                public static void PlayClip (AudioClip clip, int startSample = 0, bool loop = false)
                {
                        if (clip == null)
                                return;
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
// using System;
// using System.Reflection;
// using UnityEditor;
// using UnityEngine;
// using UnityEditor.UIElements;
// using UnityEngine.UIElements;
// using TwoBitMachines.UIElement.Editor;

// namespace TwoBitMachines.FlareEngine.Editors
// {
//         [CustomEditor(typeof(AudioManager))]
//         public class AudioManagerEditor : UnityEditor.Editor
//         {
//                 public AudioManager main;
//                 public VisualElement root;
//                 public SerializedObject so;

//                 private void OnEnable ()
//                 {
//                         so = serializedObject;
//                         main = target as AudioManager;
//                         root = ElementTools.InitializeRoot();
//                 }

//                 public override VisualElement CreateInspectorGUI ()
//                 {
//                         Block box = new Block(root, so, so, Tint.BoxGrey);
//                         {
//                                 box.Field("Music", "music");
//                                 box.Slider("Master Volume", "musicMasterVolume");
//                         }
//                         box = new Block(root, so, so, Tint.BoxGrey);
//                         {
//                                 box.Field("SFX", "sfx");
//                                 box.Slider("Master Volume", "sfxMasterVolume");
//                         }
//                         box = new Block(root, so, so, Tint.BoxGrey);
//                         {
//                                 box.Field("AudioManagerSO", "audioManagerSO");
//                                 box.Field("Fade In Time", "fadeInTime");
//                                 box.Field("Fade Out Time", "fadeOutTime");
//                                 box.FieldAndEnable("Play On Start", "startMusic", "playOnStart");
//                                 box.FieldToggle("Main Manager", "isMainManager");
//                         }
//                         Template.InputAndButton(root, "Audio Category", "Add", Tint.Blue, out TextField text).clicked += () =>
//                         {
//                                 SerializedProperty categoryArray = so.Get("categories");
//                                 categoryArray.arraySize++;
//                                 categoryArray.LastElement().Get("name").stringValue = text.value;
//                                 categoryArray.LastElement().Get("audio").arraySize = 0;
//                                 text.value = "Name";
//                                 AddCategoryElement(root, categoryArray, categoryArray.LastIndex());
//                                 so.ApplyProperties();
//                         };

//                         SerializedProperty categoryArray = so.Get("categories");
//                         for (int i = 0; i < categoryArray.arraySize; i++)
//                         {
//                                 AddCategoryElement(root, categoryArray, i);
//                         }
//                         return root;
//                 }

//                 public void AddCategoryElement (VisualElement container, SerializedProperty array, int index)
//                 {
//                         SerializedProperty property = array.Element(index);
//                         var unit = new BarPlus(container, so, property, Tint.Orange, array)
//                                 .Grip()
//                                 .FoldOut(property.String("name"), Color.white, 1, 1, bold: true)
//                                 .GetBarPlus();

//                         unit.Callback("Delete", out Button deleteButton, hide: true).clicked += () =>
//                         {
//                                 unit.DeleteUnit();
//                         };
//                         unit.Callback("DeleteAsk", hide: true).clicked += () =>
//                         {
//                                 deleteButton.ToggleVisibility();
//                         };
//                         unit.Callback("Add", hide: true).clicked += () =>
//                         {
//                                 root.Unbind();
//                                 {
//                                         SerializedProperty array = unit.property.Get("audio");
//                                         array.arraySize++;
//                                         array.LastElement().Get("volume").floatValue = 1f;
//                                         AddAudioElement(unit.content, array, array.LastIndex());
//                                 }
//                                 unit.ApplyBindChanges();
//                         };
//                         unit.LazyContentLoad(() =>
//                         {
//                                 SerializedProperty audioArray = property.Get("audio");
//                                 for (int j = 0; j < audioArray.arraySize; j++)
//                                 {
//                                         AddAudioElement(unit.content, audioArray, j);
//                                 }
//                         });
//                 }

//                 public void AddAudioElement (VisualElement container, SerializedProperty array, int index)
//                 {
//                         var unit = new Bar(container, so, array.Element(index), Tint.BoxTwo, array) // array
//                                 .Grip(spaceRight: 7)
//                                 .Field("clip", 100)
//                                 .Field("volume", 21, 0, 1f)
//                                 .Field("type", 28)
//                                 .BottomSpace();

//                         unit.Callback("Red").clicked += () =>
//                         {
//                                 StopAllClips();
//                         };
//                         unit.Callback("Play").clicked += () =>
//                         {
//                                 StopAllClips();
//                                 PlayClip(unit.property.Object("clip") as AudioClip);
//                         };
//                         unit.Callback("Delete").clicked += () =>
//                         {
//                                 unit.DeleteUnit();
//                         };
//                 }

//                 //https: //forum.unity.com/threads/way-to-play-audio-in-editor-using-an-editor-script.132042/
//                 public static void PlayClip (AudioClip clip, int startSample = 0, bool loop = false)
//                 {
//                         if (clip == null)
//                                 return;
//                         Assembly unityEditorAssembly = typeof(AudioImporter).Assembly;

//                         Type audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");
//                         MethodInfo method = audioUtilClass.GetMethod(
//                                 "PlayPreviewClip",
//                                 BindingFlags.Static | BindingFlags.Public,
//                                 null,
//                                 new Type[] { typeof(AudioClip), typeof(int), typeof(bool) },
//                                 null
//                         );

//                         method.Invoke(null, new object[] { clip, startSample, loop });
//                 }

//                 public static void StopAllClips ()
//                 {
//                         Assembly unityEditorAssembly = typeof(AudioImporter).Assembly;

//                         Type audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");
//                         MethodInfo method = audioUtilClass.GetMethod(
//                                 "StopAllPreviewClips",
//                                 BindingFlags.Static | BindingFlags.Public,
//                                 null,
//                                 new Type[] { },
//                                 null
//                         );

//                         method.Invoke(null, new object[] { });
//                 }
//         }
// }


