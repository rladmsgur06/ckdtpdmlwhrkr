using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TwoBitMachines.FlareEngine
{
        public class QuestUIAccept : QuestUI
        {
                [SerializeField] public TextMeshProUGUI title;
                [SerializeField] public TextMeshProUGUI description;
                [SerializeField] public List<QuestRewardsUI> rewards = new List<QuestRewardsUI> ( );
                [System.NonSerialized] private QuestSO questSO;

                public override void OpenQuestWindow (QuestSO questSO)
                {
                        if (questSO == null) return;

                        this.questSO = questSO;
                        gameObject.SetActive (true);
                        title?.SetText (questSO.title);
                        description?.SetText (questSO.description);
                        EventSystem.current.SetSelectedGameObject (this.gameObject);
                        EnableRewards (questSO.rewards, rewards);
                }

                public void AcceptQuest ( )
                {
                        questSO?.AcceptQuest ( );
                }
        }
}