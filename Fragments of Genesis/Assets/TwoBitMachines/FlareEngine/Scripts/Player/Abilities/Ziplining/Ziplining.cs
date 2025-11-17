#region 
#if UNITY_EDITOR
using TwoBitMachines.Editors;
using UnityEditor;
#endif
#endregion
using TwoBitMachines.FlareEngine.Interactables;
using UnityEngine;

namespace TwoBitMachines.FlareEngine.ThePlayer
{
        [AddComponentMenu("")]
        public class Ziplining : Ability
        {
                [SerializeField] public ZipInfo zip = new ZipInfo();

                public override void Initialize (Player player)
                {
                        zip.inputs = player.inputs;
                }

                public override void Reset (AbilityManager player)
                {
                        zip.state.Clear();
                        zip.gravityMomentum = 0;
                        zip.cancelled = 0;
                }

                public override bool TurnOffAbility (AbilityManager player)
                {
                        Reset(player);
                        zip.cancelled = Time.time + 0.25f;
                        return true;
                }

                public override bool IsAbilityRequired (AbilityManager player, ref Vector2 velocity)
                {
                        if (pause || zip.cancelled >= Time.time)
                                return false;
                        zip.playerDirection = player.playerDirection;
                        Zipline.Find(player, player.world.box.center, zip, ref velocity);
                        return zip.state.Count > 0 && zip.Active();
                }

                public override void ExecuteAbility (AbilityManager player, ref Vector2 velocity, bool isRunningAsException = false)
                {
                        player.world.hitInteractable = true;
                        player.signals.Set("zipline");
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField, HideInInspector] private bool eventsFoldOut;
                public override bool OnInspector (SerializedObject controller, SerializedObject parent, string[] inputList, Color barColor, Color labelColor)
                {
                        if (Open(parent, "Ziplining", barColor, labelColor))
                        {
                                SerializedProperty zip = parent.Get("zip");
                                FoldOut.Box(4, FoldOut.boxColorLight, offsetY: -2);
                                {
                                        zip.Slider("Zip Speed", "zipSpeed", 0.1f, 2f);
                                        zip.Field("Jump Force", "jumpForce");
                                        zip.Field("Y Offset", "yOffset");
                                        zip.DropDownListAndEnable(inputList, "Exit Button", "exit", "exitButton");
                                }
                                Layout.VerticalSpacing(3);

                                FoldOut.Box(2, FoldOut.boxColorLight, extraHeight: 3);
                                {
                                        zip.FieldToggle("Can Relatch", "canRelatch");
                                        zip.FieldToggle("Apply Gravity", "useGravity");
                                }

                                if (FoldOut.FoldOutButton(zip.Get("eventsFoldOut")))
                                {
                                        Fields.EventFoldOutEffect(zip.Get("onStart"), zip.Get("onStartWE"), zip.Get("onStartFoldOut"), "On Start", color: FoldOut.boxColor);
                                        Fields.EventFoldOutEffect(zip.Get("onEnd"), zip.Get("onEndWE"), zip.Get("onEndFoldOut"), "On End", color: FoldOut.boxColor);
                                }
                        }
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion

        }

}
