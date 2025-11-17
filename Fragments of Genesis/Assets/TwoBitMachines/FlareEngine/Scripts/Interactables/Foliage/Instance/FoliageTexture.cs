using UnityEngine;

namespace TwoBitMachines.FlareEngine.Interactables
{
        [System.Serializable]
        public class FoliageTexture
        {
                public Texture2D texture;
                public float interact = 1f;
                public float z = -1f;
                public FoliageOrientation orientation;

                #region ▀▄▀▄▀▄ Editor Variables ▄▀▄▀▄▀
                #if UNITY_EDITOR
                #pragma warning disable 0414
                [SerializeField] public bool isRandom;
                #pragma warning restore 0414
                #endif
                #endregion
        }

        public enum FoliageOrientation
        {
                Bottom = 0,
                Top = 1,
                Left = 2,
                Right = 3
        }
}