using System.Data;
using UnityEngine;

namespace TwoBitMachines.FlareEngine
{
        public class MovingPlatformInfo
        {
                [System.NonSerialized] public MovingPlatform mp;
                [System.NonSerialized] public LatchMPType latch;
                [System.NonSerialized] public Vector2 pushed;
                [System.NonSerialized] public Vector2 pushOutVelocity;
                [System.NonSerialized] public WorldCollision world;

                [System.NonSerialized] public bool detected;
                [System.NonSerialized] public bool ignoreBlockCollision;

                [System.NonSerialized] public MovingPlatform mpJump;
                [System.NonSerialized] public MPFollowType follow;
                [System.NonSerialized] public Vector2 launch;
                [System.NonSerialized] public bool launching;


                public bool latched => latch != LatchMPType.None;
                public bool ceiling => latch == LatchMPType.Ceiling;
                public bool holding => latch == LatchMPType.Holding;
                public bool standing => latch == LatchMPType.Standing;
                public bool jumpValid => follow == MPFollowType.Launch || jumpTransform != null;

                public Vector2 velocity => mp != null ? mp.velocity : Vector2.zero;
                public Transform transform => mp != null ? mp.transform : null;
                public Transform jumpTransform => mpJump != null ? mpJump.transform : null;

                public void Initialize (WorldCollision worldRef)
                {
                        world = worldRef;
                }

                public void ClearLaunch ()
                {
                        launching = false;
                }

                public void Clear ()
                {
                        mp = null;
                        detected = false;
                        latch = LatchMPType.None;
                }

                public void Launch (ref Vector2 velocityRef, ref float velY)
                {
                        if (world.useMovingPlatform && mp != null && latch != LatchMPType.None && velocity != Vector2.zero)
                        {
                                velY += velocity.y;
                                velocityRef.y += velocity.y;
                                launching = true;
                                follow = MPFollowType.Launch;
                                launch = Vector2.right * mp.launchVelX;
                        }
                }

                public void Launch (ref Vector2 velocityRef)
                {
                        if (world.useMovingPlatform && mp != null && latch != LatchMPType.None && velocity != Vector2.zero)
                        {
                                velocityRef.y += velocity.y;
                                launching = true;
                                follow = MPFollowType.Launch;
                                launch = Vector2.right * mp.launchVelX;
                        }
                }

                public void LaunchBoost (ref Vector2 velocityRef, ref float velY)
                {
                        if (world.useMovingPlatform && mp != null && latch != LatchMPType.None)
                        {
                                velY += mp.launchVelocity.y;
                                velocityRef.y += mp.launchVelocity.y;
                                launching = true;
                                follow = MPFollowType.Launch;
                                launch = Vector2.right * mp.launchVelocity.x;
                        }
                }

                public void Follow ()
                {
                        if (world.useMovingPlatform && mp != null && latch != LatchMPType.None)
                        {
                                launching = true;
                                follow = MPFollowType.Follow;
                                mpJump = mp;
                        }
                }

                public void CrushedByPlatform (Transform transform)
                {
                        if (transform != null && !transform.gameObject.CompareTag("Block")) // not a block, crushed by moving platform
                        {
                                world.onCrushed.Invoke(ImpactPacket.impact.Set(world.crushedWE, transform, world.boxCollider, world.position, null, Vector2.zero, world.box.directionX, 0));
                        }
                        if (transform != null && transform.gameObject.CompareTag("Block"))
                        {
                                ignoreBlockCollision = true;
                        }
                }

                public Vector2 LaunchVelocity ()
                {
                        return follow == MPFollowType.Follow && mpJump != null ? mpJump.velocity * Time.deltaTime : launch * Time.deltaTime;
                }

                public Vector2 RelevantVelocity ()
                {
                        return latch != LatchMPType.None && mp != null ? velocity * Time.deltaTime : LaunchVelocity();
                }

        }

        public enum MPFollowType
        {
                Launch,
                Follow
        }
}
