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
        public class ParallelSuccessAll : Composite
        {
                public override NodeState RunNodeLogic (Root root)
                {
                        bool allChildrenSucceeded = true;
                        bool atLeastOneChildIsRunning = false;

                        if (useSignal)
                        {
                                root.signals.Set(defaultSignal);
                        }

                        for (int i = 0; i < children.Count; i++)
                        {
                                NodeState childState = children[i].RunChild(root);
                                if (childState == NodeState.Failure)
                                {
                                        allChildrenSucceeded = false;
                                }
                                if (childState == NodeState.Running)
                                {
                                        allChildrenSucceeded = false;
                                        atLeastOneChildIsRunning = true;
                                }
                        }
                        return allChildrenSucceeded ? NodeState.Success : atLeastOneChildIsRunning ? NodeState.Running : NodeState.Failure;
                }

                public override bool InterruptLogic (Root root, bool selfAbort = false)
                {
                        if (children.Count == 0)
                                return selfAbort ? true : false;

                        for (int i = 0; i < children.Count; i++)
                        {
                                Node child = children[i];
#if UNITY_EDITOR
                                child.interruptCheck = true;
                                interruptCheck = true;
                                child.interruptCounter = 0;
                                interruptCounter = 0;
#endif

                                if (child is Conditional)
                                {
                                        if (child.RunNodeLogic(root) != NodeState.Success)
                                                return false;
                                }
                                else if (child.isInterruptType)
                                {
                                        if (!(child as Composite).InterruptLogic(root, selfAbort))
                                                return false;
                                }
                                else if (selfAbort)
                                {
                                        return true; // failed, not a true fail
                                }
                        }
                        return true;
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                public override bool OnInspector (AIBase ai, SerializedObject parent, Color color, bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(45,
                                        "Runs all child nodes at the same time until they all return Success. Will return failure if not all of them succeeded."
                                );
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
