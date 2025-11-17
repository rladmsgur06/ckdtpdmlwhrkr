using UnityEngine;

namespace TwoBitMachines.MapSystem
{
        [System.Serializable]
        public class Point
        {
                [SerializeField] public Vector2 position;
                [SerializeField] public Vector2 offsetEnd;
                [SerializeField] public Vector2 offsetStart;
                [SerializeField] public bool invisible = false;

                public Vector2 controlEnd => position + offsetEnd;
                public Vector2 controlStart => position + offsetStart;

                public Point (Vector2 position)
                {
                        this.position = position;
                }
        }
}
