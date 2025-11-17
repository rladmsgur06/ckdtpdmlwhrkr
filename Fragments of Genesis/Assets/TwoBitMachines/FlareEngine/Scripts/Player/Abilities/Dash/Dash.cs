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
        public class Dash : Ability
        {
                [SerializeField] public DashType type;
                [SerializeField] public DashTaps taps;
                [SerializeField] public DashDirection directionType;

                [SerializeField] public bool mustBeOnGround = false;
                [SerializeField] public bool nullifyGravity = false;
                [SerializeField] public bool canTakeDamage = true;
                [SerializeField] public bool crouch = false;
                [SerializeField] public bool exitOnJump = false;
                [SerializeField] public bool dashInPlace = false;
                [SerializeField] public bool changeDirectionOnWall = false;
                [SerializeField] public int airDashLimit = 1;
                [SerializeField] public float crouchHeight = 0.5f;
                [SerializeField] public float time = 0.25f;
                [SerializeField] public float coolDown = 0;
                [SerializeField] public float dashDistance = 5;
                [SerializeField] public float tapThreshold = 0.35f;
                [SerializeField] public string up = "";
                [SerializeField] public string down = "";
                [SerializeField] public string left = "";
                [SerializeField] public string right = "";
                [SerializeField] public string extraDashButton = "";
                [SerializeField] public float damage = 1f;
                [SerializeField] public float horizontalJump = 10f;
                [SerializeField] public LayerMask damageLayer;
                [SerializeField] public bool canDealDamage = false;
                [SerializeField] public bool exitOnContact;
                [SerializeField] public LayerMask exitLayer;

                [SerializeField] public string onStartWE;
                [SerializeField] public string onEndWE;
                [SerializeField] public float onDashRate = 0f;
                [SerializeField] public string worldEffect = "";
                [SerializeField] public UnityEventEffect onStart;
                [SerializeField] public UnityEventEffect onEnd;
                [SerializeField] public UnityEventEffect onDash;

                [System.NonSerialized] private int tapped;
                [System.NonSerialized] private int pressedOld;
                [System.NonSerialized] private int airDashCount;
                [System.NonSerialized] public bool dontDash;
                [System.NonSerialized] public bool isDashing;
                [System.NonSerialized] public bool dashRefreshed;
                [System.NonSerialized] private bool useCoolDown;
                [System.NonSerialized] private bool checkAirDash;
                [System.NonSerialized] private bool startedOnGround;
                [System.NonSerialized] private float doubleTapCounter;
                [System.NonSerialized] private float coolDownCounter;
                [System.NonSerialized] private DashPlay dash = new DashPlay();

                private void Awake ()
                {
                        dash.Initialize(this, transform);
                }

                public override void Reset (AbilityManager player)
                {
                        isDashing = false;
                        dashRefreshed = false;
                        useCoolDown = false;
                        doubleTapCounter = 0;
                        coolDownCounter = 0;
                        pressedOld = 0;
                        tapped = 0;
                        dash.Reset();

                        if (WorldManager.gameReset)
                        {
                                airDashCount = 0;
                                player.dashBoost = 1f;
                                checkAirDash = false;
                                player.dashAirJumpCheck = false;
                        }
                        if (crouch && player.world.boxCollider.size.y != player.world.box.boxSize.y)
                        {
                                player.world.box.ColliderReset();
                        }
                        dash.ResetDamage();
                }

                public override bool TurnOffAbility (AbilityManager player)
                {
                        if (player.world.boxCollider.size.y == player.world.box.boxSize.y)
                        {
                                dash.ResetDamage();
                                Reset(player);
                                return true;
                        }
                        else if (dash.SafelyStandUp(player.world.box))
                        {
                                dash.ResetDamage();
                                player.world.box.ColliderReset();
                                return true;
                        }
                        return false;
                }

                public override void EarlyExecute (AbilityManager player, ref Vector2 velocity)
                {
                        if (!isDashing && dash.clearYNextFrame)
                        {
                                dash.clearYNextFrame = false;
                                velocity.y = player.gravityEffect; //                      need to reset y velocity after y dash or else player will keep moving up
                        }
                        if (airDashCount > 0 && player.world.onJumpingSurface)
                        {
                                airDashCount = 0;
                                checkAirDash = false;
                        }
                        if (pause || isDashing)
                        {
                                return;
                        }
                        if (useCoolDown && Clock.Timer(ref coolDownCounter, coolDown))
                        {
                                useCoolDown = false;
                        }
                        if (useCoolDown || (mustBeOnGround && !player.ground) || (airDashLimit > 0 && airDashCount >= airDashLimit))
                        {
                                doubleTapCounter = tapped = pressedOld = 0;
                                return;
                        }

                        int dirA = player.inputs.Pressed(left) ? -1 : 0;
                        int dirB = dirA == 0 && player.inputs.Pressed(right) ? 1 : 0;
                        int pressed = dirA + dirB;
                        int dirC = player.inputs.Pressed(up) ? 1 : 0;
                        int dirD = dirC == 0 && player.inputs.Pressed(down) ? -1 : 0;
                        pressed += (dirC + dirD);

                        if (player.inputs.Pressed(extraDashButton))
                        {
                                SetDash(player, velocity);
                        }
                        else if (taps == DashTaps.DoubleTap)
                        {
                                DoubleTap(player, velocity, pressed);
                        }
                        else if (taps == DashTaps.SingleTap && pressed != 0) // single tap
                        {
                                SetDash(player, velocity);
                        }
                }

                public override bool IsAbilityRequired (AbilityManager player, ref Vector2 velocity)
                {
                        return !pause && isDashing;
                }

                public override void ExecuteAbility (AbilityManager player, ref Vector2 velocity, bool isRunningAsException = false)
                {
                        if (checkAirDash && player.dashAirJumpCheck) // can't air dash back to back in air
                        {
                                Reset(player);
                                checkAirDash = false;
                                player.dashAirJumpCheck = false;
                                player.dashBoost = 1f;
                                return;
                        }
                        if (exitOnJump && player.wasJumping && player.world.wasOnGround && !player.ground)
                        {
                                Reset(player);
                                checkAirDash = false;
                                player.dashAirJumpCheck = false;
                                player.dashBoost = 1f;
                                return;
                        }
                        if (startedOnGround && !player.ground)
                        {
                                startedOnGround = false;
                                player.CheckForAirJumps(0, false);
                        }
                        if (dashRefreshed)
                        {
                                dashRefreshed = false;
                                if (type == DashType.Incremental && directionType == DashDirection.HorizontalAxis)
                                {
                                        velocity.y += horizontalJump;
                                }
                        }
                        dash.Execute(player, ref velocity, ref isDashing);
                        if (velocity.x != 0 && isDashing)
                        {
                                player.UpdateVelocityGround();
                        }
                }

                private void DoubleTap (AbilityManager player, Vector2 velocity, int pressed)
                {
                        if (pressed != 0)
                        {
                                if (tapped == 0)
                                        pressedOld = pressed;
                                tapped++;
                        }
                        if ((tapped > 0 && Clock.Timer(ref doubleTapCounter, tapThreshold)) || (tapped >= 2 && pressedOld != pressed))
                        {
                                doubleTapCounter = tapped = pressedOld = 0;
                        }
                        if (tapped >= 2)
                        {
                                SetDash(player, velocity);
                        }
                }

                private void SetDash (AbilityManager player, Vector2 velocity)
                {
                        if (checkAirDash && !player.dashAirJumpCheck) // can't air dash back to back in air
                        {
                                Reset(player);
                                return;
                        }

                        Reset(player);
                        dontDash = false;
                        isDashing = true;
                        useCoolDown = true;
                        checkAirDash = false;
                        dashRefreshed = true;
                        player.dashAirJumpCheck = false;
                        startedOnGround = player.ground;
                        dash.GetPlayerDirectionX(player, velocity.x);

                        if (airDashLimit > 0 && airDashCount < airDashLimit)
                        {
                                if (!player.world.onJumpingSurface)
                                {
                                        airDashCount++;
                                        checkAirDash = true;
                                }
                                if (!player.ground && (airDashCount <= 1 || airDashCount < airDashLimit))
                                {
                                        player.CheckForAirJumps(player.airJumpCount, false);
                                }
                        }
                        dash.hit.Clear();
                        dash.SetDashDirection(player);
                        OnStartEvent(player);
                }

                public void OnStartEvent (AbilityManager player)
                {
                        ImpactPacket impact = ImpactPacket.impact.Set(onStartWE, this.transform, player.world.boxCollider, transform.position, null, -player.world.box.right * dash.directionX, player.playerDirection, 0);
                        onStart.Invoke(impact);
                }

                public void OnEndEvent (AbilityManager player)
                {
                        ImpactPacket impact = ImpactPacket.impact.Set(onEndWE, this.transform, player.world.boxCollider, transform.position, null, -player.world.box.right * dash.directionX, player.playerDirection, 0);
                        onEnd.Invoke(impact);
                }

                public void DashInPlace (bool enable)
                {
                        dashInPlace = enable;
                }

                public void ResetCoolDown ()
                {
                        coolDownCounter = 0;
                        useCoolDown = false;
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField, HideInInspector] public bool eventsFoldOut;
                [SerializeField, HideInInspector] public bool dashFoldOut;
                [SerializeField, HideInInspector] public bool onStartFoldOut;
                [SerializeField, HideInInspector] public bool onEndFoldOut;

                public override bool OnInspector (SerializedObject controller, SerializedObject parent, string[] inputList, Color barColor, Color labelColor)
                {
                        if (Open(parent, "Dash", barColor, labelColor))
                        {
                                FoldOut.Box(5, FoldOut.boxColorLight, extraHeight: 5, offsetY: -2);
                                {
                                        int taps = parent.Enum("taps");
                                        parent.Field("Dash Direction", "directionType");
                                        parent.DropDownList(inputList, "Dash Button", "extraDashButton");

                                        parent.Field("Dash Taps", "taps", execute: taps == 0 || taps == 2);
                                        parent.FieldDouble("Button Taps", "taps", "tapThreshold", execute: taps == 1);
                                        Labels.FieldText("Time", execute: taps == 1);

                                        parent.DropDownDoubleList(inputList, "Left, Right", "left", "right");
                                        parent.DropDownDoubleList(inputList, "Up, Down", "up", "down");
                                }

                                if (FoldOut.FoldOutButton(parent.Get("eventsFoldOut")))
                                {
                                        Fields.EventFoldOutEffectAndRate(parent.Get("onDash"), parent.Get("worldEffect"), parent.Get("onDashRate"), parent.Get("dashFoldOut"), "On Dashing", color: FoldOut.boxColor);
                                        Fields.EventFoldOutEffect(parent.Get("onStart"), parent.Get("onStartWE"), parent.Get("onStartFoldOut"), "On Start", color: FoldOut.boxColor);
                                        Fields.EventFoldOutEffect(parent.Get("onEnd"), parent.Get("onEndWE"), parent.Get("onEndFoldOut"), "On End", color: FoldOut.boxColor);
                                }

                                int durationType = parent.Enum("type");
                                FoldOut.Box(durationType == 1 ? 5 : 4, FoldOut.boxColorLight);
                                {
                                        parent.Field("Duration", "type");
                                        parent.Field("Dash Time", "time", execute: durationType == 1);
                                        parent.Field("Dash Distance", "dashDistance");
                                        parent.Field("Cool Down", "coolDown");
                                        parent.FieldAndEnable("Crouch", "crouchHeight", "crouch");
                                        Labels.FieldText("Height", rightSpacing: 17);
                                }
                                Layout.VerticalSpacing(5);

                                FoldOut.Box(3, FoldOut.boxColorLight);
                                {
                                        parent.Field("Air Dash Limit", "airDashLimit");
                                        parent.FieldAndEnable("Exit On Contact", "exitLayer", "exitOnContact");
                                        parent.FieldDoubleAndEnable("Can Deal Damage", "damageLayer", "damage", "canDealDamage");
                                        Labels.FieldText("Damage", rightSpacing: 17);
                                }
                                Layout.VerticalSpacing(5);

                                FoldOut.Box(7, FoldOut.boxColorLight);
                                {
                                        parent.FieldToggleAndEnable("Can Take Damage", "canTakeDamage");
                                        parent.FieldToggleAndEnable("On Ground Only", "mustBeOnGround");
                                        parent.FieldToggleAndEnable("Nullify Gravity", "nullifyGravity");
                                        parent.FieldToggleAndEnable("Exit On Jump", "exitOnJump");
                                        parent.FieldToggleAndEnable("Dash In Place", "dashInPlace");
                                        parent.FieldToggleAndEnable("Change Direction On Wall", "changeDirectionOnWall");
                                        parent.Field("Horizontal Jump", "horizontalJump");
                                }
                                Layout.VerticalSpacing(5);
                        }
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion
        }

        public enum DashDirection
        {
                HorizontalAxis,
                MultiDirectional,
                Mouse
        }

        public enum DashType
        {
                Instant,
                Incremental
        }

        public enum DashTaps
        {
                SingleTap,
                DoubleTap,
                None
        }
}
