using System.Collections.Generic;
using UnityEngine;

namespace TwoBitMachines.FlareEngine
{

        public class GameDifficulty : MonoBehaviour
        {
                [SerializeField] public GameDifficultySave difficulty = new GameDifficultySave();

                public void Awake()
                {
                        Restore();
                }

                public void Start()
                {
                        difficulty.Execute();
                }

                public void OnDestroy()
                {
                        Save();
                }

                public void ChangeDifficultyAndSave(int newDifficulty)
                {
                        difficulty.difficulty = newDifficulty;
                        Save();
                }

                public void Save()
                {
                        Storage.Save(difficulty, WorldManager.saveFolder, "GameDifficulty");
                }

                public void Restore()
                {
                        difficulty = Storage.Load<GameDifficultySave>(difficulty, WorldManager.saveFolder, "GameDifficulty");
                }

                public void Restore(string saveFolder)
                {
                        difficulty = Storage.Load<GameDifficultySave>(difficulty, saveFolder, "GameDifficulty");
                }

                public void Save(string saveFolder)
                {
                        Storage.Save(difficulty, saveFolder, "GameDifficulty");
                }

                public int DifficultyLevel()
                {
                        return difficulty.difficulty;
                }

        }

        [System.Serializable]
        public class GameDifficultySave
        {
                [SerializeField] public int difficulty = 0;
                [SerializeField] public List<DifficultyLevel> level = new List<DifficultyLevel>();


                #region ▀▄▀▄▀▄ Editor Variables ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField, HideInInspector] private int signalIndex;
                [SerializeField, HideInInspector] private bool active;
                [SerializeField, HideInInspector] private bool foldOut;
#pragma warning restore 0414
#endif
                #endregion
                public void Execute()
                {
                        for (int i = 0; i < level.Count; i++)
                        {
                                if (i == difficulty)
                                {
                                        level[i].Execute();
                                        return;
                                }
                        }
                }
        }

        [System.Serializable]
        public class DifficultyLevel
        {
                [SerializeField] public List<DifficultyBehaviour> behaviour = new List<DifficultyBehaviour>();

                #region ▀▄▀▄▀▄ Editor Variables ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField, HideInInspector] private bool add;
                [SerializeField, HideInInspector] private bool delete;
                [SerializeField, HideInInspector] private bool foldOut;
#pragma warning restore 0414
#endif
                #endregion
                public void Execute()
                {
                        for (int i = 0; i < behaviour.Count; i++)
                        {
                                behaviour[i].Execute();
                        }
                }
        }

        [System.Serializable]
        public class DifficultyBehaviour
        {
                [SerializeField] public DifficultyBehaviourType type;
                [SerializeField] public float value;

                public void Execute() // execute on start
                {
                        if (type == DifficultyBehaviourType.MultiplyEnemyDamage)
                        {
                                AI.AIDamage.difficulty = value;
                        }
                }
        }


        public enum DifficultyBehaviourType
        {
                MultiplyEnemyDamage
        }
}