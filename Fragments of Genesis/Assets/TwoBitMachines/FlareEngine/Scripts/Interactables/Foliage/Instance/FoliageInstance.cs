using UnityEngine;

namespace TwoBitMachines.FlareEngine.Interactables
{
        [System.Serializable]
        public class FoliageInstance
        {
                public Vector3 position;
                public int textureIndex = 0;
        }

        public enum FoliageBrush
        {
                Single,
                Random,
                Eraser,
                None
        }
}