using System.Collections.Generic;
using UnityEngine;

namespace TwoBitMachines.FlareEngine
{
        public class QuestSO : JournalObject
        {
                [SerializeField] public Sprite icon;
                [SerializeField] public string title; //unique name
                [SerializeField] public float goal;
                [SerializeField] public string extraInfo;
                [SerializeField] public string description;
                [SerializeField] public List<QuestRewards> rewards = new List<QuestRewards> ( );
                [SerializeField, HideInInspector] public SaveQuest saveQuest = new SaveQuest ( );

                public float progress => saveQuest.progress;

                #region ▀▄▀▄▀▄ Editor Variables ▄▀▄▀▄▀
                #if UNITY_EDITOR
                #pragma warning disable 0414
                [SerializeField, HideInInspector] public bool add;
                [SerializeField, HideInInspector] public bool foldOut;
                [SerializeField, HideInInspector] public bool remove;
                [SerializeField, HideInInspector] public bool delete;
                [SerializeField, HideInInspector] public bool deleteAsk;
                [SerializeField, HideInInspector] public bool deleteData;
                [SerializeField, HideInInspector] public bool descriptionFoldOut;
                [SerializeField, HideInInspector] public bool extraInfoFoldOut;
                #endif
                #endregion

                public void LoadQuestStatus ( ) // needs to be called at the beginning of the scene
                {
                        if (!saveQuest.hasBeenLoaded)
                        {
                                saveQuest.Clear ( );
                                saveQuest.hasBeenLoaded = true;
                                saveQuest = Storage.Load<SaveQuest> (saveQuest, WorldManager.saveFolder, title);
                        }
                }

                public void AcceptQuest ( )
                {
                        LoadQuestStatus ( );
                        saveQuest.isActive = true;
                        Storage.Save (saveQuest, WorldManager.saveFolder, title);
                }

                public void CompleteQuest ( )
                {
                        LoadQuestStatus ( );
                        saveQuest.isComplete = true;
                        Storage.Save (saveQuest, WorldManager.saveFolder, title);
                        ApplyRewards ( );
                }

                public bool ProgressQuest ( )
                {
                        LoadQuestStatus ( );
                        saveQuest.progress++;
                        Storage.Save (saveQuest, WorldManager.saveFolder, title);
                        return saveQuest.progress >= goal;
                }

                public bool IsActive ( )
                {
                        LoadQuestStatus ( );
                        return saveQuest.isActive;
                }

                public bool IsComplete ( )
                {
                        LoadQuestStatus ( );
                        return saveQuest.isComplete;
                }

                private void ApplyRewards ( )
                {
                        for (int i = 0; i < rewards.Count; i++)
                        {
                                rewards[i].ApplyReward ( );
                        }
                }

                public void DeleteSavedData ( )
                {
                        Storage.Delete (WorldManager.saveFolder, title);
                }

                #region Journal
                public override string Name ( )
                {
                        return title;
                }

                public override string Description ( )
                {
                        return description;
                }

                public override string ExtraInfo ( )
                {
                        return "";
                }

                public override bool Complete ( )
                {
                        return IsComplete ( );
                }

                public override Sprite Icon ( )
                {
                        return icon;
                }
                #endregion
        }

        [System.Serializable]
        public class SaveQuest
        {
                [SerializeField] public int progress = 0;
                [SerializeField] public bool isActive = false;
                [SerializeField] public bool isComplete = false;
                [System.NonSerialized] public bool hasBeenLoaded = false;

                public void Clear ( )
                {
                        isActive = isComplete = false;
                        progress = 0;
                }
        }

        [System.Serializable]
        public class QuestRewards
        {
                [SerializeField] public Sprite icon;
                [SerializeField] public float reward;
                [SerializeField] public string name;
                [SerializeField] public string description;
                [SerializeField] public WorldFloatSO worldFloat;
                [SerializeField] public InventorySO inventorySO;
                [SerializeField] public ItemSO itemSO;

                public void ApplyReward ( )
                {
                        if (worldFloat != null)
                        {
                                worldFloat.IncrementValue (reward);
                                worldFloat.Save ( );
                        }
                        if (inventorySO != null && itemSO != null)
                        {
                                inventorySO.AddToInventory (itemSO, 0);
                        }
                }

                #region ▀▄▀▄▀▄ Editor Variables ▄▀▄▀▄▀
                #if UNITY_EDITOR
                #pragma warning disable 0414
                [SerializeField, HideInInspector] public bool descriptionFoldOut;
                #endif
                #endregion
        }

}