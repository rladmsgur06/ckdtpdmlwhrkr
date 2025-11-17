using TwoBitMachines.FlareEngine.AI.BlackboardData;
using UnityEngine;

namespace TwoBitMachines.FlareEngine
{
        public class AddRTCeiling
        {
                public static void Execute (PathfindingRT map)
                {
                        if (map.ceiling.Count != 0)
                        {
                                AddCeilingNodes(map);
                        }
                }

                public static void AddCeilingNodes (PathfindingRT map)
                {
                        for (int i = 0; i < map.ceiling.Count; i++)
                        {
                                PathNode node;
                                if (map.Node(map.ceiling[i].x, map.ceiling[i].y, out node))
                                {
                                        node.ceiling = true;
                                }
                                else
                                {
                                        node = new PathNode() { position = map.ceiling[i] + map.cellOffset, x = map.ceiling[i].x, y = map.ceiling[i].y, ceiling = true };
                                        map.map[map.ceiling[i]] = node;
                                }
                                ConnectCeilingsToGround(node, map, node.x, node.y);
                        }
                }

                private static void ConnectCeilingsToGround (PathNode node, PathfindingRT map, int gridX, int gridY)
                {
                        for (int y = 1; y <= map.maxJump.y; y++)
                        {
                                if (map.Node(node.x, node.y - y, out PathNode ground) && ground.ground)
                                {
                                        node.AddNeighbor(ground);
#if UNITY_EDITOR
                                        PathfindingRT.AddVerticalPath(map, node.position + Vector2.down * map.cellSize, ground.position, map.cellSize / 4f, Color.cyan);
#endif
                                        return;
                                }
                        }
                }

        }

}

