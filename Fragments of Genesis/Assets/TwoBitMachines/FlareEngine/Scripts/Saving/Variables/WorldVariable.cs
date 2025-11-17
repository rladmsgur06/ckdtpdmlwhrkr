using System.Collections.Generic;
using UnityEngine;

namespace TwoBitMachines.FlareEngine
{
        [AddComponentMenu("")]
        public class WorldVariable : MonoBehaviour
        {
                [SerializeField] public bool isHealth = false;
                [SerializeField] public bool initialized = false;
                public static List<WorldVariable> variables = new List<WorldVariable>();

                private void OnEnable ()
                {
                        if (!variables.Contains(this))
                        {
                                variables.Add(this);
                        }
                        Register();
                }

                public static void SaveData ()
                {
                        for (int i = 0; i < variables.Count; i++)
                        {
                                variables[i].Save();
                        }
                }

                public static void ResetAndClear ()
                {
                        for (int i = 0; i < variables.Count; i++)
                        {
                                variables[i].ClearTempValue();
                                variables[i].Reset();
                        }
                }

                public static void ClearTempChildren ()
                {
                        variables.Clear();
                        Health.health.Clear();
                }

                public virtual void Initialize () { }

                public virtual void Save () { }

                public virtual void Reset () { }

                public virtual void ClearTempValue () { }

                public virtual void Register () { }

                public virtual bool IncrementValue (Transform aggressor, float floatValue, Vector2 direction) { return false; }

                public virtual void InternalSet (float newValue) { }

                public virtual void DeleteSavedData () { }

                public virtual string Name () { return ""; }
        }

        [System.Serializable]
        public class SaveFloat
        {
                public float value;
        }

        [System.Serializable]
        public class SaveString
        {
                public string value;
        }

        [System.Serializable]
        public class SaveVector3
        {
                public Vector3 value;
        }

        [System.Serializable]
        public class SaveBool
        {
                public bool value;
        }

        [System.Serializable]
        public class SaveStringList
        {
                public List<string> list = new List<string>();
        }

        public enum SaveType
        {
                Automatic,
                Manually
        }
}
