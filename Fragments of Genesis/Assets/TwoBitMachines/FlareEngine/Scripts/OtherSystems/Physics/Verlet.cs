using UnityEngine;
namespace TwoBitMachines.FlareEngine
{

        [System.Serializable]
        public class Particle
        {
                [SerializeField] public float x;
                [SerializeField] public float y;
                [SerializeField] public float oldx;
                [SerializeField] public float oldy;
                [SerializeField] public float gravity;
                [SerializeField] public bool anchor;
                [SerializeField] public Vector2 acceleration;

                public Vector2 position => new Vector2(x, y);
                public Vector2 oldPosition => new Vector2(oldx, oldy);
                public Vector2 velocity => position - oldPosition;

                public Particle () { }

                public Particle (Vector2 position, float gravity, bool anchor)
                {
                        Set(position, gravity, anchor);
                }

                public void Set (Vector2 position, float gravity, bool anchor)
                {
                        SetPosition(position);
                        this.gravity = gravity;
                        this.anchor = anchor;
                }

                public void SetPosition (Vector2 position)
                {
                        x = oldx = position.x;
                        y = oldy = position.y;
                }

                public void FixedUpdate ()
                {
                        if (anchor)
                                return;

                        float vx = x - oldx;
                        float vy = y - oldy;
                        oldx = x;
                        oldy = y;

                        x += vx;
                        y += vy;
                        y += gravity;
                        x += acceleration.x;
                        y += acceleration.y;
                        acceleration = Vector2.zero;
                }

                public void FixedUpdate (float frictionX, float frictionY)
                {
                        if (anchor)
                                return;

                        float vx = x - oldx;
                        float vy = y - oldy;
                        oldx = x;
                        oldy = y;

                        x += vx * frictionX;
                        y += vy * frictionY;
                        y += gravity;
                        x += acceleration.x;
                        y += acceleration.y;
                        acceleration = Vector2.zero;
                }

                public void Update (float delta, float yLimit)
                {
                        if (anchor)
                                return;

                        float vx = x - oldx;
                        float vy = y - oldy;
                        oldx = x;
                        oldy = y;

                        x += vx * delta;
                        y += vy * delta;
                        y += gravity * delta;
                        x += acceleration.x * delta;
                        y += acceleration.y * delta;
                        acceleration = Vector2.zero;

                        if (y < yLimit)
                                y = oldy = yLimit;
                }

                public void ApplyAcceleration (Vector2 accel)
                {
                        this.acceleration = accel;
                }

                public void Anchor (bool value)
                {
                        anchor = value;
                        acceleration = Vector2.zero;
                }
        }

        [System.Serializable]
        public class Stick
        {
                [SerializeField] public int first;
                [SerializeField] public int second;
                [SerializeField] public float length = 1;

                public Stick () { }

                public Stick (int first, int second, float length)
                {
                        Set(first, second, length);
                }

                public void Set (int first, int second, float length)
                {
                        this.first = first;
                        this.second = second;
                        this.length = length;
                }

                public void SetLength (float length)
                {
                        this.length = length;
                }

                public void FixedUpdate (Particle[] list)
                {
                        Particle a = list[first];
                        Particle b = list[second];
                        float dx = b.x - a.x;
                        float dy = b.y - a.y;

                        float squareDistance = dx * dx + dy * dy;
                        squareDistance = squareDistance == 0 ? length * length : squareDistance;
                        float difference = length * length - squareDistance;
                        float percent = difference / squareDistance * 0.25f; // percent of distance each point must move back to length

                        float offsetX = dx * percent;
                        float offsetY = dy * percent;

                        if (!a.anchor)
                        {
                                a.x -= offsetX;
                                a.y -= offsetY;
                        }
                        if (!b.anchor)
                        {
                                b.x += offsetX;
                                b.y += offsetY;
                        }
                }

                public void FixedUpdate (Particle a, Particle b)
                {
                        float dx = b.x - a.x;
                        float dy = b.y - a.y;

                        float squareDistance = dx * dx + dy * dy;
                        squareDistance = squareDistance == 0 ? length * length : squareDistance;
                        float difference = length * length - squareDistance;
                        float percent = difference / squareDistance * 0.25f; // percent of distance each point must move back to length

                        float offsetX = dx * percent;
                        float offsetY = dy * percent;

                        if (!a.anchor)
                        {
                                a.x -= offsetX;
                                a.y -= offsetY;
                        }
                        if (!b.anchor)
                        {
                                b.x += offsetX;
                                b.y += offsetY;
                        }
                }

                public void ApplyAcceleration (Particle[] list, Vector2 accel)
                {
                        list[first].ApplyAcceleration(accel);
                        list[second].ApplyAcceleration(accel);
                }

        }
}
