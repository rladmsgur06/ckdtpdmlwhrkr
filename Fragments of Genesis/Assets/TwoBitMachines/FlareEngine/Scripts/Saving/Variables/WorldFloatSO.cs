using UnityEngine;

namespace TwoBitMachines.FlareEngine
{
        public class WorldFloatSO : ScriptableObject
        {
                [SerializeField] public float value;
                [System.NonSerialized] private WorldFloat reference;

                public void SetWorldValue (float newValue)
                {
                        value = newValue;
                }

                public float GetValue ()
                {
                        return reference != null ? reference.GetValue() : value;
                }

                public void Register (WorldFloat worldFloat)
                {
                        reference = worldFloat;
                }

                public void IncrementValue (float newValue)
                {
                        if (reference != null)
                        {
                                reference.Increment(newValue);
                        }
                }

                public void SetValue (float newValue)
                {
                        if (reference != null)
                        {
                                reference.SetValue(newValue);
                        }
                }

                public void Save ()
                {
                        if (reference != null)
                        {
                                reference.Save();
                        }
                }

                public void SaveManually ()
                {
                        if (reference != null)
                        {
                                reference.SaveManually();
                        }
                }
        }
}
