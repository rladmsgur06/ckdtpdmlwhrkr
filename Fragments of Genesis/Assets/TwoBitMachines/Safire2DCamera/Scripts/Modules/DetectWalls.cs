using UnityEngine;

namespace TwoBitMachines.Safire2DCamera
{
        [System.Serializable]
        public class DetectWalls
        {
                [SerializeField] public DetectWallsType direction;
                [SerializeField] public LayerMask layerMask;

                [System.NonSerialized] public Vector2 holdPosition;
                [System.NonSerialized] private BoxCollider2D boxCollider;
                [System.NonSerialized] private Follow follow;
                [System.NonSerialized] private bool found = false;
                [System.NonSerialized] private bool waitUntilFound = false;
                [System.NonSerialized] private int edgeCount = 0;

                public bool ignoreGravity => direction == DetectWallsType.IgnoreGravity;

                public void Initialize (Follow follow)
                {
                        this.follow = follow;
                        if (follow.targetTransform != null)
                        {
                                boxCollider = follow.targetTransform.GetComponent<BoxCollider2D> ( );
                        }
                }

                public void Reset ( )
                {
                        edgeCount = 0;
                        found = false;
                        waitUntilFound = false;
                        holdPosition = follow.TargetPosition ( );
                }

                public Vector3 Position (Vector3 target, ScreenZone screenZone)
                {
                        //*  Ignore Gravity ignores target jumping. Camera can still follow in x.
                        //*  Detect Walls will only follow if target makes contact with surface.
                        if (direction == DetectWallsType.None || boxCollider == null)
                        {
                                return holdPosition = target;
                        }
                        if (screenZone.surpassedZoneY || screenZone.surpassedZoneX || waitUntilFound)
                        {
                                waitUntilFound = true;
                                holdPosition = target;
                                follow.ForceTargetSmooth (screenZone.surpassedZoneX, screenZone.surpassedZoneY);
                        }
                        FindWall (target);
                        return new Vector3 (ignoreGravity ? target.x : holdPosition.x, holdPosition.y);
                }

                public void FindWall (Vector2 target)
                {
                        Physics2D.SyncTransforms ( );
                        UnityEngine.Bounds bounds = boxCollider.bounds;

                        if (ignoreGravity) // only detect bottom collision
                        {
                                bounds.center -= bounds.extents.y * Vector3.up;
                                bounds.size = new Vector3 (bounds.size.x - 0.1f, 0.1f);
                        }
                        else //               detect all surfaces
                        {
                                bounds.Expand (0.025f);
                        }
                        bool previouslyNotFound = !found;
                        Collider2D surface = Physics2D.OverlapBox (bounds.center, bounds.size, boxCollider.transform.eulerAngles.z, layerMask);
                        found = surface != null;

                        if (previouslyNotFound && found && ignoreGravity)
                        {
                                follow.ForceTargetSmooth (x: false);
                        }
                        if (previouslyNotFound && found && !ignoreGravity)
                        {
                                follow.ForceTargetSmooth ( );
                        }
                        if (found)
                        {
                                if (waitUntilFound && (!(surface is EdgeCollider2D) || ++edgeCount > 10))
                                {
                                        edgeCount = 0;
                                        waitUntilFound = false;
                                }
                                if (!waitUntilFound)
                                {
                                        holdPosition = target;
                                }
                        }
                        else
                        {
                                edgeCount = 0;
                        }
                }

                public void Set (int key)
                {
                        direction = (DetectWallsType) key;
                }

                public enum DetectWallsType
                {
                        None = 0,
                        IgnoreGravity = 1,
                        DetectWalls = 2
                }
        }

}