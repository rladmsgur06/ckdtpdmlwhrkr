using System.Collections.Generic;
using TwoBitMachines.Editors;
using UnityEditor;
using UnityEngine;

namespace TwoBitMachines.FlareEngine.Editors
{
        [CustomEditor(typeof(ManageScenes))]
        public class ManageScenesEditor : UnityEditor.Editor
        {
                private ManageScenes main;
                private SerializedObject so;
                private List<string> sceneNames = new List<string>();

                private void OnEnable ()
                {
                        main = target as ManageScenes;
                        so = serializedObject;

                        Layout.Initialize();

                        int sceneCount = Util.SceneCount();
                        sceneNames.Clear();
                        for (int i = 0; i < sceneCount; i++)
                        {
                                sceneNames.Add(Util.GetSceneName(i));
                        }
                }

                public override void OnInspectorGUI ()
                {
                        Layout.Update();
                        Layout.VerticalSpacing(10);
                        so.Update();
                        {
                                SceneManagement();
                        }
                        so.ApplyModifiedProperties();
                }

                public void SceneManagement ()
                {
                        Block.Header(so).Style(Tint.Blue).Label("Manage Scenes", 12, false, FoldOut.titleColor).Button().Build();
                        Block.boxRect.Tooltip("Add Transition/LoadScene");

                        int size = sceneNames == null || sceneNames.Count == 0 ? 2 : 4;
                        Block.Box(size, Tint.Blue, extraHeight: 3, noGap: true);
                        {
                                if (sceneNames != null)
                                {
                                        so.DropDownList_(sceneNames, "Next Scene", "nextSceneName");
                                        so.DropDownList_(sceneNames, "Menu Scene", "menuName");
                                }
                                so.Field_("Load Scene", "loadSceneOn");
                                so.Field_("Pause Game", "pause");
                        }

                        if (Block.ExtraFoldout(so, "randomFoldOut", ""))
                        {
                                SerializedProperty array = so.Get("text");
                                Block.BoxArray(array, FoldOut.boxColor, 23, false, 1, "Random Texture", (height, index) =>
                                {
                                        Block.Header(array.Element(index)).BoxRect(FoldOut.boxColor, leftSpace: 5, height: height)
                                             .Field(array.Element(index)).ArrayButtons().BuildGet().ReadArrayButtons(array, index);
                                });
                        }

                        SerializedProperty steps = so.Get("step");
                        if (Header.SignalActive("Add") || steps.arraySize == 0)
                        {
                                steps.arraySize++;
                                steps.LastElement().Get("type").intValue = 2;
                                steps.LastElement().Get("time").floatValue = 1f;
                                steps.LastElement().Get("loadSpeed").floatValue = 1f;
                        }

                        for (int i = 0; i < steps.arraySize; i++)
                        {
                                SerializedProperty step = steps.Element(i);
                                LoadStepType type = (LoadStepType) step.Enum("type");

                                Block.Header().Style(FoldOut.boxColor).Grip(this, steps, i).Label(type.ToString(), 12, false, FoldOut.titleColor).Button("Delete").Build();

                                if (Header.SignalActive("Delete"))
                                {
                                        steps.DeleteArrayElement(i);
                                        break;
                                }

                                if (type == LoadStepType.TransitionIn || type == LoadStepType.TransitionOut)
                                {
                                        Block.Box(3, FoldOut.boxColor, extraHeight: 3, noGap: true);
                                        {
                                                step.Field_("Type", "type");
                                                step.FieldAndEnum_("GameObject", "gameObject", "deactivate");
                                                step.Field_("Transition Time", "time");
                                        }

                                        if (Block.ExtraFoldout(step, "eventFoldOut"))
                                        {
                                                Fields.EventFoldOut(step.Get("onStart"), step.Get("startFoldOut"), "On Start", space: false);
                                                Fields.EventFoldOut(step.Get("onComplete"), step.Get("completeFoldOut"), "On Complete");
                                                Layout.VerticalSpacing();
                                        }
                                }
                                else
                                {
                                        Block.Box(3, FoldOut.boxColor, extraHeight: 3, noGap: true);
                                        {
                                                step.Field_("Type", "type");
                                                step.FieldAndEnum_("GameObject", "gameObject", "deactivate");
                                                step.Slider_("Load Speed", "loadSpeed");
                                        }

                                        if (Block.ExtraFoldout(step, "eventFoldOut"))
                                        {
                                                Fields.EventFoldOut(step.Get("onStart"), step.Get("startFoldOut"), "On Start", space: false);
                                                Fields.EventFoldOut(step.Get("onComplete"), step.Get("completeFoldOut"), "On Complete");
                                                Fields.EventFoldOut(step.Get("loadingProgress"), step.Get("loadingFoldOut"), "Loading Progress Float");
                                                Fields.EventFoldOut(step.Get("loadingProgressString"), step.Get("stringFoldOut"), "Loading Progress String");
                                                Layout.VerticalSpacing();
                                        }
                                }

                        }

                }

        }
}
