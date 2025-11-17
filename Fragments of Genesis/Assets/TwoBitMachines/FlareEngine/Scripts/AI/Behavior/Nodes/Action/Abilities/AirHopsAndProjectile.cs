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
        public class AirHopsAndProjectile : Action
        {
                [SerializeField] public int hops = 4;
                [SerializeField] public float initialJump = 10f;
                [SerializeField] public string jumpSignal;
                [SerializeField] public string hopSignal;
                [SerializeField] public string hopWE;

                [SerializeField] public Vector2 hop = new Vector2(7f, 5f);
                [SerializeField] public ProjectileBase projectile;
                [SerializeField] public Transform firePoint;
                [SerializeField] public UnityEventEffect onHop;

                [System.NonSerialized] private bool inAir;
                [System.NonSerialized] private bool lastHop;
                [System.NonSerialized] private int hopIndex;
                [System.NonSerialized] private float refVelX;
                [System.NonSerialized] private float fullHeight;
                [System.NonSerialized] private Vector2 jumpTarget;

                public void Reset ()
                {
                        jumpTarget = Vector2.zero;
                        hopIndex = 0;
                        inAir = false;
                        lastHop = false;
                }

                public override NodeState RunNodeLogic (Root root)
                {
                        if (nodeSetup == NodeSetup.NeedToInitialize)
                        {
                                Reset();
                        }
                        if (lastHop)
                        {
                                if (root.world.onGround)
                                {
                                        return NodeState.Success;
                                }
                                root.velocity.x = refVelX;
                                root.signals.Set(hopSignal);

                        }
                        else if (!inAir)
                        {
                                fullHeight = root.position.y + initialJump;
                                jumpTarget = root.position + Vector2.right * root.direction * hop.x + Vector2.up * initialJump;
                                Jump(root, jumpTarget, hop.y);
                                root.signals.Set(jumpSignal);
                                inAir = true;
                        }
                        else
                        {
                                root.velocity.x = refVelX;
                                if (hopIndex == 0)
                                {
                                        root.signals.Set(jumpSignal);
                                }
                                else
                                {
                                        root.signals.Set(hopSignal);
                                }

                                if (root.velocity.y < 0 && root.position.y <= jumpTarget.y)
                                {
                                        if (root.world.onWall)
                                        {
                                                lastHop = true;
                                                return NodeState.Running;
                                        }

                                        hopIndex++;
                                        onHop.Invoke(ImpactPacket.impact.Set(hopWE, root.position, root.world.box.down));
                                        if (projectile != null)
                                                projectile.FireProjectile(firePoint, Vector2.down);
                                        jumpTarget = new Vector2(jumpTarget.x, fullHeight) + Vector2.right * root.direction * hop.x;
                                        Jump(root, jumpTarget, hop.y);
                                        if (hopIndex >= hops - 1)
                                        {
                                                lastHop = true;
                                        }
                                }
                        }
                        return NodeState.Running;
                }

                public void Jump (Root root, Vector2 jumpTarget, float jumpHeight)
                {
                        Vector2 velocity = Compute.ArchObject(root.position, jumpTarget, jumpHeight, root.gravity.gravity); //  this method will find the exact velocity to jump the necessary height.
                        velocity.y += root.gravity.gravity * Root.deltaTime * 0.5f; //                                          adjust jump
                        root.velocity.y = velocity.y;
                        refVelX = velocity.x;
                        root.hasJumped = true;
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField, HideInInspector] public bool eventFoldout;
                public override bool OnInspector (AIBase ai, SerializedObject parent, Color color, bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(45, "Jump then air hop x amount of times, shooting a projectile on each hop." +
                                        "\n \nReturns Running, Success");
                        }

                        FoldOut.Box(5, color, offsetY: -2);
                        {
                                parent.Field("Hops", "hops");
                                parent.Field("Hop Distance", "hop");
                                parent.Field("Initial Jump", "initialJump");
                                parent.FieldDouble("Signals", "jumpSignal", "hopSignal");
                                Labels.FieldDoubleText("Jump", "Hop");
                                parent.FieldDouble("Fire Point", "firePoint", "projectile");
                        }
                        Layout.VerticalSpacing(3);
                        Fields.EventFoldOutEffect(parent.Get("onHop"), parent.Get("hopWE"), parent.Get("eventFoldout"), "On Hop", color: color);
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion
        }
}
