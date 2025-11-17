#if UNITY_EDITOR
using TwoBitMachines.FlareEngine;
using TwoBitMachines.FlareEngine.AI.BlackboardData;

namespace TwoBitMachines.Editors
{
        public class FindCornersEditor : AIPathfindingEditor
        {
                private static readonly int searchLeft = -1;
                private static readonly int searchRight = 1;

                public static void Execute (Pathfinding map)
                {
                        for (int x = 0; x < map.linesX; x++)
                                for (int y = 0; y < map.linesY; y++)
                                {
                                        PathNode tempNode = map.grid[y * map.linesX + x];
                                        if (tempNode == null || tempNode.air)
                                        {
                                                continue;
                                        }
                                        FindCorners(tempNode, map, x, y);
                                }
                }

                public static void FindCorners (PathNode tempNode, Pathfinding map, int x, int y)
                {
                        if (IsCorner(tempNode, map, x, y, searchLeft))
                        {
                                tempNode.leftCorner = true;
                        }
                        if (IsCorner(tempNode, map, x, y, searchRight))
                        {
                                tempNode.rightCorner = true;
                        }
                }

                private static bool IsCorner (PathNode tempNode, Pathfinding map, int x, int y, int sign)
                {
                        bool safetyCheckX = sign > 0 ? (x + 1) < map.linesX : (x - 1) >= 0;
                        bool safetyCheckY = (y - 1) >= 0;

                        if (!safetyCheckX || !safetyCheckY)
                        {
                                return false;
                        }

                        bool nodeIsNextToWall = map.Node(x + sign, y) == null;
                        bool nodeIsACorner = map.Node(x + sign, y - 1) != null;

                        return (nodeIsNextToWall || nodeIsACorner) && NotAGroundEdge(tempNode.jumpThroughGround, map, x, y, sign);
                }

                private static bool NotAGroundEdge (bool isGroundEdge, Pathfinding map, int x, int y, int sign) // this is a jumpthrough node
                {
                        // The ground edge might not actually be a true corner since ground edges don't have colliders, so what should be a "wall" is actually air
                        if (isGroundEdge)
                        {
                                PathNode node = map.Node(x + sign, y);
                                if (node != null && node.jumpThroughGround) // if there is a ground edge in the direction of the search, it means we are not at a corner
                                        return false;
                        }
                        return true;
                }
        }
}
#endif
