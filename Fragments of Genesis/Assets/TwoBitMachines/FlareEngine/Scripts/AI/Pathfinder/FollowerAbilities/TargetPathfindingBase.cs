using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace TwoBitMachines.FlareEngine.AI.BlackboardData
{
        [AddComponentMenu("")]
        public class TargetPathfindingBase : Blackboard
        {
                [System.NonSerialized] public Stack<PathNode> path = new Stack<PathNode>();
                [SerializeField] public float followSpeed = 5f;
                [SerializeField] public float ceilingSpeed = 5f;
                [SerializeField] public float ladderSpeed = 5f;
                [SerializeField] public float wallSpeed = 5f;
                [SerializeField] public bool ignoreUnits = true;
                [SerializeField] public float pauseAfterJump;
                //[SerializeField] public float cornerYOffset;
                //[SerializeField] public bool cornerGrab;

                [System.NonSerialized] public float velRef;
                [System.NonSerialized] public float counter;
                [System.NonSerialized] public float gravity;
                [System.NonSerialized] public float pauseCounter;

                [System.NonSerialized] public float timeStamp;
                [System.NonSerialized] public bool recalculate;
                [System.NonSerialized] public bool wait;
                [System.NonSerialized] public bool waitForPath;
                [System.NonSerialized] public bool followToCenter;
                [System.NonSerialized] public bool activeUnit = true;
                [System.NonSerialized] public bool pauseAfterJumpActive;

                [System.NonSerialized] public WorldCollision world;
                [System.NonSerialized] public AnimationSignals signals;
                [System.NonSerialized] public BezierJump bezierJump = new BezierJump();

                [System.NonSerialized] public Blackboard targetRef;
                [System.NonSerialized] public PathNode nextNode; //     node ai is following
                [System.NonSerialized] public PathNode futureNode; //   node that comes after nextNode
                [System.NonSerialized] public PathNode currentNode; //  node ai currently inhabits
                [System.NonSerialized] public int targetX;
                [System.NonSerialized] public int targetY;

                [System.NonSerialized] public PathState state;
                [System.NonSerialized] public PathState previousState;

                [System.NonSerialized] public Vector2 size; //           character Size
                [System.NonSerialized] public Vector2 position;
                [System.NonSerialized] public Vector2 bottomCenter;
                [System.NonSerialized] public JumpType jumpType; // for jump state
                [System.NonSerialized] public PathState jumpToState;
                [System.NonSerialized] public float variance;

                public bool pathSafeToChagne => followingSafeNode && currentNode != null && currentNode.path;
                public bool followingSafeNodeX => state == PathState.Follow || state == PathState.Ceiling;
                public bool followingSafeNodeY => state == PathState.Ladder || state == PathState.Wall;
                public bool followingUnsafeNode => !followingSafeNodeX && !followingSafeNodeY;
                public bool followingSafeNode => followingSafeNodeX || followingSafeNodeY;
                public bool stateChanged => previousState != state;
                public bool blockUnits => !ignoreUnits;
                public bool notJumpingState => state != PathState.Jump;

                public virtual float cellSize => 1f;
                public virtual Vector2 cellYOffset => Vector2.up;

                public virtual void CalculatePath (Blackboard target)
                {

                }

                public virtual void DisposeFollower ()
                {
                }

                public virtual void OccupyNode ()
                {
                }

                public virtual bool JobIsComplete ()
                {
                        return false;
                }
        }


        public enum PathState
        {
                Follow = 0,
                Jump = 1,
                Ceiling = 2,
                CornerGrab = 3,
                Ladder = 4,
                Wall = 5,
                Moving = 6
        }
}
