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
        public class Parallel : Composite
        {
                private bool atLeastOneChildSucceeded = false;

                public override NodeState RunNodeLogic (Root root)
                {
                        bool isRunning = false;
                        if (nodeSetup == NodeSetup.NeedToInitialize)
                        {
                                atLeastOneChildSucceeded = false;
                                for (int i = 0; i < children.Count; i++)
                                {
                                        children[i].completed = false;
                                }
                        }

                        if (useSignal)
                        {
                                root.signals.Set(defaultSignal);
                        }

                        for (int i = 0; i < children.Count; i++)
                        {
                                if (children[i].completed)
                                        continue;
                                isRunning = true;
                                NodeState childState = children[i].RunChild(root);
                                if (childState == NodeState.Success)
                                {
                                        atLeastOneChildSucceeded = true;
                                        children[i].completed = true;
                                }
                                if (childState == NodeState.Failure)
                                {
                                        children[i].completed = true;
                                }
                        }
                        return isRunning ? NodeState.Running : atLeastOneChildSucceeded ? NodeState.Success : NodeState.Failure;
                }

                // Basically acts as a selector (OR) gate. If used for interrupts should really only have conditional and composite nodes. 
                public override bool InterruptLogic (Root root, bool selfAbort = false)
                {
                        if (children.Count == 0)
                                return selfAbort ? true : false;

                        for (int i = 0; i < children.Count; i++)
                        {
                                Node child = children[i];

#if UNITY_EDITOR
                                child.interruptCheck = true;
                                child.interruptCounter = 0;
                                interruptCheck = true;
                                interruptCounter = 0;
#endif

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
                                Labels.InfoBoxTop(65,
                                        "Runs all child nodes at the same time. This will return Running as long as one of them is running.Once all child nodes complete, it will return Success if one child succeeded, else failure."
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
