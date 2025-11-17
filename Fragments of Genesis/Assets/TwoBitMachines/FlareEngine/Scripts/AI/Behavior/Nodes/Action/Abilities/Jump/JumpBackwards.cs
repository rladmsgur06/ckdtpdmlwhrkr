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
        public class JumpBackwards : Action
        {
                [SerializeField] public float jumpForce = 7f;
                [SerializeField] public float velocityX = 10f;
                [SerializeField] public bool forceJump;

                [System.NonSerialized] public int direction;
                [System.NonSerialized] public float velocityXReference;

                public override NodeState RunNodeLogic (Root root)
                {
                        if (nodeSetup == NodeSetup.NeedToInitialize)
                        {
                                if (!root.world.onGround && !forceJump)
                                {
                                        return NodeState.Failure;
                                }
                                direction = root.direction * -1;
                                root.velocity.y = jumpForce;
                                velocityXReference = velocityX * direction;
                                root.hasJumped = true;
                        }
                        else
                        {
                                root.signals.ForceDirection(-direction);
                                if (root.world.onGround)
                                {
                                        return NodeState.Success;
                                }
                                root.velocity.x = velocityXReference;
                        }
                        return NodeState.Running;
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                public override bool OnInspector (AIBase ai, SerializedObject parent, Color color, bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(75, "Jumps backwards and retains its direction. If AI is not on ground, force jump will force it to jump." +
                                        "\n \nReturns Running, Success, Failure");
                        }

                        FoldOut.Box(3, color, offsetY: -2);
                        parent.Field("Jump Force", "jumpForce");
                        parent.Field("Velocity X", "velocityX");
                        parent.FieldToggle("Force Jump", "forceJump");
                        Layout.VerticalSpacing(3);
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion

        }
}
