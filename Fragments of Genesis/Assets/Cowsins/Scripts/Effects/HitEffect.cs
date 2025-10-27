using UnityEngine;

namespace cowsins2D
{
    [System.Serializable]
    public class HitEffect
    {
        [Tooltip("Name of the hit effect. This should match the name of the layer.")]public string name;
        [Tooltip("Effect to display.")]public GameObject hitVFX;
    }
}
