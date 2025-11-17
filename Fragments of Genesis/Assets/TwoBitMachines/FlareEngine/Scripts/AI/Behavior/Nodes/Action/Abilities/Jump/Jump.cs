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
        public class Jump : Action
        {
                [SerializeField] public bool fullJump;
                [SerializeField] public bool isHalfJump;
                [SerializeField] public bool mustBeOnGround = true;
                [SerializeField] public float height = 5f;
                [SerializeField] public Vector2 jump = new Vector2(5f, 5f);

                [System.NonSerialized] private bool isJumping;
                [System.NonSerialized] private float refVelX;

                public override NodeState RunNodeLogic (Root root)
                {
                        if (nodeSetup == NodeSetup.NeedToInitialize)
                        {
                                isJumping = false;
                        }

                        if (!isJumping)
                        {
                                if (mustBeOnGround && !root.world.onGround)
                                {
                                        return NodeState.Running;
                                }
                                else
                                {
                                        isJumping = true;

                                        if (fullJump)
                                        {
                                                float direction = root.world.rightWall ? -1 : root.world.leftWall ? 1f : root.direction;
                                                JumpNow(root, root.position + Vector2.right * direction * jump.x, jump.y);
                                        }
                                        else
                                        {
                                                JumpNow(root, root.position, height);
                                        }
                                }
                        }
                        else
                        {
                                if (fullJump)
                                {
                                        root.velocity.x = refVelX;
                                }
                                if ((isHalfJump && root.velocity.y < 0) || root.world.onGround)
                                {
                                        return NodeState.Success;
                                }
                        }
                        return NodeState.Running;
                }

                public void JumpNow (Root root, Vector2 jumpTarget, float height)
                {
                        Vector2 velocity = Compute.ArchObject(root.position, jumpTarget, height, root.gravity.gravity);
                        velocity.y += root.gravity.gravity * Root.deltaTime * 0.5f;
                        root.velocity.y = velocity.y;
                        root.velocity.x = refVelX = velocity.x;
                        root.hasJumped = true;
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                public override bool OnInspector (AIBase ai, SerializedObject parent, Color color, bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(80, "Jump. If HalfJumpOnly is enabled, this returns success once the AI begins to fall. If MustBeOnGround is enabled, the AI must be on the ground to jump." +
                                        "\n \nReturns Running, Success");
                        }

                        bool fullJump = parent.Bool("fullJump");
                        FoldOut.Box(3, color, offsetY: -2);
                        {
                                if (!fullJump)
                                        parent.FieldAndEnableRaw("Jump Height", "height", "fullJump");
                                if (fullJump)
                                        parent.FieldAndEnableRaw("Jump", "jump", "fullJump");
                                parent.FieldToggleAndEnable("Must Be On Ground", "mustBeOnGround");
                                parent.FieldToggleAndEnable("Half Jump Only", "isHalfJump");
                        }
                        Layout.VerticalSpacing(3);
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion

        }
}
