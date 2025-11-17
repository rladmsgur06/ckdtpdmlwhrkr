using TwoBitMachines.FlareEngine.AI.BlackboardData;
using UnityEngine;

namespace TwoBitMachines.FlareEngine
{
        public class AddRTLadder
        {
                public static void Execute (PathfindingRT map)
                {
                        if (map.ladder.Count != 0)
                        {
                                AddLadderNodes(map);
                                ConnectLadders(map);
                        }
                }

                public static void AddLadderNodes (PathfindingRT map)
                {
                        for (int i = 0; i < map.ladder.Count; i++)
                        {
                                if (map.Node(map.ladder[i].x, map.ladder[i].y, out PathNode node))
                                {
                                        node.ladder = true;
                                        node.edgeOfCorner = false;
                                }
                                else
                                {
                                        map.map[map.ladder[i]] = new PathNode() { position = map.ladder[i] + map.cellOffset, x = map.ladder[i].x, y = map.ladder[i].y, ladder = true };
                                }
                        }
                }

                public static void ConnectLadders (PathfindingRT map)
                {
                        for (int i = 0; i < map.ladder.Count; i++)
                        {
                                if (map.Node(map.ladder[i].x, map.ladder[i].y, out PathNode node) && node.ladder)
                                {
                                        ConnectLadders(map, node);
                                        ConnectLaddersToGround(map, node);
                                }
                        }
                }

                private static void ConnectLadders (PathfindingRT map, PathNode node)
                {
                        for (int x = 1; x <= map.maxJump.x; x++) //      search for ladders from left to right
                        {
                                if (map.Node(node.x + x, node.y, out PathNode otherNode))
                                {
                                        if (otherNode.ground)
                                        {
                                                return;
                                        }
                                        if (otherNode.ladder)
                                        {
                                                node.AddNeighbor(otherNode);
#if UNITY_EDITOR
                                                PathfindingRT.AddHorizontalPath(map, node.position + Vector2.right * map.cellSize, otherNode.position, map.cellSize / 4f, Color.cyan);
#endif
                                                return;
                                        }
                                }
                        }
                }

                private static void ConnectLaddersToGround (PathfindingRT map, PathNode node)
                {
                        if (node.ground || map.ladder.Contains(new Vector2Int(node.x, node.y - 1)))
                        {
                                return;
                        }
                        for (int y = node.y - 1; y >= node.y - 20; y--)
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

