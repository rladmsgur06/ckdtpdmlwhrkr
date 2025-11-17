using UnityEngine;

namespace TwoBitMachines.FlareEngine.AI.BlackboardData
{
        [AddComponentMenu("")]
        public class TargetAI : Blackboard
        {
                [SerializeField] public Vector2 offset;

                public override Vector2 GetTarget (int index = 0)
                {
                        return (Vector2) transform.position + offset;
                }

                public override void Set (Vector3 vector3)
                {
                        if (transform != null)
                                transform.position = vector3 + (Vector3) offset;
                }

                public override void Set (Vector2 vector2)
                {
                        if (transform != null)
                                transform.position = vector2 + offset;
                }

                public override Transform GetTransform ()
                {
                        return this.transform;
                }

                public override Vector2 GetOffset ()
                {
                        return offset;
                }

        }
}
