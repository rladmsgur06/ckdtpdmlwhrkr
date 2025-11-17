#if UNITY_6000_0_OR_NEWER
using NativeMultiHashMap = Unity.Collections.NativeParallelMultiHashMap<UnityEngine.Vector2Int, UnityEngine.Vector2Int>;
#else
using NativeMultiHashMap = Unity.Collections.NativeMultiHashMap<UnityEngine.Vector2Int, UnityEngine.Vector2Int>;
#endif
using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace TwoBitMachines.FlareEngine
{
        [BurstCompile]
        public struct PathfindingJobRT : IJob
        {
                [ReadOnly] public NativeMultiHashMap jobNeighbors;
                [ReadOnly] public NativeHashMap<Vector2Int, PathNodeStruct> jobGrid;
                [ReadOnly] public int startGridX;
                [ReadOnly] public int startGridY;
                [ReadOnly] public int endGridX;
                [ReadOnly] public int endGridY;

                public NativeList<Vector2Int> result;
                private PathNodeStruct start;
                private PathNodeStruct target;

                public void Execute ()
                {
                        if (!CheckTarget(ref target, endGridX, endGridY))
                        {
                                return;
                        }
                        if (!CheckTarget(ref start, startGridX, startGridY))
                        {
                                return;
                        }

#if UNITY_6000_0_OR_NEWER
int gridCount = jobGrid.Count;
#else
                        int gridCount = jobGrid.Count();
#endif

                        NativeList<Vector2Int> openSet = new NativeList<Vector2Int>(Allocator.Temp);
                        NativeList<Vector2Int> closedSet = new NativeList<Vector2Int>(Allocator.Temp);
                        NativeList<PathNodeStruct> neighbors = new NativeList<PathNodeStruct>(Allocator.Temp);
                        NativeHashMap<Vector2Int, NodeCost> nodeCost = new NativeHashMap<Vector2Int, NodeCost>(gridCount, Allocator.Temp);

                        openSet.Add(start.cell);
                        nodeCost.Add(start.cell, new NodeCost(0, 0, Vector2Int.zero));

                        bool foundPath = false;
                        bool canSearch = false;
                        float nearestDistance = Mathf.Infinity;
                        Vector2Int nearestTarget = Vector2Int.zero;

                        while (openSet.Length > 0)
                        {
                                PathNodeStruct node = jobGrid[openSet[0]];

                                for (int i = 1; i < openSet.Length; i++)
                                {
                                        PathNodeStruct openSetNode = jobGrid[openSet[i]];
                                        if (nodeCost[openSetNode.cell].fCost() > nodeCost[node.cell].fCost())
                                        {
                                                continue;
                                        }
                                        if (nodeCost[openSetNode.cell].hCost < nodeCost[node.cell].hCost)
                                        {
                                                node = openSetNode;
                                        }
                                }

                                for (int i = 0; i < openSet.Length; i++)
                                {
                                        if (openSet[i] == node.cell)
                                        {
                                                openSet.RemoveAtSwapBack(i);
                                                break;
                                        }
                                }

                                closedSet.Add(node.cell);

                                if (node.cell == target.cell)
                                {
                                        foundPath = true;
                                        RetracePath(start.cell, target.cell, nodeCost);
                                        break;
                                }

                                neighbors.Clear();
                                if (jobNeighbors.ContainsKey(node.cell))
                                {
                                        var values = jobNeighbors.GetValuesForKey(node.cell);
                                        while (values.MoveNext())
                                        {
                                                if (jobGrid.TryGetValue(values.Current, out PathNodeStruct connectNode))
                                                {
                                                        neighbors.Add(connectNode);
                                                        if (!nodeCost.ContainsKey(values.Current))
                                                        {
                                                                nodeCost.Add(values.Current, new NodeCost(0, 0, Vector2Int.zero));
                                                        }
                                                }
                                        }
                                }

                                for (int x = -1; x <= 1; x++)
                                {
                                        for (int y = -1; y <= 1; y++)
                                        {
                                                int block = x + y;
                                                if (block != 1 && block != -1) // don't test corners or middle nodes
                                                {
                                                        continue;
                                                }
                                                Vector2Int cell = new Vector2Int(node.x + x, node.y + y);
                                                if (jobGrid.TryGetValue(cell, out PathNodeStruct nextNode) && node.Path() && nextNode.Path())
                                                {
                                                        neighbors.Add(nextNode);
                                                        if (!nodeCost.ContainsKey(nextNode.cell))
                                                        {
                                                                nodeCost.Add(nextNode.cell, new NodeCost(0, 0, Vector2Int.zero));
                                                        }
                                                }
                                        }
                                }

                                for (int i = 0; i < neighbors.Length; i++)
                                {
                                        PathNodeStruct neighbour = jobGrid[neighbors[i].cell];

                                        int moveCostToNeighbour = nodeCost[node.cell].gCost + GetDistance(node.x, node.y, neighbour.x, neighbour.y); // nodeCost[node.gridID].penalty;

                                        if (closedSet.Contains(neighbour.cell) && moveCostToNeighbour >= nodeCost[neighbour.cell].gCost)
                                                continue;

                                        if (moveCostToNeighbour < nodeCost[neighbour.cell].gCost || !openSet.Contains(neighbour.cell))
                                        {
                                                int gCost = moveCostToNeighbour;
                                                int hCost = GetDistance(neighbour.x, neighbour.y, target.x, target.y);
                                                nodeCost[neighbour.cell] = new NodeCost(gCost, hCost, node.cell);

                                                if (hCost < nearestDistance && neighbour.ground)
                                                {
                                                        nearestDistance = hCost;
                                                        nearestTarget = neighbour.cell;
                                                        canSearch = true;
                                                }
                                                if (!openSet.Contains(neighbour.cell))
                                                {
                                                        openSet.Add(neighbour.cell);
                                                }
                                        }
                                }
                        }

                        if (canSearch && !foundPath)
                        {
                                if (start.ground && GetDistance(start.x, start.y, target.x, target.y) < nearestDistance) // star path is actually closer, stay here
                                {
                                        nearestTarget = start.cell;
                                }
                                RetracePath(start.cell, nearestTarget, nodeCost);
                        }

                        openSet.Dispose();
                        closedSet.Dispose();
                        neighbors.Dispose();
                        nodeCost.Dispose();
                }

                public int GetDistance (int cellaX, int cellaY, int cellbX, int cellbY) // don't pass struct since they are being copied
                {
                        int dX = math.abs(cellaX - cellbX);
                        int dY = math.abs(cellaY - cellbY);
                        return (dX + dY); //manhattan heuristic
                }

                public void RetracePath (Vector2Int start, Vector2Int end, NativeHashMap<Vector2Int, NodeCost> nodeCost)
                {
                        result.Clear();
                        Vector2Int currentNode = end;
                        while (currentNode != start)
                        {
                                result.Add(currentNode);
                                currentNode = nodeCost[currentNode].parentV2;
                        }
                        result.Add(start);

                        if (result.Length > 0 && jobGrid[result[0]].edgeDrop)
                        {
                                result.RemoveAt(0); // if final target point is an edgeDrop, remove, since an edgeDrop is just air, ai should't move into air, edge drop useful for detecting ambiguous states and drops
                        }
                }

                public bool CheckTarget (ref PathNodeStruct nodeT, int cellX, int cellY)
                {
                        for (int i = 0; i <= 25; i++)
                        {
                                if (jobGrid.TryGetValue(new Vector2Int(cellX, cellY - i), out PathNodeStruct node))
                                {
                                        nodeT = node;
                                        return true;
                                }
                        }
                        return false;
                }
        }
}


// using Unity.Burst;
// using System.Collections.Generic;
// using Unity.Collections;
// using Unity.Jobs;
// using Unity.Mathematics;
// using UnityEngine;
// using TwoBitMachines.FlareEngine.AI.BlackboardData;

// namespace TwoBitMachines.FlareEngine
// {
//         // [BurstCompile]
//         public class PathfindingJobRT // : IJob
//         {
//                 [System.NonSerialized] private List<PathNode> openSet = new List<PathNode>();
//                 [System.NonSerialized] private List<PathNode> closedSet = new List<PathNode>();
//                 [System.NonSerialized] private List<PathNode> neighbors = new List<PathNode>();
//                 [System.NonSerialized] public List<PathNode> result = new List<PathNode>();
//                 [System.NonSerialized] public Dictionary<PathNode, NodeCostRT> cost = new Dictionary<PathNode, NodeCostRT>();

//                 [System.NonSerialized] public PathNode begin = new PathNode();
//                 [System.NonSerialized] public PathNode goal = new PathNode();

//                 public void Execute (DictionaryGrid grid, PathNode start, PathNode end, int gridX, int gridY)
//                 {
//                         if (start == null)
//                         {
//                                 Debug.Log("Exit c");
//                                 return;
//                         }
//                         begin = start;
//                         goal = end;
//                         if (begin.air && !NodeFound(grid, begin, start.gridX, start.gridY))
//                         {
//                                 Debug.Log("Exit b");
//                                 return;
//                         }
//                         if ((goal == null || goal.air) && !NodeFound(grid, goal, gridX, gridY))
//                         {
//                                 Debug.Log("Exit a");
//                                 return;
//                         }

//                         cost.Clear();
//                         openSet.Clear();
//                         closedSet.Clear();
//                         openSet.Add(begin);


//                         bool foundPath = false;
//                         bool canSearch = false;
//                         float nearestDistance = Mathf.Infinity;
//                         PathNode nearestTarget = null;

//                         while (openSet.Count > 0)
//                         {
//                                 PathNode node = openSet[0];
//                                 if (!cost.ContainsKey(node))
//                                 {
//                                         cost[node] = new NodeCostRT(0, 0, null);
//                                 }

//                                 for (int i = 1; i < openSet.Count; i++)
//                                 {
//                                         PathNode openSetNode = openSet[i];
//                                         if (cost.ContainsKey(openSetNode))
//                                         {
//                                                 if (cost[openSetNode].fCost() <= cost[node].fCost())
//                                                 {
//                                                         if (cost[openSetNode].hCost < cost[node].hCost)
//                                                                 node = openSetNode;
//                                                 }
//                                         }
//                                 }

//                                 openSet.Remove(node);
//                                 closedSet.Add(node);

//                                 if (node == goal)
//                                 {
//                                         foundPath = true;
//                                         RetracePath(begin, goal, cost);
//                                         break;
//                                 }

//                                 neighbors.Clear();
//                                 for (int i = 0; i < node.neighbor.Count; i++)
//                                 {
//                                         if (grid.TryGetValue(node.neighbor[i], out PathNode nextNode)) // checkX >= 0 && checkX < linesX && checkY >= 0 && checkY < linesY)
//                                         {
//                                                 // if (!neighbors.Contains(nextNode))
//                                                 {
//                                                         neighbors.Add(nextNode);
//                                                 }
//                                                 if (!cost.ContainsKey(nextNode))
//                                                         cost[nextNode] = new NodeCostRT(0, 0, null);
//                                         }
//                                 }

//                                 for (int x = -1; x <= 1; x++)
//                                 {
//                                         for (int y = -1; y <= 1; y++)
//                                         {
//                                                 int block = x + y;
//                                                 if (block != 1 && block != -1) // don't test corners or middle nodes
//                                                         continue;
//                                                 Vector2Int checkID = new Vector2Int(node.gridX + x, node.gridY + y);

//                                                 if (grid.TryGetValue(checkID, out PathNode nextNode)) // checkX >= 0 && checkX < linesX && checkY >= 0 && checkY < linesY)
//                                                 {
//                                                         if (node.path && nextNode.path)
//                                                         {
//                                                                 // if (!neighbors.Contains(nextNode))
//                                                                 {
//                                                                         neighbors.Add(nextNode);
//                                                                 }
//                                                                 if (!cost.ContainsKey(nextNode))
//                                                                         cost[nextNode] = new NodeCostRT(0, 0, null);
//                                                         }
//                                                 }
//                                         }
//                                 }

//                                 for (int i = 0; i < neighbors.Count; i++)
//                                 {
//                                         PathNode neighbour = neighbors[i];


//                                         int moveCostToNeighbour = cost[node].gCost + GetDistance(node.gridX, node.gridY, neighbour.gridX, neighbour.gridY); // cost[node.index].penalty;

//                                         if (closedSet.Contains(neighbour) && moveCostToNeighbour >= cost[neighbour].gCost)
//                                                 continue;

//                                         if (!openSet.Contains(neighbour) || moveCostToNeighbour < cost[neighbour].gCost)
//                                         {
//                                                 int gCost = moveCostToNeighbour;
//                                                 int hCost = GetDistance(neighbour.gridX, neighbour.gridY, goal.gridX, goal.gridY);
//                                                 cost[neighbour] = new NodeCostRT(gCost, hCost, node);

//                                                 if (hCost < nearestDistance && neighbour.ground)
//                                                 {
//                                                         nearestDistance = hCost;
//                                                         nearestTarget = neighbour;
//                                                         canSearch = true;
//                                                 }
//                                                 if (!openSet.Contains(neighbour))
//                                                 {
//                                                         openSet.Add(neighbour);
//                                                 }
//                                         }
//                                 }

//                         }

//                         if (canSearch && !foundPath)
//                         {
//                                 // if (begin.ground && GetDistance(begin.gridX, begin.gridY, goal.gridX, goal.gridY) < nearestDistance) // star path is actually closer, stay here
//                                 // {
//                                 //         nearestTarget = begin;
//                                 // }
//                                 // if (nearestTarget != null)
//                                 // {
//                                 //         RetracePath(begin, nearestTarget, cost);
//                                 // }
//                         }
//                 }

//                 public int GetDistance (int grid_a_X, int grid_a_Y, int grid_b_X, int grid_b_Y) // don't pass struct since they are being copied
//                 {
//                         int dX = math.abs(grid_a_X - grid_b_X);
//                         int dY = math.abs(grid_a_Y - grid_b_Y);
//                         return (dX + dY); //manhattan heuristic
//                 }

//                 public void RetracePath (PathNode start, PathNode end, Dictionary<PathNode, NodeCostRT> cost)
//                 {
//                         result.Clear();
//                         PathNode currentNode = end;
//                         if (currentNode == null)
//                         {
//                                 Debug.Log("current node is null");
//                         }
//                         while (currentNode != start && currentNode != null)
//                         {

//                                 result.Add(currentNode);
//                                 Debug.Log("Add Node");
//                                 currentNode = cost[currentNode].parent;
//                         }
//                         //result.Add(start);

//                         if (result.Count > 0 && result[0].edgeOfCorner)
//                         {
//                                 result.RemoveAt(0); // if final target point is an edgeDrop, remove, since an edgeDrop is just air, ai should't move into air, edge drop useful for detecting ambiguous states and drops
//                         }
//                 }

//                 public bool NodeFound (DictionaryGrid grid, PathNode node, int gridX, int gridY)
//                 {
//                         for (int i = 0; i <= 25; i++)
//                         {
//                                 Vector2Int gridID = new Vector2Int(gridX, gridY - i);
//                                 if (grid.TryGetValue(gridID, out PathNode ground)) //(target.gridY - i) >= 0)
//                                 {
//                                         node = ground;
//                                         return true;
//                                 }
//                         }
//                         return false;
//                 }

//         }

//         public struct NodeCostRT
//         {
//                 public int gCost;
//                 public int hCost;
//                 public PathNode parent;
//                 public int fCost () { return gCost + hCost; }

//                 public NodeCostRT (int newGCost, int newHCost, PathNode newParent)
//                 {
//                         gCost = newGCost;
//                         hCost = newHCost;
//                         parent = newParent;
//                 }
//         }
// }
