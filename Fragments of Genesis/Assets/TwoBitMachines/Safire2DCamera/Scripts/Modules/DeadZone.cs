using UnityEngine;

namespace TwoBitMachines.Safire2DCamera
{
        [System.Serializable]
        public class DeadZone
        {
                [SerializeField] public Vector2 size;

                [System.NonSerialized] public Vector2 originalSize;
                [System.NonSerialized] public Zone x = new Zone ( );
                [System.NonSerialized] public Zone y = new Zone ( );
                public Vector2 center => new Vector2 (x.position, y.position);

                public void Set (Vector3 target)
                {
                        x.Set (target.x, Mathf.Abs (originalSize.x = size.x));
                        y.Set (target.y, Mathf.Abs (originalSize.y = size.y));
                }

                public Vector3 Position (Vector3 target, bool isUser)
                {
                        return new Vector2 (size.x == 0 || isUser ? target.x : x.Push (target.x), size.y == 0 || isUser ? target.y : y.Push (target.y));
                }

                public class Zone
                {
                        private float a, b; // zone walls, p = target position
                        public float position => (a + b) * 0.5f;

                        public void Set (float p, float size)
                        {
                                a = p - size * 0.5f;
                                b = p + size * 0.5f;
                        }

                        public float Push (float p)
                        {
                                float shift = 0;
                                shift = p < a ? p - a : shift;
                                shift = p > b ? p - b : shift;
                                a += shift;
                                b += shift;
                                return (a + b) * 0.5f;
                        }
                }
        }
}