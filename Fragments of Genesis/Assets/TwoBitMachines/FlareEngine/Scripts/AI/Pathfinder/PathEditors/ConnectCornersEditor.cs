#if UNITY_EDITOR
using System.Collections.Generic;
using TwoBitMachines.FlareEngine;
using TwoBitMachines.FlareEngine.AI.BlackboardData;
using UnityEngine;

namespace TwoBitMachines.Editors
{
        public class ConnectCornersEditor : AIPathfindingEditor
        {
                public static List<PathNode> airNode = new List<PathNode>();

                public static void Execute (PathNode[] grid, Pathfinding map) //                       Connect edges so that ai can jump from platform to platform
                {
                        for (int x = 0; x < map.linesX; x++)
                                for (int y = 0; y < map.linesY; y++)
                                {
                                        PathNode cornerNode = grid[y * map.linesX + x];
                                        if (cornerNode != null && (cornerNode.leftCorner || cornerNode.rightCorner))
                                        {
                                                SearchForCorners(grid, map, cornerNode);
                                        }
                                }
                }

                private static void SearchForCorners (PathNode[] grid, Pathfinding map, PathNode cornerA)
                {
                        for (int x = 0; x < map.linesX; x++)
                                for (int y = 0; y < map.linesY; y++)
                                {
                                        PathNode cornerB = grid[y * map.linesX + x];

                                        if (cornerB == null || (!cornerB.leftCorner && !cornerB.rightCorner) || cornerA.Same(cornerB)) // cornerB is not valid
                                        {

                                                continue;
                                        }
                                        if (CornersNotRelativeToEachOther(map, cornerA, cornerB))
                                        {
                                                continue;
                                        }

                                        if (CornersWithinJumpingDistance(map, cornerA, cornerB))
                                        {
                                                airNode.Clear();
                                                if (HorizontalBlock(grid, map, cornerA, cornerB, y) || HorizontalBlock(grid, map, cornerB, cornerA, y, 1)) // since ai has to jump to platform, make sure the jumping path is not blocked
                                                {
                                                        continue;
                                                }
                                                if (VerticalBlock(grid, map, cornerA, y))
                                                {
                                                        continue;
                                                }
                                                if (CornerNextToWall(map, cornerA, cornerB))
                                                {
                                                        continue;
                                                }
                                                PathIsValidConnectNodes(map, cornerA, cornerB);
                                        }
                                }
                }

                private static bool CornersNotRelativeToEachOther (Pathfinding map, PathNode cornerA, PathNode cornerB)
                {
                        // left edge is to the right and higher than right edge. Vice versa. If both false, exit out.
                        if (cornerA.leftCorner && cornerB.rightCorner && cornerA.x > cornerB.x && cornerB.y >= cornerA.y)
                                return false; // valid left edge
                        if (cornerA.rightCorner && cornerB.leftCorner && cornerA.x < cornerB.x && cornerB.y >= cornerA.y)
                                return false;
                        return true;
                }

                private static bool CornersWithinJumpingDistance (Pathfinding map, PathNode cornerA, PathNode cornerB)
                {
                        bool withinDistanceX = Mathf.Abs(cornerA.x - cornerB.x) <= map.maxJumpDistance;
                        bool withinDistanceY = Mathf.Abs(cornerA.y - cornerB.y) <= map.maxJumpHeight;
                        return (withinDistanceX && withinDistanceY);
                }

                private static bool CornerNextToWall (Pathfinding map, PathNode cornerA, PathNode cornerB)
                {
                        if (cornerA.leftCorner && cornerB.rightCorner && cornerA.ShiftX(map, -1) == null && cornerB.x < (cornerA.x - 1))
                                return true; // wall is between corners. Corner might already have a connection on said wall. Exit out.
                        if (cornerA.rightCorner && cornerB.leftCorner && cornerA.ShiftX(map, 1) == null && cornerB.x > (cornerA.x + 1))
                                return true;
                        return false;
                }

                private static bool HorizontalBlock (PathNode[] grid, Pathfinding map, PathNode cornerA, PathNode cornerB, int y, int g = 0)
                {
                        if (cornerA.rightCorner && cornerB.leftCorner)
                        {
                                for (int searchX = cornerA.x; searchX <= cornerB.x; searchX++)
                                {
                                        PathNode connectNode = grid[y * map.linesX + searchX];
                                        if (connectNode != null && (cornerA.Same(connectNode) || cornerB.Same(connectNode)))
                                                continue;
                                        if (connectNode == null || connectNode.block || !connectNode.air || connectNode.height < 1)
                                        {
                                                return true;
                                        }
                                        if (cornerA.y == cornerB.y && connectNode.height <= 1)
                                        {
                                                return true;
                                        }
                                        if (searchX < cornerB.x)
                                                airNode.Add(connectNode);
                                }
                        }
                        return false;
                }

                private static bool VerticalBlock (PathNode[] grid, Pathfinding map, PathNode cornerA, int y)
                {
                        for (int searchY = cornerA.y + 1; searchY <= y; searchY++)
                        {
                                PathNode connectNode = grid[searchY * map.linesX + cornerA.x];
                                if (connectNode == null || connectNode.block || !connectNode.air || connectNode.height < 1)
                                {
                                        return true;
                                }
                                airNode.Add(connectNode);
                        }
                        return false;
                }

                private static void PathIsValidConnectNodes (Pathfinding map, PathNode cornerA, PathNode cornerB)
                {
                        AddNodeToParent(cornerA, cornerB, map);
                        for (int i = 0; i < airNode.Count; i++)
                                AddVisualConnection(map, airNode[i].position, map.cellSize / 4f, Color.cyan, 1);
                }
        }

        public struct FilterData
        {
                public float height;
                public PathNode node;

                public FilterData (float newHeight, PathNode newNode)
                {
                        height = newHeight;
                        node = newNode;
                }

        }
}
#endif
