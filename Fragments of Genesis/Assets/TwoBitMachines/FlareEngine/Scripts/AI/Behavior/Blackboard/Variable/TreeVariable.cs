using UnityEngine;

namespace TwoBitMachines.FlareEngine.AI.BlackboardData
{
        [AddComponentMenu ("")]
        public class TreeVariable : Blackboard
        {
                public AITree value;

                public override AITree GetTree ( )
                {
                        return value;
                }
        }
}