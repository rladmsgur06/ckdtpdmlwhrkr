#region 
#if UNITY_EDITOR
using TwoBitMachines.Editors;
using UnityEditor;
#endif
#endregion
using System.Collections.Generic;
using TwoBitMachines.FlareEngine.AI.BlackboardData;
using UnityEngine;

namespace TwoBitMachines.FlareEngine.AI
{
        [AddComponentMenu("")]
        public class FollowGraphPoints : Action
        {
                [SerializeField] public Blackboard target;
                [SerializeField] public InputButtonSO left;
                [SerializeField] public InputButtonSO right;
                [SerializeField] public InputButtonSO up;
                [SerializeField] public InputButtonSO down;
                [SerializeField] public float speed = 10f;

                [System.NonSerialized] private bool isActive;
                [System.NonSerialized] private Vector2 currentTarget;
                [System.NonSerialized] private TargetPoints targetPoints;

                Vector3 targetP;
                private void Start ()
                {
                        targetPoints = (TargetPoints) target;
                }

                public override NodeState RunNodeLogic (Root root)
                {
                        if (targetPoints == null)
                                return NodeState.Failure;

                        if (nodeSetup == NodeSetup.NeedToInitialize)
                        {
                                isActive = true;
                                currentTarget = targetPoints.GetNearestTarget(this.transform.position);
                        }

                        if (left != null && left.Pressed())
                                FindTarget(root.position, lookLeft: true);
                        if (right != null && right.Pressed())
                                FindTarget(root.position, lookRight: true);
                        if (down != null && down.Pressed())
                                FindTarget(root.position, lookDown: true);
                        if (up != null && up.Pressed())
                                FindTarget(root.position, lookUp: true);

                        if (isActive && Root.deltaTime != 0)
                        {
                                Vector2 newPosition = Vector2.MoveTowards(root.position, currentTarget, Root.deltaTime * speed);
                                root.velocity = (newPosition - root.position) / Root.deltaTime;
                                if ((root.position - currentTarget).sqrMagnitude == 0)
                                        isActive = false;
                        }

                        return NodeState.Running;
                }

                private void FindTarget (Vector2 position, bool lookRight = false, bool lookLeft = false, bool lookUp = false, bool lookDown = false)
                {
                        List<Vector2> point = targetPoints.point;

                        float sqrMagnitude = Mathf.Infinity;
                        for (int i = 0; i < point.Count; i++)
                        {
                                if (lookRight && (point[i].x <= position.x || Mathf.Abs(point[i].y - position.y) > 0.25f))
                                        continue;
                                if (lookLeft && (point[i].x >= position.x || Mathf.Abs(point[i].y - position.y) > 0.25f))
                                        continue;
                                if (lookDown && (point[i].y >= position.y || Mathf.Abs(point[i].x - position.x) > 0.25f))
                                        continue;
                                if (lookUp && (point[i].y <= position.y || Mathf.Abs(point[i].x - position.x) > 0.25f))
                                        continue;

                                float squareDistance = (position - point[i]).sqrMagnitude;
                                if (squareDistance < sqrMagnitude)
                                {
                                        sqrMagnitude = squareDistance;
                                        currentTarget = point[i];
                                        isActive = true;
                                }
                        }
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                public override bool HasNextState () { return false; }
                public override bool OnInspector (AIBase ai, SerializedObject parent, Color color, bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(60, "Navigate to points with the specified speed. Target must be TargetPoints." +
                                        "\n \nReturns Running, Failure");
                        }

                        FoldOut.Box(4, color, offsetY: -2);
                        AIBase.SetRef(ai.data, parent.Get("target"), 0);
                        parent.FieldDouble("Left, Right", "left", "right");
                        parent.FieldDouble("Up, Down", "up", "down");
                        parent.Field("Speed", "speed");
                        Layout.VerticalSpacing(3);
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion
        }
}
