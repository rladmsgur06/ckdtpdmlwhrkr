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
        public class PathfinderBasic : Action
        {
                [SerializeField] public Blackboard pathfindingBasic;
                [SerializeField] public Blackboard target;
                [SerializeField] public float resetDistance = 3f;
                [SerializeField] public Vector2 findDistance = Vector2.one;
                [SerializeField] public bool useFindDistance = true;
                [SerializeField] public PathTargetFind findType;

                [System.NonSerialized] private TargetPathfindingBasic path;
                [System.NonSerialized] private Vector2 previousPosition;
                [System.NonSerialized] private bool refreshed = false;

                private void Awake ()
                {
                        if (pathfindingBasic != null)
                        {
                                path = pathfindingBasic as TargetPathfindingBasic;
                        }
                }

                public override NodeState RunNodeLogic (Root root)
                {
                        if (path == null || target == null)
                        {
                                return NodeState.Failure;
                        }

                        if (useFindDistance) // exit out if found to prevent the path from recalculating
                        {
                                if (findType == PathTargetFind.TargetWithinDistance && Found(root.position, target.GetTarget()) && !target.hasNoTargets)
                                {
                                        return NodeState.Success;
                                }
                                else if (findType == PathTargetFind.ReachedPathEnd && path.path.Count == 0) // at final target
                                {
                                        return NodeState.Success;
                                }
                        }

                        if (nodeSetup == NodeSetup.NeedToInitialize)
                        {
                                previousPosition = target.GetTarget();
                                refreshed = true;
                        }
                        else
                        {
                                path.RunPathFollower(ref root.velocity);
                        }

                        if ((((previousPosition - target.GetTarget()).sqrMagnitude > resetDistance * resetDistance) || refreshed)) // path always safe to changepathfindingBasic.PathSafeToChange ( ) &&
                        {
                                if (this.target.hasNoTargets)
                                {
                                        return NodeState.Running;
                                }
                                previousPosition = target.GetTarget();
                                path.CalculatePath(target);
                                refreshed = false;
                        }

                        return NodeState.Running;
                }

                public bool Found (Vector2 position, Vector2 target)
                {
                        if (findDistance.x != 0 && (target.x > (position.x + findDistance.x * 0.52f) || target.x < (position.x - findDistance.x * 0.52f)))
                                return false;
                        if (findDistance.y != 0 && (target.y > (position.y + findDistance.y * 0.52f) || target.y < (position.y - findDistance.y * 0.52f)))
                                return false;
                        return true;
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                public override bool OnInspector (AIBase ai, SerializedObject parent, Color color, bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(125, "Follow a path to a target using the pathfinding algorithm. This algorithm does not take gravity into account. When the target has changed its position by the reset distance amount, the path will recalculate. If Success On is enabled, success is returned when the specified setting is met." +
                                        "\n \nReturns Running, Success, Failure");
                        }

                        int index = (int) findType;
                        int height = index == 1 ? 1 : 0;
                        FoldOut.Box(4 + height, color, offsetY: -2);
                        AIBase.SetRef(ai.data, parent.Get("pathfindingBasic"), 0);
                        AIBase.SetRef(ai.data, parent.Get("target"), 1);
                        parent.Field("Reset Distance", "resetDistance");
                        parent.FieldAndEnable("Success On", "findType", "useFindDistance");
                        parent.Field("Find Distance", "findDistance", execute: index == 1);
                        Layout.VerticalSpacing(3);

                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion
        }
}
