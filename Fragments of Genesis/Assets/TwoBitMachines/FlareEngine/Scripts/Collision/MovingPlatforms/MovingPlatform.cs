using System.Collections.Generic;
using UnityEngine;

namespace TwoBitMachines.FlareEngine
{
        [System.Serializable]
        public class MovingPlatform
        {
                [System.NonSerialized] public List<WorldCollision> passenger = new List<WorldCollision>();
                [System.NonSerialized] public Transform transform;
                [System.NonSerialized] public BoxCollider2D box;
                [System.NonSerialized] public Vector2 velocity;
                [System.NonSerialized] public Vector2 launchVelocity;
                [System.NonSerialized] public bool isShaking;
                [System.NonSerialized] public bool canBoostLaunch;

                public static MovingPlatform foundPlatform;
                public int passengerCount => passenger.Count;
                public bool hasPassengers => passenger.Count > 0;
                public float launchVelX => isShaking ? 0 : velocity.x; // don't include shake as part of launch velocity

                public void Initialize (Transform transformRef)
                {
                        transform = transformRef;
                        box = transform.GetComponent<BoxCollider2D>();
                        transform.gameObject.layer = LayerMask.NameToLayer("Platform");

                        float angle = transform.eulerAngles.z; //* Moving platform must have a proper 90 degree angle
                        if ((angle % 90f) != 0)
                        {
                                float newAngle = Mathf.Round(angle / 90f) * 90f;
                                float rotateBy = newAngle - angle;
                                transform.RotateAround(transform.position, Vector3.forward, rotateBy);
                        }
                }

                public void ResetAll ()
                {
                        isShaking = false;
                        foundPlatform = null;
                        velocity = Vector2.zero;
                        passenger.Clear();
                }

                public void UpdatePlatform (Vector2 velocityRef, ref bool isShakingRef, bool applyPosition = true)
                {
                        isShaking = isShakingRef;
                        velocity = velocityRef;
                        isShaking = false;
                        passenger.Clear();

                        if (applyPosition)
                        {
                                transform.position += (Vector3) velocity * Time.deltaTime;
                        }
                }

                public void RemovePassenger (WorldCollision passenger)
                {
                        if (this.passenger.Contains(passenger))
                        {
                                this.passenger.Remove(passenger);
                        }
                }

                public void AddPassenger (WorldCollision passenger)
                {
                        if (!this.passenger.Contains(passenger))
                        {
                                this.passenger.Add(passenger);
                        }
                }

                public bool InXRange (float point)
                {
                        float width = box != null ? box.size.x * transform.localScale.x * 0.5f : 0;
                        float p = transform.position.x;
                        return point >= p - width && point <= p + width;
                }

                public static bool LatchToPlatform (Transform transform, WorldCollision passenger, ref MovingPlatform currentPlatform)
                {
                        if (Character.movingPlatforms.TryGetValue(transform, out MovingPlatform newPlatform) && newPlatform != null)
                        {
                                if (currentPlatform != null && currentPlatform != newPlatform)
                                {
                                        currentPlatform.RemovePassenger(passenger);
                                }
                                newPlatform.AddPassenger(passenger);
                                currentPlatform = newPlatform;
                                return true;
                        }
                        return false;
                }

                public static void LatchToPlatform (WorldCollision character, Transform transform, LatchMPType type)
                {
                        if (transform == null || transform.gameObject.layer != WorldManager.platformLayer)
                        {
                                return;
                        }
                        if (LatchToPlatform(transform, character, ref character.mp.mp))
                        {
                                character.mp.latch = type;
                                character.mp.ClearLaunch();
                        }
                }

                public static bool Exists (Transform transform)
                {
                        if (transform != null && Character.movingPlatforms.TryGetValue(transform, out MovingPlatform newPlatform))
                        {
                                foundPlatform = newPlatform;
                                return foundPlatform != null;
                        }
                        return false;
                }

                public static bool IsBlock ()
                {
                        return foundPlatform != null && foundPlatform.transform != null && foundPlatform.transform.gameObject.CompareTag("Block");
                }
        }

        public enum LatchMPType
        {
                None,
                Standing,
                Ceiling,
                Holding
        }
}
