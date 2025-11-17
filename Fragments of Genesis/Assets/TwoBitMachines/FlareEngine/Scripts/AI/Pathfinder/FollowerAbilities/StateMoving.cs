using UnityEngine;

namespace TwoBitMachines.FlareEngine.AI.BlackboardData
{
        //* moving platforms that work with pathfinding must be one cellsize in height
        public class StateMoving : FollowerState
        {
                [System.NonSerialized] public bool canFollow;
                [System.NonSerialized] public float waitTimer; //        for moving platform state

                public override void Execute (TargetPathfindingBase ai, ref Vector2 velocity)
                {
                        if (!ai.world.mp.standing)
                                return;

                        if (ai.nextNode.moving && ai.futureNode != null && ai.futureNode.moving)
                        {
                                RemoveNode(ai);
                        }
                        if (!ai.stateChanged && !ai.currentNode.moving)
                        {
                                ai.state = PathState.Follow;
                                return;
                        }
                        if (ai.world.mp.velocity.sqrMagnitude < 0.001f) // platform not moving
                        {
                                if (canFollow && ai.futureNode != null)
                                {
                                        if (ai.world.onGround && ai.currentNode.Same(ai.nextNode) && !ai.futureNode.moving)
                                        {
                                                Jump(ai, ref velocity);
                                        }
                                        else if (ai.nextNode.moving && !ai.futureNode.moving && InPlatformXrange(ai.nextNode.position.x)) // only move towards an exit node
                                        {
                                                MoveToCenterX(ai, ai.nextNode, ref velocity.x);
                                        }
                                }
                        }
                        else
                        {
                                canFollow = true;
                        }
                }

                public static bool WaitForMovingPlatform (TargetPathfindingBase ai, FollowerState state, ref Vector2 velocity)
                {
                        // if (ai.futureNode.moving && TwoBitMachines.Clock.Timer (ref ai.waitTimer, 0.5f)) // don't check every frame, moving platform should have wait times
                        // {
                        //       Collider2D platform = Physics2D.OverlapPoint (ai.futureNode.position - ai.map.cellYOffset * 2.95f, ); //Need to rethink layermask
                        //       if (platform && MovingPlatform.Exists (platform.transform) && MovingPlatform.foundPlatform.velocity == Vector2.zero) // wait until moving platform has zero velocity to jump on
                        //       {
                        //             ai.canFollow = false;
                        //             state.JumpTo (ai, ai.futureNode, 1f, ref velocity, StateJumpType.Moving);
                        //             return true;
                        //       }
                        // }
                        return ai.futureNode != null && ai.futureNode.moving;
                }

                public bool InPlatformXrange (float point)
                {
                        return false; // world != null && world.mp != null ? world.mp.InXRange (point) : false;
                }

                public static void OnMovingPlatforms (TargetPathfindingBase ai)
                {
                        if (ai.currentNode.moving)
                        {
                                ai.state = PathState.Moving;
                                // ai.canFollow = false;
                        }
                }
        }
}
