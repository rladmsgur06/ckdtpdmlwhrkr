using UnityEngine;

namespace cowsins2D
{
    [System.Serializable]
    public class SurfaceEffect
    {
        [Tooltip("Type the name of your surface here.")]public string name = "Type the name of your surface here.";
        [Tooltip("Effect to display for the player steps")] public GameObject stepVFX;
        [Tooltip("Sound for the player steps")] public AudioClip stepSFX;
        [Tooltip("Effect to display for the player landing")] public GameObject landVFX;
        [Tooltip("Sound for the player landing")] public AudioClip landSFX;
        [Tooltip("Speed multiplier on this certain surface.")] [Range(.3f,2)]public float speedModifier = 1; 
    }
}
