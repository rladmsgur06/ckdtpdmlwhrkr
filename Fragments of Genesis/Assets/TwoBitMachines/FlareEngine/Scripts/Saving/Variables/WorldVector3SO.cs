using UnityEngine;

namespace TwoBitMachines.FlareEngine
{
        public class WorldVector3SO : ScriptableObject
        {
                public Vector3 value;
                private WorldVector3 reference;

                public void SetWorldValue (Vector3 newValue)
                {
                        value = newValue;
                }

                public void Register (WorldVector3 worldVector3)
                {
                        this.reference = worldVector3;
                }

                public void SetValue (Vector3 newValue)
                {
                        if (reference != null)
                        {
                                reference.SetValue (newValue);
                        }
                }
        }
}