using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace TwoBitMachines.FlareEngine
{
        [RequireComponent(typeof(ManageScenes))]
        public class OverworldMap : MonoBehaviour
        {
                [SerializeField] public GameObject player;
                [SerializeField] public InputButtonSO enterButton;
                [SerializeField] public Tween tween = Tween.Linear;
                [SerializeField] public float speed = 10f;
                [SerializeField] public float offset = 0f;
                [SerializeField] public float actionActive;

                [SerializeField] public string mapSavekey;
                [SerializeField] public OverworldNode[] list;
                [SerializeField] public OverworldMapSave save = new OverworldMapSave();
                [SerializeField] public MoveTargetManually manual = new MoveTargetManually();
                [SerializeField] public MoveTargetAutomatically automatic = new MoveTargetAutomatically();

                [SerializeField] public UnityEvent onIdle;
                [SerializeField] public UnityEvent onMoveBegin;
                [SerializeField] public UnityEvent onMoveFailed;
                [SerializeField] public UnityEvent onEnterSuccess;
                [SerializeField] public UnityEvent onEnterFailed;
                [SerializeField] public UnityEvent onEnteringLevel;
                [SerializeField] public UnityEventFloat onMoving;
                [SerializeField] public UnityEventString onSignalUnlock;

                [System.NonSerialized] private UnityEvent signalComplete;
                [System.NonSerialized] private int currentIndex = 0;
                [System.NonSerialized] private float blockCounter = 0;
                [System.NonSerialized] private float blockTime = 0;
                [System.NonSerialized] private float isMovingCounter = 0;
                [System.NonSerialized] private bool isMovingNextDelay;
                [System.NonSerialized] private string blockSignal;
                [System.NonSerialized] private bool playBlockSignal;
                [System.NonSerialized] private bool playingBlockSignal;
                [System.NonSerialized] private ManageScenes scenes;
                [System.NonSerialized] private bool enteringLevel;
                public Vector3 offsetY => Vector3.down * offset;

                #region ▀▄▀▄▀▄ Editor Variables ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField] public int editIndex;
                [SerializeField] public bool foldOut;
                [SerializeField] public bool idleFoldOut;
                [SerializeField] public bool enterFoldOut;
                [SerializeField] public bool movingFoldOut;
                [SerializeField] public bool moveBeginFoldOut;
                [SerializeField] public bool moveFailedFoldOut;
                [SerializeField] public bool enterFailedFoldOut;
                [SerializeField] public bool enteringLevelFoldOut;
                [SerializeField] public bool signalUnlockFoldOut;
                [SerializeField] public GameObject basicNodes;
#pragma warning restore 0414
#endif
                #endregion

                [System.Serializable]
                public class OverworldMapSave
                {
                        [SerializeField] public int currentIndex;
                        [SerializeField] public int moveToNextIndex = -1;
                        [SerializeField] public List<string> unlockKey = new List<string>();
                }

                private void Awake ()
                {
                        manual.Initialize();
                        scenes = this.gameObject.GetComponent<ManageScenes>();
                }

                private void Start ()
                {
                        RestoreValue();
                        SetTarget();

                        isMovingNextDelay = false;
                        if (save.moveToNextIndex != -1)
                        {
                                isMovingNextDelay = true;
                                isMovingCounter = 0;
                        }
                }

                private void OnDisable ()
                {
                        Save();
                }

                public void RestoreValue ()
                {
                        save = Storage.Load<OverworldMapSave>(save, WorldManager.saveFolder, mapSavekey);
                        currentIndex = save.currentIndex;
                }

                public void Save ()
                {
                        save.currentIndex = currentIndex;
                        Storage.Save(save, WorldManager.saveFolder, mapSavekey);
                }

                private void Update ()
                {
                        if (player == null || list == null || list.Length == 0)
                        {
                                return;
                        }
                        if (isMovingNextDelay)
                        {
                                if (Clock.Timer(ref isMovingCounter, 1f))
                                {
                                        automatic.AutomaticSetNext(this, GetNode(save.moveToNextIndex), player.transform, Node());
                                        save.moveToNextIndex = -1;
                                        isMovingNextDelay = false;
                                }
                                return;
                        }
                        if (IsPlayingBlockSignal())
                        {
                                return;
                        }
                        if (enteringLevel)
                        {
                                onEnteringLevel.Invoke();
                                return;
                        }
                        if (automatic.Moving(this, player.transform, tween, speed))
                        {
                                return;
                        }
                        if (manual.Moving(this, player.transform, tween, speed))
                        {
                                return;
                        }
                        if (automatic.WaitForInput(this, list, player.transform, Node()))
                        {
                                return;
                        }
                        if (manual.WaitForInput(this, player.transform, Node()))
                        {
                                return;
                        }
                        if (enterButton != null && enterButton.Pressed() && Node(out OverworldNode node))
                        {
                                EnterLevel(node);
                                UnlockBlock(node);
                                Teleport(node);
                        }
                        else if (DetectMouseAndTouch(out OverworldNode touchNode) && touchNode == Node())
                        {
                                EnterLevel(touchNode);
                                UnlockBlock(touchNode);
                                Teleport(touchNode);
                        }
                        if (!enteringLevel)
                        {
                                onIdle.Invoke();
                        }
                }

                public void EnterLevel (OverworldNode node)
                {
                        if (!node.isLevel || scenes == null)
                        {
                                return;
                        }

                        scenes.LoadScene(node.sceneName);
                        if (ManageScenes.enteringScene)
                        {
                                enteringLevel = true;
                                onEnterSuccess.Invoke();
                                if (node.setNextNodeType == SetNextNodeType.TeleportTo && node.nextNode != null)
                                {
                                        SetIndex(node.nextNode);
                                }
                                if (node.setNextNodeType == SetNextNodeType.MoveTo && node.nextNode != null)
                                {
                                        save.moveToNextIndex = GetIndex(node.nextNode);
                                }
                                return;
                        }
                        onEnterFailed.Invoke();
                }

                public void UnlockBlock (OverworldNode node)
                {
                        if (node.isBlock && save.unlockKey.Contains(node.unlockKey) && !node.unlocked)
                        {
                                node.UnlockBlock();
                                playBlockSignal = true;
                                blockSignal = node.signal;
                                blockTime = node.signalTime;
                                signalComplete = node.signalComplete;
                                blockCounter = 0;
                        }
                }

                public bool IsPlayingBlockSignal ()
                {
                        if (playBlockSignal)
                        {
                                onSignalUnlock.Invoke(blockSignal);
                                if (Clock.Timer(ref blockCounter, blockTime))
                                {
                                        playBlockSignal = false;
                                        if (signalComplete != null)
                                        {
                                                signalComplete.Invoke();
                                        }

                                }
                                return playBlockSignal;
                        }
                        return false;
                }

                public void Teleport (OverworldNode node)
                {
                        if (scenes != null && player != null && node != null && node.isTeleport && node.teleportToNode != null)//
                        {
                                player.transform.position = node.teleportToNode.transform.position + offsetY;
                                SetIndex(node.teleportToNode);
                                node.onTeleport.Invoke();
                        }
                }

                public OverworldNode Node ()
                {
                        currentIndex = Mathf.Clamp(currentIndex, 0, list.Length - 1);
                        return list.Length > 0 ? list[currentIndex] : null;
                }

                public bool Node (out OverworldNode node)
                {
                        node = Node();
                        return node == null ? false : true;
                }

                public void SetTarget ()
                {
                        OverworldNode node = Node();
                        if (node != null && player != null)
                        {
                                player.transform.position = node.transform.position + offsetY;
                        }
                }

                public void SetIndex (OverworldNode atLevel)
                {
                        for (int i = 0; i < list.Length; i++)
                        {
                                if (list[i] == atLevel)
                                {
                                        currentIndex = i;
                                }
                        }
                }

                public int GetIndex (OverworldNode atLevel)
                {
                        for (int i = 0; i < list.Length; i++)
                        {
                                if (list[i] == atLevel)
                                {
                                        return i;
                                }
                        }
                        return -1;
                }

                public OverworldNode GetNode (int index)
                {
                        for (int i = 0; i < list.Length; i++)
                        {
                                if (i == index)
                                {
                                        return list[i];
                                }
                        }
                        return null;
                }

                public void RegisterUnlockKey (string key)
                {
                        if (!save.unlockKey.Contains(key))
                        {
                                save.unlockKey.Add(key);
                        }
                }

                private bool DetectMouseAndTouch (out OverworldNode touchNode)
                {
                        touchNode = null;
                        if (Input.GetMouseButton(0) || Input.touchCount > 0)
                        {
                                Vector2 position = Input.GetMouseButton(0) ? (Vector2) Input.mousePosition : Input.GetTouch(0).position;
                                position = Camera.main.ScreenToWorldPoint(position);
                                for (int i = 0; i < list.Length; i++)
                                {
                                        if ((list[i].position - position).magnitude < 1f)
                                        {
                                                touchNode = list[i];
                                                return true;
                                        }
                                }
                        }
                        return false;
                }

        }

        [System.Serializable]
        public class MoveTargetManually
        {
                [SerializeField] public InputButtonSO left;
                [SerializeField] public InputButtonSO right;
                [SerializeField] public InputButtonSO down;
                [SerializeField] public InputButtonSO up;

                [System.NonSerialized] private Vector2 start;
                [System.NonSerialized] private Vector2 destination;
                [System.NonSerialized] private bool movingTarget;
                [System.NonSerialized] private float startTime;

                public void Initialize ()
                {
                        if (left != null)
                                WorldManager.RegisterInput(left);
                        if (right != null)
                                WorldManager.RegisterInput(right);
                        if (down != null)
                                WorldManager.RegisterInput(down);
                        if (up != null)
                                WorldManager.RegisterInput(up);
                }

                public bool Moving (OverworldMap map, Transform target, Tween tween, float speed)
                {
                        if (!movingTarget)
                        {
                                return false;
                        }

                        float velX = destination.x - start.x;
                        map.onMoving.Invoke(velX);
                        target.transform.position = Compute.DistanceLerp(start, destination, speed, startTime, tween, out float percent);
                        if (percent >= 1f)
                        {
                                if (map.Node(out OverworldNode node))
                                {
                                        node.onEnterNode.Invoke(node);
                                }
                                return movingTarget = false;
                        }
                        if ((target.transform.position - (Vector3) destination).sqrMagnitude <= 1f)
                        {
                                return false;
                        }
                        return true;
                }

                public bool WaitForInput (OverworldMap map, Transform target, OverworldNode level)
                {
                        if (level == null)
                        {
                                return false;
                        }
                        Vector2 direction = Vector2.zero;
                        if (left != null && left.Released())
                        {
                                direction.x = -1;
                        }
                        if (right != null && right.Released())
                        {
                                direction.x = 1;
                        }
                        if (down != null && down.Released())
                        {
                                direction.y = -1;
                        }
                        if (up != null && up.Released())
                        {
                                direction.y = 1;
                        }

                        if (direction == Vector2.zero)
                        {
                                return false;
                        }

                        OverworldNode nextLevel = NearestLevelDirection(direction.normalized, level);
                        if (nextLevel != null)
                        {
                                if (level != null && level.IsLocked() && level.PathBlocked(nextLevel))
                                {
                                        map.onMoveFailed.Invoke();
                                        return false;
                                }

                                movingTarget = true;
                                start = target.position;
                                destination = nextLevel.position + (Vector2) map.offsetY;
                                startTime = Time.time;
                                map.SetIndex(nextLevel);
                                map.onMoveBegin.Invoke();
                                level.onExitNode.Invoke(level);
                                return true;
                        }
                        return false;
                }

                private OverworldNode NearestLevelDirection (Vector2 direction, OverworldNode level)
                {
                        if (level == null)
                        {
                                return null;
                        }

                        int path = -1;
                        if (path == -1)
                        {
                                ValidateDirection(level, direction, ref path, false, false);
                        }
                        if (path == -1)
                        {
                                ValidateDirection(level, direction, ref path, false, true);
                        }
                        if (path == -1)
                        {
                                ValidateDirection(level, direction, ref path, true, false);
                        }
                        return path >= 0 ? level.path[path] : null;
                }

                private void ValidateDirection (OverworldNode level, Vector2 direction, ref int path, bool skipX, bool skipY)
                {
                        float smallestAngle = float.MaxValue;
                        for (int i = 0; i < level.path.Count; i++)
                        {
                                Vector2 pathDirection = (level.path[i].position - level.position).normalized;
                                if (SameSign(pathDirection, direction, skipX, skipY) && Compute.Angle(pathDirection, direction, out float angle) < smallestAngle)
                                {
                                        smallestAngle = angle;
                                        path = i;
                                }
                        }
                }

                private bool SameSign (Vector2 a, Vector2 b, bool skipX, bool skipY)
                {
                        return (skipX || Compute.SameSign(a.x, b.x)) && (skipY || Compute.SameSign(a.y, b.y));
                }
        }

        [System.Serializable]
        public class MoveTargetAutomatically
        {
                [System.NonSerialized] private Vector2 start;
                [System.NonSerialized] private Vector2 destination;
                [System.NonSerialized] private bool movingTarget;
                [System.NonSerialized] private float startTime;

                [System.NonSerialized] private AStarAlgorithm aStar = new AStarAlgorithm();
                [System.NonSerialized] private List<OverworldNode> path = new List<OverworldNode>();
                [System.NonSerialized] private OverworldNode lastPath;

                public bool Moving (OverworldMap map, Transform target, Tween tween, float speed)
                {
                        if (!movingTarget)
                        {
                                return false;
                        }

                        float velX = destination.x - start.x;
                        map.onMoving.Invoke(velX);
                        target.transform.position = Compute.DistanceLerp(start, destination, speed, startTime, tween, out float percent);

                        if (percent >= 1f)
                        {
                                if ((path == null || path.Count == 0) || (lastPath != null && lastPath.IsLocked() && lastPath.PathBlocked(path[0])))
                                {
                                        map.SetIndex(lastPath);
                                        if (map.Node(out OverworldNode node))
                                        {
                                                node.onEnterNode.Invoke(node);
                                        }
                                        return movingTarget = false;
                                }
                                else
                                {
                                        start = target.position;
                                        destination = path[0].position + (Vector2) map.offsetY;
                                        startTime = Time.time;
                                        lastPath = path[0];
                                        path.RemoveAt(0);
                                }
                        }
                        return true;
                }

                public bool WaitForInput (OverworldMap map, OverworldNode[] list, Transform target, OverworldNode level)
                {
                        if (level == null)
                        {
                                return false;
                        }
                        OverworldNode moveToLevel = DetectMouseAndTouch(list);
                        if (moveToLevel != null)
                        {
                                AutomaticSetNext(map, moveToLevel, target, level);
                        }
                        return false;
                }

                public bool AutomaticSetNext (OverworldMap map, OverworldNode moveToLevel, Transform target, OverworldNode level)
                {
                        if (moveToLevel == null)
                        {
                                return false;
                        }
                        path = aStar.FindPath(level, moveToLevel);
                        if (path != null && path.Count > 0)
                        {
                                if (level != null && level.IsLocked() && level.PathBlocked(path[0]))
                                {
                                        map.onMoveFailed.Invoke();
                                        return false;
                                }
                                movingTarget = true;
                                start = target.position;
                                destination = path[0].position + (Vector2) map.offsetY;
                                startTime = Time.time;
                                lastPath = path[0];
                                path.RemoveAt(0);
                                map.onMoveBegin.Invoke();
                                level.onExitNode.Invoke(level);
                                return true;
                        }
                        else
                        {
                                map.onMoveFailed.Invoke();
                        }
                        return false;
                }

                private OverworldNode DetectMouseAndTouch (OverworldNode[] list)
                {
                        if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
                        {
                                Vector2 position = Input.GetMouseButtonDown(0) ? (Vector2) Input.mousePosition : Input.GetTouch(0).position;
                                position = Camera.main.ScreenToWorldPoint(position);
                                for (int i = 0; i < list.Length; i++)
                                {
                                        if ((list[i].position - position).magnitude < 1f)
                                        {
                                                return list[i];
                                        }
                                }
                        }
                        return null;
                }

                private OverworldNode NearestLevelPosition (OverworldNode[] list, Vector2 position)
                {
                        for (int i = 0; i < list.Length; i++)
                        {
                                Vector2 pathDirection = (list[i].position - position).normalized;
                                if ((list[i].position - position).magnitude < 1f)
                                {
                                        return list[i];
                                }
                        }
                        return null;
                }
        }

        public class AStarAlgorithm
        {
                [System.NonSerialized] private List<OverworldNode> path = new List<OverworldNode>();
                [System.NonSerialized] private List<OverworldNode> openList = new List<OverworldNode>();
                [System.NonSerialized] private List<OverworldNode> closedList = new List<OverworldNode>();
                [System.NonSerialized] private List<OverworldNode> alternatePaths = new List<OverworldNode>();

                public List<OverworldNode> FindPath (OverworldNode start, OverworldNode goal, bool lockPaths = false)
                {
                        openList.Clear();
                        closedList.Clear();
                        openList.Add(start);

                        if (!lockPaths)
                        {
                                alternatePaths.Clear();
                        }

                        while (openList.Count > 0)
                        {
                                OverworldNode current = openList[0];
                                for (int i = 1; i < openList.Count; i++)
                                {
                                        if (openList[i].F < current.F)
                                        {
                                                current = openList[i];
                                        }
                                }

                                openList.Remove(current);
                                closedList.Add(current);

                                if (current.Equals(goal))
                                {
                                        return ReconstructPath(start, goal);
                                }

                                List<OverworldNode> neighbors = current.path;
                                for (int i = 0; i < neighbors.Count; i++)
                                {
                                        OverworldNode neighbor = neighbors[i];
                                        if (closedList.Contains(neighbor))
                                        {
                                                continue;
                                        }
                                        if (current.IsLocked() && current.PathBlocked(neighbor))
                                        {
                                                if (!lockPaths && !alternatePaths.Contains(current))
                                                {
                                                        alternatePaths.Add(current);
                                                }
                                                continue; // Skip blocked neighbors
                                        }

                                        float tentativeG = current.G + CalculateDistance(current, neighbor);

                                        if (!openList.Contains(neighbor) || tentativeG < neighbor.G)
                                        {
                                                neighbor.Parent = current;
                                                neighbor.G = tentativeG;
                                                neighbor.H = CalculateDistance(neighbor, goal);

                                                if (!openList.Contains(neighbor))
                                                {
                                                        openList.Add(neighbor);
                                                }
                                        }
                                }
                        }
                        for (int i = 0; i < alternatePaths.Count; i++)
                        {
                                List<OverworldNode> newPath = FindPath(start, alternatePaths[i], true);
                                if (newPath != null && newPath.Count > 0)
                                {
                                        return newPath;
                                }
                        }
                        return null;
                }

                private float CalculateDistance (OverworldNode node1, OverworldNode node2)
                {
                        float dx = node1.position.x - node2.position.x;
                        float dy = node1.position.y - node2.position.y;
                        return Mathf.Sqrt(dx * dx + dy * dy);
                }

                private List<OverworldNode> ReconstructPath (OverworldNode start, OverworldNode goal, int limit = 0)
                {
                        path.Clear();
                        OverworldNode current = goal;
                        while (current != null && current != start && limit++ < 10000)
                        {
                                path.Insert(0, current);
                                current = current.Parent;
                        }
                        return path;
                }

        }

}




