using TwoBitMachines.Editors;
using TwoBitMachines.FlareEngine.Interactables;
using UnityEditor;
using UnityEngine;

namespace TwoBitMachines.FlareEngine.Editors
{
        [CustomEditor(typeof(Teleport))]
        public class TeleportEditor : UnityEditor.Editor
        {
                private Teleport main;
                private SerializedObject parent;

                private void OnEnable ()
                {
                        main = target as Teleport;
                        parent = serializedObject;
                        Layout.Initialize();
                }

                public override void OnInspectorGUI ()
                {
                        Layout.Update();
                        Layout.VerticalSpacing(10);

                        parent.Update();
                        if (FoldOut.Bar(parent, Tint.Blue).Label("Teleport", Color.white).FoldOut())
                        {
                                int type = parent.Enum("type");
                                FoldOut.Box(4, FoldOut.boxColor, offsetY: -2);
                                {
                                        parent.Field("Destination", "destination");
                                        parent.Field("Execute", "type", execute: type == 0);
                                        parent.FieldDouble("Execute", "type", "input", execute: type == 1);
                                        parent.Field("Layer", "layerMask");
                                        parent.Field("Delay", "delay");
                                }
                                Layout.VerticalSpacing(3);

                                Fields.EventFoldOut(parent.Get("onDelayStart"), parent.Get("delayFoldOut"), "On Delay Start", color: FoldOut.boxColor);
                                Fields.EventFoldOutEffect(parent.Get("onTriggerEnter"), parent.Get("enterTriggerWE"), parent.Get("enterFoldOut"), "On Trigger Enter", color: FoldOut.boxColor);
                                Fields.EventFoldOutEffect(parent.Get("onTriggerExit"), parent.Get("exitTriggerWE"), parent.Get("exitFoldOut"), "On Trigger Exit", color: FoldOut.boxColor);
                                Fields.EventFoldOutEffect(parent.Get("onTeleport"), parent.Get("teleportWE"), parent.Get("eventFoldOut"), "On Teleport", color: FoldOut.boxColor);
                        }
                        parent.ApplyModifiedProperties();
                        Layout.VerticalSpacing(10);
                }
        }
}
