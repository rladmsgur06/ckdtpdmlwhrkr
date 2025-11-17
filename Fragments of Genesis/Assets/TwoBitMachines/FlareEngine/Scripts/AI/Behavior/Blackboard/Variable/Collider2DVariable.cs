using UnityEngine;

namespace TwoBitMachines.FlareEngine.AI.BlackboardData
{
        [AddComponentMenu("")]
        public class Collider2DVariable : Blackboard
        {
                public Collider2D value;

                public override Vector2 GetTarget (int index = 0)
                {
                        return value != null ? (Vector2) value.transform.position : (Vector2) this.transform.position;
                        ;
                }

                public override void Set (Collider2D collider2D)
                {
                        value = collider2D;
                }

                public override void Set (Vector3 vector3)
                {
                        if (value.transform != null)
                                value.transform.position = vector3;
                }

                public override void Set (Vector2 vector2)
                {
                        if (value.transform != null)
                                value.transform.position = vector2;
                }

                public override Transform GetTransform ()
                {
                        return value != null ? value.transform : null;
                }
        }
}
