using UnityEngine;

namespace TwoBitMachines.FlareEngine.AI.BlackboardData
{
        [System.Serializable]
        public class StateCeiling : FollowerState
        {
                public override void Execute (TargetPathfindingBase ai, ref Vector2 velocity)
                {
                        if (ai.currentNode.SameX(ai.nextNode))
                        {
                                RemoveNode(ai);
                        }
                        if (!ai.nextNode.ceiling)
                        {
                                JumpFall(ai, ref velocity);
                                return;
                        }
                        if (ai.state == PathState.Ceiling && !ai.stateChanged && ai.world.onGround)
                        {
                                ai.state = PathState.Follow;
                                ai.CalculatePath(ai.targetRef);
                                return;
                        }

                        ai.signals.Set("ceilingClimb", true);
                        MoveToTarget(ai.position.x, ai.nextNode.position.x, ai.ceilingSpeed, ref velocity.x);
                        MoveToTarget(ai.position.y, ai.nextNode.position.y, ai.cellSize, ref velocity.y);
                }
        }

}
