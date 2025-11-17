using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace TwoBitMachines.FlareEngine.Interactables
{
        [BurstCompile]
        public struct FoliageJob : IJobParallelFor
        {
                [WriteOnly] public NativeArray<float> movement;
                [ReadOnly] public NativeArray<Vector2> interact; // x holds interaction amount, y holds top or bottom value
                [ReadOnly] public NativeArray<Vector2> position;
                [ReadOnly] public NativeArray<Vector3> playerPosition;
                [ReadOnly] public NativeArray<Vector2> playerOldPosition;
                [ReadOnly] public NativeArray<Vector2> playerVelocity;

                public NativeArray<float> vertexOffset;
                public NativeArray<float> perlinNoise;
                public NativeArray<float> velocity;

                public Vector2 size;
                public float jiggle;
                public float damping;
                public float strength;
                public float deltaTime;
                public float frequency;
                public float sizeScale;
                public float uniformity;
                public float squareDistance;

                public void Execute (int id)
                {
                        Vector2 grassPosition = ((Vector2) position[id] + size * 0.5f);

                        for (int i = 0; i < playerPosition.Length; i++)
                        {
                                Vector2 currentPosition = playerPosition[i];
                                Vector2 oldPosition = playerOldPosition[i];
                                float interaction = interact[id].x;
                                //                                                                                    interact x == interaction, Interact y == orientation
                                if (interact[id].y == 1) //                                                           top vertices
                                {
                                        currentPosition += playerPosition[i].z * Vector2.up; //                       offset player position with player height if testing for ceiling grass
                                        oldPosition += playerPosition[i].z * Vector2.up; //                           z is player height
                                }

                                if (interaction > 0 && (currentPosition - grassPosition).sqrMagnitude < squareDistance) // Within grass range
                                {
                                        bool collided = false;
                                        float ID = interact[id].y;
                                        bool allOthers = interact[id].y != 1;
                                        bool isCeiling = interact[id].y == 1 && playerVelocity[i].y > 0;

                                        if (ID == 0 || ID == 1) // bottom, top
                                        {
                                                if (CrossedThreshold (currentPosition.x, oldPosition.x, grassPosition.x))
                                                {
                                                        float xStrength = math.clamp (math.abs (playerVelocity[i].x), 0, 0.25f);
                                                        velocity[id] = math.sign (playerVelocity[i].x) < 0 ? interaction * -xStrength : interaction * xStrength;
                                                        collided = true;
                                                }
                                                if (CrossedThreshold (currentPosition.y, oldPosition.y, grassPosition.y))
                                                {
                                                        float yStrength = math.clamp (math.abs (playerVelocity[i].y), 0, 0.10f);
                                                        velocity[id] = currentPosition.x > grassPosition.x ? interaction * -yStrength * 0.7f : interaction * yStrength * 0.7f;
                                                        collided = true;
                                                }
                                        }
                                        if (ID == 2 || ID == 3) // left, right
                                        {
                                                if (CrossedThreshold (currentPosition.x, oldPosition.x, grassPosition.x))
                                                {
                                                        float xStrength = math.clamp (math.abs (playerVelocity[i].x), 0, 0.3f);
                                                        velocity[id] = math.sign (playerVelocity[i].x) < 0 ? interaction * -xStrength : interaction * xStrength;
                                                        collided = true;
                                                }
                                                if (CrossedThreshold (currentPosition.y, oldPosition.y, grassPosition.y))
                                                {
                                                        float yStrength = math.clamp (math.abs (playerVelocity[i].y), 0, 0.1f);
                                                        velocity[id] = currentPosition.y > grassPosition.y ? interaction * yStrength : interaction * -yStrength;
                                                        collided = true;
                                                }
                                        }
                                        if (collided) break;
                                }
                        }

                        //* spring physics
                        float acceleration = -jiggle * vertexOffset[id] * deltaTime * 25f - velocity[id] * damping; // * deltaTime * 25f;
                        velocity[id] += acceleration * deltaTime;
                        if (math.abs (velocity[id]) > 0.35f) velocity[id] = 0.35f * math.sign (velocity[id]);
                        vertexOffset[id] = vertexOffset[id] + velocity[id];
                        if (math.abs (vertexOffset[id]) > 2f) vertexOffset[id] = 2f * math.sign (vertexOffset[id]);

                        //* Perlin noise
                        float perlinNoise = noise.cnoise (new Vector2 (this.perlinNoise[id], grassPosition.x * (1f - uniformity) + grassPosition.y));
                        this.perlinNoise[id] = this.perlinNoise[id] + frequency * deltaTime;
                        perlinNoise = perlinNoise * strength;
                        movement[id] = (perlinNoise + vertexOffset[id]) * sizeScale;
                }

                private bool CrossedThreshold (float newP, float oldP, float target)
                {
                        return (newP > target && oldP < target) || (newP < target && oldP > target);
                }
        }
}