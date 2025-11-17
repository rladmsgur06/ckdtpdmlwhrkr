using UnityEngine;

namespace TwoBitMachines.FlareEngine.AI.BlackboardData
{
        [AddComponentMenu("")]
        public class IntVariable : Blackboard
        {
                public int value;

                public override float GetValue ()
                {
                        return value;
                }


                public override void Set (float newValue)
                {
                        value = (int) newValue;
                }
        }
}
