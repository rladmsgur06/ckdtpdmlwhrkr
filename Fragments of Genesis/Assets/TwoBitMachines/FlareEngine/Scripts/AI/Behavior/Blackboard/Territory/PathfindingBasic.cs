using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace TwoBitMachines.FlareEngine.AI.BlackboardData
{
        [AddComponentMenu("")]
        public class PathfindingBasic : Blackboard
        {
                [SerializeField] public LayerMask layerWorld;
                [SerializeField] public float cellSize = 1f;
                [SerializeField] public int penalty = 10;
                [SerializeField, HideInInspector] public BasicNode[] grid;
                [SerializeField, HideInInspector] public SimpleBounds bounds = new SimpleBounds();
                [SerializeField, HideInInspector] public int linesX;
                [SerializeField, HideInInspector] public int linesY;

                [System.NonSerialized] public NativeArray<PathNodeBasicStruct> jobGrid;
                [System.NonSerialized] private List<BasicNode> openSet = new List<BasicNode>();
                [System.NonSerialized] private HashSet<BasicNode> closedSet = new HashSet<BasicNode>();
                [System.NonSerialized] private List<TargetPathfindingBasic> unit = new List<TargetPathfindingBasic>();
                [System.NonSerialized] public static List<PathfindingBasic> maps = new List<PathfindingBasic>();
                public Vector2 cellYOffset { get; private set; }

                #region ▀▄▀▄▀▄ Editor Variables ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField] public bool showGrid = true;
                [SerializeField, HideInInspector] public bool createPaths = false;
                [SerializeField, HideInInspector] public List<Vector2> visuals = new List<Vector2>();
#pragma warning restore 0414
#endif
                #endregion

                private void Awake ()
                {
                        cellYOffset = cellSize * 0.5f * Vector2.up;
                        bounds.Initialize();
                        InitializeJobSystem();
                }

                public void OnEnable ()
                {
                        if (!maps.Contains(this))
                                maps.Add(this);
                }

                public void OnDisable ()
                {
                        if (maps.Contains(this))
                                maps.Remove(this);
                }

                public static void OccupiedNodes ()
                {
                        for (int i = maps.Count - 1; i >= 0; i--)
                                if (maps[i] != null)
                                        maps[i].SetOccupiedPaths();
                }

                public void RegisterFollower (TargetPathfindingBasic newUnit)
                {
                        if (!unit.Contains(newUnit))
                                unit.Add(newUnit);
                }

                public void OnDestroy ()
                {
                        for (int i = 0; i < unit.Count; i++) //                           Must dispose of all jobs before disposing of jobGrid and jobNeighbors
                                if (unit[i] != null || !unit[i].Equals(null))
                                {
                                        unit[i].DisposeFollower();
                                }
                        if (jobGrid.IsCreated)
                                jobGrid.Dispose();
                }

                public void InitializeJobSystem ()
                {
                        jobGrid = new NativeArray<PathNodeBasicStruct>(grid.Length, Allocator.Persistent);

                        // this may be a costly operation at beginning of scene
                        for (int i = 0; i < grid.Length; i++)
                        {
                                PathNodeBasicStruct node = new PathNodeBasicStruct();
                                BasicNode n = grid[i];
                                node.index = i;
                                node.gridX = n.gridX;
                                node.gridY = n.gridY;
                                node.path = n.path;
                                node.penalty = n.penalty;
                                jobGrid[i] = node;
                        }

                }

                public override bool Contains (Vector2 position)
                {
                        return bounds.Contains(position);
                }

                public BasicNode PositionToNode (Vector2 position)
                {
                        Vector2 gridPosition = (position - bounds.position) / cellSize; // cell size cannot be zero!
                        int x = Mathf.FloorToInt(Mathf.Clamp(Mathf.Abs(gridPosition.x), 0, linesX - 1));
                        int y = Mathf.FloorToInt(Mathf.Clamp(Mathf.Abs(gridPosition.y), 0, linesY - 1));
                        return grid[y * linesX + x];
                }

                public void SetOccupiedPaths ()
                {
                        // for (int i = 0; i < grid.Length; i++) // list of only paths
                        // {
                        //       grid[i].isOccupied = false; // clear old values
                        // }
                        // for (int i = 0; i < unit.Count; i++)
                        // {
                        //       if (unit[i] == null || !unit[i].activeUnit) continue;
                        //       unit[i].OccupyNode ( );
                        // }
                }

                public Vector2 Avoidance (TargetPathfindingBasic u)
                {
                        Vector2 intention = Vector2.zero;
                        for (int i = 0; i < unit.Count; i++)
                        {
                                if (unit[i] == null || !unit[i].activeUnit || unit[i] == u)
                                        continue;

                                Vector2 direction = u.transform.position - unit[i].transform.position;
                                float distance = (u.transform.position - unit[i].transform.position).magnitude;

                                float spring = 1f / (1f + distance * distance * distance);
                                intention -= direction * spring;
                        }
                        if (intention.magnitude < 0.5f)
                                return Vector2.zero;

                        return intention.normalized;
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
                public override void OnSceneGUI (UnityEditor.Editor editor)
                {
                        if (this.bounds.position == Vector2.zero)
                        {
                                this.bounds.position = SceneTools.SceneCenter(this.transform.position);
                        }
                        SceneTools.DrawAndModifyBounds(ref this.bounds.position, ref this.bounds.size, Color.green);

                        float snapSize = cellSize > 1 ? cellSize * 0.25f : cellSize;
                        bounds.position = Compute.Round(bounds.position, snapSize);
                        bounds.size = Compute.Round(bounds.size, cellSize);

                        if (showGrid)
                        {
                                SceneTools.TwoDGrid(bounds.position, bounds.size, new Vector2(cellSize, cellSize), new Color32(4, 184, 236, 50));
                        }
                        Draw.GLStart();
                        for (int i = 0; i < visuals.Count; i++)
                        {
                                Draw.GLCircle(visuals[i], 0.5f, Color.red);
                        }
                        Draw.GLEnd();

                        if (!createPaths)
                                return;
                        createPaths = false;
                        visuals.Clear();
                        Vector2 gridSize = bounds.size;
                        linesX = Mathf.RoundToInt(gridSize.x / cellSize);
                        linesY = Mathf.RoundToInt(gridSize.y / cellSize);
                        grid = new BasicNode[linesX * linesY];
                        Vector2 cellOffset = new Vector2(cellSize * 0.5f, cellSize * 0.5f);
                        Execute(bounds.position, cellOffset);
                }

                public void Execute (Vector2 startingPosition, Vector2 cellOffset)
                {
                        for (int x = 0; x < linesX; x++)
                        {
                                Vector2 nodeBasePosition = startingPosition + cellOffset + Vector2.right * cellSize * x;
                                for (int y = 0; y < linesY; y++)
                                {
                                        Vector2 nodePosition = nodeBasePosition + Vector2.up * cellSize * y;
                                        if (NodeIsAir(nodePosition))
                                        {
                                                grid[y * linesX + x] = new BasicNode() { position = nodePosition, gridX = x, gridY = y, path = true };
                                        }
                                        else
                                        {
                                                grid[y * linesX + x] = new BasicNode() { position = nodePosition, gridX = x, gridY = y, path = false };
                                                visuals.Add(nodePosition);
                                        }
                                }
                        }
                        for (int x = 0; x < linesX; x++)
                                for (int y = 0; y < linesY; y++)
                                {
                                        BasicNode node = grid[y * linesX + x];
                                        if (!node.path)
                                                Penalty(x, y);
                                }
                }

                private void Penalty (int baseX, int baseY)
                {
                        if (penalty == 0)
                                return;

                        for (int x = -1; x <= 1; x++)
                                for (int y = -1; y <= 1; y++)
                                {
                                        int targetX = baseX + x;
                                        int targetY = baseY + y;
                                        if (targetX >= 0 && targetX < linesX && targetY >= 0 && targetY < linesY)
                                        {
                                                BasicNode node = grid[targetY * linesX + targetX];
                                                if (NodeIsAir(node.position))
                                                {
                                                        node.penalty = penalty;
                                                }
                                        }
                                }
                }

                private bool NodeIsAir (Vector2 nodePosition)
                {
                        return !Physics2D.OverlapPoint(nodePosition, layerWorld);
                }

#endif
                #endregion

        }

        [System.Serializable]
        public class BasicNode
        {
                public int gridX = 0;
                public int gridY = 0;
                public int penalty = 0;
                public Vector2 position;
                public bool path = false;
                public bool isOccupied = false;
                [System.NonSerialized] public TargetPathfindingBasic unit;

                public int gCost;
                public int hCost;
                public int fCost => gCost + hCost;
                [System.NonSerialized] public BasicNode parent;

                public bool Same (BasicNode other)
                {
                        return gridX == other.gridX && gridY == other.gridY;
                }

        }

        public struct PathNodeBasicStruct
        {
                public int index;
                public int gridX;
                public int gridY;
                public int penalty;
                public bool path;

                public bool Same (PathNodeBasicStruct other)
                {
                        return gridX == other.gridX && gridY == other.gridY;
                }

        }

        [BurstCompile]
        public struct PathfindingBasicJob : IJob
        {
                [ReadOnly] public NativeArray<PathNodeBasicStruct> jobGrid;
                [ReadOnly] public int linesX;
                [ReadOnly] public int linesY;
                public PathNodeBasicStruct start;
                public PathNodeBasicStruct target;

                public NativeList<Vector2> targets;
                public Vector2 position;
                public float cellSize;

                public void Execute ()
                {

                        NativeList<int> openSet = new NativeList<int>(Allocator.Temp);
                        NativeList<int> closedSet = new NativeList<int>(Allocator.Temp);

                        NativeList<PathNodeBasicStruct> neighbors = new NativeList<PathNodeBasicStruct>(Allocator.Temp);
                        NativeHashMap<int, NodeCost> nodeCost = new NativeHashMap<int, NodeCost>(jobGrid.Length, Allocator.Temp);

                        openSet.Add(start.index);
                        nodeCost.Add(start.index, new NodeCost(0, 0, 0));

                        while (openSet.Length > 0)
                        {
                                PathNodeBasicStruct node = jobGrid[openSet[0]]; //

                                for (int i = 1; i < openSet.Length; i++)
                                {
                                        PathNodeBasicStruct openSetNode = jobGrid[openSet[i]];
                                        if (nodeCost[openSetNode.index].fCost() <= nodeCost[node.index].fCost())
                                        {
                                                if (nodeCost[openSetNode.index].hCost < nodeCost[node.index].hCost)
                                                        node = openSetNode;
                                        }
                                }

                                for (int i = 0; i < openSet.Length; i++)
                                        if (openSet[i] == node.index)
                                        {
                                                openSet.RemoveAtSwapBack(i);
                                                break;
                                        }

                                closedSet.Add(node.index);

                                if (node.index == target.index)
                                {
                                        RetracePath(start.index, target.index, nodeCost);
                                        break;
                                }

                                neighbors.Clear();
                                for (int x = -1; x <= 1; x++)
                                        for (int y = -1; y <= 1; y++)
                                        {
                                                if (x == 0 && y == 0)
                                                        continue;

                                                int checkX = node.gridX + x;
                                                int checkY = node.gridY + y;

                                                if (checkX >= 0 && checkX < linesX && checkY >= 0 && checkY < linesY)
                                                {
                                                        PathNodeBasicStruct nextNode = GetNode(checkX, checkY);
                                                        if (nextNode.path)
                                                        {
                                                                neighbors.Add(nextNode);
                                                                if (!nodeCost.ContainsKey(nextNode.index))
                                                                {
                                                                        nodeCost.Add(nextNode.index, new NodeCost(0, 0, 0));
                                                                }
                                                        }
                                                }
                                        }

                                for (int i = 0; i < neighbors.Length; i++) //
                                {
                                        PathNodeBasicStruct neighbour = jobGrid[neighbors[i].index];

                                        int moveCostToNeighbour = nodeCost[node.index].gCost + GetDistance(node, neighbour) + neighbour.penalty;

                                        if (closedSet.Contains(neighbour.index) && moveCostToNeighbour >= nodeCost[neighbour.index].gCost)
                                                continue;

                                        if (moveCostToNeighbour < nodeCost[neighbour.index].gCost || !openSet.Contains(neighbour.index))
                                        {
                                                int gCost = moveCostToNeighbour;
                                                int hCost = GetDistance(neighbour, target);
                                                nodeCost[neighbour.index] = new NodeCost(gCost, hCost, node.index);

                                                if (!openSet.Contains(neighbour.index))
                                                        openSet.Add(neighbour.index);
                                        }
                                }
                        }

                        openSet.Dispose();
                        closedSet.Dispose();
                        neighbors.Dispose();
                        nodeCost.Dispose();
                }

                public int GetDistance (PathNodeBasicStruct nodeA, PathNodeBasicStruct nodeB)
                {
                        int dX = math.abs(nodeA.gridX - nodeB.gridX);
                        int dY = math.abs(nodeA.gridY - nodeB.gridY);
                        return dX + dY; //manhattan heuristic
                }

                public PathNodeBasicStruct GetNode (int x, int y)
                {
                        return jobGrid[y * linesX + x];
                }

                public Vector2 NodeToPosition (Vector2 position, float cellSize, int index)
                {
                        PathNodeBasicStruct node = jobGrid[index];
                        Vector2 gridPosition = new Vector2(node.gridX * cellSize + cellSize * 0.5f, node.gridY * cellSize + cellSize * 0.5f);
                        return position + gridPosition;
                }

                public void RetracePath (int start, int end, NativeHashMap<int, NodeCost> nodeCost)
                {
                        targets.Clear();
                        int currentNode = end;
                        while (currentNode != start)
                        {
                                targets.Add(NodeToPosition(position, cellSize, currentNode));
                                currentNode = nodeCost[currentNode].parent;
                        }
                        SmoothCorners();
                }

                public void SmoothCorners ()
                {
                        NativeList<Vector2> curvedPoints = new NativeList<Vector2>(Allocator.Temp);

                        for (int i = targets.Length - 1; i >= 2; i--)
                        {
                                Vector2 a = targets[i];
                                Vector2 b = targets[i - 1];
                                Vector2 c = targets[i - 2];

                                if (!Collinear(a.x, a.y, b.x, b.y, c.x, c.y)) // not on same line, this is a corner
                                {
                                        GetCurvedPoints(a, b, c, out Vector2 newA, out Vector2 newB, out Vector2 newC);
                                        curvedPoints.Add(newA);
                                        curvedPoints.Add(newB);
                                        curvedPoints.Add(newC);

                                        bool dExists = (i - 3) >= 0 && (i - 3) < targets.Length;
                                        Vector2 d = dExists ? targets[i - 3] : Vector2.zero;

                                        targets.RemoveAt(i - 1); // this is a corner, remove it
                                        i--;

                                        if (dExists && !Collinear(b.x, b.y, c.x, c.y, d.x, d.y))
                                        {
                                                GetCurvedPoints(b, c, d, out Vector2 newNone, out Vector2 newD, out Vector2 newE);
                                                curvedPoints.Add(newD);
                                                curvedPoints.Add(newE);
                                                targets.RemoveAt(i - 1); // this is a corner, remove it
                                                i--;
                                        }
                                }
                                else
                                {
                                        curvedPoints.Add(a); // points are on the same line, skip, we only want corners
                                }
                        }

                        targets.Clear();
                        for (int i = curvedPoints.Length - 1; i >= 0; i--)
                                targets.Add(curvedPoints[i]);

                        curvedPoints.Dispose();
                }

                public bool Collinear (float x1, float y1, float x2, float y2, float x3, float y3)
                {
                        float slope1 = (y1 - y2) * (x1 - x3);
                        float slope2 = (y1 - y3) * (x1 - x2);
                        return math.abs(slope1 - slope2) < 0.0001f; // * (x1 - x3) == (y1 - y3) * (x1 - x2);
                }

                public void GetCurvedPoints (Vector2 a, Vector2 b, Vector2 c, out Vector2 newA, out Vector2 newB, out Vector2 newC)
                {
                        Vector2 directionA = (b - a) * 0.5f;
                        Vector2 directionC = (b - c) * 0.5f;
                        newA = a + directionA;
                        newC = c + directionC;
                        Vector2 directionB = (newA - newC) * 0.5f;
                        newB = (newC + directionB);
                        Vector2 offset = (b - newB) * 0.5f;
                        newB += offset;
                }
        }

}
