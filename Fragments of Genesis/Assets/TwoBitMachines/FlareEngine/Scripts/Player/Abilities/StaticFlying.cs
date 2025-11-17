#region
#if UNITY_EDITOR
using TwoBitMachines.Editors;
using UnityEditor;
#endif
#endregion
using UnityEngine;

namespace TwoBitMachines.FlareEngine.ThePlayer
{
        [AddComponentMenu("")]
        public class StaticFlying : Ability
        {
                [SerializeField] public float speed = 5f; // this will assume, Up, Down inputs exist. If pushback exists, add this to its exception
                [SerializeField] public float smooth = 1f;
                [SerializeField] public string enterButton;
                [SerializeField] public string exitButton;
                [SerializeField] public FlyEnter enter;
                [SerializeField] public FlyExit exit;

                [System.NonSerialized] private float velocityY;
                [System.NonSerialized] private bool isFlying;
                public enum FlyEnter { Button, Always }
                public enum FlyExit { Button, GroundHit, Never }

                public override void Reset (AbilityManager player)
                {
                        isFlying = false;
                }

                public override bool TurnOffAbility (AbilityManager player)
                {
                        isFlying = false;
                        return true;
                }

                public override bool IsAbilityRequired (AbilityManager player, ref Vector2 velocity)
                {
                        if (pause)
                                return false;

                        if (isFlying && exit != FlyExit.Never && ((exit == FlyExit.GroundHit && player.ground) || player.inputs.Pressed(exitButton)))
                        {
                                return isFlying = false;
                        }
                        if (isFlying || enter == FlyEnter.Always || player.inputs.Pressed(enterButton))
                        {
                                return true;
                        }
                        return false;
                }

                public override void ExecuteAbility (AbilityManager player, ref Vector2 velocity, bool isRunningAsException = false)
                {
                        isFlying = true;
                        velocity.y = 0;
                        player.signals.Set("staticFlying");

                        if (player.inputs.Holding("Up"))
                        {
                                velocity.y = speed;
                        }
                        if (player.inputs.Holding("Down"))
                        {
                                velocity.y = -speed;
                        }
                        if (smooth > 0 && smooth < 1f)
                        {
                                velocity.y = Compute.Lerp(velocityY, velocity.y, smooth);
                        }
                        velocityY = velocity.y;
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                public override bool OnInspector (SerializedObject controller, SerializedObject parent, string[] inputList, Color barColor, Color labelColor)
                {
                        if (Open(parent, "Static Flying", barColor, labelColor))
                        {
                                FoldOut.Box(4, FoldOut.boxColorLight, offsetY: -2);
                                parent.Field("Speed", "speed");
                                parent.Slider("Smooth", "smooth");

                                parent.Field("Enter", "enter", execute: enter != FlyEnter.Button);
                                parent.FieldAndDropDownList(inputList, "Enter", "enter", "enterButton", execute: enter == FlyEnter.Button);

                                parent.Field("Exit", "exit", execute: exit != FlyExit.Button);
                                parent.FieldAndDropDownList(inputList, "Exit", "exit", "exitButton", execute: exit == FlyExit.Button);
                                Layout.VerticalSpacing(3);
                        }
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion

        }

}
