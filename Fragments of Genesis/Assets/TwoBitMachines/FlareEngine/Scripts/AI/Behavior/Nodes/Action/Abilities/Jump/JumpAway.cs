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
        public class JumpAway : Action
        {
                [SerializeField] public Blackboard target;
                [SerializeField] public float jumpForce = 7f;
                [SerializeField] public float velocityX = 10f;
                [SerializeField] public bool forceJump;

                [System.NonSerialized] public float direction;
                [System.NonSerialized] public float velocityXReference;

                public override NodeState RunNodeLogic (Root root)
                {
                        if (target == null)
                                return NodeState.Failure;

                        if (nodeSetup == NodeSetup.NeedToInitialize)
                        {
                                if (!root.world.onGround && !forceJump)
                                {
                                        return NodeState.Failure;
                                }
                                direction = Mathf.Sign(root.position.x - target.GetTarget().x);
                                if (this.target.hasNoTargets)
                                {
                                        return NodeState.Failure;
                                }
                                root.velocity.y = jumpForce;
                                velocityXReference = velocityX * direction;
                                root.hasJumped = true;
                        }
                        else
                        {
                                if (root.world.onGround)
                                {
                                        return NodeState.Success;
                                }
                                root.velocity.x = velocityXReference;
                        }
                        root.signals.Set("jumpAwayLeft", -direction < 0);
                        root.signals.Set("jumpAwayRight", -direction > 0);
                        return NodeState.Running;
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                public override bool OnInspector (AIBase ai, SerializedObject parent, Color color, bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(75, "Jump away from the target. If AI is not on ground, force jump will force it to jump. Signals: jumpAwayLeft, jumpAwayRight" +
                                        "\n \nReturns Running, Success, Failure");
                        }

                        FoldOut.Box(4, color, offsetY: -2);
                        AIBase.SetRef(ai.data, parent.Get("target"), 0);
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
