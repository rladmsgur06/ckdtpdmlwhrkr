#region 
#if UNITY_EDITOR
using TwoBitMachines.Editors;
using UnityEditor;
#endif
#endregion
using System.Collections.Generic;
using UnityEngine;

namespace TwoBitMachines.FlareEngine.ThePlayer
{
        [AddComponentMenu("")]
        public class Melee : Ability
        {
                [SerializeField] public List<FlareEngine.Melee> melee = new List<FlareEngine.Melee>();
                [SerializeField] public float coolDown = 0;
                [System.NonSerialized] public float coolDownCounter = 0;

                public override void Initialize (Player player)
                {
                        melee.Clear();
                        FlareEngine.Melee[] melees = this.gameObject.GetComponentsInChildren<FlareEngine.Melee>(true);
                        for (int i = 0; i < melees.Length; i++)
                        {
                                melee.Add(melees[i]);
                        }
                        for (int i = 0; i < melee.Count; i++)
                        {
                                if (melee[i] != null)
                                {
                                        melee[i].SetReference(this);
                                }
                        }
                        for (int i = 0; i < melee.Count; i++)
                        {
                                if (melee[i] != null && melee[i].gameObject.activeInHierarchy)
                                {
                                        ChangeMeleeAttack(melee[i].meleeName);
                                        break;
                                }
                        }
                }

                public override bool TurnOffAbility (AbilityManager player)
                {
                        Reset(player);
                        return true;
                }

                public override void Reset (AbilityManager player)
                {
                        coolDownCounter = 0;
                        for (int i = 0; i < melee.Count; i++)
                        {
                                if (melee[i] != null)
                                {
                                        melee[i].ResetAll();
                                }
                        }
                }

                public override bool IsAbilityRequired (AbilityManager player, ref Vector2 velocity)
                {
                        if (pause)
                                return false;

                        int meleeInputs = 0;
                        int meleeIndex = -1;
                        bool meleeActive = false;

                        for (int i = 0; i < melee.Count; i++)
                        {
                                if (melee[i] != null && melee[i].inMelee)
                                {
                                        meleeActive = true;
                                        break;
                                }
                        }

                        for (int i = 0; i < melee.Count; i++)
                        {
                                if (melee[i] == null || melee[i].pause || !melee[i].ActivateFromSleep())
                                {
                                        continue;
                                }
                                if ((!meleeActive || melee[i].melee.cancelOtherAttacks) && melee[i].melee.inputs >= meleeInputs)
                                {
                                        meleeInputs = melee[i].melee.inputs;
                                        meleeIndex = i;
                                }
                        }

                        if (coolDown > 0 && Clock.TimerInverseExpired(ref coolDownCounter, coolDown))
                        {
                                meleeIndex = -1;
                        }

                        if (meleeIndex >= 0 && meleeIndex < melee.Count && (melee[0] == null || (melee[0] != melee[meleeIndex]) || !melee[meleeIndex].gameObject.activeInHierarchy))
                        {
                                if (melee[0] != null)
                                {
                                        melee[0].ResetAll();
                                }
                                ChangeMeleeAttack(meleeIndex);
                                if (melee[0] != null)
                                {
                                        melee[0].SkipCoolDown();
                                }
                        }

                        // only one melee can be active at a time, unlike weapons, first in list is one to run
                        if (melee.Count > 0 && melee[0] != null && melee[0].MeleeIsActive(player.character, player.inputs))
                        {
                                return true;
                        }
                        return false;
                }

                public override void ExecuteAbility (AbilityManager player, ref Vector2 velocity, bool isRunningAsException = false)
                {
                        if (melee.Count > 0 && melee[0] != null)
                        {
                                melee[0].Execute(player.signals, player.playerDirection, player.ground, player.crouching, player.world.position, ref velocity); // only called if ability is active
                        }
                }

                public void CompleteAttack ()
                {
                        if (melee.Count > 0 && melee[0] != null)
                        {
                                melee[0].CompleteAttack();
                        }
                }

                public void ChangeMeleeAttack (int index)
                {
                        coolDownCounter = 0;
                        TurnOffMeleeAttack();
                        for (int i = 0; i < melee.Count; i++)
                        {
                                if (melee[i] != null && i == index)
                                {
                                        Set(i, true);
                                        return;
                                }
                        }
                }

                public void ChangeMeleeAttack (string meleeName)
                {
                        coolDownCounter = 0;
                        TurnOffMeleeAttack();
                        for (int i = 0; i < melee.Count; i++)
                        {
                                if (melee[i] != null && melee[i].meleeName == meleeName)
                                {
                                        Set(i, true);
                                        return;
                                }
                        }
                }

                public void ChangeMeleeAttack (string meleeName, bool reset)
                {
                        coolDownCounter = 0;
                        TurnOffMeleeAttack();
                        for (int i = 0; i < melee.Count; i++)
                        {
                                if (melee[i] != null && melee[i].meleeName == meleeName)
                                {
                                        Set(i, reset);
                                        return;
                                }
                        }
                }

                private void Set (int i, bool reset = true)
                {
                        FlareEngine.Melee newMelee = melee[i];
                        melee.RemoveAt(i);
                        melee.Insert(0, newMelee);
                        if (reset)
                        {
                                newMelee.ResetAll();
                        }
                        if (newMelee.gameObject != null)
                        {
                                newMelee.gameObject.SetActive(true);
                        }
                }

                public void TurnOffMeleeAttack ()
                {
                        for (int i = 0; i < melee.Count; i++)
                        {
                                if (melee[i] != null && melee[i].gameObject != null)
                                {
                                        melee[i].gameObject.SetActive(false);
                                }
                        }
                }

                public bool MeleeAttackIsActive ()
                {
                        if (melee.Count > 0 && melee[0] != null)
                        {
                                return melee[0].inMelee;
                        }
                        return false;
                }

                #region ▀▄▀▄▀▄ Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                public override bool OnInspector (SerializedObject controller, SerializedObject parent, string[] inputList, Color barColor, Color labelColor)
                {
                        if (Open(parent, "Melee", barColor, labelColor))
                        {
                                FoldOut.Box(1, FoldOut.boxColorLight, offsetY: -2);
                                parent.Field("Cool Down", "coolDown");
                                Layout.VerticalSpacing(3);
                        }
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion

        }
}
