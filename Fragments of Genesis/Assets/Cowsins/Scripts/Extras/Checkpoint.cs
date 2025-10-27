using UnityEngine;
using UnityEngine.Events;

namespace cowsins2D
{
    public class Checkpoint : Trigger
    {
        public override void EnterTrigger(GameObject target)
        {
            // Store a new checkpoint in the checkpoint manager
            CheckPointManager.Instance.SetCheckpoint(this.transform);
            Destroy(this);
            base.EnterTrigger(target);
        }
    }
}