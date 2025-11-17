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
        public class JumpTo : Action
        {
                [SerializeField] public Blackboard target;
                [SerializeField] public float height = 5;
                [SerializeField] public float maxDistanceX = 5;
                [SerializeField] public float minDistanceX = 1;
                [SerializeField] public float scaleDistance = 1;
                [SerializeField] public bool useSignal;
                [SerializeField] public string signal;
                [System.NonSerialized] public bool bypass = false;
                [System.NonSerialized] public float velocityXReference;

                public override NodeState RunNodeLogic (Root root)
                {
                        if (target == null)
                                return NodeState.Failure;

                        if (nodeSetup == NodeSetup.NeedToInitialize)
                        {
                                if (!root.world.onGround && !bypass)
                                {
                                        return NodeState.Failure;
                                }
                                bypass = false;
                                float distanceX = (target.GetTarget().x - root.position.x) * scaleDistance;
                                if (this.target.hasNoTargets)
                                {
                                        return NodeState.Failure;
                                }
                                distanceX = Mathf.Abs(distanceX) < minDistanceX ? minDistanceX * Mathf.Sign(distanceX) : distanceX;
                                distanceX = Mathf.Clamp(distanceX, -maxDistanceX, maxDistanceX);
                                Vector2 jumpTo = Vector2.right * distanceX;
                                Vector2 velocity = Compute.ArchObject(root.position, root.position + jumpTo, height, root.gravity.gravity);
                                velocity.y += root.gravity.gravity * Root.deltaTime * 0.5f; // Added for jump precision, will more or less jump the correct archHeight
                                root.velocity.y = velocity.y;
                                velocityXReference = velocity.x;
                                root.hasJumped = true;
                        }
                        else
                        {
                                if (root.world.onGround)
                                {
                                        return NodeState.Success;
                                }
                                if (useSignal)
                                        root.signals.Set(signal);
                                root.velocity.x = velocityXReference;
                        }
                        return NodeState.Running;
                }

                public void SetJumpPoint (Vector2 origin, float direction)
                {
                        bypass = true;
                        target.Set(origin + Vector2.right * maxDistanceX * direction);
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                public override bool OnInspector (AIBase ai, SerializedObject parent, Color color, bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(110, "If on the ground, the AI will jump towards the specified target. Clamp how far in the x direction the AI can jump." +
                                        " You can also scale this distance. For example, the AI can always jump half the distance to the player. Will return success once the AI is on the ground." +
                                        "\n \nReturns Running, Success, Failure");
                        }

                        FoldOut.Box(5, color, offsetY: -2);
                        {
                                AIBase.SetRef(ai.data, parent.Get("target"), 0);
                                parent.Field("Arch Height", "height");
                                parent.FieldDouble("Distance X", "minDistanceX", "maxDistanceX");
                                Labels.FieldDoubleText("Min", "Max", rightSpacing: 4f);
                                parent.Field("Scale Distance X", "scaleDistance");
                                parent.FieldAndEnable("Animation Signal", "signal", "useSignal");
                        }
                        Layout.VerticalSpacing(3);
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion

        }
}
