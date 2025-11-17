using UnityEngine;

namespace TwoBitMachines.FlareEngine
{
        public class RememberTransform : ReactionBehaviour
        {
                [SerializeField] public TransformTracker tracker;

                public override void Activate (ImpactPacket impact)
                {
                        if (tracker != null)
                        {
                                tracker.AddToList (impact);
                        }
                        else if (TransformTracker.get != null)
                        {
                                TransformTracker.get.AddToList (impact);
                        }
                }

                public void Activate ( )
                {
                        if (tracker != null)
                        {
                                tracker.AddToList (transform);
                        }
                        else if (TransformTracker.get != null)
                        {
                                TransformTracker.get.AddToList (transform);
                        }
                }
        }
}