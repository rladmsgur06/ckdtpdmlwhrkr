using UnityEngine;

namespace TwoBitMachines.FlareEngine.AI.BlackboardData
{
        [System.Serializable]
        public class StateFollow : FollowerState
        {
                public override void Execute (TargetPathfindingBase ai, ref Vector2 velocity)
                {
                        if (ai.pauseAfterJumpActive && ai.pauseAfterJump > 0 && TwoBitMachines.Clock.TimerInverse(ref ai.pauseCounter, ai.pauseAfterJump))
                        {
                                return; // Maybe pause can be based on jump distance and jump direction, maybe no pause when jumping up or down, etc.
                        }
                        ai.pauseAfterJumpActive = false;

                        if (ai.world.onGround)
                        {
                                RemoveMoveSafelyX(ai);
                                velocity.x = MoveToCenterX(ai, ai.nextNode);
                        }
                        FindNewState(ai, ai.world.onGround, ref velocity);

                        if (ai.state == PathState.Follow && ai.currentNode.onGround && ai.currentNode.DistanceY(ai.nextNode) > 0) // not reachable
                        {
                                ai.CalculatePath(ai.targetRef);
                        }
                }

                public void RemoveMoveSafelyX (TargetPathfindingBase ai)
                {
                        if (ai.nextNode != null && ai.futureNode != null && ai.currentNode.SameX(ai.nextNode))
                        {
                                if (ai.nextNode.NextToX(ai.futureNode))
                                {
                                        RemoveNode(ai);
                                }
                                else if (ai.nextNode.bridge && ai.futureNode.bridge && ai.nextNode.NextToY(ai.futureNode))
                                {
                                        RemoveNode(ai);
                                }
                        }
                }

                public void FindNewState (TargetPathfindingBase ai, bool onGround, ref Vector2 velocity)
                {
                        if (onGround && ai.state == PathState.Follow)
                        {
                                StateLadder.FindLadderClimb(ai, ref velocity);
                        }
                        if (ai.futureNode != null && onGround && ai.state == PathState.Follow)
                        {
                                StateWall.FindWallToClimb(ai, ref velocity);
                        }
                        if (ai.futureNode != null && ai.state == PathState.Follow && !ai.futureNode.isOccupied)
                        {
                                StateJump.Search(this, ai, ref velocity);
                        }
                }

        }
}
