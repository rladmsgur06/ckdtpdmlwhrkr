using UnityEngine;

namespace TwoBitMachines.FlareEngine.AI.BlackboardData
{
        [AddComponentMenu ("")]
        public class QuaternionVariable : Blackboard
        {
                public Quaternion value;

                public override Quaternion GetQuaternion ( )
                {
                        return value;
                }

                public override void SetQuaternion (Quaternion newValue)
                {
                        value = newValue;
                }
        }
}