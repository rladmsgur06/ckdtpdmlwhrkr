#region 
#if UNITY_EDITOR
using TwoBitMachines.Editors;
using UnityEditor;
#endif
#endregion
using System.Collections.Generic;
using UnityEngine;

namespace TwoBitMachines.FlareEngine.AI
{
        [AddComponentMenu("")]
        public class RandomSequence : Composite
        {
                System.Random range = new System.Random();

                public override NodeState RunNodeLogic (Root root)
                {
                        if (useSignal)
                        {
                                root.signals.Set(defaultSignal);
                        }

                        if (nodeSetup == NodeSetup.NeedToInitialize)
                        {
                                Shuffle(children);
                        }

                        for (int i = currentChildIndex; i < children.Count; i++)
                        {
                                NodeState childState = children[i].RunChild(root);
                                if (childState == NodeState.Success)
                                {
                                        currentChildIndex++; // only execute the current node unless composite can interrupt itself
                                        continue;
                                }
                                if (childState == NodeState.Failure)
                                {
                                        return NodeState.Failure;
                                }
                                if (childState == NodeState.Running)
                                {
                                        return NodeState.Running;
                                }
                        }

                        return NodeState.Success;
                }

                public void Shuffle (IList<Node> list)
                {
                        int n = list.Count;
                        while (n > 1)
                        {
                                n--;
                                int k = range.Next(n + 1);
                                Node value = list[k];
                                list[k] = list[n];
                                list[n] = value;
                        }
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                public override bool OnInspector (AIBase ai, SerializedObject parent, Color color, bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(45, "This will shuffle the list of nodes so that the execution order is always randomized.");
                        }

                        FoldOut.Box(3, color, offsetY: -2);
                        parent.Field("Can Interrupt", "canInterrupt");
                        parent.Field("On Interrupt", "onInterrupt");
                        parent.FieldAndEnable("Default Signal", "defaultSignal", "useSignal");
                        Layout.VerticalSpacing(3);

                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion
        }
}
