using UnityEngine;

namespace TwoBitMachines.FlareEngine
{
        [AddComponentMenu ("")]
        public class CameraShake : ReactionBehaviour
        {
                [SerializeField] public string shakeName;
                [SerializeField] public Safire2DCamera.Safire2DCamera safire;

                public override void Activate (ImpactPacket packet)
                {
                        if (safire != null)
                        {
                                safire.Shake (shakeName);
                                return;
                        }
                        Safire2DCamera.Safire2DCamera.mainCamera?.Shake (shakeName);
                }
        }
}