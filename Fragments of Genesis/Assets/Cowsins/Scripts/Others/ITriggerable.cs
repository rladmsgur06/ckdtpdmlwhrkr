using UnityEngine;

namespace cowsins2D
{
    public interface ITriggerable  
    { 
        void EnterTrigger(GameObject target);
        void StayTrigger(GameObject target);
        void ExitTrigger(GameObject target); 
    }
}
