using System.Collections.Generic;
using TwoBitMachines.Editors;
using TwoBitMachines.FlareEngine.AI;
using TwoBitMachines.FlareEngine.AI.BlackboardData;
using UnityEditor;
using UnityEngine;

namespace TwoBitMachines.FlareEngine.Editors
{
        [CustomEditor(typeof(AITree))]
        public class AITreeEditor : UnityEditor.Editor
        {
                public AITree tree;
                private GameObject objReference;
                private SerializedObject parent;
                private bool onEnable;

                public SerializedProperty rootChildren;
                public string[] barNames = new string[] { "Inspect", "Target", "Territory", "Variable" };
                public static List<string> dataList = new List<string>();

                private void OnEnable ()
                {
                        onEnable = true;
                        tree = target as AITree;
                        objReference = tree.gameObject;
                        parent = serializedObject;

                        if (dataList == null || dataList.Count == 0)
                        {
                                Util.GetFolderStructure("TwoBitMachines", "/FlareEngine/Scripts/AI/Behavior/Blackboard", "Blackboard", dataList);
                                Util.GetFolderStructure("", UserFolderPaths.Path(UserFolder.Blackboard), UserFolderPaths.FolderName(UserFolder.Blackboard), dataList, false, true);

                        }
                        Layout.Initialize();
                        if (tree.root != null)
                        {
                                HideNodes(tree.root.children);
                        }
                        if (tree.window != null)
                        {
                                tree.window.Repaint();
                        }
                        else if (AIBase.windowStatic != null)
                        {
                                AIBase.windowStatic.Repaint();
                        }
                }

                public void HideNodes (List<Node> children)
                {
                        if (children == null)
                                return;
                        for (int i = 0; i < children.Count; i++)
                        {
                                if (children[i] == null)
                                        continue;
                                children[i].hideFlags = HideFlags.HideInInspector;
                                HideNodes(children[i].Children());
                        }
                }

                private void OnDisable ()
                {
                        if (tree == null && objReference != null && !EditorApplication.isPlayingOrWillChangePlaymode)
                        {
                                objReference.AddComponent<AIClean>();
                        }
                }

                public override void OnInspectorGUI ()
                {
                        Layout.VerticalSpacing(10);
                        Layout.Update();
                        bool createUnits = false;

                        if (tree.root != null)
                                tree.root.hideFlags = HideFlags.HideInInspector;
                        parent.Update();
                        {
                                AIBase.AIType(tree.transform, parent, ref createUnits);
                                SerializedProperty bar = parent.Get("barIndex");
                                FoldOut.TabBarString(Icon.Get("BackgroundLight"), FoldOut.boxColor, FoldOut.boxColor * Tint.LightGrey, barNames, bar, LabelType.White);
                                AIBase.BlackboardDisplay(parent, bar, tree, dataList, 0);
                                if (bar.intValue != 0 && FoldOut.CornerButton(Tint.Delete))
                                {
                                        BlackboardMenu.Open(tree, dataList, bar.intValue + 1);
                                }
                                InspectNode(bar);
                        }
                        parent.ApplyModifiedProperties();

                        if (tree != null && tree.reset == null)
                        {
                                BlackboardMenu.ai = tree as AIBase;
                                Blackboard newData = BlackboardMenu.CreateBlackboard(tree.data, "Variable", "BoolVariable");
                                if (newData != null)
                                {
                                        newData.dataName = "Reset";
                                        tree.reset = newData as BoolVariable;
                                }
                        }

                        if (createUnits)
                                AIBase.CreateUnits(tree);
                }

                private void InspectNode (SerializedProperty bar)
                {
                        if (bar.intValue != 0)
                                return;

                        if (tree.inspectNode != null)
                        {
                                SerializedObject element = new SerializedObject(tree.inspectNode);
                                element.Update();
                                {
                                        Color color = Tint.Orange;
                                        FoldOut.Bar(element, Tint.Orange * Tint.WarmWhiteB)
                                                .Label(Util.ToProperCase(tree.inspectNameType), Color.black, false)
                                                .BR("showInfo", "Info");

                                        if (!tree.inspectNode.OnInspector(tree, element, color, onEnable))
                                        {
                                                AIBase.IterateObject(element, tree.data, tree.inspectNode, true); // if not custom inspector, display it raw
                                        }
                                }
                                element.ApplyModifiedProperties();
                        }

                        if (tree.nodeMessage != null && tree.showNodeMessage)
                        {
                                SerializedProperty message = parent.Get("nodeMessage");
                                FoldOut.Box(5, FoldOut.boxColor);
                                {
                                        message.Field("Message Size", "size");
                                        Layout.VerticalSpacing(2);
                                        SerializedProperty messg = message.Get("message");
                                        Rect rect = Layout.CreateRect(width: Layout.infoWidth + 4, height: 70, offsetX: -6);
                                        messg.stringValue = EditorGUI.TextArea(rect, messg.stringValue);
                                }
                                Layout.VerticalSpacing(5);
                        }
                        onEnable = false;
                }

                private void OnSceneGUI ()
                {
                        if (tree == null || tree.root == null)
                                return;
                        Mouse.Update();
                        for (int i = 0; i < tree.data.Count; i++)
                        {
                                if (tree.data[i] == null)
                                {
                                        tree.data.RemoveAt(i);
                                        continue;
                                }
                                tree.data[i].OnSceneGUI(this);
                                PrefabUtility.RecordPrefabInstancePropertyModifications(tree.data[i]);
                        }
                        RunOnSceneGUI(tree.tempChildren);
                        RunOnSceneGUI(tree.root.children);

                        // if (Event.current.type == EventType.Layout) UnityEditor.HandleUtility.AddDefaultControl (GUIUtility.GetControlID (FocusType.Passive)); // keep object selected, default control
                }

                private void RunOnSceneGUI (List<Node> children)
                {
                        for (int i = children.Count - 1; i >= 0; i--)
                        {
                                if (children[i] == null)
                                {
                                        children.RemoveAt(i);
                                        continue;
                                }
                                children[i].OnSceneGUI(this);
                                PrefabUtility.RecordPrefabInstancePropertyModifications(children[i]);
                                if (children[i].CanHaveChildren() && children[i].Children() != null)
                                {
                                        RunOnSceneGUI(children[i].Children());
                                }
                        }
                }

                [DrawGizmo(GizmoType.NotInSelectionHierarchy)]
                public static void DrawWhenObjectIsNotSelected (AITree tree, GizmoType gizmoType)
                {
                        if (tree == null || tree.root == null)
                                return;
                        for (int i = 0; i < tree.data.Count; i++)
                        {
                                if (tree.data[i] != null)
                                {
                                        tree.data[i].DrawWhenNotSelected();
                                }
                        }
                }

        }
}
