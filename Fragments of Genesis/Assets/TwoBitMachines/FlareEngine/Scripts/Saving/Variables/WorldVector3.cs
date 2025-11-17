using UnityEngine;

namespace TwoBitMachines.FlareEngine
{
        [AddComponentMenu("Flare Engine/一Saving/WorldVector3")]
        public class WorldVector3 : WorldVariable
        {
                [SerializeField] public string variableName = "name"; // name must be unique
                [SerializeField] private Vector3 currentValue;
                [SerializeField] private bool isScriptableObject = false;
                [SerializeField] private WorldVector3SO soReference;
                [SerializeField] private bool save = false;
                [SerializeField] private UnityEventVector3 afterLoad = new UnityEventVector3();
                [SerializeField] private SaveVector3 saveVector = new SaveVector3();

                private bool sOAvailable => isScriptableObject && soReference != null;

                #region EDITOR
#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField, HideInInspector] private bool foldOut = false;
                [SerializeField, HideInInspector] private bool eventFoldOut = false;
                [SerializeField, HideInInspector] private bool loadFoldOut = false;
                [SerializeField, HideInInspector] private bool createSO = false;
#pragma warning restore 0414
#endif
                #endregion

                private void Start ()
                {
                        Initialize();
                }

                public override void Initialize ()
                {
                        SetSOValue();
                        if (save)
                        {
                                RestoreValue();
                        }
                        afterLoad.Invoke(currentValue);
                        initialized = true;
                }

                public override void Register ()
                {
                        if (sOAvailable)
                        {
                                soReference.Register(this);
                        }
                        if (initialized)
                        {
                                Initialize();
                        }
                }

                public void RestoreValue ()
                {
                        saveVector.value = currentValue;
                        SaveVector3 newSavedVector3 = Storage.Load<SaveVector3>(saveVector, WorldManager.saveFolder, variableName);
                        if (newSavedVector3 != null)
                        {
                                currentValue = newSavedVector3.value;
                                SetSOValue();
                        }
                }

                public override void Save ()
                {
                        if (save)
                        {
                                saveVector.value = currentValue;
                                Storage.Save(saveVector, WorldManager.saveFolder, variableName);
                        }
                }

                public override void DeleteSavedData ()
                {
                        Storage.Delete(WorldManager.saveFolder, variableName);
                }

                public void Refresh (Vector3 value)
                {
                        currentValue = value;
                }

                public void SetValue (Vector3 value)
                {
                        currentValue = value;
                        SetSOValue();
                }

                public void SetValueAndSave (Vector3 value)
                {
                        currentValue = value;
                        SetSOValue();
                        Save();
                }

                public Vector3 GetValue ()
                {
                        return currentValue;
                }

                private void SetSOValue ()
                {
                        if (sOAvailable)
                        {
                                soReference.SetWorldValue(currentValue);
                        }
                }
        }
}
