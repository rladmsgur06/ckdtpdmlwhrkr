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
        public class PathfinderRT : Action
        {
                [SerializeField] public Blackboard pathfinding;
                [SerializeField] public Blackboard target;
                [SerializeField] public PathTargetFind findType;
                [SerializeField] public Vector2 findDistance = Vector2.one;
                [SerializeField] public float resetDistance = 1f;
                [SerializeField] public bool useFindDistance = true;

                [System.NonSerialized] private TargetPathfindingRT path;
                [System.NonSerialized] private Vector2 previousPosition;
                [System.NonSerialized] private bool mapInitializing;
                [System.NonSerialized] private bool calculatePath;
                [System.NonSerialized] private int targetGridY;

                private void Awake ()
                {
                        if (pathfinding != null)
                        {
                                path = pathfinding as TargetPathfindingRT;
                        }
                }

                public override NodeState RunNodeLogic (Root root)
                {
                        if (path == null || target == null)
                        {
                                return NodeState.Failure;
                        }

                        if (nodeSetup == NodeSetup.NeedToInitialize)
                        {
                                path.ResetState(root.world, root.gravity, root.signals, target.GetTarget());
                                targetGridY = path.targetY;
                        }
                        if (useFindDistance) // exit out if found to prevent the path from recalculating
                        {
                                if (findType == PathTargetFind.TargetWithinDistance && Found(root.position, target.GetTarget()) && path.CanExit() && !target.hasNoTargets)
                                {
                                        return NodeState.Success;
                                }
                                else if (findType == PathTargetFind.ReachedPathEnd && path.AtFinalTarget() && path.CanExit())
                                {
                                        return NodeState.Success;
                                }
                        }

                        if (nodeSetup == NodeSetup.NeedToInitialize)
                        {
                                float cellSize = path.map != null ? path.map.cellSize : 1f;
                                findDistance.x = Mathf.Clamp(findDistance.x, cellSize, float.MaxValue);
                                findDistance.y = Mathf.Clamp(findDistance.y, cellSize, float.MaxValue);
                                previousPosition = target.GetTarget();
                                calculatePath = true;
                        }
                        else
                        {
                                path.RunPathFollower(ref root.velocity);
                        }

                        // if (path.map.isInitializingGrid)
                        // {
                        //         mapInitializing = true;
                        //         calculatePath = false;
                        // }
                        // if (mapInitializing && path.map.isInitializingGrid)
                        // {
                        //         mapInitializing = false;
                        //         calculatePath = true;
                        // }

                        if (path.pathSafeToChagne)
                        {
                                Vector2 targetPosition = target.GetTarget();
                                if (this.target.hasNoTargets)
                                {
                                        return NodeState.Running;
                                }
                                if (calculatePath || TargetPlaneChanged(targetPosition) || ((previousPosition - targetPosition).sqrMagnitude > resetDistance * resetDistance))
                                {
                                        previousPosition = targetPosition;
                                        path.instant = calculatePath;
                                        path.CalculatePath(target);
                                        calculatePath = false;
                                }
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

                public bool TargetPlaneChanged (Vector2 position) // for recalculating path
                {
                        PathNode targetNode = path.map.PositionToNode(position + path.map.cellYOffset);
                        if (targetNode != null && targetNode.ground && targetNode.y != targetGridY)
                        {
                                targetGridY = targetNode.y;
                                return true;
                        }
                        return false;
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                public override bool OnInspector (AIBase ai, SerializedObject parent, Color color, bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(125, "Follow a path to a target using the pathfinding algorithm. This algorithm takes gravity into account, making it ideal for platformers. When the target has changed its position by the reset distance amount, the path will recalculate. If Success On is enabled, success is returned when the specified setting is met." +
                                        "\n \nReturns Running, Success, Failure");
                        }

                        int index = (int) findType;
                        int height = index == 1 ? 1 : 0;
                        FoldOut.Box(4 + height, color, offsetY: -2);
                        AIBase.SetRef(ai.data, parent.Get("pathfinding"), 0);
                        AIBase.SetRef(ai.data, parent.Get("target"), 1);
                        parent.Field("Reset Distance", "resetDistance");
                        parent.FieldAndEnable("Success On", "findType", "useFindDistance");
                        if (parent.Bool("useFindDistance"))
                                parent.Field("Find Distance", "findDistance", execute: index == 1);
                        Layout.VerticalSpacing(3);
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion
        }

        // public enum PathTargetFind
        // {
        //         ReachedPathEnd,
        //         TargetWithinDistance
        // }
}
