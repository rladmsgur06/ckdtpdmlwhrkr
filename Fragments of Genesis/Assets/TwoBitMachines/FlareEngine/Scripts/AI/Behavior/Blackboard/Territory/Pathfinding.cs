#if UNITY_EDITOR
using TwoBitMachines.Editors;
using UnityEditor;
#endif
#if UNITY_6000_0_OR_NEWER
using NativeMultiHashMap = Unity.Collections.NativeParallelMultiHashMap<int, int>;
#else
using NativeMultiHashMap = Unity.Collections.NativeMultiHashMap<int, int>;
#endif
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace TwoBitMachines.FlareEngine.AI.BlackboardData
{
        [AddComponentMenu("")]
        public partial class Pathfinding : Blackboard
        {
                [SerializeField] public LayerMask layerWorld;
                [SerializeField] public float maxJumpHeight = 4f;
                [SerializeField] public float maxJumpDistance = 4f;
                [SerializeField] public float cellSize = 1f;

                [SerializeField, HideInInspector] public List<Vector2Int> ladder = new List<Vector2Int>();
                [SerializeField, HideInInspector] public List<Vector2Int> ceiling = new List<Vector2Int>();
                [SerializeField, HideInInspector] public List<Vector2Int> moving = new List<Vector2Int>();
                [SerializeField, HideInInspector] public List<Vector2Int> bridge = new List<Vector2Int>();
                [SerializeField, HideInInspector] public List<Vector2Int> wall = new List<Vector2Int>();
                [SerializeField, HideInInspector] public List<Vector2Int> fall = new List<Vector2Int>();
                [SerializeField, HideInInspector] public SimpleBounds bounds = new SimpleBounds();
                [SerializeField, HideInInspector] public int linesX = 0;
                [SerializeField, HideInInspector] public int linesY = 0;
                [SerializeField, HideInInspector] public PathNode[] grid;
                [SerializeField, HideInInspector] public List<NeighborList> neighbor; // this list should actually exist inside PathNode, but the inspector crashed every time this AIAction get's instantiated

                [SerializeField, HideInInspector] public NativeArray<PathNodeStruct> jobGrid;
                [SerializeField, HideInInspector] public NativeMultiHashMap jobNeighbors;
                [SerializeField, HideInInspector] private List<TargetPathfindingBase> unit = new List<TargetPathfindingBase>();

                [System.NonSerialized] public static List<Pathfinding> maps = new List<Pathfinding>();

                public Vector2 cellYOffset { get; private set; }
                public Vector2 cellXOffset { get; private set; }

                #region ▀▄▀▄▀▄ Editor Variables ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField] public bool showPaths = true;
                [SerializeField] public bool addLadders = false;
                [SerializeField] public bool addWalls = false;
                [SerializeField] public bool addCeilings = false;
                //[SerializeField] public bool addMoving = false; // moving platforms
                // [SerializeField] public bool enableFall = false;
                [SerializeField] public bool addBridge = false;
                [SerializeField, HideInInspector] public bool createPaths = false;
                [SerializeField, HideInInspector] public List<GridConnections> connections = new List<GridConnections>();

                [System.Serializable]
                public struct GridConnections
                {
                        public Vector2 position;
                        public Color color;
                        public float size;
                        public int rays;

                }
#pragma warning restore 0414
#endif
                #endregion

                #region Setup
                public void Awake ()
                {
                        cellYOffset = cellSize * 0.5f * Vector2.up;
                        cellXOffset = cellSize * 0.5f * Vector2.right;
                        bounds.Initialize();
                        InitializeJobSystem();
                }

                public void OnEnable ()
                {
                        if (!maps.Contains(this))
                        {
                                maps.Add(this);
                        }
                }

                public void OnDisable ()
                {
                        if (maps.Contains(this))
                        {
                                maps.Remove(this);
                        }
                }

                public void RegisterFollower (TargetPathfindingBase newUnit)
                {
                        if (!unit.Contains(newUnit))
                        {
                                unit.Add(newUnit);
                        }
                }

                public override bool Contains (Vector2 position)
                {
                        return bounds.Contains(position);
                }

                private void OnDestroy ()
                {
                        for (int i = 0; i < unit.Count; i++) //                     Must dispose of all jobs before disposing of jobGrid and jobNeighbors
                                if (unit[i] != null || !unit[i].Equals(null))
                                {
                                        unit[i].DisposeFollower();
                                }
                        if (jobGrid.IsCreated)
                                jobGrid.Dispose();
                        if (jobNeighbors.IsCreated)
                                jobNeighbors.Dispose();
                }

                public static void OccupiedNodes ()
                {
                        for (int i = maps.Count - 1; i >= 0; i--)
                        {
                                if (maps[i] != null)
                                {
                                        maps[i].SetOccupiedPaths();
                                }
                        }
                }

                public void InitializeJobSystem ()
                {
                        jobGrid = new NativeArray<PathNodeStruct>(grid.Length, Allocator.Persistent);
                        List<PathNode> listN = new List<PathNode>();
                        // this is a costly operation
                        for (int i = 0; i < grid.Length; i++)
                        {
                                PathNodeStruct node = new PathNodeStruct();
                                PathNode n = grid[i];
                                node.moving = n.moving;
                                node.jumpThroughGround = n.jumpThroughGround;
                                node.rightCorner = n.rightCorner;
                                node.leftCorner = n.leftCorner;
                                node.wall = n.wall;
                                node.edgeDrop = n.edgeOfCorner;
                                node.ceiling = n.ceiling;
                                node.ground = n.ground;
                                node.height = n.height;
                                node.ladder = n.ladder;
                                node.bridge = n.bridge;
                                node.block = n.block;
                                node.exact = n.exact;
                                node.air = n.air;
                                node.x = n.x;
                                node.y = n.y;
                                node.index = i;
                                jobGrid[i] = node;
                        }

                        jobNeighbors = new NativeMultiHashMap(listN.Count, Allocator.Persistent);

                        for (int i = 0; i < neighbor.Count; i++)
                                for (int j = 0; j < neighbor[i].neighbor.Count; j++)
                                {
                                        int index = neighbor[i].gridX + neighbor[i].gridY * linesX;
                                        jobNeighbors.Add(index, (int) neighbor[i].neighbor[j].x + (int) neighbor[i].neighbor[j].y * linesX);
                                }
                }
                #endregion

                public PathNode PositionToNode (Vector2 position)
                {
                        Vector2 gridPosition = (position - bounds.position) / cellSize; // cell size cannot be zero!
                        int x = Mathf.FloorToInt(Mathf.Clamp(Mathf.Abs(gridPosition.x), 0, linesX - 1));
                        int y = Mathf.FloorToInt(Mathf.Clamp(Mathf.Abs(gridPosition.y + 0.1f), 0, linesY - 1));
                        return grid[y * linesX + x];
                }

                public PathNode Node (int x, int y)
                {
                        return grid[Mathf.Clamp(y, 0, linesY - 1) * linesX + Mathf.Clamp(x, 0, linesX - 1)];
                }

                public void SetOccupiedPaths () // Occupied by units
                {
                        for (int i = 0; i < grid.Length; i++)
                        {
                                grid[i].isOccupied = false; // clear old values
                        }
                        for (int i = 0; i < unit.Count; i++)
                        {
                                if (unit[i] != null && unit[i].activeUnit)
                                {
                                        unit[i].OccupyNode();
                                }
                        }
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

                        if (showPaths)
                        {
                                SceneTools.TwoDGrid(bounds.position, bounds.size, new Vector2(cellSize, cellSize), new Color32(4, 184, 236, 50));
                                PathExtras(addLadders, ladder, Color.yellow, cellSize);
                                PathExtras(addWalls, wall, Color.black, cellSize);
                                PathExtras(addCeilings, ceiling, Color.blue, cellSize);
                                //PathExtras (addMoving, moving, Color.red, cellSize);
                                PathExtras(addBridge, bridge, Tint.DarkOrange, cellSize);
                                // PathExtras (enableFall, fall, Tint.Orange, cellSize, true);
                                if (addLadders || addWalls || addCeilings || addBridge)
                                {
                                        if (Event.current.type == EventType.Layout)
                                                UnityEditor.HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
                                }
                        }
                        AIPathfindingEditor.CreateGrid(this, showPaths);
                }

                public override void DrawWhenNotSelected ()
                {
                        if (showPaths)
                        {
                                SceneTools.TwoDGrid(bounds.position, bounds.size, new Vector2(cellSize, cellSize), new Color32(4, 184, 236, 50));
                                ShowExtras(ladder, Color.yellow, cellSize);
                                ShowExtras(wall, Color.black, cellSize);
                                ShowExtras(ceiling, Color.blue, cellSize);
                                ShowExtras(bridge, Tint.DarkOrange, cellSize);
                                ShowExtras(fall, Tint.Orange, cellSize);
                        }
                        AIPathfindingEditor.DisplayMapAfterEditing(this, showPaths);
                }

                private void PathExtras (bool edit, List<Vector2Int> list, Color color, float cellSize, bool isFall = false)
                {
                        if (edit && grid.Length > 0 && linesX != 0 && linesY != 0)
                        {
                                PathNode node = PositionToNode(SceneTools.MousePosition());
                                if (node != null)
                                {
                                        Draw.GLCircleInit(node.position, cellSize / 1.97f, color, 5);
                                        if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
                                        {
                                                Vector2Int index = new Vector2Int(node.x, node.y);
                                                if (list.Contains(index))
                                                {
                                                        list.Remove(index);
                                                        // if (isFall) node.isFall = false;
                                                }
                                                else
                                                {
                                                        list.Add(index);
                                                        // if (isFall) node.isFall = true;
                                                }
                                        }
                                }
                        }
                }

                private void ShowExtras (List<Vector2Int> list, Color color, float cellSize)
                {
                        Draw.GLStart();
                        for (int i = list.Count - 1; i >= 0; i--)
                        {
                                int index = list[i].y * linesX + list[i].x;
                                if (index < grid.Length && index >= 0)
                                {
                                        Draw.GLCircle(grid[index].position, cellSize / 1.8f, color, 5);
                                }
                                else
                                {
                                        list.RemoveAt(i);
                                }
                        }
                        Draw.GLEnd();
                }

                [DrawGizmo(GizmoType.NotInSelectionHierarchy | GizmoType.InSelectionHierarchy)]
                static void DrawWhenObjectIsNotSelected (Pathfinding pathfinding, GizmoType gizmoType)
                {
                        pathfinding.DrawWhenNotSelected();
                }

#endif
                #endregion

        }

        [System.Serializable]
        public class NeighborList
        {
                public int gridX = -1;
                public int gridY = -1;
                public Vector2Int gridID => new Vector2Int(gridX, gridY);
                [SerializeField] public List<PathNode> neighbor = new List<PathNode>();

        }

}
