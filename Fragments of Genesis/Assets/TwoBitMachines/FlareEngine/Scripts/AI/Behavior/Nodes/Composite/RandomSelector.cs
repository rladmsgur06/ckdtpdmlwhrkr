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
        public class RandomSelector : Composite
        {
                public override NodeState RunNodeLogic (Root root)
                {
                        if (useSignal)
                        {
                                root.signals.Set(defaultSignal);
                        }

                        if (nodeSetup == NodeSetup.NeedToInitialize)
                        {
                                currentChildIndex = Random.Range(0, children.Count);
                        }
                        return children[currentChildIndex].RunChild(root);
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
                                else if (selfAbort)
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
                                Labels.InfoBoxTop(35, "This will run one random child node.");
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
