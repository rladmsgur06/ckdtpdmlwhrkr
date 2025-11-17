#if UNITY_EDITOR
using TwoBitMachines.FlareEngine;
using TwoBitMachines.FlareEngine.AI.BlackboardData;
using UnityEngine;

namespace TwoBitMachines.Editors
{
        public class AirAndGroundNodesEditor : AIPathfindingEditor
        {
                public static void Execute (Vector2 startingPosition, PathNode[] grid, Pathfinding map, Vector2 cellOffset)
                {
                        for (int x = 0; x < map.linesX; x++)
                        {
                                Vector2 nodeBasePosition = startingPosition + cellOffset + Vector2.right * map.cellSize * x;
                                for (int y = 0; y < map.linesY; y++)
                                {
                                        Vector2 nodePosition = nodeBasePosition + Vector2.up * map.cellSize * y;
                                        if (NodeIsAir(map, nodePosition))
                                        {
                                                Vector2 nodeBelowPosition = nodePosition + Vector2.down * map.cellSize;
                                                if (NodeIsGround(map, nodeBelowPosition))
                                                {
                                                        grid[y * map.linesX + x] = new PathNode() { position = nodePosition, x = x, y = y, ground = true };
                                                        AddVisualConnection(map, nodePosition, map.cellSize / 4f, Color.green, 1);
                                                }
                                                else
                                                {
                                                        grid[y * map.linesX + x] = new PathNode() { position = nodePosition, x = x, y = y, air = true };

                                                }
                                        }
                                }
                        }
                }

                private static bool NodeIsAir (Pathfinding map, Vector2 nodePosition)
                {
                        return !Physics2D.OverlapPoint(nodePosition, map.layerWorld);
                }

                private static bool NodeIsGround (Pathfinding map, Vector2 nodePosition)
                {
                        return Physics2D.OverlapPoint(nodePosition, map.layerWorld);
                }
        }

}
#endif
