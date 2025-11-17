using TwoBitMachines.FlareEngine.Interactables;
using UnityEngine;

namespace TwoBitMachines.FlareEngine.ThePlayer
{
        public class RopeInteraction
        {
                [System.NonSerialized] private Particle[] particle;
                [System.NonSerialized] private RopeSwing ropeSwing;
                [System.NonSerialized] private float tetherRadius;
                [System.NonSerialized] private Rope rope;

                public void Set (RopeSwing ropeSwing, Rope rope)
                {
                        this.rope = rope;
                        this.ropeSwing = ropeSwing;
                        this.particle = rope.particle;
                        this.tetherRadius = rope.tetherRadius;
                }

                public Vector2 RopeHoldPoint (WorldCollision world, Vector2 center, Vector2 climbVelocity, out bool climbing)
                {
                        climbing = false;
                        if (ropeSwing.particle1 < particle.Length && ropeSwing.particle2 < particle.Length)
                        {
                                // current holdPoint
                                Vector2 direction = (particle[ropeSwing.particle2].position - particle[ropeSwing.particle1].position).normalized;
                                Vector2 holdPoint = particle[ropeSwing.particle1].position + direction * ropeSwing.grabDistance;

                                if (climbVelocity.y == 0)
                                {
                                        return holdPoint;
                                }

                                // next hold point
                                Vector2 movePoint = holdPoint + world.box.up * climbVelocity.y;
                                if (Rope.HoldPoint (ropeSwing, particle, movePoint, world.box.right, tetherRadius, ref holdPoint))
                                {
                                        climbing = true;
                                }
                                return holdPoint;
                        }
                        return center;
                }

                public Vector2 RopeDirection (Vector2 center)
                {
                        if (particle.Length <= 1)
                        {
                                return Vector2.up;
                        }
                        if (particle.Length == 2)
                        {
                                return (particle[0].position - particle[1].position).normalized;
                        }

                        int index = -1;
                        float distance = Mathf.Infinity;

                        for (int i = 1; i < particle.Length; i++)
                        {
                                float sqrDist = (center - particle[i].position).sqrMagnitude;
                                if (sqrDist < distance)
                                {
                                        distance = sqrDist;
                                        index = i;
                                }
                        }
                        if (index >= 0)
                        {
                                Vector2 firstPoint = particle[index].position;
                                Vector2 secondPoint = particle[index - 1].position;
                                return (secondPoint - firstPoint).normalized;
                        }
                        return Vector2.up;
                }

                public void RotatePlayerToRope (WorldCollision world, Vector2 center, Vector2 direction, float rate)
                {
                        float angle = Vector2.Angle (world.box.up, direction);
                        float maxAngle = Mathf.Clamp (angle * Time.deltaTime * rate * 10f, 0, angle);
                        maxAngle = angle > 0 && angle < 1.5f ? angle : maxAngle; //                                             
                        world.transform.RotateAround (center, Vector3.forward, maxAngle * world.box.up.CrossSign (direction));
                        world.box.Update ( );
                }

        }
}