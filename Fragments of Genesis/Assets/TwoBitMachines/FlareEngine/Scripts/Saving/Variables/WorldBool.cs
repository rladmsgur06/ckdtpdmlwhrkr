using UnityEngine;

namespace TwoBitMachines.FlareEngine
{
        [AddComponentMenu("Flare Engine/一Saving/WorldBool")]
        public class WorldBool : WorldVariable
        {
                [SerializeField] public string variableName = "name"; // name must be unique
                [SerializeField] private bool currentValue;
                [SerializeField] private bool isScriptableObject = false;
                [SerializeField] private WorldBoolSO soReference;
                [SerializeField] private bool save = false;
                [SerializeField] private bool saveManually = false;
                [SerializeField] private UnityEventBool onLoadConditionTrue = new UnityEventBool();
                [SerializeField] private UnityEventBool onLoadConditionFalse = new UnityEventBool();
                [SerializeField] private SaveBool saveBool = new SaveBool();

                private bool sOAvailable => isScriptableObject && soReference != null;

                #region 
#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField, HideInInspector] private bool foldOut = false;
                [SerializeField, HideInInspector] private bool eventFoldOut = false;
                [SerializeField, HideInInspector] private bool saveFoldOut = false;
                [SerializeField, HideInInspector] private bool objFoldOut = false;
                [SerializeField, HideInInspector] private bool loadFoldOutTrue = false;
                [SerializeField, HideInInspector] private bool loadFoldOutFalse = false;
                [SerializeField, HideInInspector] private bool createSO = false;
                [SerializeField, HideInInspector] private bool isSceneName = false;
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
                        if (IsTrue())
                        {
                                onLoadConditionTrue.Invoke(currentValue);
                        }
                        else
                        {
                                onLoadConditionFalse.Invoke(currentValue);
                        }
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
                                Initialize(); // register is called from onEnable, ensure objects is always initialized on enable but only if the start method has been used
                        }
                }

                public override void Save ()
                {
                        if (save && !saveManually)
                        {
                                saveBool.value = currentValue;
                                Storage.Save(saveBool, WorldManager.saveFolder, variableName);
                        }
                }

                public void RestoreValue ()
                {
                        saveBool.value = currentValue;
                        SaveBool newSavedBool = Storage.Load<SaveBool>(saveBool, WorldManager.saveFolder, variableName);
                        if (newSavedBool != null)
                        {
                                currentValue = newSavedBool.value;
                                SetSOValue();
                        }
                }

                public void SaveManually ()
                {
                        saveBool.value = currentValue;
                        Storage.Save(saveBool, WorldManager.saveFolder, variableName);
                }

                public override void DeleteSavedData ()
                {
                        Storage.Delete(WorldManager.saveFolder, variableName);
                }

                public void Refresh (bool value)
                {
                        currentValue = value;
                }

                public void SetValue (bool value)
                {
                        currentValue = value;
                        SetSOValue();
                }

                public void SetValueAndSave (bool value)
                {
                        currentValue = value;
                        SetSOValue();
                        Save();
                }

                public void SetTrue ()
                {
                        currentValue = true;
                        SetSOValue();
                }

                public void SetFalse ()
                {
                        currentValue = false;
                        SetSOValue();
                }

                public bool IsTrue ()
                {
                        return currentValue == true;
                }

                public bool IsFalse ()
                {
                        return currentValue == false;
                }

                public bool GetValue ()
                {
                        return currentValue;
                }

                public void WorldBoolTrackerRegister ()
                {
                        if (WorldBoolTracker.get != null)
                        {
                                WorldBoolTracker.get.Register(this);
                        }
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
