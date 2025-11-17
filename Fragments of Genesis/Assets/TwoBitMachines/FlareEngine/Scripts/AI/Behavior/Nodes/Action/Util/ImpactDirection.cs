#if UNITY_EDITOR
using TwoBitMachines.Editors;
using UnityEditor;
#endif
using UnityEngine;

namespace TwoBitMachines.FlareEngine.AI
{
        [AddComponentMenu("")]
        public class ImpactDirection : Action
        {
                [SerializeField] public ImpactDirectionType type;
                [SerializeField] public float directionX;

                public override void OnReset (bool skip = false, bool enteredState = false)
                {
                        if (enteredState && ImpactPacket.impact != null)
                        {
                                directionX = ImpactPacket.impact.direction.x;
                        }
                }

                public override NodeState RunNodeLogic (Root root)
                {
                        bool success = false;
                        if (type == ImpactDirectionType.Left)
                        {
                                success = directionX < 0;
                        }
                        else if (type == ImpactDirectionType.Right)
                        {
                                success = directionX > 0;
                        }
                        else if (type == ImpactDirectionType.Up)
                        {
                                success = directionX > 0;
                        }
                        else if (type == ImpactDirectionType.Down)
                        {
                                success = directionX < 0;
                        }
                        return success ? NodeState.Success : NodeState.Failure;
                }

                public enum ImpactDirectionType
                {
                        Left,
                        Right,
                        Up,
                        Down
                }

                #region ▀▄▀▄▀▄ Custom Inspector▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                public override bool OnInspector (AIBase ai, SerializedObject parent, Color color, bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(45, "Returns Success if the ImpactPacket direction is true." +
                                        "\n \nReturns Success, Failure");
                        }
                        FoldOut.Box(1, color, offsetY: -2);
                        {
                                parent.Field("Direction", "type");
                        }
                        Layout.VerticalSpacing(3);
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion
        }

}
