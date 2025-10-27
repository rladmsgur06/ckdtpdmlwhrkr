using UnityEngine;

namespace cowsins2D
{
    [System.Serializable]
    public class Padding
    {
        public float horizontal;
        public float vertical;

        public Padding(float x = 0, float y = 0)
        {
            this.horizontal = x;
            this.vertical = y;
        }

        public Vector2 ToVector2()
        {
            return new Vector2(horizontal, vertical);
        }

        public Vector3 ToVector3()
        {
            return new Vector3(horizontal, vertical, 0);
        }
    }
}