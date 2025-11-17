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
        public class IsClamped : Conditional
        {
                [SerializeField] public Clamp clamp;
                [SerializeField] public ClampDirection type;

                public override NodeState RunNodeLogic (Root root)
                {
                        if (clamp == null)
                                return NodeState.Failure;
                        if (type == ClampDirection.Left)
                                return clamp.IsClampingLeft(root) ? NodeState.Success : NodeState.Failure;
                        if (type == ClampDirection.Right)
                                return clamp.IsClampingRight(root) ? NodeState.Success : NodeState.Failure;
                        if (type == ClampDirection.Up)
                                return clamp.IsClampingUp(root) ? NodeState.Success : NodeState.Failure;
                        if (type == ClampDirection.Down)
                                return clamp.IsClampingDown(root) ? NodeState.Success : NodeState.Failure;
                        return clamp.IsClamping(root) ? NodeState.Success : NodeState.Failure;
                }

                public enum ClampDirection
                {
                        Left,
                        Right,
                        Up,
                        Down,
                        Any
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR


                public override bool OnInspector (AIBase ai, SerializedObject parent, Color color, bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(45, "Check if the specified clamp is clamping the AI." +
                                        "\n \nReturns Success, Failure");
                        }

                        FoldOut.Box(2, color, offsetY: -2);
                        {
                                parent.Field("Clamp", "clamp");
                                parent.Field("Direction", "type");
                        }
                        Layout.VerticalSpacing(3);
                        return true;
                }
#endif
                #endregion

        }

}
