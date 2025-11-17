using System.Collections.Generic;
using UnityEngine;

namespace TwoBitMachines.FlareEngine
{
        public class MovingPlatformMove
        {
                private static Transform slopeHit;

                public static void MovePassengers (List<WorldCollision> passengerList)
                {
                        Physics2D.SyncTransforms();

                        for (int i = 0; i < passengerList.Count; i++)
                        {
                                WorldCollision passenger = passengerList[i];
                                passenger.mp.pushed = Vector2.zero;

                                if (passenger.mp.launching)
                                {
                                        if (passenger.onGround || passenger.onCeiling || passenger.interactableWasHit || !passenger.mp.jumpValid || (passenger.onWall && passenger.oldVelocity.y < 0))
                                        {
                                                passenger.mp.launching = false;
                                        }
                                        else
                                        {
                                                MovePassenger(passengerList[i], passenger.mp.LaunchVelocity(), LatchMPType.None, passenger.mp.jumpTransform);
                                        }
                                }
                                else if (passenger.mp.latched)
                                {
                                        MovePassenger(passengerList[i], passenger.mp.velocity * Time.deltaTime, passenger.mp.latch, passenger.mp.transform);
                                }
                        }
                }

                public static void MovePassenger (WorldCollision passenger, Vector2 movingVelocity, LatchMPType mpLatch, Transform platformReference = null)
                {
                        passenger.box.Update();
                        slopeHit = null;
                        FindSlopeToClimbDown(ref movingVelocity, passenger.box, passenger);
                        HorizontalCollision(ref movingVelocity, passenger.box, mpLatch, platformReference);
                        VerticalCollision(ref movingVelocity, passenger.box, mpLatch, platformReference);
                        passenger.transform.position += (Vector3) movingVelocity;
                        passenger.box.Update();
                }

                private static void HorizontalCollision (ref Vector2 velocity, BoxInfo box, LatchMPType mpLatch, Transform platform = null)
                {
                        if (velocity.x == 0)
                                return;

                        float signX = Mathf.Sign(velocity.x);
                        float magnitude = Mathf.Abs(velocity.x) + box.skin.x * 2f;
                        Vector2 corner = signX > 0 ? box.bottomRight - box.skinX : box.bottomLeft + box.skinX; // X ray start from inside collider

                        for (int i = 0; i < box.rays.x; i++)
                        {
                                Vector2 origin = corner + box.up * box.spacing.y * i;
                                RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.right * signX, magnitude, MovingPlatformDetect.collisionLayer);

                                #region Debug
#if UNITY_EDITOR
                                if (WorldManager.viewDebugger)
                                {
                                        Debug.DrawRay(origin, Vector2.right * signX * magnitude, Color.green);
                                }
#endif
                                #endregion

                                if (!hit || hit.transform == platform || hit.transform == slopeHit)
                                {
                                        continue;
                                }
                                if (i == 0 && FindSlopeToClimbUp(ref velocity, box, box.world, hit))
                                {
                                        continue;
                                }
                                if (box.world.mp.pushed.x != 0)
                                {
                                        box.world.mp.CrushedByPlatform(platform);
                                }
                                if (mpLatch == LatchMPType.Holding)
                                {
                                        box.world.mp.CrushedByPlatform(platform);
                                }

                                box.world.SetWall(hit, signX);
                                magnitude = Mathf.Max(hit.distance, box.skin.x * 2f);
                                velocity.x = (magnitude - box.skin.x * 2f) * signX;
                        }
                }

                private static void VerticalCollision (ref Vector2 velocity, BoxInfo box, LatchMPType mpLatch, Transform platform = null)
                {
                        if (velocity.y == 0)
                                return;

                        float signY = Mathf.Sign(velocity.y);
                        float magnitude = Mathf.Abs(velocity.y) + box.skin.y;
                        Vector2 corner = signY > 0 ? box.topLeft : box.bottomLeft;

                        for (int i = 0; i < box.rays.y; i++)
                        {
                                Vector2 origin = corner + box.right * (box.spacing.x * i + velocity.x);
                                RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.up * signY, magnitude, MovingPlatformDetect.collisionLayer);

                                #region Debug
#if UNITY_EDITOR
                                if (WorldManager.viewDebugger)
                                {
                                        Debug.DrawRay(origin, Vector2.up * signY * magnitude, Color.red);
                                }
#endif
                                #endregion

                                if (hit && hit.transform != platform)
                                {
                                        if (box.world.mp.pushed.y != 0)
                                        {
                                                box.world.mp.CrushedByPlatform(platform);
                                        }
                                        if (mpLatch != LatchMPType.None)
                                        {
                                                if ((mpLatch == LatchMPType.Standing && velocity.y > 0) || (mpLatch == LatchMPType.Ceiling && velocity.y < 0))
                                                {
                                                        box.world.mp.CrushedByPlatform(platform);
                                                }
                                        }
                                        magnitude = Mathf.Max(hit.distance, box.skin.y);
                                        velocity.y = (magnitude - box.skin.y) * signY;
                                        box.world.onGround = signY <= 0 || box.world.onGround; //                          Since slope climb Up can set onGround true, keep true
                                        box.world.onCeiling = !box.world.onGround && signY > 0;
                                        box.world.groundNormal = hit.normal;
                                }
                        }
                }

                private static bool FindSlopeToClimbUp (ref Vector2 velocity, BoxInfo box, WorldCollision world, RaycastHit2D slope)
                {
                        if (slope.distance <= 0 || !Compute.Between(Compute.Angle(slope.normal, box.up, out float slopeAngle), 0, world.maxSlopeAngle))
                        {
                                return false;
                        }

                        float sign = Mathf.Sign(velocity.x);
                        float magnitude = Mathf.Abs(velocity.x) / Mathf.Cos(slopeAngle * Mathf.Deg2Rad); // increase magnitude to ensure it moves by x amount that platform is pushing if moving on a diagonal
                        Vector2 corner = box.BottomCorner(velocity.x);
                        Vector2 climbVelocity = ClimbVelocity(box, corner, magnitude, sign, slope);
                        slope = ValidateSlope(box, corner, magnitude, sign, slope, slopeAngle, ref climbVelocity);

                        if (climbVelocity.y < velocity.y || climbVelocity.x == 0)
                        {
                                return false;
                        }

                        velocity = climbVelocity;
                        slopeHit = slope.transform;
                        world.groundNormal = slope.normal;
                        world.climbingSlopeUp = world.onGround = true;
                        return true;
                }

                private static void FindSlopeToClimbDown (ref Vector2 velocity, BoxInfo box, WorldCollision world)
                {
                        if (velocity.x == 0 || velocity.y > 0)
                        {
                                return;
                        }

                        Vector2 corner = box.BottomCorner(-velocity.x);
                        float magnitude = Mathf.Abs(velocity.x) + box.skin.y;
                        RaycastHit2D slope = Physics2D.Raycast(corner, box.down, magnitude, MovingPlatformDetect.collisionLayer);

                        #region Debug
#if UNITY_EDITOR
                        if (WorldManager.viewDebugger)
                        {
                                Debug.DrawRay(corner, box.down * magnitude, Color.yellow);
                        }
#endif
                        #endregion

                        if (slope && slope.distance > 0 && Compute.Between(Compute.Angle(box.up, slope.normal, out float slopeAngle), 0, world.maxSlopeAngle))
                        {
                                if (Compute.Dot(box.right * Mathf.Sign(velocity.x), slope.normal) > 0) // only  climb if pointing in the same direction
                                {
                                        float magnitudeX = Mathf.Abs(velocity.x) / Mathf.Cos(slopeAngle * Mathf.Deg2Rad);
                                        velocity.y -= Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * magnitudeX;
                                        velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * magnitudeX * Mathf.Sign(velocity.x);
                                        world.climbingSlopeDown = world.onGround = true;
                                        world.groundNormal = slope.normal;
                                }
                        }
                }

                private static Vector2 ClimbVelocity (BoxInfo box, Vector2 origin, float radius, float signX, RaycastHit2D hit)
                {
                        Vector2 intersection = Compute.LineCircleIntersect(origin - box.skinY, radius, hit.point, hit.normal.Rotate(90f * -signX)); //       Find the exact point on slope that character needs to move to.
                        return intersection - (origin - box.skinY);
                }

                private static RaycastHit2D ValidateSlope (BoxInfo box, Vector2 corner, float magnitude, float sign, RaycastHit2D slope, float slopeAngle, ref Vector2 climbVelocity)
                {
                        RaycastHit2D hit = Physics2D.Raycast(corner, climbVelocity / magnitude, magnitude, MovingPlatformDetect.collisionLayer); //                                   Check for wall, skewed wall, or another slope
                        if (hit && hit.distance > 0 && Compute.Angle(hit.normal, box.up, out float newAngle) > slopeAngle)
                        {
                                if (Compute.Between(newAngle, 0, box.world.maxSlopeAngle))
                                {
                                        climbVelocity = ClimbVelocity(box, corner, magnitude / Mathf.Cos(newAngle * Mathf.Deg2Rad), sign, hit);
                                        return hit;
                                }
                        }
                        return slope;
                }
        }
}
