using UnityEngine;

namespace TwoBitMachines.FlareEngine
{
        public class SpriteTrailUnit : MonoBehaviour
        {
                [System.NonSerialized] public SpriteTrailEffect effect;

                private void LateUpdate ()
                {
                        if (effect != null)
                        {
                                effect.RunTrail();
                        }
                }
        }
}
