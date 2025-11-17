#if UNITY_EDITOR
using TwoBitMachines.FlareEngine;
using TwoBitMachines.FlareEngine.AI.BlackboardData;
using UnityEngine;

namespace TwoBitMachines.Editors
{
        public class CeilingAndBridgeNodesEditor : AIPathfindingEditor
        {
                public static void Execute (Pathfinding map)
                {
                        if (map.ceiling.Count != 0)
                        {
                                Register(map);
                        }

                        if (map.bridge.Count != 0)
                        {
                                RegisterBridges(map);
                        }
                }

                public static void Register (Pathfinding map)
                {
                        for (int x = 0; x < map.linesX; x++)
                                for (int y = 0; y < map.linesY; y++)
                                {
                                        PathNode tempNode = map.Node(x, y);
                                        if (tempNode != null && map.ceiling.Contains(new Vector2Int(tempNode.x, tempNode.y)))
                                        {
                                                tempNode.ceiling = true;
                                                ConnectCeilingsToGround(tempNode, map, x, y);
                                        }
                                }
                }

                private static void ConnectCeilingsToGround (PathNode ceiling, Pathfinding map, int x, int y)
                {
                        // connect air ladders to ground for ai to jump to
                        int jumpCount = 0;
                        for (int searchDown = 1; searchDown <= map.maxJumpHeight; searchDown++) //                                 search for ladders from left to right
                        {
                                PathNode ground = map.Node(x, y - searchDown);
                                if (ground == null || ground.ceiling)
                                {
                                        return;
                                }
                                jumpCount++;
                                if (ground.ground || ground.jumpThroughGround)
                                {
                                        for (int searchDownY = 1; searchDownY <= map.maxJumpHeight; searchDownY++)
                                        {
                                                ground = map.Node(x, y - searchDownY);
                                                if (ground.ground || ground.jumpThroughGround)
                                                {
                                                        ground.exact = true;
                                                        AddNodeToParent(ceiling, ground, map);
                                                        break;
                                                }
                                                AddVisualConnection(map, ground.position, map.cellSize / 4f, Color.cyan, 1);
                                        }
                                        return;
                                }
                        }
                }

                public static void RegisterBridges (Pathfinding map)
                {
                        for (int x = 0; x < map.linesX; x++)
                                for (int y = 0; y < map.linesY; y++)
                                {
                                        PathNode tempNode = map.Node(x, y);
                                        if (tempNode != null && map.bridge.Contains(new Vector2Int(tempNode.x, tempNode.y)))
                                        {
                                                tempNode.bridge = true;
                                        }
                                }
                }

        }

}
#endif
