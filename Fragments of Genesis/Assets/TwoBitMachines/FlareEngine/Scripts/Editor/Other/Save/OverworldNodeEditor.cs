using System.Collections.Generic;
using TwoBitMachines.Editors;
using UnityEditor;
using UnityEngine;

namespace TwoBitMachines.FlareEngine.Editors
{
        [CustomEditor(typeof(OverworldNode))]
        [CanEditMultipleObjects]
        public class OverworldNodeEditor : UnityEditor.Editor
        {
                private OverworldNode main;
                private SerializedObject so;
                private List<string> sceneNames = new List<string>();

                private void OnEnable ()
                {
                        main = target as OverworldNode;
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
                        UpdateNode(so, main, sceneNames);
                }

                public static void UpdateNode (SerializedObject so, OverworldNode main, List<string> sceneNames, bool space = true)
                {
                        main.transform.localScale = Vector3.one;
                        Layout.Update();
                        if (space)
                        {
                                Layout.VerticalSpacing(10);
                        }
                        so.Update();

                        SerializedProperty typeProperty = so.Get("type");
                        OverworldNodeType type = (OverworldNodeType) typeProperty.intValue;
                        int typeRef = (int) type;

                        if (type == OverworldNodeType.Basic || type == OverworldNodeType.HasItem || type == OverworldNodeType.Start)
                        {
                                Block.Box(2, FoldOut.boxColor, extraHeight: 3, noGap: !space);
                                {
                                        so.Field_("Node Type", "type");
                                        NodeName(so);
                                }
                                if (Block.ExtraFoldout(so, "enterFoldOut"))
                                {
                                        Fields.EventFoldOut(so.Get("onEnterNode"), so.Get("onEnterFoldOut"), "On Enter Node", space: false);
                                        Fields.EventFoldOut(so.Get("onExitNode"), so.Get("onExitFoldOut"), "On Exit Node");
                                        Layout.VerticalSpacing();
                                }
                        }
                        else if (type == OverworldNodeType.Level)
                        {
                                Block.Box(4, FoldOut.boxColor, extraHeight: 3, noGap: !space);
                                {
                                        so.Field_("Node Type", "type");
                                        NodeName(so);
                                        so.DropDownList_(sceneNames, "Scene Name", "sceneName");
                                        int nextType = so.Enum("setNextNodeType");
                                        so.Field_("Set Next Node", "setNextNodeType", execute: nextType == 0);
                                        so.FieldDouble_("Set Next Node", "setNextNodeType", "nextNode", execute: nextType > 0);
                                }
                                if (Block.ExtraFoldout(so, "enterFoldOut"))
                                {
                                        Fields.EventFoldOut(so.Get("onEnterNode"), so.Get("onEnterFoldOut"), "On Enter Node", space: false);
                                        Fields.EventFoldOut(so.Get("onExitNode"), so.Get("onExitFoldOut"), "On Exit Node");
                                        Layout.VerticalSpacing();
                                }
                                Block.Box(2, FoldOut.boxColor, extraHeight: 3);
                                {
                                        so.Field_("Image Locked", "imageLocked");
                                        so.Field_("Image Unlocked", "imageUnlocked");
                                }
                        }
                        else if (type == OverworldNodeType.Block)
                        {
                                Block.Box(3, FoldOut.boxColor, extraHeight: 3, noGap: !space);
                                {
                                        so.Field_("Node Type", "type");
                                        NodeName(so);
                                        so.Field_("Unlock Key", "unlockKey");
                                }
                                if (Block.ExtraFoldout(so, "enterFoldOut"))
                                {
                                        Fields.EventFoldOut(so.Get("onEnterNode"), so.Get("onEnterFoldOut"), "On Enter Node", space: false);
                                        Fields.EventFoldOut(so.Get("onExitNode"), so.Get("onExitFoldOut"), "On Exit Node");
                                        Layout.VerticalSpacing();
                                }
                                Block.Box(3, FoldOut.boxColor, extraHeight: 3);
                                {
                                        so.Field_("Image Locked", "imageLocked");
                                        so.Field_("Image Unlocked", "imageUnlocked");
                                        so.FieldDouble_("Unlock Signal", "signal", "signalTime");
                                        Block.HelperText("Time");
                                }
                                if (Block.ExtraFoldout(so, "foldOut"))
                                {
                                        Fields.EventFoldOut(so.Get("onUnlock"), so.Get("unlockFoldOut"), "On Unlock", space: false);
                                        Fields.EventFoldOut(so.Get("isBlocked"), so.Get("isBlockedFoldOut"), "Is Blocked");
                                        Fields.EventFoldOut(so.Get("isUnblocked"), so.Get("isUnblockedFoldOut"), "Is Unblocked");
                                        Fields.EventFoldOut(so.Get("signalComplete"), so.Get("signalFoldOut"), "Unlock Signal Complete");
                                        Layout.VerticalSpacing();
                                }
                        }
                        else if (type == OverworldNodeType.Teleport)
                        {
                                Block.Box(3, FoldOut.boxColor, extraHeight: 3, noGap: !space);
                                {
                                        so.Field_("Node Type", "type");
                                        NodeName(so);
                                        so.Field_("Teleport To", "teleportToNode");
                                }
                                if (Block.ExtraFoldout(so, "foldOut"))
                                {
                                        Fields.EventFoldOut(so.Get("onTeleport"), so.Get("teleportFoldOut"), "On Teleport", space: false);
                                        Fields.EventFoldOut(so.Get("onEnterNode"), so.Get("onEnterFoldOut"), "On Enter Node");
                                        Fields.EventFoldOut(so.Get("onExitNode"), so.Get("onExitFoldOut"), "On Exit Node");
                                        Layout.VerticalSpacing();
                                }
                        }


                        so.ApplyModifiedProperties();
                        Layout.VerticalSpacing(10);

                        if (typeRef != typeProperty.intValue)
                        {
                                type = (OverworldNodeType) typeProperty.intValue;
                                if (type != OverworldNodeType.Basic)
                                {
                                        main.name = type.ToString();
                                }
                                else
                                {
                                        main.name = "Basic";
                                }
                        }
                }

                public static void NodeName (SerializedObject so)
                {
                        SerializedProperty name = so.Get("nodeName");
                        name.stringValue = so.targetObject.name;
                        so.Field_("Node Name", "nodeName");
                        if (name.stringValue != so.targetObject.name)
                        {
                                so.targetObject.name = name.stringValue;
                                EditorUtility.SetDirty(so.targetObject);
                        }
                }
        }
}
