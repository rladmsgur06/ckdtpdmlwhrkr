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
        public class Inverter : Decorator
        {
                public override NodeState RunNodeLogic (Root root)
                {
                        InterruptCheck(root);

                        switch (children[0].RunChild(root))
                        {
                                case NodeState.Running:
                                        return NodeState.Running;
                                case NodeState.Success:
                                        return NodeState.Failure;
                                case NodeState.Failure:
                                        return NodeState.Success;
                        }
                        return NodeState.Failure;
                }

                public override bool InterruptLogic (Root root, bool selfAbort = false)
                {
                        // This is an inverter. All valid logic must be inverted.
                        if (children.Count == 0)
                                return selfAbort ? true : false; // this is not a true exit, no need to invert

                        Node child = children[0];
                        #region
#if UNITY_EDITOR
                        child.interruptCheck = true;
                        child.interruptCounter = 0;
                        interruptCheck = true;
                        interruptCounter = 0;
#endif
                        #endregion

                        if (child is Conditional)
                        {
                                if (child.RunNodeLogic(root) != NodeState.Success)
                                        return true; // invert this
                        }
                        else if (child.isInterruptType)
                        {
                                if (!(child as Composite).InterruptLogic(root, selfAbort))
                                        return true; // invert this
                        }
                        else if (selfAbort)
                        {
                                return true; // failed, not a true fail, don't invert this
                        }
                        return false; // dont invert this
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                public override bool OnInspector (AIBase ai, SerializedObject parent, Color color, bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(45,
                                        "This will invert the output logic of the child node (except for Running). Interrupt logic will work with this decorator."
                                );
                        }

                        FoldOut.Box(2, color, offsetY: -2);
                        parent.Field("Can Interrupt", "canInterrupt");
                        parent.Field("On Interrupt", "onInterrupt");
                        Layout.VerticalSpacing(3);
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion

        }
}
