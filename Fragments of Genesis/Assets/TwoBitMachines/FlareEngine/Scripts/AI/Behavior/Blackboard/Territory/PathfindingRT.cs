#if UNITY_EDITOR
using TwoBitMachines.Editors;
using UnityEditor;
#endif
#if UNITY_6000_0_OR_NEWER
using NativeMultiHashMap = Unity.Collections.NativeParallelMultiHashMap<UnityEngine.Vector2Int, UnityEngine.Vector2Int>;
#else
using NativeMultiHashMap = Unity.Collections.NativeMultiHashMap<UnityEngine.Vector2Int, UnityEngine.Vector2Int>;
#endif
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace TwoBitMachines.FlareEngine.AI.BlackboardData
{
        [AddComponentMenu("")]
        public class PathfindingRT : Blackboard
        {
                [SerializeField, HideInInspector] public float cellSize = 1f;
                [SerializeField] public Vector2 maxJump = new Vector2(4f, 4f);
                [SerializeField] public AddExtras addExtra;
                [SerializeField] public List<Tilemap> tilemaps = new List<Tilemap>();

                [SerializeField, HideInInspector] public bool createPaths;
                [SerializeField, HideInInspector] public Vector2 cellOffset;
                [SerializeField, HideInInspector] public LayerMask layerWorld;
                [SerializeField, HideInInspector] public PathNode air = new PathNode();
                [SerializeField, HideInInspector] public DictionaryMap map = new DictionaryMap();
                [SerializeField, HideInInspector] public List<Vector2Int> wall = new List<Vector2Int>();
                [SerializeField, HideInInspector] public List<Vector2Int> ladder = new List<Vector2Int>();
                [SerializeField, HideInInspector] public List<Vector2Int> bridge = new List<Vector2Int>();
                [SerializeField, HideInInspector] public List<Vector2Int> ceiling = new List<Vector2Int>();
                [SerializeField, HideInInspector] public DictionaryNeighbor neighbors = new DictionaryNeighbor();

                [System.NonSerialized] private List<Tilemap> queueTempTilemap = new List<Tilemap>();
                [System.NonSerialized] private List<Tilemap> tempTilemap = new List<Tilemap>();
                [System.NonSerialized] private DictionaryMap tempMap = new DictionaryMap();
                [System.NonSerialized] private DictionaryNeighbor tempNeighbors = new DictionaryNeighbor();

                [System.NonSerialized] public NativeHashMap<Vector2Int, PathNodeStruct> jobPath;
                [System.NonSerialized] public NativeMultiHashMap jobNeighbors;
                [System.NonSerialized] public List<TargetPathfindingBase> unit = new List<TargetPathfindingBase>();
                [System.NonSerialized] public static List<PathfindingRT> maps = new List<PathfindingRT>();

                public bool unitsCanOccupy { get; private set; }
                public bool isCreatingMap { get; private set; }
                public Vector2 cellYOffset { get; private set; }
                public Vector2 cellXOffset { get; private set; }

                private void Awake ()
                {
                        air.air = true;
                        isCreatingMap = false;
                        cellYOffset = cellSize * 0.5f * Vector2.up;
                        cellXOffset = cellSize * 0.5f * Vector2.right;

                        jobPath = new NativeHashMap<Vector2Int, PathNodeStruct>(map.Count, Allocator.Persistent);
                        jobNeighbors = new NativeMultiHashMap(map.Count, Allocator.Persistent);
                        CreateJobSystem();
                }

                private void OnEnable ()
                {
                        if (!maps.Contains(this))
                        {
                                maps.Add(this);
                        }
                }

                private void OnDisable ()
                {
                        if (maps.Contains(this))
                        {
                                maps.Remove(this);
                        }
                }

                private void OnDestroy ()
                {
                        for (int i = 0; i < unit.Count; i++) //                     Must dispose of all jobs before disposing of jobGrid and jobNeighbors
                        {
                                if (unit[i] != null || !unit[i].Equals(null))
                                {
                                        unit[i].DisposeFollower();
                                }
                        }
                        if (jobPath.IsCreated)
                        {
                                jobPath.Dispose();
                        }
                        if (jobNeighbors.IsCreated)
                        {
                                jobNeighbors.Dispose();
                        }
                }

                public void RegisterFollower (TargetPathfindingBase newUnit, bool canBlock)
                {
                        if (!unit.Contains(newUnit))
                        {
                                unit.Add(newUnit);
                                if (canBlock)
                                {
                                        unitsCanOccupy = true;
                                }
                        }
                }

                public static void OccupiedNodes ()
                {
                        for (int i = 0; i < maps.Count; i++)
                        {
                                if (maps[i] != null)
                                {
                                        maps[i].SetOccupiedPaths();
                                }
                        }
                }

                public void SetOccupiedPaths () // Occupied by units
                {
                        if (!unitsCanOccupy || isCreatingMap)
                        {
                                return;
                        }
                        foreach (var node in map)
                        {
                                node.Value.isOccupied = false;
                        }
                        for (int i = 0; i < unit.Count; i++)
                        {
                                if (unit[i] != null && unit[i].activeUnit)
                                {
                                        unit[i].OccupyNode();
                                }
                        }
                }

                public PathNode PositionToNode (Vector2 position)
                {
                        int x = Mathf.FloorToInt(position.x);
                        int y = Mathf.FloorToInt(position.y);
                        if (map.TryGetValue(new Vector2Int(x, y), out PathNode node))
                        {
                                return node;
                        }
                        return null;
                }

                public PathNode PositionFindNode (Vector2 position)
                {
                        int x = Mathf.FloorToInt(position.x);
                        int y = Mathf.FloorToInt(position.y);
                        if (map.TryGetValue(new Vector2Int(x, y), out PathNode node))
                        {
                                return node;
                        }
                        air.x = x;
                        air.y = y;
                        air.position = new Vector2(x, y) + cellOffset;
                        return air;
                }

                public void GridPosition (Vector2 position, out int x, out int y)
                {
                        x = Mathf.FloorToInt(position.x);
                        y = Mathf.FloorToInt(position.y);
                }

                public bool Node (int x, int y, out PathNode node)
                {
                        return map.TryGetValue(new Vector2Int(x, y), out node);
                }

                public bool Contains (int x, int y)
                {
                        return map.ContainsKey(new Vector2Int(x, y));
                }

                public void AddTilemaps (Tilemap tilemap)
                {
                        tempTilemap.Clear();
                        tempTilemap.Add(tilemap);
                        AddTilemaps(tempTilemap);
                }

                public void AddTilemaps (List<Tilemap> tilemaps)
                {
                        if (isCreatingMap)
                        {
                                for (int i = 0; i < tilemaps.Count; i++)
                                {
                                        if (!queueTempTilemap.Contains(tilemaps[i]))
                                        {
                                                queueTempTilemap.Add(tilemaps[i]);
                                        }
                                }
                                return;
                        }

                        layerWorld = 1 << LayerMask.NameToLayer("World");
                        cellSize = 1f;// cellSize <= 0 ? 1f : Mathf.Clamp(cellSize, 0.1f, 100f);
                        cellOffset = new Vector2(cellSize * 0.5f, cellSize * 0.5f);
                        CreateMap(tilemaps);

                        if (Application.isPlaying)
                        {
                                StartCoroutine(ExpandMap());
                        }
                }

                private void CreateMap (List<Tilemap> tilemaps)
                {
                        CreateRTNodes.Execute(this, tilemaps);
                        AddRTLadder.Execute(this);
                        AddRTCeiling.Execute(this);
                        AddRTBridge.Execute(this);
                        AddRTWall.Execute(this);
                        AddRTDrop.Execute(this);
                        ConnectJumpNodes.Execute(this);
                }

                private IEnumerator ExpandMap ()
                {
                        // DebugTimer.Start();
                        isCreatingMap = true;
                        for (int i = 0; i < unit.Count; i++)
                        {
                                if (unit[i] != null && !unit[i].JobIsComplete())
                                {
                                        yield return null;
                                }
                        }

                        CreateJobSystem(checkForDuplicates: true);

                        if (queueTempTilemap.Count > 0)
                        {
                                for (int i = 0; i < queueTempTilemap.Count; i++)
                                {
                                        tempTilemap.Add(queueTempTilemap[i]);
                                }
                                queueTempTilemap.Clear();
                                AddTilemaps(tempTilemap);
                        }
                        //  DebugTimer.Stop("Map exapanded: ");
                }

                private void CreateJobSystem (bool checkForDuplicates = false)
                {
                        isCreatingMap = true;
                        foreach (var element in map)
                        {
                                PathNode node = element.Value;

                                if (checkForDuplicates && jobNeighbors.ContainsKey(node.cell))
                                {
                                        continue;
                                }

                                PathNodeStruct nodeStruct = new PathNodeStruct();
                                nodeStruct.edgeDrop = node.edgeOfCorner;
                                nodeStruct.ceiling = node.ceiling;
                                nodeStruct.ground = node.ground;
                                nodeStruct.ladder = node.ladder;
                                nodeStruct.bridge = node.bridge;
                                nodeStruct.wall = node.wall;
                                nodeStruct.cell = node.cell;
                                nodeStruct.x = node.x;
                                nodeStruct.y = node.y;
                                jobPath[node.cell] = nodeStruct;

                                for (int j = 0; j < node.neighbor.Count; j++)
                                {
                                        jobNeighbors.Add(node.cell, node.neighbor[j]);
                                }
                        }
                        isCreatingMap = false;
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
                [SerializeField, HideInInspector] public List<GridConnections> connections = new List<GridConnections>();

                [System.Serializable]
                public struct GridConnections
                {
                        public Vector2 position;
                        public Vector2 end;
                        public Color color;
                        public float size;
                        public int rays;
                        public bool verticalLine;
                        public bool horizontalLine;
                }

                public override void OnSceneGUI (UnityEditor.Editor editor)
                {
                        if (foldOut)
                        {
                                PathExtras(AddExtras.Ladder, ladder, Color.yellow, cellSize);
                                PathExtras(AddExtras.Wall, wall, Color.black, cellSize);
                                PathExtras(AddExtras.Ceiling, ceiling, Color.blue, cellSize);
                                PathExtras(AddExtras.Bridge, bridge, Tint.DarkOrange, cellSize);

                                if (addExtra != AddExtras.None)
                                {
                                        if (Event.current.type == EventType.Layout)
                                        {
                                                UnityEditor.HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
                                        }
                                }
                        }

                        if (createPaths)
                        {
                                DebugTimer.Start();
                                map.Clear();
                                neighbors.Clear();
                                connections.Clear();
                                createPaths = false;
                                isCreatingMap = false;
                                addExtra = AddExtras.None;
                                AddTilemaps(tilemaps);

                                DebugTimer.Stop("Created PathfindingRT Nodes ");
                        }
                        DrawWhenNotSelected();
                }

                public override void DrawWhenNotSelected ()
                {
                        if (foldOut)
                        {
                                Draw.GLStart();
                                ShowExtras(ladder, Color.yellow, cellSize);
                                ShowExtras(wall, Color.black, cellSize);
                                ShowExtras(ceiling, Color.blue, cellSize);
                                ShowExtras(bridge, Tint.DarkOrange, cellSize);
                                Draw.GLEnd();
                        }
                        DisplayMapAfterEditing();
                }

                private void PathExtras (AddExtras type, List<Vector2Int> list, Color color, float cellSize, bool isFall = false)
                {
                        if (type == addExtra && map.Count > 0)
                        {
                                Vector2 mousePosition = SceneTools.MousePosition();
                                int x = Mathf.FloorToInt(mousePosition.x);
                                int y = Mathf.FloorToInt(mousePosition.y);
                                Vector2 position = new Vector2(x, y);
                                Vector2 cellOffset = new Vector2(cellSize * 0.5f, cellSize * 0.5f);

                                Draw.GLCircleInit(position + cellOffset, cellSize / 1.97f, color, 5);
                                if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
                                {
                                        Vector2Int index = new Vector2Int(x, y);
                                        if (list.Contains(index))
                                        {
                                                list.Remove(index);
                                        }
                                        else
                                        {
                                                list.Add(index);
                                        }
                                }
                        }
                }

                private void ShowExtras (List<Vector2Int> list, Color color, float cellSize)
                {
                        Vector2 cellOffset = new Vector2(cellSize * 0.5f, cellSize * 0.5f);
                        for (int i = 0; i < list.Count; i++)
                        {
                                Draw.GLCircle(new Vector2Int(list[i].x, list[i].y) + cellOffset, cellSize / 1.8f, color, 5);
                        }
                }

                private void DisplayMapAfterEditing ()
                {
                        if (foldOut && !createPaths)
                        {
                                Draw.GLStart();
                                for (int i = 0; i < connections.Count; i++)
                                {
                                        if (connections[i].verticalLine)
                                        {
                                                float size = Mathf.Abs(connections[i].position.y - connections[i].end.y);
                                                for (int j = 0; j < size; j++)
                                                {
                                                        Draw.GLCircle(connections[i].position + Vector2.down * j, connections[i].size, connections[i].color, 1);
                                                }
                                        }
                                        else if (connections[i].horizontalLine)
                                        {
                                                float dir = connections[i].position.x <= connections[i].end.x ? 1f : -1f;
                                                float size = Mathf.Abs(connections[i].position.x - connections[i].end.x);
                                                for (int j = 0; j < size; j++)
                                                {
                                                        Draw.GLCircle(connections[i].position + Vector2.right * j * dir, connections[i].size, connections[i].color, 1);
                                                }
                                        }
                                        else
                                        {
                                                Draw.GLCircle(connections[i].position, connections[i].size, connections[i].color, connections[i].rays);
                                        }
                                }
                                Draw.GLEnd();
                        }
                }

                public static void AddNodeDrawing (PathfindingRT map, Vector2 position, float size, Color color, int rays)
                {
                        map.connections.Add(new PathfindingRT.GridConnections() { position = position, size = size, color = color, rays = rays });
                }

                public static void AddVerticalPath (PathfindingRT map, Vector2 start, Vector2 end, float size, Color color)
                {
                        map.connections.Add(new PathfindingRT.GridConnections() { position = start, end = end, color = color, size = size, verticalLine = true });
                }

                public static void AddHorizontalPath (PathfindingRT map, Vector2 start, Vector2 end, float size, Color color)
                {
                        map.connections.Add(new PathfindingRT.GridConnections() { position = start, end = end, color = color, size = size, horizontalLine = true });
                }

                [DrawGizmo(GizmoType.NotInSelectionHierarchy)]
                static void DrawWhenObjectIsNotSelected (Pathfinding pathfinding, GizmoType gizmoType)
                {
                        pathfinding.DrawWhenNotSelected();
                }

#endif
                #endregion
        }

        [System.Serializable] public class DictionaryMap : SerializableDictionary<Vector2Int, PathNode> { }
        [System.Serializable] public class DictionaryNeighbor : SerializableDictionary<PathNode, NeighborList> { }

        [System.Serializable]
        public enum AddExtras
        {
                None,
                Ladder,
                Wall,
                Ceiling,
                Bridge
        }


}
