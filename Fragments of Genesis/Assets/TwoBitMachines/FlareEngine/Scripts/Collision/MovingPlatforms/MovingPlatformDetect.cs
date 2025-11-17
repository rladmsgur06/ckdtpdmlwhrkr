using System.Collections.Generic;
using UnityEngine;

namespace TwoBitMachines.FlareEngine
{
        public class MovingPlatformDetect
        {
                public static readonly int top_Left = 1;
                public static readonly int top_Right = 2;
                public static readonly int bottom_Right = 3;
                public static readonly int bottom_Left = 0;

                public static bool queriesSetting;
                public static LayerMask collisionLayer;
                public static Vector2[] platformCorners = new Vector2[5];
                public static Vector2[] passengerCorners = new Vector2[5];
                public static ContactFilter2D filter = new ContactFilter2D ( );
                public static List<Collider2D> results = new List<Collider2D> ( );

                public static void Setup ( )
                {
                        filter.useLayerMask = true;
                        filter.layerMask = WorldManager.platformMask;
                        collisionLayer = WorldManager.collisionMask;
                        queriesSetting = Physics2D.queriesStartInColliders;
                }

                public static void ResetQuery ( )
                {
                        Physics2D.queriesStartInColliders = queriesSetting;
                }

                public static void Run ( )
                {
                        if (Time.deltaTime == 0) return;

                        if (Character.movingPlatforms.Count != 0)
                        {
                                // when a transform is moved manually, the collider doesn't move with it. The physic engine will apply the change at the 
                                // end of the physics update. However, we need the transform and the collider to be in sync to properly detect moving platforms.
                                // To force both transform and collider to be in the same position, call SyncTransforms(), this however, might have a small performance impact.
                                Physics2D.queriesStartInColliders = true;
                                Character.AIMovingPlatforms ( );
                                Physics2D.queriesStartInColliders = false;
                                MovingPlatformMove.MovePassengers (Character.passengers);
                                DetectPlatforms (Character.passengers);
                        }
                        Physics2D.queriesStartInColliders = true;
                }

                public static void DetectPlatforms (List<WorldCollision> passengerList)
                {
                        Physics2D.SyncTransforms ( );
                        for (int i = 0; i < passengerList.Count; i++)
                        {
                                WorldCollision passenger = passengerList[i];
                                if (DetectPlatforms (passenger, out Transform movingPlatform)) //   push out velocity                                                                                                push character out from platform if detected
                                {
                                        passenger.mp.pushOutVelocity.x += passenger.mp.pushOutVelocity.x != 0 ? 0.00025f * Mathf.Sign (passenger.mp.pushOutVelocity.x) : 0; // add a bit more length to pushOut
                                        passenger.mp.pushOutVelocity.y += passenger.mp.pushOutVelocity.y != 0 ? 0.00025f * Mathf.Sign (passenger.mp.pushOutVelocity.y) : 0;
                                        MovingPlatformMove.MovePassenger (passenger, passenger.mp.pushOutVelocity, LatchMPType.None, movingPlatform);
                                }
                        }
                        Physics2D.SyncTransforms ( );
                }

                private static bool DetectPlatforms (WorldCollision passenger, out Transform movingPlatform)
                {
                        movingPlatform = null;
                        passenger.mp.pushOutVelocity = Vector2.zero;
                        int platforms = passenger.box.collider.OverlapCollider (filter, results);

                        if (platforms == 0 || (platforms == 1 && results[0].transform == passenger.mp.transform))
                        {
                                return false; //                       no platforms detected or passenger is already being moved by this platform, exit out
                        }

                        if (platforms > 1 && passenger.mp.latched && passenger.mp.transform != null)
                        {
                                for (int i = 0; i < platforms; i++)
                                {
                                        if (results[i].transform == passenger.mp.transform)
                                        {
                                                platforms--; //         remove platform if already on it
                                                results.RemoveAt (i);
                                                break;
                                        }
                                }
                        }

                        SetPassengerCorners (passenger.box);

                        for (int p = 0; p < platforms; p++) //    run twice to completely push character out if there are multiple collisions
                        {
                                for (int i = 0; i < platforms; i++)
                                {
                                        if (MovingPlatform.Exists (results[i].transform) && SetPlatformCorners (results[i], out bool isEdge))
                                        {
                                                Vector2 current_mpVelocity = passenger.mp.RelevantVelocity ( ) * 1.001f;
                                                Vector2 new_mpVelocity = MovingPlatform.foundPlatform.velocity * Time.deltaTime * 1.001f; //  increase search length by a bit
                                                Vector2 relativeVelocity = RelativeVelocity (new_mpVelocity, current_mpVelocity, passenger.mp.pushOutVelocity);

                                                Vector2 newPushOutVelocity = PushCornersOut (relativeVelocity, isEdge);
                                                for (int j = 0; j < 5; j++) passengerCorners[j] += newPushOutVelocity; //                     offset passenger corners
                                                Crushed (newPushOutVelocity, passenger);
                                                passenger.mp.pushOutVelocity += newPushOutVelocity;
                                                passenger.mp.detected = true;
                                                movingPlatform = MovingPlatform.foundPlatform.transform;
                                        }
                                }
                        }
                        return passenger.mp.detected;
                }

                private static void Crushed (Vector2 velocity, WorldCollision passenger)
                {
                        if (passenger.mp.pushed.x != 0 && velocity.x != 0 && velocity.y == 0 && !Compute.SameSign (passenger.mp.pushed.x, velocity.x))
                        {
                                passenger.mp.CrushedByPlatform (MovingPlatform.foundPlatform.transform);
                        }
                        if (passenger.mp.pushed.y != 0 && velocity.y != 0 && velocity.x == 0 && !Compute.SameSign (passenger.mp.pushed.y, velocity.y))
                        {
                                passenger.mp.CrushedByPlatform (MovingPlatform.foundPlatform.transform);
                        }
                        if (velocity.x != 0 && velocity.y == 0)
                        {
                                passenger.mp.pushed.x = Mathf.Sign (velocity.x);
                        }
                        if (velocity.y != 0 && velocity.x == 0)
                        {
                                passenger.mp.pushed.y = Mathf.Sign (velocity.y);
                        }
                }

                private static Vector2 RelativeVelocity (Vector2 new_mpVelocity, Vector2 current_mpVelocity, Vector2 pushInVelocity)
                {
                        // if character is pushed into this platform by another platform, subtract velocities to get relative velocity. Relative velocity will yield a larger velocity for searching and pushing out
                        if (new_mpVelocity.x != 0 && !Compute.SameSign (current_mpVelocity.x, new_mpVelocity.x)) new_mpVelocity.x -= current_mpVelocity.x; // if velocities point towards each other, it's a collision, else they're moving away
                        if (new_mpVelocity.y != 0 && !Compute.SameSign (current_mpVelocity.y, new_mpVelocity.y)) new_mpVelocity.y -= current_mpVelocity.y; // don't add initialPushIn with regular pushIn velocity since they can potentially cancel each other out.
                        if (new_mpVelocity.x != 0 && !Compute.SameSign (pushInVelocity.x, new_mpVelocity.x)) new_mpVelocity.x -= pushInVelocity.x;
                        if (new_mpVelocity.y != 0 && !Compute.SameSign (pushInVelocity.y, new_mpVelocity.y)) new_mpVelocity.y -= pushInVelocity.y;
                        return new_mpVelocity;
                }

                private static Vector2 PushCornersOut (Vector2 velocity, bool isEdgeCollider = false)
                {
                        Vector2 pushOutVelocity = Vector2.zero;
                        if (velocity.y > 0) //            platform moving up
                        {
                                PlatformCornerInsidePassenger (-Vector2.up * velocity.y, ref pushOutVelocity, platformCorner : top_Left, passengerCorner1 : bottom_Left, passengerCorner2 : bottom_Right);
                                PlatformCornerInsidePassenger (-Vector2.up * velocity.y, ref pushOutVelocity, platformCorner : top_Right, passengerCorner1 : bottom_Left, passengerCorner2 : bottom_Right);
                                PassengerCornerInsidePlatform (Vector2.up * velocity.y, ref pushOutVelocity, passengerCorner : bottom_Left, platformCorner1 : top_Left, platformCorner2 : top_Right);
                                PassengerCornerInsidePlatform (Vector2.up * velocity.y, ref pushOutVelocity, passengerCorner : bottom_Right, platformCorner1 : top_Left, platformCorner2 : top_Right);
                        }
                        if (isEdgeCollider) return pushOutVelocity;

                        if (velocity.y < 0) //            platform moving down
                        {
                                PlatformCornerInsidePassenger (-Vector2.up * velocity.y, ref pushOutVelocity, platformCorner : bottom_Left, passengerCorner1 : top_Left, passengerCorner2 : top_Right);
                                PlatformCornerInsidePassenger (-Vector2.up * velocity.y, ref pushOutVelocity, platformCorner : bottom_Right, passengerCorner1 : top_Left, passengerCorner2 : top_Right);
                                PassengerCornerInsidePlatform (Vector2.up * velocity.y, ref pushOutVelocity, passengerCorner : top_Left, platformCorner1 : bottom_Left, platformCorner2 : bottom_Right);
                                PassengerCornerInsidePlatform (Vector2.up * velocity.y, ref pushOutVelocity, passengerCorner : top_Right, platformCorner1 : bottom_Left, platformCorner2 : bottom_Right);
                        }

                        if (velocity.x < 0) //            platform moving left
                        {
                                PlatformCornerInsidePassenger (-Vector2.right * velocity.x, ref pushOutVelocity, platformCorner : bottom_Left, passengerCorner1 : top_Right, passengerCorner2 : bottom_Right);
                                PlatformCornerInsidePassenger (-Vector2.right * velocity.x, ref pushOutVelocity, platformCorner : top_Left, passengerCorner1 : top_Right, passengerCorner2 : bottom_Right);
                                PassengerCornerInsidePlatform (Vector2.right * velocity.x, ref pushOutVelocity, passengerCorner : top_Right, platformCorner1 : bottom_Left, platformCorner2 : top_Left);
                                PassengerCornerInsidePlatform (Vector2.right * velocity.x, ref pushOutVelocity, passengerCorner : bottom_Right, platformCorner1 : bottom_Left, platformCorner2 : top_Left);
                        }
                        else if (velocity.x > 0) //       platform moving right
                        {
                                PlatformCornerInsidePassenger (-Vector2.right * velocity.x, ref pushOutVelocity, platformCorner : top_Right, passengerCorner1 : bottom_Left, passengerCorner2 : top_Left);
                                PlatformCornerInsidePassenger (-Vector2.right * velocity.x, ref pushOutVelocity, platformCorner : bottom_Right, passengerCorner1 : bottom_Left, passengerCorner2 : top_Left);
                                PassengerCornerInsidePlatform (Vector2.right * velocity.x, ref pushOutVelocity, passengerCorner : bottom_Left, platformCorner1 : top_Right, platformCorner2 : bottom_Right);
                                PassengerCornerInsidePlatform (Vector2.right * velocity.x, ref pushOutVelocity, passengerCorner : top_Left, platformCorner1 : top_Right, platformCorner2 : bottom_Right);
                        }
                        return pushOutVelocity;
                }

                private static void PlatformCornerInsidePassenger (Vector2 velocity, ref Vector2 pushOutVelocity, int platformCorner, int passengerCorner1, int passengerCorner2)
                {
                        #region Debug
                        #if UNITY_EDITOR
                        if (WorldManager.viewDebugger)
                        {
                                Debug.DrawLine (platformCorners[platformCorner], platformCorners[platformCorner] + velocity, Color.red);
                        }
                        #endif
                        #endregion
                        if (Compute.LineIntersection (platformCorners[platformCorner], platformCorners[platformCorner] + velocity, passengerCorners[passengerCorner1], passengerCorners[passengerCorner2], out Vector2 intersection))
                        {
                                Util.OverrideIfBigger (ref pushOutVelocity, platformCorners[platformCorner] - intersection);
                        }
                }

                private static void PassengerCornerInsidePlatform (Vector2 velocity, ref Vector2 pushOutVelocity, int passengerCorner, int platformCorner1, int platformCorner2)
                {
                        #region Debug
                        #if UNITY_EDITOR
                        if (WorldManager.viewDebugger)
                        {
                                Debug.DrawLine (passengerCorners[passengerCorner], passengerCorners[passengerCorner] + velocity, Color.blue);
                        }
                        #endif
                        #endregion
                        if (Compute.LineIntersection (passengerCorners[passengerCorner], passengerCorners[passengerCorner] + velocity, platformCorners[platformCorner1], platformCorners[platformCorner2], out Vector2 intersection))
                        {
                                Util.OverrideIfBigger (ref pushOutVelocity, intersection - passengerCorners[passengerCorner]);
                        }
                }

                private static void SetPassengerCorners (BoxInfo passenger)
                {
                        passenger.CornerUpdateUnmodified ( );
                        passengerCorners[0] = passenger.bottomLeft;
                        passengerCorners[1] = passenger.topLeft;
                        passengerCorners[2] = passenger.topRight;
                        passengerCorners[3] = passenger.bottomRight;
                        passengerCorners[4] = passengerCorners[0];
                }

                private static bool SetPlatformCorners (Collider2D platform, out bool isEdge)
                {
                        isEdge = false;
                        if (platform is BoxCollider2D)
                        {
                                BoxInfo.GetColliderCorners ((BoxCollider2D) platform);
                                platformCorners[0] = BoxInfo.bottomLeftCorner;
                                platformCorners[1] = BoxInfo.topLeftCorner;
                                platformCorners[2] = BoxInfo.topRightCorner;
                                platformCorners[3] = BoxInfo.bottomRightCorner;
                                platformCorners[4] = platformCorners[0];
                                return true;
                        }
                        else if (platform is EdgeCollider2D)
                        {
                                EdgeCollider2D edge = (EdgeCollider2D) platform;
                                if (edge.points.Length != 2) return false;
                                BoxInfo.GetEdgeCorners (edge);
                                platformCorners[1] = BoxInfo.topLeftCorner;
                                platformCorners[2] = BoxInfo.topRightCorner;
                                return isEdge = true;
                        }
                        return false;
                }

        }
}