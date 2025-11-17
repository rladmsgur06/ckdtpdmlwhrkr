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
        public class Selector : Composite
        {
                public override NodeState RunNodeLogic (Root root)
                {
                        InterruptCheck(root); // will check higher priority children

                        if (useSignal)
                        {
                                root.signals.Set(defaultSignal);
                        }

                        for (int i = currentChildIndex; i < children.Count; i++)
                        {
                                NodeState childState = children[i].RunChild(root);
                                if (childState == NodeState.Failure)
                                {
                                        currentChildIndex++;
                                        continue;
                                }
                                if (childState == NodeState.Success)
                                {
                                        return NodeState.Success;
                                }
                                if (childState == NodeState.Running)
                                {
                                        return NodeState.Running;
                                }
                        }

                        return NodeState.Failure;
                }

                public override bool InterruptLogic (Root root, bool selfAbort = false)
                {
                        if (children.Count == 0)
                                return selfAbort ? true : false;

                        for (int i = 0; i < children.Count; i++)
                        {
                                Node child = children[i];
                                #region ▀▄▀▄▀▄ Editor Variables ▄▀▄▀▄▀
#if UNITY_EDITOR
                                child.interruptCheck = true;
                                child.interruptCounter = 0;
                                interruptCheck = true;
                                interruptCounter = 0;
#endif
                                #endregion

                                if (child is Conditional)
                                {
                                        if (child.RunNodeLogic(root) == NodeState.Success)
                                                return true;
                                }
                                else if (child.isInterruptType)
                                {
                                        if ((child as Composite).InterruptLogic(root, selfAbort))
                                                return true;
                                }
                                else if (selfAbort) // if  not a conditional or an interrupt, then we encountered an action/ thus we exit if in self abort
                                {
                                        return true; // failed, not a true fail
                                }

                        }
                        return false;
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                public override bool OnInspector (AIBase ai, SerializedObject parent, Color color, bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(35, "This will run every node in the list until one returns Success.");
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
