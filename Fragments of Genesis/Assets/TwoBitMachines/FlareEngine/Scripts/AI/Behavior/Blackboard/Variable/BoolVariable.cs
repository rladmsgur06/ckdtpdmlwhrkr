using UnityEngine;

namespace TwoBitMachines.FlareEngine.AI.BlackboardData
{
        [AddComponentMenu ("")]
        public class BoolVariable : Blackboard
        {
                public bool value;

                public override float GetValue ( )
                {
                        return value ? 1f : 0;
                }

                public override void Set (bool newValue)
                {
                        value = newValue;
                }

                public override void Set (float newValue)
                {
                        value = newValue > 0;
                }

        }
}