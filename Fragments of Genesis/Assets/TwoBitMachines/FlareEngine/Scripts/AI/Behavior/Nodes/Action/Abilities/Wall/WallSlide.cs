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
        public class WallSlide : Action
        {
                [SerializeField] public float slideSpeed = 5f;
                [SerializeField] public float slideTime = 3f;
                [SerializeField] public string signal;
                [SerializeField] public Vector2 hop = new Vector2(3f, 3f);
                [SerializeField] public bool hopOff;

                [System.NonSerialized] private float refVelX;
                [System.NonSerialized] public float counter;
                [System.NonSerialized] public float accelerate;
                [System.NonSerialized] public bool isHoping;

                public void Reset ()
                {
                        counter = 0;
                        accelerate = 0;
                        isHoping = false;
                }

                public override NodeState RunNodeLogic (Root root)
                {
                        if (nodeSetup == NodeSetup.NeedToInitialize)
                        {
                                Reset();
                        }
                        if ((!hopOff && !root.world.onWall) || root.world.onGround)
                        {
                                return NodeState.Success;
                        }
                        if (isHoping)
                        {
                                root.velocity.x = refVelX;
                        }
                        else
                        {
                                accelerate += Root.deltaTime * slideSpeed;
                                root.velocity.x = root.direction;
                                root.velocity.y = -accelerate;
                                root.signals.Set(signal);

                                if (hopOff)
                                {
                                        RaycastHit2D hit = Physics2D.Raycast(root.world.box.bottomCenter, Vector2.down, hop.y, WorldManager.collisionMask);
                                        if (hit && hit.distance > 0)
                                        {
                                                isHoping = true;
                                                Jump(root, hit.point + Vector2.right * hop.x * -root.direction, 0.5f);
                                        }
                                }
                                else if (TwoBitMachines.Clock.Timer(ref counter, slideTime))
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
                                Labels.InfoBoxTop(45, "Slide down a wall." +
                                        "\n \nReturns Running, Success");
                        }
                        FoldOut.Box(3, color, offsetY: -2);
                        {
                                parent.Field("Slide Time", "slideTime");
                                parent.FieldDouble("slide Speed", "slideSpeed", "signal");
                                Labels.FieldText("Signal");
                                parent.FieldAndEnable("Hop Off", "hop", "hopOff");
                        }
                        Layout.VerticalSpacing(3);
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion
        }
}
