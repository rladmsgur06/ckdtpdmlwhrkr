#region 
#if UNITY_EDITOR
using TwoBitMachines.Editors;
using UnityEditor;
#endif
#endregion
using UnityEngine;
using UnityEngine.Events;

namespace TwoBitMachines.FlareEngine.ThePlayer
{
        [AddComponentMenu("")]
        public class Firearms : Ability
        {
                [SerializeField] public bool checkIfLowerPriority;
                [SerializeField] public UnityEvent onCancel;
                private float ammunition = 0;

                public override bool TurnOffAbility (AbilityManager player)
                {
                        onCancel.Invoke();
                        player.character.canUseTool = false;
                        Character equipment = player.character;
                        for (int i = equipment.tools.Count - 1; i >= 0; i--)
                        {
                                if (equipment.tools[i] != null && equipment.tools[i].gameObject != null && equipment.tools[i].gameObject.activeInHierarchy)
                                {
                                        equipment.tools[i].TurnOff(player);
                                }
                        }
                        return true;
                }

                public override bool IsAbilityRequired (AbilityManager player, ref Vector2 velocity)
                {
                        Character equipment = player.character;
                        equipment.canUseTool = false;
                        player.character.canUseTool = false;

                        if (pause || equipment.tools == null || player.inputs.block)
                        {
                                return false;
                        }

                        ammunition = 0;
                        for (int i = equipment.tools.Count - 1; i >= 0; i--)
                        {
                                if (equipment.tools[i] == null || equipment.tools[i].gameObject == null)
                                {
                                        equipment.tools.RemoveAt(i);
                                        continue;
                                }
                                if (equipment.tools[i].gameObject.activeInHierarchy)
                                {
                                        ammunition = equipment.tools[i].ToolValue();
                                        if (equipment.tools[i].ToolActive())
                                        {
                                                return true;
                                        }
                                }
                        }
                        return false;
                }

                public override void ExecuteAbility (AbilityManager player, ref Vector2 velocity, bool isRunningAsException = false)
                {
                        player.character.canUseTool = true; //tools
                }

                public override void LateExecute (AbilityManager player, ref Vector2 velocity)
                {
                        Character equipment = player.character;
                        if (pause || equipment.tools == null)
                                return;

                        if (checkIfLowerPriority && player.HigherPriority(abilityName, ID))
                        {
                                onCancel.Invoke();
                                return;
                        }

                        for (int i = equipment.tools.Count - 1; i >= 0; i--)
                        {
                                if (equipment.tools[i] == null || equipment.tools[i].gameObject == null)
                                {
                                        equipment.tools.RemoveAt(i);
                                        continue;
                                }
                                if (equipment.tools[i].gameObject.activeInHierarchy)
                                {
                                        equipment.tools[i].LateExecute(player, ref velocity);
                                }
                                if (equipment.tools[i].gameObject.activeInHierarchy && equipment.tools[i].IsRecoiling())
                                {

                                        equipment.tools[i].Recoil(ref velocity, player.signals);
                                }
                        }
                }

                public float Ammunition ()
                {
                        return ammunition;
                }

                #region ▀▄▀▄▀▄ Editor Variables ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField, HideInInspector] public bool foldOutCancel;
                [SerializeField, HideInInspector] public bool toolSetFoldOut;
                public override bool OnInspector (SerializedObject controller, SerializedObject parent, string[] inputList, Color barColor, Color labelColor)
                {
                        if (Open(parent, "Firearms", barColor, labelColor))
                        {
                                FoldOut.Box(1, FoldOut.boxColorLight, offsetY: -2);
                                {
                                        parent.FieldToggle("Cancel If Lower Priority", "checkIfLowerPriority");
                                }
                                Layout.VerticalSpacing(3);
                                Fields.EventFoldOut(parent.Get("onCancel"), parent.Get("foldOutCancel"), "On Cancel");
                                Fields.EventFoldOut(controller.Get("onToolSet"), parent.Get("toolSetFoldOut"), "On Tool Set");
                        }
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion

        }

}
