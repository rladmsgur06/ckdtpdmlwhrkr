#if UNITY_EDITOR
using TwoBitMachines.FlareEngine;
using TwoBitMachines.FlareEngine.AI.BlackboardData;
using UnityEngine;

namespace TwoBitMachines.Editors
{
        public class AIPathfindingEditor
        {
                public static void CreateGrid (Pathfinding map, bool show)
                {
                        if (!Application.isPlaying && map.foldOut)
                        {
                                BeginGridCreation(map);
                        }
                }

                private static void BeginGridCreation (Pathfinding map)
                {
                        if (map.createPaths && map.cellSize > 0 && map.bounds.size != Vector2.zero)
                        {
                                DebugTimer.Start();
                                CreateGrid(map.bounds.position, map.bounds.size, map.cellSize, map);
                                map.createPaths = false;
                                DebugTimer.Stop("Created Pathfinding Nodes ");

                                for (int x = 0; x < map.linesX; x++)
                                        for (int y = 0; y < map.linesY; y++)
                                        {
                                                PathNode node = map.grid[y * map.linesX + x];
                                                Vector2Int index = new Vector2Int(node.x, node.y);
                                                if (map.fall.Contains(index))
                                                {
                                                        node.isFall = true;
                                                }
                                                else
                                                {
                                                        node.isFall = false;
                                                }
                                        }
                        }

                }

                public static void DisplayMapAfterEditing (Pathfinding map, bool show)
                {
                        if (show && map != null && map.grid != null && !map.createPaths)
                        {
                                if (map.grid.Length == 0)
                                {
                                        map.connections.Clear();
                                }
                                Draw.GLStart();
                                for (int i = 0; i < map.connections.Count; i++)
                                {
                                        Draw.GLCircle(map.connections[i].position, map.connections[i].size, map.connections[i].color, map.connections[i].rays);
                                }
                                Draw.GLEnd();
                        }
                }

                private static void CreateGrid (Vector2 startingPosition, Vector2 gridSize, float cellSize, Pathfinding map)
                {
                        if (cellSize <= 0)
                                cellSize = 1f;

                        map.linesX = Mathf.RoundToInt(gridSize.x / cellSize);
                        map.linesY = Mathf.RoundToInt(gridSize.y / cellSize);
                        map.grid = new PathNode[map.linesX * map.linesY];
                        Vector2 cellOffset = new Vector2(map.cellSize * 0.5f, map.cellSize * 0.5f);

                        map.connections.Clear();
                        map.neighbor.Clear();
                        map.bounds.Initialize();

                        Draw.GLStart();
                        AirAndGroundNodesEditor.Execute(startingPosition, map.grid, map, cellOffset); //  sets air, ground
                        JumpThroughGroundNodesEditor.Execute(map, cellOffset); //                         sets jumpThroughGround (jTG), ground, air, -- reads ground
                        NodePropertiesEditor.SetNodeHeadSpace(map); //                                    sets height
                        FindCornersEditor.Execute(map); //                                                sets leftCorner, rightCorner   -- reads air, jTG

                        LadderNodesEditor.Execute(map); //                                                sets ladder -- reads ground, jTG
                        CeilingAndBridgeNodesEditor.Execute(map); //                                      sets ceiling, bridge, exact -- reads ground, jTG
                        MovingPlatformNodesEditor.Execute(map); //                                        sets moving, also sets rightCorner, leftCorner
                        WallNodesEditor.Execute(map); //                                                  sets wall, exact -- reads ground, jTG

                        EdgesNodesEditor.Execute(map); //                                                 sets edgeOfCorner, bigDrop, exact -- reads leftCorner, rightCorner, air, ground, ladder, bridge, height, exact
                        ConnectCornersEditor.Execute(map.grid, map); //                                   reads leftCorner, rightCorner, air, height
                        NodePropertiesEditor.TurnNullNodesIntoBlocks(map, startingPosition, cellOffset);
                        Draw.GLEnd();
                }

                public static void AddVisualConnection (Pathfinding map, Vector2 position, float size, Color color, int rays)
                {
                        map.connections.Add(new Pathfinding.GridConnections() { position = position, size = size, color = color, rays = rays });
                        Draw.GLCircle(position, size, color, rays);
                }

                public static void AddNodeToParent (PathNode parent, PathNode child, Pathfinding map, bool addToBoth = true)
                {
                        if (parent == null || child == null)
                        {
                                return;
                        }

                        AddNeighbor(parent, map);
                        AppendNeighborList(parent, child, map);

                        if (addToBoth)
                        {
                                AddNeighbor(child, map);
                                AppendNeighborList(child, parent, map);
                        }

                }

                private static void AddNeighbor (PathNode node, Pathfinding map)
                {
                        bool contain = false;
                        for (int i = 0; i < map.neighbor.Count; i++)
                        {
                                if (map.neighbor[i].gridX == node.x && map.neighbor[i].gridY == node.y)
                                {
                                        contain = true;
                                        break;
                                }
                        }

                        if (!contain)
                        {
                                NeighborList neighbor = new NeighborList();
                                neighbor.gridX = node.x;
                                neighbor.gridY = node.y;
                                map.neighbor.Add(neighbor);
                        }
                }

                private static void AppendNeighborList (PathNode parent, PathNode child, Pathfinding map)
                {
                        for (int i = 0; i < map.neighbor.Count; i++)
                        {
                                if (map.neighbor[i].gridX == parent.x && map.neighbor[i].gridY == parent.y)
                                {
                                        map.neighbor[i].neighbor.Add(child);
                                        break;
                                }
                        }
                }

                public static void CreateSecondDropPoint (Pathfinding map, PathNode dropNode, PathNode cornerNode, int direction)
                {
                        PathNode newDropNode = map.Node(dropNode.x + direction, dropNode.y); // let's shift drop node so that ai has more room to jump
                        if (newDropNode != null && newDropNode.ground)
                        {
                                for (int searchUp = 1; searchUp < map.maxJumpHeight; searchUp++)
                                {
                                        PathNode airNode = map.Node(newDropNode.x, newDropNode.y + searchUp);
                                        if (airNode == null || airNode.ground)
                                        {
                                                return;
                                        }
                                }
                                AddNodeToParent(newDropNode, cornerNode, map);
                                AddVisualConnection(map, newDropNode.position, map.cellSize / 8f, Color.green, 1);
                        }
                }
        }

}
#endif
