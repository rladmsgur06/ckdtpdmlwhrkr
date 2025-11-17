using UnityEngine;
using UnityEngine.Events;

namespace TwoBitMachines.FlareEngine
{
        [AddComponentMenu ("Flare Engine/Quest")]
        public class Quest : MonoBehaviour
        {
                [SerializeField] public QuestSO questSO;
                [SerializeField] public QuestUI questUI;
                [SerializeField] public string journal = "journal";

                [SerializeField] public UnityEvent onQuestAccepted;
                [SerializeField] public UnityEvent onQuestCompleted;

                #region ▀▄▀▄▀▄ Editor Variables ▄▀▄▀▄▀
                #if UNITY_EDITOR
                #pragma warning disable 0414
                [SerializeField, HideInInspector] private bool acceptFoldOut;
                [SerializeField, HideInInspector] private bool completeFoldOut;
                #endif
                #endregion

                private void Start ( )
                {
                        if (questSO != null)
                        {
                                questSO.LoadQuestStatus ( );
                        }
                }

                public void OpenQuestWindow ( )
                {
                        if (questUI != null)
                        {
                                questUI.OpenQuestWindow (questSO);
                        }
                }

                public void AcceptQuest ( )
                {
                        if (questSO != null && !questSO.IsActive ( ))
                        {
                                questSO.AcceptQuest ( );
                                onQuestAccepted.Invoke ( );
                                Journal.AddToJournal (questSO, journal);
                        }
                }

                public void ProgressQuest ( )
                {
                        if (questSO != null && questSO.IsActive ( ) && !questSO.IsComplete ( ))
                        {
                                if (questSO.ProgressQuest ( ))
                                {
                                        questSO.CompleteQuest ( );
                                        onQuestCompleted.Invoke ( );
                                }
                        }
                }
        }

}