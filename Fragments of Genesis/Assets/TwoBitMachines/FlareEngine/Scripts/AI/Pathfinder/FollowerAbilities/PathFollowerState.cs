using UnityEngine;

namespace TwoBitMachines.FlareEngine.AI.BlackboardData

{
        [System.Serializable]
        public class FollowerState
        {
                #region 
#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField] private bool foldOut = false;
#pragma warning restore 0414
#endif
                #endregion

                public virtual void Execute (TargetPathfindingBase ai, ref Vector2 velocity)
                {

                }

                public void Jump (TargetPathfindingBase ai, ref Vector2 velocity)
                {
                        BezierJump(ai);
                        velocity = Vector2.zero;
                        ai.state = PathState.Jump;
                        ai.jumpType = JumpType.Jump;
                        ai.jumpToState = StateJump.JumpToState(ai.futureNode);
                        FollowerState.RemoveNode(ai);
                }

                private void BezierJump (TargetPathfindingBase ai)
                {
                        Vector2 start = ai.bottomCenter;
                        Vector2 end = ai.futureNode.position - ai.cellYOffset;
                        Vector2 direction = end.x > start.x ? Vector2.right : Vector2.left;
                        Vector2 control1, control2;
                        float height = Mathf.Abs(end.y - start.y) + 0.5f;
                        float distance = Mathf.Abs(end.x - start.x);
                        float scale = 1f;

                        if (ai.futureNode.wall)
                        {
                                control1 = new Vector2(start.x, end.y);
                                control2 = end;
                        }
                        else if (ai.futureNode.ladder && ai.futureNode.y > ai.nextNode.y)
                        {
                                control1 = new Vector2(start.x, end.y);
                                control2 = end;
                        }
                        else if (ai.futureNode.ceiling)
                        {
                                control1 = start;
                                control2 = end = ai.futureNode.position + ai.cellYOffset + Vector2.down * (ai.size.y);
                        }
                        else if (ai.futureNode.y < ai.nextNode.y)
                        {
                                control1 = start + direction * distance * 0.5f + Vector2.up * 0.7f;
                                control2 = new Vector2(end.x, start.y);
                                scale = 1f + distance * 0.25f;
                        }
                        else if (ai.futureNode.y > ai.nextNode.y)
                        {
                                control1 = new Vector2(start.x, end.y);
                                control2 = end + Vector2.up * distance * 0.25f - direction;
                                scale = 1f + distance * 0.20f;
                        }
                        else // same level
                        {
                                control1 = start + direction * 0.25f * distance + Vector2.up * 0.25f * distance;
                                control2 = end - direction * 0.25f * distance + Vector2.up * 0.25f * distance;
                                scale = 2f + distance * 0.25f;
                        }

                        float gravity = ai.gravity;
                        ai.signals.ForceDirection((int) direction.x);
                        ai.bezierJump.ResetCurve(start, end, control1, control2);
                        ai.bezierJump.ResetJumpTime(ai.transform.position, gravity, height, ai.followSpeed, scale);
                        ai.bezierJump.FollowJumpCurve(ai.transform, ref ai.signals.forcedVelocity);
                        //Debug.Log("Jump time: " + ai.bezierJump.jumpTime);
                }

                public void JumpFall (TargetPathfindingBase ai, ref Vector2 velocity, bool followToCenter = false)
                {
                        FollowerState.MoveToTarget(ai.position.x, ai.futureNode == null ? ai.position.x : ai.futureNode.position.x, ai.followSpeed, ref velocity.x);
                        ai.jumpType = JumpType.Fall;
                        ai.state = PathState.Jump;
                        FollowerState.RemoveNode(ai);
                        ai.followToCenter = followToCenter;
                        ai.velRef = followToCenter ? velocity.x : 0;
                }

                public static void MoveToTarget (float position, float target, float speed, ref float velocity) //
                {
                        float newPosition = Mathf.MoveTowards(position, target, speed * Time.deltaTime);
                        velocity = Time.deltaTime == 0 ? 0 : (newPosition - position) / Time.deltaTime;
                }

                public static bool MoveToCenterX (TargetPathfindingBase ai, PathNode node, ref float velocity, float corner = 0, bool arrived = true)
                {
                        float direction = ai.futureNode.SameX(node) ? 0 : ai.futureNode.DirectionX(node);
                        float target = corner == 0 ? node.position.x : node.position.x + direction * ai.cellSize * corner;
                        if (direction != 0 && ((ai.position.x >= target && direction > 0) || (ai.position.x <= target && direction < 0)))
                        {
                                return true;
                        }
                        float position = Mathf.MoveTowards(ai.position.x, target, ai.followSpeed * Time.deltaTime);
                        velocity = Time.deltaTime == 0 ? 0 : (position - ai.position.x) / Time.deltaTime;
                        return Mathf.Abs(position - target) <= 0.01f;
                }

                public static float MoveToCenterX (TargetPathfindingBase ai, PathNode target)
                {
                        float newPosition = Mathf.MoveTowards(ai.position.x, target.position.x, ai.followSpeed * Time.deltaTime);
                        return Time.deltaTime == 0 ? 0 : (newPosition - ai.position.x) / Time.deltaTime;
                }

                public static void RemoveMoveSafelyY (TargetPathfindingBase ai)
                {
                        if (ai.nextNode != null && ai.futureNode != null && ai.currentNode.SameY(ai.nextNode) && ai.nextNode.NextToY(ai.futureNode))
                        {
                                RemoveNode(ai);
                        }
                }

                public void RemoveOutOfOrderY (TargetPathfindingBase ai)
                {
                        if (ai.futureNode != null && ai.futureNode.Below(ai.nextNode) && ai.currentNode.Below(ai.nextNode)) // ai can sometimes be in incorrect order
                        {
                                RemoveNode(ai);
                        }
                        if (ai.futureNode != null && ai.futureNode.Above(ai.nextNode) && ai.currentNode.Above(ai.nextNode))
                        {
                                RemoveNode(ai);
                        }
                }

                public static void RemoveNode (TargetPathfindingBase ai)
                {
                        if (ai.path.Count == 0)
                        {
                                ai.nextNode = ai.currentNode;
                                ai.futureNode = null;
                                return;
                        }
                        else if (ai.path.Count == 1)
                        {
                                ai.nextNode = ai.path.Pop(); // was peek
                                ai.futureNode = null;
                                return;
                        }
                        else // at least two nodes remain
                        {
                                ai.nextNode = ai.path.Pop();
                                ai.futureNode = ai.path.Peek();
                        }
                }

                public static void RemoveLandingNodes (TargetPathfindingBase ai, PathNode node)
                {
                        // int max = 0;
                        // while (max++ < 2) //max node check
                        // {
                        //         if (ai.path.Count > 1 && ai.path.Peek().gridY == node.gridY)
                        //         {
                        //                 bool sameX = ai.path.Peek().gridX == node.gridX;
                        //                 RemoveNode(ai);
                        //                 if (sameX)
                        //                         break;
                        //         }
                        //         else
                        //                 break;
                        // }
                }

                public static void RemoveIncorrectLanding (TargetPathfindingBase ai, int max = 0)
                {
                        while (max++ < 3) //max node check
                        {
                                if (ai.path.Count > 1 && !ai.currentNode.Same(ai.nextNode))
                                {
                                        RemoveNode(ai);
                                        continue;
                                }
                                break;
                        }
                }

        }
}

// public static void RemoveNextNode (TargetPathfindingBase ai)
// {
//         if (ai.currentNode.Same(ai.nextNode))
//         {
//                 RemoveNode(ai);
//         }
// }

// private void RemoveNode (Stack<PathNode> path)
// {
//         if (path.Count > 0)
//         {
//                 path.Pop();
//         }
// }

// public void Jump (TargetPathfindingBase ai, PathNode jumpTo, float archHeight, ref Vector2 velocity, StateJumpType type = StateJumpType.Jump, bool xOffset = false, float offsetX = 1f, float offsetY = 1f)
// {
//         BezierJump(ai, archHeight);
//         SetupJump(ai, type, ai.futureNode);
// }

// public void Jump (TargetPathfindingBase ai, PathNode jumpTo, float archHeight, ref Vector2 velocity, StateJumpType type = StateJumpType.Jump)
// {
//         BezierJump(ai, archHeight);
//         SetupJump(ai, type, ai.futureNode);
// }
