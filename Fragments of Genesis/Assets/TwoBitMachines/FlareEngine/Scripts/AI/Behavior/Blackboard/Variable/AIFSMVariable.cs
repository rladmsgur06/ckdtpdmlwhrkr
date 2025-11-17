using UnityEngine;

namespace TwoBitMachines.FlareEngine.AI.BlackboardData
{
        [AddComponentMenu("")]
        public class AIFSMVariable : Blackboard
        {
                public AIFSM value;

                public override AIFSM GetAIFSM ()
                {
                        return value;
                }
        }
}
