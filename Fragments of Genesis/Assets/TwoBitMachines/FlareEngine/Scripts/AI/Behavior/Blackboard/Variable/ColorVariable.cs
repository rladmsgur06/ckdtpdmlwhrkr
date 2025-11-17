using UnityEngine;

namespace TwoBitMachines.FlareEngine.AI.BlackboardData
{
        [AddComponentMenu ("")]
        public class ColorVariable : Blackboard
        {
                public Color value;

                public override Color GetColor ( )
                {
                        return value;
                }

                public override void SetColor (Color newValue)
                {
                        value = newValue;
                }
        }
}