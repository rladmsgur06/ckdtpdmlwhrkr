using UnityEngine;

namespace TwoBitMachines.FlareEngine.AI.BlackboardData
{
        [AddComponentMenu ("")]
        public class FloatVariable : Blackboard
        {
                public float value;

                public override float GetValue ( )
                {
                        return value;
                }

                public override void Set (float newValue)
                {
                        value = newValue;
                }
        }
}