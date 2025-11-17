using UnityEngine;

namespace TwoBitMachines.FlareEngine
{
        public class WorldBoolSO : ScriptableObject
        {
                public bool value;
                private WorldBool reference;

                public void SetWorldValue (bool newValue)
                {
                        value = newValue;
                }

                public void Register (WorldBool worldBool)
                {
                        this.reference = worldBool;
                }

                public void SetValue (bool newValue)
                {
                        if (reference != null)
                        {
                                reference.SetValue (newValue);
                        }
                }
        }
}