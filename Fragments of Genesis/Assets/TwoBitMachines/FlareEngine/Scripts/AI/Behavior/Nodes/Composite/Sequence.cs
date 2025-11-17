#region 
#if UNITY_EDITOR
using TwoBitMachines.Editors;
using UnityEditor;
#endif
#endregion
using UnityEngine;

namespace TwoBitMachines.FlareEngine.AI
{
        [AddComponentMenu("")]
        public class Sequence : Composite
        {
                public override NodeState RunNodeLogic (Root root)
                {
                        InterruptCheck(root);
                        if (useSignal)
                        {
                                root.signals.Set(defaultSignal);
                        }
                        for (int i = currentChildIndex; i < children.Count; i++)
                        {
                                NodeState childState = children[i].RunChild(root);
                                if (childState == NodeState.Success)
                                {
                                        currentChildIndex++; // only execute the current node
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

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                public override bool OnInspector (AIBase ai, SerializedObject parent, Color color, bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(80, "This will execute one node at a time and move into the next node when the current node returns Success. When the current node fails, it will return Failure. If all nodes complete, it will return Success. Otherwise, it will return Running.");
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
