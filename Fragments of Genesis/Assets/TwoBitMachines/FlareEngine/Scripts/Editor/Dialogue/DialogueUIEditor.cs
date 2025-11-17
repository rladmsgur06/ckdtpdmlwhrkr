using System.Threading;
using TwoBitMachines.Editors;
using UnityEditor;
using UnityEngine;

namespace TwoBitMachines.FlareEngine.Editors
{
        [CustomEditor(typeof(DialogueUI))]
        public class DialogueUIEditor : UnityEditor.Editor
        {
                private DialogueUI main;
                private SerializedObject parent;

                private void OnEnable ()
                {
                        main = target as DialogueUI;
                        parent = serializedObject;
                        Layout.Initialize();
                }

                public override void OnInspectorGUI ()
                {
                        Layout.Update();
                        Layout.VerticalSpacing(10);

                        parent.Update();
                        {
                                if (FoldOut.Bar(parent, Tint.Orange).Label("References").FoldOut("referenceFoldOut"))
                                {
                                        FoldOut.Box(5, FoldOut.boxColor, offsetY: -2);
                                        {
                                                parent.Field("Icon", "icon");
                                                parent.Field("Background", "background");
                                                parent.Field("NextSignal", "nextSignal");
                                                parent.Field("Message", "message");
                                                parent.Field("Messenger Name", "messenger");
                                        }
                                        Layout.VerticalSpacing(3);

                                        if (
                                                FoldOut.Bar(parent, Tint.BoxTwo)
                                                .Label("Choices", FoldOut.titleColor, false)
                                                .BR("addOption", execute: parent.Bool("choicesFoldOut"))
                                                .FoldOut("choicesFoldOut")
                                        )
                                        {
                                                SerializedProperty choices = parent.Get("choices");
                                                bool oldValue = Labels.useWhite;
                                                Labels.useWhite = true;
                                                if (parent.ReadBool("addOption"))
                                                {
                                                        choices.arraySize++;
                                                }
                                                FoldOut.Box(choices.arraySize, Tint.BoxTwo, offsetY: -2);
                                                {
                                                        for (int i = 0; i < choices.arraySize; i++)
                                                        {
                                                                Fields.ArrayProperty(choices, choices.Element(i), i, "Button");
                                                        }
                                                }
                                                Layout.VerticalSpacing(3);
                                                Labels.useWhite = oldValue;
                                        }
                                }
                                if (FoldOut.Bar(parent).Label("Events").FoldOut("eventsFoldOut"))
                                {
                                        Fields.EventFoldOut(parent.Get("onBegin"), parent.Get("beginFoldOut"), "On Begin", color: FoldOut.boxColor);
                                        Fields.EventFoldOut(parent.Get("onEnd"), parent.Get("endFoldOut"), "On End", color: FoldOut.boxColor);
                                        Fields.EventFoldOut(parent.Get("transitionIn"), parent.Get("inFoldOut"), "On Transition In", color: FoldOut.boxColor);
                                        Fields.EventFoldOut(parent.Get("transitionOut"), parent.Get("outFoldOut"), "On Transition Out", color: FoldOut.boxColor);
                                        Fields.EventFoldOut(parent.Get("loadMessage"), parent.Get("loadFoldOut"), "On Load Message", color: FoldOut.boxColor);
                                }
                        }
                        parent.ApplyModifiedProperties();
                        Layout.VerticalSpacing(10);
                }
        }
}
