using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace TwoBitMachines.FlareEngine.AI.BlackboardData
{
        [AddComponentMenu("")]
        public class TargetPathfinding : TargetPathfindingBase
        {
                [SerializeField] public Pathfinding map;
                [System.NonSerialized] public JobHandle jobHandle;
                [System.NonSerialized] public NativeList<int> pathReference;
                [System.NonSerialized] public bool instant = false;

                public override float cellSize => map.cellSize;
                public override Vector2 cellYOffset => map.cellYOffset;

                [SerializeField, HideInInspector]
                public FollowerState[] stateMachine = new FollowerState[]
                {
                        new StateFollow ( ),
                        new StateJump ( ),
                        new StateCeiling ( ),
                        new StateCornerGrab ( ),
                        new StateLadder ( ),
                        new StateWall ( ),
                        new StateMoving ( )
                };

                public void Awake ()
                {
                        waitForPath = false;
                        activeUnit = true;
                        pathReference = new NativeList<int>(Allocator.Persistent);
                        if (map != null)
                        {
                                map.RegisterFollower(this);
                        }
                        variance = Random.Range(-0.05f, 0.05f);
                }

                private void OnEnable ()
                {
                        activeUnit = true;
                }

                private void OnDisable ()
                {
                        activeUnit = false;
                }

                private void OnDestroy ()
                {
                        DisposeFollower(); // follower should be disposed by pathfinding action. However, if this follower is destroyed by user, then onDestroy must be called to prevent memory leak
                }

                public override void DisposeFollower ()
                {
                        jobHandle.Complete();
                        if (pathReference.IsCreated)
                        {
                                pathReference.Dispose();
                        }
                }

                public override void ResetState (WorldCollision world, Gravity gravity, AnimationSignals signals, Vector2 target)
                {
                        this.world = world;
                        this.signals = signals;
                        this.gravity = gravity.gravity;

                        size = CharacterSize();
                        state = PathState.Follow;
                        jumpType = JumpType.Fall;

                        SetCurrentNode();
                        nextNode = futureNode = currentNode;
                        PathNode targetNode = map.PositionToNode(target + map.cellYOffset);
                        targetX = targetNode.x;
                        targetY = targetNode.y;
                }

                public void GetPath ()
                {
                        if (!waitForPath || map == null || !jobHandle.IsCompleted)
                        {
                                return;
                        }
                        waitForPath = false;
                        jobHandle.Complete();
                        path.Clear();

                        float length = pathReference.Length;
                        for (int i = 0; i < length; i++)
                        {
                                path.Push(map.grid[pathReference[i]]);
                                if (i == length - 2)
                                {
                                        if (map.grid[pathReference[i]].Same(currentNode))
                                        {
                                                break;
                                        }
                                }
                        }
                        FollowerState.RemoveNode(this); // need to call this to set future node
                }

                public override void CalculatePath (Blackboard target) //, bool instant = false)
                {
                        targetRef = target;
                        if (instant)
                        {
                                instant = false;
                                Recalculate();
                                return;
                        }
                        if (!recalculate && map.jobGrid.Length > 0)
                        {
                                recalculate = true;
                                timeStamp = Time.time + 0.30f + variance;
                        }
                }

                public void Recalculate ()
                {
                        if (map == null || currentNode == null || targetRef == null || map.jobGrid.Length == 0)
                        {
                                return;
                        }

                        PathNode targetNode = map.PositionToNode(targetRef.GetTarget() + map.cellYOffset);

                        if (this.targetRef.hasNoTargets)
                        {
                                return;
                        }

                        targetX = targetNode.x;
                        targetY = targetNode.y;
                        jobHandle.Complete(); // ensure job is complete before doing a reset

                        PathfindingJob calculateJob = new PathfindingJob
                        {
                                linesX = map.linesX,
                                linesY = map.linesY,
                                jobGrid = map.jobGrid,
                                result = pathReference,
                                characterSizeY = size.y,
                                jobNeighbors = map.jobNeighbors,
                                target = map.jobGrid[targetX + targetY * map.linesX],
                                start = map.jobGrid[currentNode.x + currentNode.y * map.linesX]
                        };
                        jobHandle = calculateJob.Schedule();
                        waitForPath = true;
                }

                private void SetCurrentNode ()
                {
                        bottomCenter = world != null ? world.box.bottomCenter : (Vector2) transform.position;
                        position = bottomCenter + map.cellYOffset;
                        currentNode = map.PositionToNode(position);
                }

                public void RunPathFollower (ref Vector2 velocity)
                {
                        SetCurrentNode();
                        if (recalculate && Time.time > timeStamp && notJumpingState && jobHandle.IsCompleted)
                        {
                                recalculate = false;
                                Recalculate();
                        }

                        GetPath();
                        if (waitForPath || currentNode == null || map == null || world == null)
                        {
                                return;
                        }
                        //Debug.Log("State: " + state);
                        previousState = state;
                        nextNode = nextNode == null ? currentNode : nextNode;
                        stateMachine[(int) state].Execute(this, ref velocity);
                        WaitForOccupiedNode(ref velocity);

                        #region Debug
#if UNITY_EDITOR
                        if (WorldManager.viewDebugger)
                        {
                                for (int i = 0; i < pathReference.Length; i++)
                                {
                                        Draw.Circle(map.grid[pathReference[i]].position, 0.4f, Color.red);
                                }
                                Draw.Circle(currentNode.position, 0.5f, Color.yellow, 1);
                                Draw.Circle(nextNode.position, 0.5f, Color.blue, 1);
                                if (futureNode != null)
                                        Draw.Circle(futureNode.position, 0.5f, Color.red, 1);
                        }
#endif
                        #endregion
                }

                public void WaitForOccupiedNode (ref Vector2 velocity)
                {
                        if (blockUnits && followingSafeNode && nextNode.isOccupied && nextNode.unit != this)
                        {
                                wait = true;
                                counter = 0;
                        }
                        if (wait && TwoBitMachines.Clock.TimerInverse(ref counter, 0.5f))
                        {
                                velocity.x = followingSafeNodeX ? 0 : velocity.x;
                                velocity.y = followingSafeNodeY ? 0 : velocity.y;
                                return;
                        }
                        wait = false;
                }

                public override void OccupyNode ()
                {
                        if (blockUnits && followingSafeNode && currentNode != null && !currentNode.isOccupied)
                        {
                                currentNode.isOccupied = true;
                                currentNode.unit = this;
                        }
                }

                public bool CanExit ()
                {
                        wait = followingSafeNode ? false : wait;
                        counter = followingSafeNode ? 0 : counter;
                        return followingSafeNode;
                }

                public bool AtFinalTarget ()
                {
                        if ((path.Count <= 1 && futureNode == null) || currentNode.Same(targetX, targetY))
                        {
                                if (followingSafeNodeX && currentNode.DistanceX(transform.position) < 0.001f)
                                        return true;
                                if (followingSafeNodeY && currentNode.DistanceY(transform.position) < 0.001f)
                                        return true;
                        }
                        return false;
                }

                public Vector2 CharacterSize ()
                {
                        Vector2 size = world == null ? Vector2.one : world.box.boxSize;
                        return new Vector2(size.x * transform.localScale.x, size.y * transform.localScale.y);
                }

                public override Vector2 GetTarget (int index = 0)
                {
                        return transform.position;
                }

        }

}
