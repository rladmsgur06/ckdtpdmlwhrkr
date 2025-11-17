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
        public class RunIf : Composite
        {
                public override NodeState RunNodeLogic (Root root)
                {
                        InterruptCheck(root);
                        if (useSignal)
                        {
                                root.signals.Set(defaultSignal);
                        }

                        NodeState state = NodeState.Failure;

                        if (children.Count > 0)
                        {
                                state = children[0].RunChild(root);
                        }

                        if (state == NodeState.Running)
                        {
                                for (int i = 1; i < children.Count; i++)
                                {
                                        children[i].RunChild(root);
                                }
                        }
                        return state;

                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                public override bool OnInspector (AIBase ai, SerializedObject parent, Color color, bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(35, "This will only run its children if the first node is running.");
                        }

                        FoldOut.Box(2, color, offsetY: -2);
                        //parent.Field ("Can Interrupt", "canInterrupt");
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
