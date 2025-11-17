using UnityEngine;

namespace TwoBitMachines.FlareEngine.AI.BlackboardData
{
        [AddComponentMenu("")]
        public class StringVariable : Blackboard
        {
                public string value;

                public override void Set (string stringValue)
                {
                        value = stringValue;
                }
        }
}
