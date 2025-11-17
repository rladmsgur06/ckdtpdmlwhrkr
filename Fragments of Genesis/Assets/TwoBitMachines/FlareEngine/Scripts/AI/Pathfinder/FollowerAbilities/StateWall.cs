using UnityEngine;

namespace TwoBitMachines.FlareEngine.AI.BlackboardData
{
        [System.Serializable]
        public class StateWall : FollowerState
        {
                public override void Execute (TargetPathfindingBase ai, ref Vector2 velocity)
                {
                        RemoveMoveSafelyY(ai);
                        RemoveOutOfOrderY(ai);
                        if (ai.currentNode.Same(ai.nextNode) && ai.futureNode != null && !ai.futureNode.isOccupied)
                        {
                                if (!ai.nextNode.SameX(ai.futureNode))
                                {
                                        if (!StateCornerGrab.GrabFromWall(ai))
                                        {
                                                Jump(ai, ref velocity);
                                        }
                                        return;
                                }
                                else if (!ai.nextNode.ground && ai.futureNode.ground)
                                {
                                        JumpFall(ai, ref velocity);
                                        return;
                                }
                        }
                        if (ai.state == PathState.Wall && !ai.stateChanged && !ai.currentNode.wall)
                        {
                                ai.state = PathState.Follow;
                                ai.CalculatePath(ai.targetRef);
                                return;
                        }
                        velocity = Vector2.zero;
                        int direction = ai.nextNode.wallLeft ? -1 : 1;// ai.nextNode.wallLeft ? -1 : 1; // CHECK FOR BLOCK
                        MoveToTarget(ai.position.x, ai.nextNode.position.x + direction, ai.followSpeed * 0.5f, ref velocity.x);
                        if (ai.world.onWall)
                        {
                                ai.signals.Set("wallClimb", true);
                                MoveToTarget(ai.position.y, ai.nextNode.position.y, ai.wallSpeed, ref velocity.y);
                        }
                }

                public static void FindWallToClimb (TargetPathfindingBase ai, ref Vector2 velocity)
                {
                        if (ai.nextNode.wall && ai.futureNode.wall)
                        {
                                ai.state = PathState.Wall;
                        }
                }

        }
}


// if (StateMoving.WaitForMovingPlatform(ai, this, ref velocity))
// {
//         if (ai.state == PathState.Wall && ai.HitWall())
//         {
//                 ai.SetAnimation("wallClimb", true);
//         }
//         return;
// }
// else 
