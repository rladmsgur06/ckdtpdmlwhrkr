#if UNITY_EDITOR
using TwoBitMachines.FlareEngine;
using TwoBitMachines.FlareEngine.AI.BlackboardData;
using UnityEngine;

namespace TwoBitMachines.Editors
{
        public class MovingPlatformNodesEditor : AIPathfindingEditor
        {
                public static void Execute (Pathfinding map)
                {
                        if (map.moving.Count != 0)
                        {
                                Register(map);
                                FindCorners(map);
                        }
                }

                public static void Register (Pathfinding map)
                {
                        for (int x = 0; x < map.linesX; x++)
                                for (int y = 0; y < map.linesY; y++)
                                {
                                        PathNode tempNode = map.Node(x, y);
                                        if (tempNode != null && map.moving.Contains(new Vector2Int(tempNode.x, tempNode.y)))
                                        {
                                                tempNode.moving = true;
                                        }
                                }
                }

                public static void FindCorners (Pathfinding map)
                {
                        for (int x = 0; x < map.linesX; x++)
                                for (int y = 0; y < map.linesY; y++)
                                {
                                        PathNode moving = map.Node(x, y);
                                        if (moving != null && moving.moving)
                                        {
                                                PathNode right = moving.ShiftX(map, 1);
                                                PathNode left = moving.ShiftX(map, -1);
                                                PathNode up = moving.ShiftY(map, 1);
                                                PathNode down = moving.ShiftY(map, -1);

                                                if ((right == null || !right.moving) && ((up != null && !up.moving) || (down != null && !down.moving)))
                                                {
                                                        AddVisualConnection(map, moving.position, map.cellSize / 4f, Tint.Delete, 1);
                                                        moving.rightCorner = true;
                                                }
                                                if ((left == null || !left.moving) && ((up != null && !up.moving) || (down != null && !down.moving)))
                                                {

                                                        AddVisualConnection(map, moving.position, map.cellSize / 4f, Tint.Delete, 1);
                                                        moving.leftCorner = true;
                                                }

                                        }
                                }
                }
        }

}
#endif
