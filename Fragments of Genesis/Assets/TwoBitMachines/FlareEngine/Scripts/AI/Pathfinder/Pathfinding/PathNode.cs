using System.Collections.Generic;
using TwoBitMachines.FlareEngine.AI.BlackboardData;
using UnityEngine;

namespace TwoBitMachines.FlareEngine
{
        [System.Serializable]
        public class PathNode
        {
                [SerializeField] public int x;
                [SerializeField] public int y;
                [SerializeField] public int height;
                [SerializeField] public Vector2 position;

                [SerializeField] public bool air;
                [SerializeField] public bool wall;
                [SerializeField] public bool exact;
                [SerializeField] public bool block;
                [SerializeField] public bool ladder;
                [SerializeField] public bool moving;
                [SerializeField] public bool ground;
                [SerializeField] public bool bridge;
                [SerializeField] public bool isFall;
                [SerializeField] public bool ceiling;
                [SerializeField] public bool isOccupied;
                [SerializeField] public bool leftCorner;
                [SerializeField] public bool rightCorner;
                [SerializeField] public bool edgeOfCorner;
                [SerializeField] public bool jumpThroughGround;
                [SerializeField] public bool nextToWall;
                [SerializeField] public bool wallLeft = false;
                [SerializeField] public List<Vector2Int> neighbor = new List<Vector2Int>();

                public Vector2Int cell => new Vector2Int(x, y);
                public bool path => ground || jumpThroughGround || ladder || wall || moving || ceiling || bridge;
                public bool onGround => ground || jumpThroughGround;
                public bool uniCorner => leftCorner && rightCorner;
                [System.NonSerialized] public TargetPathfindingBase unit;


                public bool Same (PathNode other)
                {
                        if (other == null)
                                return false;
                        return x == other.x && y == other.y;
                }

                public bool Same (Vector2 coordinates)
                {
                        return x == coordinates.x && y == coordinates.y;
                }

                public bool Same (int x, int y)
                {
                        return this.x == x && this.y == y;
                }


                public bool SameY (PathNode other)
                {
                        return y == other.y;
                }

                public bool SameX (PathNode other)
                {
                        return x == other.x;
                }

                public bool Above (PathNode other)
                {
                        return y > other.y;
                }

                public bool Below (PathNode other)
                {
                        return y < other.y;
                }

                public int DirectionX (PathNode other)
                {
                        return (int) Mathf.Sign(x - other.x);
                }

                public float SqrMagnitude (PathNode other)
                {
                        return (Coorddinates() - other.Coorddinates()).sqrMagnitude;
                }

                public Vector2 Coorddinates ()
                {
                        return new Vector2(x, y);
                }

                public float DistanceX (Vector2 position)
                {
                        return Mathf.Abs(this.position.x - position.x);
                }

                public float DistanceX (PathNode node)
                {
                        return Mathf.Abs(this.position.x - node.position.x);
                }

                public bool NextToX (PathNode node)
                {
                        return y == node.y && Mathf.Abs(x - node.x) <= 1.1f;
                }

                public bool NextToY (PathNode node)
                {
                        return x == node.x && Mathf.Abs(y - node.y) <= 1.1f;
                }

                public bool NextToGridX (PathNode node)
                {
                        return Mathf.Abs(x - node.x) < 1.1f;
                }

                public bool DistanceXOne (PathNode node)
                {
                        float distance = Mathf.Abs(x - node.x);
                        return distance > 0 && distance < 1.1f;
                }

                public float DistanceY (Vector2 position)
                {
                        return Mathf.Abs(this.position.y - position.y);
                }

                public float DistanceY (PathNode node)
                {
                        return Mathf.Abs(this.position.y - node.position.y);
                }

                public PathNode Shift (Pathfinding map, int shiftX, int shiftY)
                {
                        return map.Node(x + shiftX, y + shiftY);
                }

                public PathNode ShiftX (Pathfinding map, int shiftX)
                {
                        return map.Node(x + shiftX, y);
                }

                public PathNode ShiftX (Pathfinding map, PathNode directionNode)
                {
                        return map.Node(x + DirectionX(directionNode), y);
                }

                public PathNode ShiftY (Pathfinding map, int shiftY)
                {
                        return map.Node(x, y + shiftY);
                }

                public void AddNeighbor (PathNode child, bool addToBoth = true)
                {
                        if (!neighbor.Contains(child.cell))
                        {
                                neighbor.Add(child.cell);
                        }
                        if (addToBoth)
                        {
                                if (!child.neighbor.Contains(cell))
                                {
                                        child.neighbor.Add(cell);
                                }
                        }
                }
        }

        public struct PathNodeStruct
        {
                public int index;
                public int x;
                public int y;
                public int height;
                public bool air;
                public bool wall;
                public bool block;
                public bool exact;
                public bool ladder;
                public bool moving;
                public bool ground;
                public bool bridge;
                public bool ceiling;
                public bool edgeDrop;
                public bool leftCorner;
                public bool rightCorner;
                public bool jumpThroughGround;
                public Vector2Int cell;

                public bool Path () { return ground || jumpThroughGround || ladder || wall || moving || ceiling || bridge; }

        }

}
