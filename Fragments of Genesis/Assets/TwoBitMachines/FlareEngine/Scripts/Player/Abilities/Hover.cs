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
        public class Hover : Ability // anim signal: hover
        {
                [SerializeField] public string thrust;
                [SerializeField] public float hoverThrust = 1;
                [SerializeField] public float maintainThrust = 1;
                [SerializeField] public float descendThrust = 1;
                [SerializeField] public int thrustLimit = 1;
                [SerializeField] public bool thrustLimitEnable;
                [SerializeField] public string descend;
                [SerializeField] public float airFrictionX = 0.5f;
                [SerializeField] public ThrustExit exitType;
                [SerializeField] public string exit;
                [SerializeField] public UnityEventEffect onThrust = new UnityEventEffect();
                [SerializeField] public UnityEventEffect onDescend = new UnityEventEffect();
                [SerializeField] public UnityEventFloat thrustCount = new UnityEventFloat();
                [SerializeField] public string thrustWE;
                [SerializeField] public string descendWE;


                [System.NonSerialized] public bool hovering = false;
                [System.NonSerialized] public int thrustCounter = 0;


                public override void Reset (AbilityManager player)
                {
                        hovering = false;
                        thrustCounter = 0;
                        OnThrustCount();

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

                        bool thrust = player.inputs.Pressed(this.thrust);

                        if (hovering)
                        {
                                if (exitType == ThrustExit.OnGroundHit || exitType == ThrustExit.OnGroundHitOrButton)
                                {
                                        if (player.ground && !thrust)
                                        {
                                                hovering = false;
                                                thrustCounter = 0;
                                                OnThrustCount();
                                        }
                                }
                                if (exitType == ThrustExit.Button || exitType == ThrustExit.OnGroundHitOrButton)
                                {
                                        if (player.inputs.Pressed(this.exit))
                                        {
                                                hovering = false;
                                                thrustCounter = 0;
                                                OnThrustCount();
                                        }
                                }
                        }
                        bool isHovering = hovering;
                        hovering = false;
                        return thrust || isHovering;
                }

                public override void ExecuteAbility (AbilityManager player, ref Vector2 velocity, bool isRunningAsException = false)
                {
                        hovering = true;
                        player.world.hitInteractable = true;
                        player.signals.Set("hover", true);
                        HorizontalVelocity(player.inputX, player.speed, player.velocityX, 1f - airFrictionX, ref velocity); // reapply x velocity, will also override impede change (if it exists)

                        if (player.inputs.Pressed(this.thrust) && ThrustLimitCounter())
                        {
                                velocity.y = player.maxJumpVel * hoverThrust;
                                onThrust.Invoke(ImpactPacket.impact.Set(thrustWE, this.transform, player.world.boxCollider, player.world.position, null, player.world.box.down, player.playerDirection, 0));

                        }
                        if (player.inputs.Pressed(this.descend))
                        {
                                velocity.y += player.gravityEffect * descendThrust;
                                onDescend.Invoke(ImpactPacket.impact.Set(descendWE, this.transform, player.world.boxCollider, player.world.position, null, player.world.box.down, player.playerDirection, 0));

                        }
                        if (velocity.y >= 0)
                        {
                                velocity.y -= player.gravityEffect; // undo gravity
                                velocity.y += player.gravityEffect * 0.60f;
                        }
                        else if (velocity.y < 0)
                        {
                                velocity.y -= player.gravityEffect * maintainThrust;
                                velocity.y = velocity.y > 0 ? 0 : velocity.y;
                        }
                }

                public void HorizontalVelocity (float inputX, float speed, float velocityX, float smooth, ref Vector2 velocity)
                {
                        if (smooth > 0 && smooth < 1f)
                        {
                                velocity.x = Compute.Lerp(velocityX, inputX * speed, smooth);
                        }
                        else
                        {
                                velocity.x = inputX * speed;
                        }
                }

                private bool ThrustLimitCounter ()
                {
                        if (!thrustLimitEnable)
                        {
                                return true;
                        }
                        if (thrustCounter++ >= thrustLimit)
                        {
                                thrustCounter = thrustLimit;
                                OnThrustCount();
                                return false;
                        }
                        OnThrustCount();
                        return true;
                }

                public void SetThrustLimit (float limit)
                {
                        thrustLimit = (int) limit;
                }

                private void OnThrustCount ()
                {
                        if (thrustLimitEnable)
                        {
                                thrustCount.Invoke(thrustCounter);
                        }
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414

                [SerializeField] private bool countFoldOut = false;
                [SerializeField] private bool eventsFoldOut = false;
                [SerializeField] private bool thrustFoldOut = false;
                [SerializeField] private bool descendFoldOut = false;

                public override bool OnInspector (SerializedObject controller, SerializedObject parent, string[] inputList, Color barColor, Color labelColor)
                {
                        if (Open(parent, "Hover", barColor, labelColor))
                        {
                                FoldOut.Box(4, FoldOut.boxColorLight, offsetY: -2, extraHeight: 5);
                                {
                                        parent.Slider("Thrust", "hoverThrust", 0, 1f);
                                        parent.Slider("Maintain", "maintainThrust", 0.5f, 1f);
                                        parent.DropDownList(inputList, "Thrust Button", "thrust");
                                        parent.FieldAndEnable("Thrust Limit", "thrustLimit", "thrustLimitEnable");

                                        bool eventOpen = FoldOut.FoldOutButton(parent.Get("eventsFoldOut"));
                                        Fields.EventFoldOutEffect(parent.Get("onThrust"), parent.Get("thrustWE"), parent.Get("thrustFoldOut"), "On Thrust", execute: eventOpen, color: FoldOut.boxColorLight);
                                        Fields.EventFoldOutEffect(parent.Get("onDescend"), parent.Get("descendWE"), parent.Get("descendFoldOut"), "On Descend", execute: eventOpen, color: FoldOut.boxColorLight);
                                        Fields.EventFoldOut(parent.Get("thrustCount"), parent.Get("countFoldOut"), "Thrust Count", execute: eventOpen, color: FoldOut.boxColorLight);
                                }
                                // Layout.VerticalSpacing(1);

                                FoldOut.Box(2, FoldOut.boxColorLight);
                                {
                                        parent.Slider("Descend", "descendThrust", 0, 10f);
                                        parent.DropDownList(inputList, "Descend Button", "descend");
                                }
                                Layout.VerticalSpacing(5);

                                FoldOut.Box(2, FoldOut.boxColorLight);
                                {
                                        int exitType = parent.Enum("exitType");
                                        parent.Field("Exit", "exitType", execute: exitType == 0);
                                        parent.FieldAndDropDownList(inputList, "Exit", "exitType", "exit", execute: exitType > 0);
                                        parent.Slider("Air Friction X", "airFrictionX", 0, 1f);

                                }
                                Layout.VerticalSpacing(3);
                        }
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion

                public enum ThrustExit
                {
                        OnGroundHit,
                        Button,
                        OnGroundHitOrButton
                }
        }
}
