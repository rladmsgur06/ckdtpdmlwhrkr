using UnityEngine;

namespace TwoBitMachines
{
        [System.Serializable]
        public class SimpleBounds
        {
                [SerializeField] public Vector2 position = new Vector2(0, 0);
                [SerializeField] public Vector2 size = new Vector2(10f, 5f);

                [System.NonSerialized] private Transform target;
                [System.NonSerialized] private Vector3 startOffset;

                public float left { get; private set; }
                public float bottom { get; private set; }
                public float right { get; private set; }
                public float top { get; private set; }

                public Vector2 rawPosition => target.position + startOffset;
                public Vector2 center => position + size * 0.5f;
                public Vector2 bottomCenter => position + Vector2.right * size.x * 0.5f;

                public void Initialize()
                {
                        left = position.x;
                        bottom = position.y;
                        right = position.x + size.x;
                        top = position.y + size.y;
                }

                public void Initialize(Vector2 origin)
                {
                        position = origin;
                        left = position.x;
                        bottom = position.y;
                        right = position.x + size.x;
                        top = position.y + size.y;
                }

                public void Initialize(Transform targetRef)
                {
                        target = targetRef;
                        startOffset = position - (Vector2)targetRef.position;
                }

                public bool Contains(Vector2 p)
                {
                        return p.x >= left && p.x <= right && p.y >= bottom - 0.1f && p.y <= top + 0.1f;
                }

                public bool ContainsRaw(Vector2 p)
                {
                        Vector2 position = rawPosition;
                        return p.x > position.x && p.x < (position.x + size.x) && p.y > position.y && p.y < (position.y + size.y);
                }

                public bool DetectRaw(Vector2 p)
                {
                        return p.x > position.x && p.x < (position.x + size.x) && p.y > position.y && p.y < (position.y + size.y);
                }

                public void MoveRaw(Vector2 p)
                {
                        if (p.x < position.x)
                                position.x = p.x;
                        if (p.x > (position.x + size.x))
                                position.x = (p.x - size.x);
                        if (p.y < position.y)
                                position.y = p.y;
                        if (p.y > (position.y + size.y))
                                position.y = p.y - size.y;
                }

        }

        [System.Serializable]
        public class Bounds
        {
                [SerializeField] public Vector2 position = new Vector2(0, 0);
                [SerializeField] public Vector2 size = new Vector2(10f, 5f);

                [SerializeField, HideInInspector] public float detectTop;
                [SerializeField, HideInInspector] public float detectLeft;
                [SerializeField, HideInInspector] public float detectRight;
                [SerializeField, HideInInspector] public float detectBottom;
                [SerializeField, HideInInspector] public float boundsTop;
                [SerializeField, HideInInspector] public float boundsLeft;
                [SerializeField, HideInInspector] public float boundsRight;
                [SerializeField, HideInInspector] public float boundsBottom;

                public float width => size.x * 0.5f;
                public float height => size.y * 0.5f;

                public float top { get; set; }
                public float left { get; private set; }
                public float right { get; private set; }
                public float bottom { get; private set; }
                public Vector2 center => position + size * 0.5f;

                public void Initialize()
                {
                        Vector2 center = this.center;

                        boundsLeft = center.x - size.x * 0.5f * (detectLeft == 0 ? 1f : detectLeft);
                        boundsRight = center.x + size.x * 0.5f * (detectRight == 0 ? 1f : detectRight);
                        boundsTop = center.y + size.y * 0.5f * (detectTop == 0 ? 1f : detectTop);
                        boundsBottom = center.y - size.y * 0.5f * (detectBottom == 0 ? 1f : detectBottom);

                        left = center.x - size.x * 0.5f;
                        right = center.x + size.x * 0.5f;
                        top = center.y + size.y * 0.5f;
                        bottom = center.y - size.y * 0.5f;
                }

                public bool DetectBounds(Vector2 p)
                {
                        return p.x > boundsLeft && p.x < boundsRight && p.y > boundsBottom && p.y < boundsTop;
                }

                public bool DetectBounds(Vector2 p, float buffer = 1f)
                {
                        return p.x > boundsLeft + buffer && p.x < boundsRight - buffer && p.y > boundsBottom + buffer && p.y < boundsTop - buffer;
                }

                public bool DetectWalls(Vector2 p)
                {
                        return p.x > left && p.x < right && p.y > bottom && p.y < top;
                }

                public bool DetectRaw(Vector2 p)
                {
                        return p.x > position.x && p.x < (position.x + size.x) && p.y > position.y && p.y < (position.y + size.y);
                }

                public bool Overlap(Bounds other)
                {
                        return boundsLeft < other.boundsRight && boundsRight > other.boundsLeft && boundsTop > other.boundsBottom && boundsBottom < other.boundsTop;
                }
        }
}