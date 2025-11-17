using UnityEngine;

namespace TwoBitMachines.FlareEngine.AI.BlackboardData
{
        [System.Serializable]
        public class StateJump : FollowerState
        {
                public override void Execute (TargetPathfindingBase ai, ref Vector2 velocity)
                {
                        if (ai.jumpType == JumpType.Fall)
                        {
                                velocity.x = ai.followToCenter ? MoveToCenterX(ai, ai.nextNode) : ai.velRef;
                                if (ai.world.onGround)
                                {
                                        RemoveLandingNodes(ai, ai.currentNode);
                                        Complete(ai, PathState.Follow);
                                }
                        }
                        else if (ai.jumpType == JumpType.Jump)
                        {
                                velocity = Vector2.zero;
                                if (ai.bezierJump.FollowJumpCurve(ai.transform, ref ai.signals.forcedVelocity))
                                {
                                        RemoveIncorrectLanding(ai);
                                        Complete(ai, ai.jumpToState);
                                }
                        }
                }

                public static void Search (StateFollow state, TargetPathfindingBase ai, ref Vector2 velocity)
                {
                        if (ai.nextNode.bridge && ai.futureNode.bridge)
                        {
                                return;
                        }
                        if (ai.futureNode.y == ai.nextNode.y) //                                platform on same level
                        {
                                if (!ai.nextNode.NextToGridX(ai.futureNode) && MoveToCenterX(ai, ai.nextNode, ref velocity.x, 0.5f))
                                {
                                        state.Jump(ai, ref velocity);
                                }
                        }
                        else if (ai.futureNode.y > ai.nextNode.y) //                             platform is above
                        {
                                if (MoveToCenterX(ai, ai.nextNode, ref velocity.x))
                                {
                                        state.Jump(ai, ref velocity);
                                }
                        }
                        else if (ai.nextNode.DistanceX(ai.futureNode) <= 1f) //                          platform is below
                        {
                                if (MoveToCenterX(ai, ai.futureNode, ref velocity.x))
                                {
                                        state.JumpFall(ai, ref velocity, true);
                                }
                        }
                        else if (MoveToCenterX(ai, ai.nextNode, ref velocity.x, 0.75f))
                        {
                                state.Jump(ai, ref velocity);
                        }
                }

                private void Complete (TargetPathfindingBase ai, PathState nextState)
                {
                        ai.state = nextState;
                        ai.pauseAfterJumpActive = true;
                        ai.pauseCounter = 0;
                }

                public static PathState JumpToState (PathNode node)
                {
                        return node.ladder ? PathState.Ladder : node.wall ? PathState.Wall : node.ceiling ? PathState.Ceiling : PathState.Follow;
                }
        }

        public enum JumpType
        {
                Jump,
                Fall
        }
}
