using UnityEngine;

namespace TwoBitMachines.FlareEngine.AI.BlackboardData
{
        [AddComponentMenu("")]
        public class TransformVariable : Blackboard
        {
                public Transform value;

                public override Vector2 GetTarget (int index = 0)
                {
                        return value != null ? value.position : this.transform.position;
                        ;
                }

                public override void Set (Transform transform)
                {
                        value = transform;
                }

                public override Transform GetTransform ()
                {
                        return value;
                }

                public override void Set (Vector3 vector3)
                {
                        if (value != null)
                                value.transform.position = vector3;
                }

                public override void Set (Vector2 vector2)
                {
                        if (value != null)
                                value.transform.position = vector2;
                }
        }
}
