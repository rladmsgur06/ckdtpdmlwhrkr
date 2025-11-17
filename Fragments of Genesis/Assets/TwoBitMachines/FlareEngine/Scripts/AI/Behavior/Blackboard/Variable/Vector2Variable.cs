using UnityEngine;

namespace TwoBitMachines.FlareEngine.AI.BlackboardData
{
        [AddComponentMenu("")]
        public class Vector2Variable : Blackboard
        {
                public Vector2 value;

                public override Vector2 GetTarget (int index = 0)
                {
                        return value;
                }

                public override Vector3 GetVector ()
                {
                        return value;
                }

                public override void Set (Vector2 vector2)
                {
                        value = vector2;
                }

                public override void Set (Vector3 vector3)
                {
                        value = vector3;
                }
        }
}
