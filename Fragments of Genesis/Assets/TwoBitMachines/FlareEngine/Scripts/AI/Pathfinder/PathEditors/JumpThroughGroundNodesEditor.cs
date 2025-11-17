#if UNITY_EDITOR
using TwoBitMachines.FlareEngine;
using TwoBitMachines.FlareEngine.AI.BlackboardData;
using UnityEngine;

namespace TwoBitMachines.Editors
{
        public class JumpThroughGroundNodesEditor : AIPathfindingEditor
        {
                public static void Execute (Pathfinding map, Vector2 cellOffset)
                {
                        for (int x = 0; x < map.linesX; x++)
                                for (int y = 0; y < map.linesY; y++)
                                {
                                        PathNode tempNode = map.Node(x, y);
                                        if (tempNode == null || !tempNode.ground)
                                        {
                                                continue;
                                        }
                                        SearchForGroundNodes(tempNode, map, cellOffset);
                                }

                        for (int x = 0; x < map.linesX; x++)
                                for (int y = 0; y < map.linesY; y++)
                                {
                                        PathNode tempNode = map.Node(x, y);
                                        if (tempNode == null || !tempNode.jumpThroughGround)
                                        {
                                                continue;
                                        }
                                        ConnectJumpThroughNodes(tempNode, map, x, y);
                                }
                }

                private static void SearchForGroundNodes (PathNode tempNode, Pathfinding map, Vector2 offset)
                {
                        // Since ground edges are jump through, platforms can be stacked, which means we have 
                        // to search an x (arbitrary) amount of times to find possible jump through platforms
                        Vector2 origin = tempNode.position;
                        for (int searchUp = 0; searchUp < 100; searchUp++) // arbitrary check limit
                        {
                                RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.up, map.linesY, map.layerWorld);
                                if (!hit || !(hit.collider is EdgeCollider2D))
                                {
                                        return; //no Edge Collider found, exit out
                                }
                                origin = hit.point + Vector2.up * 0.01f; // update origin to test above edge incase edges are stacked
                                PathNode jumpTroughNode = map.PositionToNode(hit.point + Vector2.up * offset.y);
                                if (jumpTroughNode != null) // create jump trough node
                                {
                                        jumpTroughNode.jumpThroughGround = true;
                                        jumpTroughNode.ground = true;
                                        jumpTroughNode.air = false;
                                        AddVisualConnection(map, jumpTroughNode.position, map.cellSize / 4f, Color.green, 1);
                                }
                        }
                }

                private static void ConnectJumpThroughNodes (PathNode tempNode, Pathfinding map, int x, int y)
                {
                        for (int searchDown = 1; searchDown < map.maxJumpHeight; searchDown++)
                        {
                                PathNode lowerPlatform = map.Node(x, y - searchDown);
                                if (lowerPlatform == null)
                                {
                                        return;
                                }

                                if (lowerPlatform.ground || lowerPlatform.jumpThroughGround)
                                {
                                        AddNodeToParent(lowerPlatform, tempNode, map, false);
                                        ShowVisualConnections(map, x, y);
                                        return;
                                }
                        }
                }

                private static void ShowVisualConnections (Pathfinding map, int x, int y)
                {
                        for (int searchDown = 1; searchDown < map.maxJumpHeight; searchDown++)
                        {
                                PathNode connections = map.Node(x, y - searchDown);
                                if (connections == null || connections.ground || connections.jumpThroughGround)
                                {
                                        return;
                                }
                                AddVisualConnection(map, connections.position, map.cellSize / 4f, Color.cyan, 1);
                        }
                }

        }
}
#endif
