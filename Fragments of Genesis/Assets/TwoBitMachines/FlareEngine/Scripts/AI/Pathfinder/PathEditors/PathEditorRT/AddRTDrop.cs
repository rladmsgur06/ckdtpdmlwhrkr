using TwoBitMachines.FlareEngine.AI.BlackboardData;
using UnityEngine;

namespace TwoBitMachines.FlareEngine
{
        public class AddRTDrop
        {
                public static void Execute (PathfindingRT map)
                {
                        foreach (PathNode node in map.map.Values)
                        {
                                if (node.leftCorner && map.Node(node.x - 1, node.y, out PathNode edgea) && edgea.edgeOfCorner && CheckForOtherNodes(map, edgea))
                                {
                                        GetGround(map, node, node.x - 1, node.y);
                                }
                                if (node.rightCorner && map.Node(node.x + 1, node.y, out PathNode edgeb) && edgeb.edgeOfCorner && CheckForOtherNodes(map, edgeb))
                                {
                                        GetGround(map, node, node.x + 1, node.y);
                                }
                        }
                }

                private static void GetGround (PathfindingRT map, PathNode node, int gridX, int gridY)
                {
                        for (int y = gridY; y >= gridY - map.maxJump.x * 3f; y--)
                        {
                                if (!map.Node(gridX, y + 1, out PathNode ground) || !(ground.ground || ground.bridge))
                                {
                                        continue;
                                }
                                if (Mathf.Abs(gridY - y) - 1 > map.maxJump.x)
                                {
                                        node.AddNeighbor(ground, false);
#if UNITY_EDITOR
                                        PathfindingRT.AddVerticalPath(map, new Vector2(gridX, gridY) + map.cellOffset, ground.position, map.cellSize / 6f, Color.red); // big drop, fall
#endif
                                }
                                else
                                {
                                        node.AddNeighbor(ground);
#if UNITY_EDITOR
                                        PathfindingRT.AddVerticalPath(map, new Vector2(gridX, gridY) + map.cellOffset, ground.position, map.cellSize / 4f, Color.cyan); // can jump to
#endif
                                }
                                return;

                        }
                }

                private static bool CheckForOtherNodes (PathfindingRT map, PathNode node)
                {
                        for (int y = node.y; y >= node.y - map.maxJump.x * 3f; y--)
                        {
                                if (map.Node(node.x, node.y - y, out PathNode tempNode) && (tempNode.ladder || tempNode.wall))
                                {
                                        return false;
                                }
                        }
                        return true;
                }

        }
}
