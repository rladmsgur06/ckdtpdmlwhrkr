using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TwoBitMachines.FlareEngine
{
        public class QuestUI : MonoBehaviour
        {
                [SerializeField] public bool usePauseOnActive = false;

                private void OnEnable ()
                {
                        if (usePauseOnActive)
                                WorldManager.get.PauseNoInvoke();
                }

                private void OnDisable ()
                {
                        if (usePauseOnActive)
                                WorldManager.get.UnpauseNoInvoke();
                }

                public virtual void OpenQuestWindow (QuestSO questSO)
                {

                }

                public void CloseQuestWindow ()
                {
                        this.gameObject.SetActive(false);
                }

                public static void DisableRewards (List<QuestRewards> questRewards, List<QuestRewardsUI> rewards)
                {
                        for (int i = 0; i < rewards.Count; i++)
                        {
                                if (rewards[i].icon != null)
                                        rewards[i].icon.enabled = false;
                                if (rewards[i].name != null)
                                        rewards[i].name.enabled = false;
                                if (rewards[i].reward != null)
                                        rewards[i].reward.enabled = false;
                                if (rewards[i].description != null)
                                        rewards[i].description.enabled = false;
                                if (rewards[i].icon != null)
                                        rewards[i].icon.gameObject.SetActive(false); // icon should exist and be parent of the rest
                        }
                }

                public static void EnableRewards (List<QuestRewards> questRewards, List<QuestRewardsUI> rewards)
                {
                        DisableRewards(questRewards, rewards);
                        for (int i = 0; i < rewards.Count; i++)
                        {
                                if (i < questRewards.Count)
                                {
                                        if (rewards[i].icon != null)
                                        {
                                                rewards[i].icon.enabled = true;
                                                rewards[i].icon.sprite = questRewards[i].icon;
                                                rewards[i].icon.gameObject.SetActive(true);
                                        }
                                        if (rewards[i].name != null)
                                        {
                                                rewards[i].name.enabled = true;
                                                rewards[i].name.SetText(questRewards[i].name);
                                        }
                                        if (rewards[i].reward != null)
                                        {
                                                rewards[i].reward.enabled = true;
                                                rewards[i].reward.SetText(questRewards[i].reward.ToString());
                                        }
                                        if (rewards[i].description != null)
                                        {
                                                rewards[i].description.enabled = true;
                                                rewards[i].description.SetText(questRewards[i].description);
                                        }
                                }
                        }
                }

        }

        [System.Serializable]
        public class QuestRewardsUI
        {
                [SerializeField] public Image icon;
                [SerializeField] public TextMeshProUGUI name;
                [SerializeField] public TextMeshProUGUI reward;
                [SerializeField] public TextMeshProUGUI description;
        }

}
