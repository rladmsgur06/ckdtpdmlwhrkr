using UnityEngine;

namespace TwoBitMachines.Safire2DCamera
{
        public class AddFiniteParallax : MonoBehaviour
        {
                [SerializeField] public Vector2 parallaxRate = new Vector2(0.1f , 0f);
                [SerializeField] public Vector2 startingOffset = Vector2.zero;
                [SerializeField] public bool mustBeVisible = true;

                private void Start ()
                {
                        ParallaxFinite.AddParallax(transform , parallaxRate , startingOffset , mustBeVisible);
                }
        }
}
