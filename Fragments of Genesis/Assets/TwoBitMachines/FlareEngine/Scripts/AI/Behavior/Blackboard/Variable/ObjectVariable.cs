using UnityEngine;

namespace TwoBitMachines.FlareEngine.AI.BlackboardData
{
        [AddComponentMenu ("")]
        public class ObjectVariable : Blackboard
        {
                public Object value;

                public override Object GetObject ( )
                {
                        return value;
                }

                public override void SetObject (Object newValue)
                {
                        value = newValue;
                }
        }
}