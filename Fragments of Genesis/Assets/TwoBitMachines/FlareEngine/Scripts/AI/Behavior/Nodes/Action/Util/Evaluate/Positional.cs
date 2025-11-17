#region 
#if UNITY_EDITOR
using TwoBitMachines.Editors;
using UnityEditor;
#endif
#endregion
using TwoBitMachines.FlareEngine.AI.BlackboardData;
using UnityEngine;

namespace TwoBitMachines.FlareEngine.AI
{
        [AddComponentMenu("")]
        public class Positional : Conditional
        {
                //* Conditional nodes should not hold state, that is variables used for memory.
                [SerializeField] public AIPositionalType type;
                [SerializeField] public Blackboard target;

                public override NodeState RunNodeLogic (Root root)
                {
                        if (target == null)
                        {
                                return NodeState.Failure;
                        }
                        return CheckPosition(target.GetTarget(), root.position) ? NodeState.Success : NodeState.Failure;
                }

                public bool CheckPosition (Vector2 target, Vector2 aiPosition)
                {
                        if (this.target.hasNoTargets)
                        {
                                return false;
                        }
                        if (type == AIPositionalType.ToTheLeft)
                        {
                                return aiPosition.x < target.x;
                        }
                        if (type == AIPositionalType.ToTheRight)
                        {
                                return aiPosition.x > target.x;
                        }
                        if (type == AIPositionalType.ToTheTop)
                        {
                                return aiPosition.y > target.y;
                        }
                        if (type == AIPositionalType.ToTheBottom)
                        {
                                return aiPosition.y < target.y;
                        }
                        if (type == AIPositionalType.SameX)
                        {
                                return Mathf.Abs(aiPosition.x - target.x) < 0.00001f;
                        }
                        if (type == AIPositionalType.SameY)
                        {
                                return Mathf.Abs(aiPosition.y - target.y) < 0.00001f;
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
                                Labels.InfoBoxTop(35, "Check the AI's position in relation to a target.");
                        }

                        FoldOut.Box(2, color, offsetY: -2);
                        AIBase.SetRef(ai.data, parent.Get("target"), 0);
                        parent.Field("Type", "type");
                        Layout.VerticalSpacing(3);
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion
        }

        public enum AIPositionalType
        {
                ToTheLeft,
                ToTheRight,
                ToTheTop,
                ToTheBottom,
                SameX,
                SameY,
        }
}
