using System.Collections.Generic;
#region 
#if UNITY_EDITOR
using TwoBitMachines.Editors;
using UnityEditor;
#endif
#endregion
using TwoBitMachines.FlareEngine.AI.BlackboardData;
using UnityEngine;
using UnityEngine.Events;

namespace TwoBitMachines.FlareEngine.AI
{
        [AddComponentMenu("")]
        public class MultiJumpAndSlam : Action
        {
                [SerializeField] public JumpType jumpType;
                [SerializeField] public Blackboard target;
                [SerializeField] public List<MultiJump> jump = new List<MultiJump>();

                [SerializeField] public bool canSlam;
                [SerializeField] public float slamRadius = 3f;
                [SerializeField] public float slamSpeed = 50f;
                [SerializeField] public float slamHoldTime = 1f;
                [SerializeField] public float slamDamage = 1f;
                [SerializeField] public float slamForce = 1f;
                [SerializeField] public string slamSignalAir;
                [SerializeField] public string slamSignalHold;
                [SerializeField] public LayerMask slamLayer;
                [SerializeField] public SlamDirectionType slamDirectionType;
                [SerializeField] public UnityEvent onSlam;

                [SerializeField] public string peakSignal;
                [SerializeField] public float peakTime = 0.25f;

                [System.NonSerialized] private Vector2 slamDirection;
                [System.NonSerialized] private float jumpDirection;
                [System.NonSerialized] private float groundCounter;
                [System.NonSerialized] private float airCounter;
                [System.NonSerialized] private float velX;
                [System.NonSerialized] private int jumpIndex;
                [System.NonSerialized] private bool hasJumped;
                [System.NonSerialized] private bool finalJump;
                [System.NonSerialized] private bool groundHold;
                [System.NonSerialized] private bool airHold;
                public enum JumpType { Jump, JumpToTarget }
                public enum SlamDirectionType { Down, TowardsTarget }
                public bool slamTowardsTarget => slamDirectionType == SlamDirectionType.TowardsTarget;

                private void Reset ()
                {
                        jumpIndex = -1;
                        groundCounter = 0;
                        airCounter = 0;
                        groundHold = false;
                        finalJump = false;
                        hasJumped = false;
                        airHold = false;
                }

                public override NodeState RunNodeLogic (Root root)
                {
                        if (nodeSetup == NodeSetup.NeedToInitialize)
                        {
                                if (jump.Count == 0)
                                        return NodeState.Failure;
                                jumpDirection = root.direction;
                                Reset();
                        }
                        if (canSlam && finalJump)
                        {
                                if (airHold)
                                {
                                        root.velocity.y = 0;
                                        root.signals.Set(peakSignal);
                                        if (TwoBitMachines.Clock.Timer(ref airCounter, peakTime))
                                        {
                                                airHold = false;
                                        }
                                }
                                if (groundHold)
                                {
                                        root.signals.Set(slamSignalHold);
                                        if (TwoBitMachines.Clock.Timer(ref groundCounter, slamHoldTime))
                                        {
                                                return NodeState.Success;
                                        }
                                }
                                if (!groundHold && !airHold)
                                {
                                        root.signals.Set(slamSignalAir);
                                        if (slamTowardsTarget)
                                        {
                                                root.velocity = slamDirection * Mathf.Abs(slamSpeed);
                                        }
                                        {
                                                root.velocity.y = -Mathf.Abs(slamSpeed);
                                        }
                                        if (root.world.onGround)
                                        {
                                                onSlam.Invoke();
                                                SlamDamage(root);
                                                root.velocity.x = 0;
                                                groundCounter = 0;
                                                groundHold = slamHoldTime > 0;
                                                root.signals.Set(slamSignalAir, false);
                                                root.signals.Set(slamSignalHold, slamHoldTime > 0);
                                                if (slamHoldTime <= 0)
                                                {
                                                        return NodeState.Success;
                                                }
                                        }
                                }
                        }
                        else
                        {
                                if (hasJumped)
                                {
                                        Vector2 targetPosition = target.GetTarget();
                                        if (this.target.hasNoTargets)
                                        {
                                                return NodeState.Failure;
                                        }
                                        if (jumpIndex < jump.Count)
                                        {
                                                root.signals.Set(jump[jumpIndex].signal);
                                                root.velocity.x = jumpType == JumpType.Jump ? jump[jumpIndex].velX * jumpDirection : velX;
                                        }
                                        if (canSlam && root.velocity.y < 0 && jumpIndex == jump.Count - 1)
                                        {
                                                finalJump = true;
                                                airHold = peakTime > 0;
                                                if (slamTowardsTarget && target != null)
                                                {
                                                        slamDirection = SlamDirection((targetPosition - root.position).normalized);
                                                }
                                        }
                                        else if (root.world.onGround)
                                        {
                                                hasJumped = false;
                                        }
                                }
                                if (!hasJumped)
                                {
                                        Vector2 targetPosition = target.GetTarget();
                                        if (this.target.hasNoTargets)
                                        {
                                                return NodeState.Failure;
                                        }
                                        jumpIndex++;
                                        hasJumped = true;
                                        if (jumpIndex < jump.Count)
                                        {
                                                if (jumpType == JumpType.JumpToTarget && target != null)
                                                {
                                                        jump[jumpIndex].JumpTo(root, targetPosition, ref velX);
                                                }
                                                else
                                                {
                                                        jump[jumpIndex].Jump(root);
                                                }
                                        }
                                        if (jumpIndex >= jump.Count)
                                        {
                                                return NodeState.Success;
                                        }
                                }
                        }
                        return NodeState.Running;
                }

                public Vector2 SlamDirection (Vector2 direction)
                {
                        float angleInDegrees = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                        float clampedAngle = angleInDegrees < 0 ? angleInDegrees + 360f : angleInDegrees;
                        clampedAngle = Mathf.Clamp(clampedAngle, 225f, 315f);
                        return new Vector2(Mathf.Cos(clampedAngle * Mathf.Deg2Rad), Mathf.Sin(clampedAngle * Mathf.Deg2Rad));
                }

                public void SlamDamage (Root root)
                {
                        int hit = Compute.OverlapCircle(root.position, slamRadius, slamLayer);
                        Health.HitContactResults(this.transform, Compute.contactResults, hit, -slamDamage, slamForce, root.position); // blast radius should be in all directions, derive direction from position

                }
                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField, HideInInspector] public bool eventFoldOut;
                [SerializeField, HideInInspector] public bool slamFoldOut;
                public override bool OnInspector (AIBase ai, SerializedObject parent, Color color, bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(45, "Jump towards a target x time, then slam to ground, dealing damage." +
                                        "\n \nReturns Running, Success, Failure");
                        }

                        int type = parent.Enum("jumpType");
                        FoldOut.Box(2, color, offsetY: -2);
                        {
                                parent.Field("Jump Type", "jumpType");
                                AIBase.SetRef(ai.data, parent.Get("target"), 0);
                        }
                        Layout.VerticalSpacing(3);

                        SerializedProperty array = parent.Get("jump");
                        if (array.arraySize == 0)
                        {
                                array.arraySize++;
                        }

                        for (int i = 0; i < array.arraySize; i++)
                        {
                                SerializedProperty element = array.Element(i);
                                FoldOut.Box(2, color);
                                {
                                        element.FieldAndDoubleButton("Jump", "height", "xsAdd", "xsMinus", out bool Add, out bool Minus);
                                        Labels.FieldText("Height", rightSpacing: Layout.contentWidth * 0.28f);
                                        if (type == 0)
                                        {
                                                element.FieldDouble("", "signal", "velX");
                                                Labels.FieldDoubleText("Signal", "VelX");
                                        }
                                        else
                                        {
                                                element.FieldDoubleAndEnum("", "signal", "maxDistance", "jumpAboveType");
                                                Labels.FieldDoubleText("Signal", "Max Dist", rightSpacing: 18);
                                        }
                                        if (Add)
                                        {
                                                array.InsertArrayElement(i);
                                                break;
                                        }
                                        if (Minus)
                                        {
                                                array.DeleteArrayElementAtIndex(i);
                                                break;
                                        }
                                }
                                Layout.VerticalSpacing(5);
                        }
                        FoldOut.Box(6, color, extraHeight: 3);
                        {
                                parent.FieldDoubleAndEnable("Slam", "slamRadius", "slamLayer", "canSlam");
                                GUI.enabled = parent.Bool("canSlam");
                                Labels.FieldText("Radius", rightSpacing: 18 + Layout.contentWidth * 0.45f);
                                parent.FieldDouble("Damage", "slamDamage", "slamForce");
                                Labels.FieldDoubleText("Damage", "Force");
                                parent.FieldDouble("Speed", "slamSpeed", "slamSignalAir");
                                Labels.FieldText("Signal");
                                parent.FieldDouble("Hold at Ground", "slamHoldTime", "slamSignalHold");
                                Labels.FieldDoubleText("Time", "Signal");
                                parent.FieldDouble("Hold At Peak", "peakTime", "peakSignal");
                                Labels.FieldDoubleText("Time", "Signal");
                                parent.Field("Slam Direction", "slamDirectionType");
                                GUI.enabled = true;
                        }

                        if (FoldOut.FoldOutButton(parent.Get("eventFoldOut")))
                        {
                                Fields.EventFoldOut(parent.Get("onSlam"), parent.Get("slamFoldOut"), "On Slam", color: color);
                        }
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion

        }

        [System.Serializable]
        public class MultiJump
        {
                [SerializeField] public float velX = 5f;
                [SerializeField] public float height = 5f;
                [SerializeField] public string signal;
                [SerializeField] public float maxDistance = 8f;
                [SerializeField] public JumpAboveType jumpAboveType;
                public enum JumpAboveType { NormalJump, JumpAboveTarget }

                public void Jump (Root root)
                {
                        root.signals.Set(signal);
                        Vector2 velocity = Compute.ArchObject(root.position, root.position, height, root.gravity.gravity); //  this method will find the exact velocity to jump the necessary height.
                        velocity.y += root.gravity.gravity * Root.deltaTime * 0.5f; //                                          adjust jump
                        root.velocity.y = velocity.y;
                        root.hasJumped = true;
                }

                public void JumpTo (Root root, Vector2 target, ref float velX)
                {
                        float distanceX = target.x - root.position.x;
                        distanceX = Mathf.Abs(distanceX) < 1f ? Mathf.Sign(distanceX) : distanceX;
                        distanceX = Mathf.Clamp(distanceX, -maxDistance, maxDistance);
                        Vector2 jumpTo = Vector2.right * distanceX;
                        Vector2 velocity = Compute.ArchObject(root.position, root.position + jumpTo, height, root.gravity.gravity);
                        velocity.y += root.gravity.gravity * Root.deltaTime * 0.5f; // Added for jump precision, will more or less jump the correct archHeight
                        root.velocity.y = velocity.y;
                        velX = jumpAboveType == JumpAboveType.NormalJump ? velocity.x : velocity.x * 2f;
                }
        }
}
