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
        public class RunAndJumpToWall : Action
        {
                [SerializeField] public float speed = 12f;
                [SerializeField] public float holdTime = 2f;
                [SerializeField] public string signal;
                [SerializeField] public Vector2 jump = new Vector2(8f, 5f);

                [System.NonSerialized] private State state;
                [System.NonSerialized] private float refVelX;
                [System.NonSerialized] private float direction;
                [System.NonSerialized] private float counter;

                private enum State { Run, Jumping, Hold }

                public void Reset ()
                {
                        state = 0;
                        counter = 0;
                }

                public override NodeState RunNodeLogic (Root root)
                {
                        if (nodeSetup == NodeSetup.NeedToInitialize)
                        {
                                Reset();
                                direction = root.direction;
                        }
                        if (state == State.Run)
                        {
                                root.velocity.x = speed * direction;
                                RaycastHit2D hit = Physics2D.Raycast(root.world.box.center, Vector2.right * direction, jump.x + 0.5f, WorldManager.collisionMask);
                                if (hit && hit.distance > 0)
                                {
                                        state = State.Jumping;
                                        Jump(root, hit.point, Mathf.Clamp(jump.y - root.world.box.sizeY * 0.5f, 1f, 1000f));
                                }
                        }
                        if (state == State.Jumping)
                        {
                                root.velocity.x = refVelX;
                                if (root.world.onWall)
                                {
                                        state = State.Hold;
                                        root.velocity.y = 0;
                                }
                        }
                        if (state == State.Hold)
                        {
                                root.velocity.y = 0;
                                if (TwoBitMachines.Clock.Timer(ref counter, holdTime))
                                {
                                        return NodeState.Success;
                                }
                        }
                        return NodeState.Running;
                }

                public void Jump (Root root, Vector2 jumpTarget, float jumpHeight)
                {
                        Vector2 velocity = Compute.ArchObject(root.position, jumpTarget, jumpHeight, root.gravity.gravity); //  this method will find the exact velocity to jump the necessary height.
                        velocity.y += root.gravity.gravity * Root.deltaTime * 0.5f; //                                          adjust jump
                        root.velocity.y = velocity.y;
                        refVelX = velocity.x * 2f;
                        root.hasJumped = true;
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                public override bool OnInspector (AIBase ai, SerializedObject parent, Color color, bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(55, "Run and jump towards a wall. Then wall hold for a specified time." +
                                        "\n \nReturns Running, Success");
                        }
                        FoldOut.Box(4, color, offsetY: -2);
                        {
                                parent.Field("Speed", "speed");
                                parent.Field("Jump", "jump");
                                parent.Field("Hold Time", "holdTime");
                                parent.Field("Signals", "signal");
                        }
                        Layout.VerticalSpacing(3);
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion
        }
}
