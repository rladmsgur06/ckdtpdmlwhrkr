using UnityEngine;

namespace TwoBitMachines.FlareEngine.AI.BlackboardData
{
        [AddComponentMenu("")]
        public class RaycastHit2DVariable : Blackboard
        {
                public RaycastHit2D raycastHit2D;

                public override Vector2 GetTarget (int index = 0)
                {
                        return raycastHit2D.transform != null ? (Vector2) raycastHit2D.transform.position : Vector2.zero;
                }
        }
}
