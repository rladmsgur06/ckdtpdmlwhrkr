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
        public class InfiniteWalk : Ability
        {
                [SerializeField] public string button;
                [SerializeField] public bool autoWallClimb;
                [SerializeField] public bool changeOnInput;

                [System.NonSerialized] public int direction;
                [System.NonSerialized] public bool infiniteWalking;
                [System.NonSerialized] public bool autoWallClimbing;

                public override void Reset (AbilityManager player)
                {
                        direction = 1;
                        infiniteWalking = false;
                        autoWallClimbing = false;
                }

                public override bool TurnOffAbility (AbilityManager player)
                {
                        Reset(player);
                        return true;
                }

                public override bool IsAbilityRequired (AbilityManager player, ref Vector2 velocity)
                {
                        if (pause)
                                return false;

                        if (infiniteWalking && player.inputs.Pressed(button))
                        {
                                Reset(player);
                                return false;
                        }
                        if (infiniteWalking)
                        {
                                return true;
                        }
                        return player.inputs.Pressed(button);
                }

                public override void ExecuteAbility (AbilityManager player, ref Vector2 velocity, bool isRunningAsException = false)
                {
                        if (!infiniteWalking)
                        {
                                direction = player.playerDirection;
                        }

                        infiniteWalking = true;

                        if (changeOnInput && player.inputX != 0 && !Compute.SameSign(player.inputX, direction))
                        {
                                direction = (int) Mathf.Sign(player.inputX);
                        }

                        if (autoWallClimbing && (!player.world.onWall || player.ground))
                        {
                                autoWallClimbing = false;
                        }
                        if (player.world.leftWall && !player.world.onBridge)
                        {
                                direction = 1;
                        }
                        if (player.world.rightWall && !player.world.onBridge)
                        {
                                direction = -1;
                        }
                        if (autoWallClimb && (autoWallClimbing || !player.ground))
                        {
                                if (player.world.leftWall)
                                {
                                        direction = -1;
                                        autoWallClimbing = true;
                                }
                                if (player.world.rightWall)
                                {
                                        direction = 1;
                                        autoWallClimbing = true;
                                }
                        }

                        OverrideDirection(player, ref velocity);
                }

                private void OverrideDirection (AbilityManager player, ref Vector2 velocity)
                {
                        velocity.x = player.speed * Mathf.Sign(direction);

                        if (player.ground)
                        {
                                player.UpdateVelocityGround();
                        }

                        player.playerDirection = direction;
                        player.inputX = direction;
                }

                public void InfiniteWalkActivate (bool value)
                {
                        infiniteWalking = value;
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                public override bool OnInspector (SerializedObject controller, SerializedObject parent, string[] inputList, Color barColor, Color labelColor)
                {
                        if (Open(parent, "Infinite Walk", barColor, labelColor))
                        {
                                FoldOut.Box(3, FoldOut.boxColorLight, offsetY: -2);
                                {
                                        parent.DropDownList(inputList, "Button", "button");
                                        parent.Field("Auto Wall Climb", "autoWallClimb");
                                        parent.Field("Change On Input", "changeOnInput");
                                }
                                Layout.VerticalSpacing(3);
                        }
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion

        }
}
