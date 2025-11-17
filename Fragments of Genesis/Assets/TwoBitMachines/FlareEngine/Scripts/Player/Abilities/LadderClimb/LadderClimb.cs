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
        // remaning, reverse hit only for health, coins can only collide on reverse, ai that reverse on x and y
        [AddComponentMenu("")]
        public class LadderClimb : Ability
        {
                [SerializeField] private LadderClimbType climb;
                [SerializeField] private float jump = 15f;
                [SerializeField] private float climbSpeed = 5f;
                [SerializeField] private float sideSpeed = 4f;
                [SerializeField] private float fallPoint = 0f;
                [SerializeField] private bool autoLatch = false;
                [SerializeField] private string up = "Up";
                [SerializeField] private string down = "Down";
                [SerializeField] private string fenceFlip = "None";

                [SerializeField] public string enterWE;
                [SerializeField] public string exitWE;
                [SerializeField] public string ladderTopWE;
                [SerializeField] public UnityEventEffect onEnter;
                [SerializeField] public UnityEventEffect onExit;
                [SerializeField] public UnityEventEffect onLadderTop;

                [System.NonSerialized] private int autoDirection;
                [System.NonSerialized] private bool onLadder;
                [System.NonSerialized] private bool jumpingFromLadder;
                [System.NonSerialized] private bool fallingToLadder;
                [System.NonSerialized] private Ladder ladderRef;
                [System.NonSerialized] private Vector2 ladderPosition;

                [System.NonSerialized] private float jumpPoint;
                [System.NonSerialized] public float playerTop;
                [System.NonSerialized] public float playerBottom;
                [System.NonSerialized] public bool hasFlipped;
                [System.NonSerialized] public Vector2 playerCenter;
                [System.NonSerialized] public FenceReverse fenceReverse = new FenceReverse();

                public LadderInstance ladder => ladderRef.ladder;
                private Vector2 ladderVelocity => ladder.rawPosition - ladderPosition;

                public override void Reset (AbilityManager player)
                {
                        if (onLadder)
                        {
                                OnExit(player);
                        }
                        if (ladderRef != null)
                        {
                                ladderRef.fenceFlip.Reset();
                        }

                        autoDirection = 0;
                        jumpingFromLadder = false;
                        fallingToLadder = false;
                        hasFlipped = false;
                        onLadder = false;
                        ladderRef = null;
                        fenceReverse.Reset(player);

                }

                public override bool TurnOffAbility (AbilityManager player)
                {
                        Reset(player);
                        return true;
                }

                public override bool IsAbilityRequired (AbilityManager player, ref Vector2 velocity)
                {
                        if (pause)
                        {
                                return false;
                        }

                        if (onLadder && ladder == null)
                        {
                                Reset(player);
                        }

                        if (onLadder && player.ground)
                        {
                                Reset(player);
                        }


                        if (fallingToLadder && (player.world.hitInteractable || player.ground))
                        {
                                fallingToLadder = false;
                        }

                        if (onLadder)
                        {
                                if (!Ladder.Find(player.world, velocity, ref ladderRef))
                                {
                                        if (ladder.standOnLadder && jumpingFromLadder && player.inputX == 0 && !player.world.touchingASurface)
                                        {
                                                return false;
                                        }
                                        else
                                        {
                                                Reset(player);
                                                fallingToLadder = true;
                                        }
                                }
                                else
                                {
                                        return true;
                                }
                        }
                        if ((autoLatch && velocity.y < 0 && Mathf.Abs(velocity.x) < 0.1f) || fallingToLadder || player.inputs.Holding(up) || (player.inputs.Holding(down) && !player.ground))
                        {
                                if (Ladder.Find(player.world, velocity, ref ladderRef))
                                {
                                        return true;
                                }
                        }
                        return false;
                }

                public override void ExecuteAbility (AbilityManager player, ref Vector2 velocity, bool isRunningAsException = false)
                {
                        if (ladder == null)
                        {
                                Reset(player);
                                return;
                        }
                        if (!onLadder)
                        {
                                ladderPosition = ladder.rawPosition;
                                onEnter.Invoke(ImpactPacket.impact.Set(enterWE, transform, player.world.boxCollider, player.world.position, null, player.world.box.down, player.playerDirection, 0));
                        }

                        GetPlayerPositions(player);

                        if (ExitFromJump(velocity.y))
                        {
                                return;
                        }

                        bool wasOnLadder = onLadder;
                        onLadder = true;
                        jumpingFromLadder = false;
                        player.world.hitInteractable = true;

                        if (climb == LadderClimbType.Automatic)
                        {
                                AutomaticClimb(player, wasOnLadder, ref velocity);
                        }
                        else
                        {
                                ManualClimb(player, ref velocity);
                        }

                        if (hasFlipped)
                        {
                                player.world.isHiding = true;
                                player.signals.Set("ladderReverse");
                        }
                }

                private bool ExitFromJump (float velocityY)
                {
                        if (!jumpingFromLadder)
                        {
                                return false;
                        }
                        if (playerBottom >= ladder.top)
                        {
                                return false;
                        }
                        if (velocityY > 0)
                        {
                                return true;
                        }
                        if (fallPoint != 0 && playerBottom > (jumpPoint + fallPoint))
                        {
                                return true;
                        }
                        return false;
                }

                private void AutomaticClimb (AbilityManager player, bool wasOnLadder, ref Vector2 velocity)
                {
                        if (!wasOnLadder || autoDirection == 0)
                        {
                                autoDirection = playerTop >= ladder.top ? -1 : 1;
                        }

                        bool autoUp = autoDirection == 1;
                        bool autoDown = autoDirection == -1;

                        Climb(player, autoUp, autoDown, ref velocity);
                        LadderTopReached(false, ref velocity);
                        Jump(player, ref velocity);
                }

                private void ManualClimb (AbilityManager player, ref Vector2 velocity)
                {
                        if (ladderRef.fenceFlip.Flip(ladder, this, player, fenceFlip, ref velocity))
                        {
                                return;
                        }

                        bool climbUp = player.inputs.Holding(up);
                        bool climbDown = player.inputs.Holding(down);

                        if (FallOnLadderTop(player, climbDown, ref velocity))
                        {
                                return;
                        }

                        Climb(player, climbUp, climbDown, ref velocity);
                        fenceReverse.Flip(player, this, ladderRef, ref velocity);
                        ClimbToLadderTop(player, climbDown, ref velocity);
                        LadderTopReached(ladder.standOnLadder, ref velocity);
                        Jump(player, ref velocity);
                }

                private bool FallOnLadderTop (AbilityManager player, bool climbDown, ref Vector2 velocity)
                {
                        if (!ladder.standOnLadder || Time.deltaTime == 0 || climbDown || velocity.y >= 0 || player.jumpButtonActive)
                        {
                                return false;
                        }
                        if (playerBottom > ladder.top && ladder.ContainsY(playerBottom + velocity.y * Time.deltaTime))
                        {
                                velocity.y = (ladder.top - playerBottom) / Time.deltaTime;
                                player.OnSurface();
                                return true;
                        }
                        return false;
                }

                private void Climb (AbilityManager player, bool climbUp, bool climbDown, ref Vector2 velocity)
                {
                        velocity.y = 0;
                        if (climbUp)
                        {
                                velocity.y += climbSpeed;
                        }
                        if (climbDown && !player.ground)
                        {
                                velocity.y -= climbSpeed;
                        }
                        if (player.inputX != 0 && (playerBottom + (velocity.y * Time.deltaTime) + 0.01f) < ladder.top)
                        {
                                velocity.x = sideSpeed * Mathf.Sign(player.inputX);
                        }

                        bool alignToCenter = ladder.alignToCenter && player.inputX == 0 && velocity.y != 0;
                        bool belowLadder = (playerBottom + velocity.y * Time.deltaTime) < ladder.top;

                        if (alignToCenter && belowLadder && Time.deltaTime != 0)
                        {
                                float from = playerCenter.x;
                                float to = ladder.CenterX();
                                float speed = player.speed * Time.deltaTime * 0.10f;
                                float moveTo = Mathf.MoveTowards(from, to, speed);
                                velocity.x = (moveTo - from) / Time.deltaTime;
                        }
                        if (climbUp && playerBottom >= ladder.top)
                        {
                                velocity.y = 0;
                        }
                        player.signals.Set("ladderClimb");
                }

                private void ClimbToLadderTop (AbilityManager player, bool climbDown, ref Vector2 velocity)
                {
                        if (!ladder.standOnLadder || Time.deltaTime == 0)
                        {
                                return;
                        }
                        if (climbDown)
                        {
                                return;
                        }
                        if ((playerBottom + (velocity.y * Time.deltaTime) + 0.01f) >= ladder.top)
                        {
                                velocity.y = (ladder.top - playerBottom) / Time.deltaTime;
                                player.signals.Set("ladderClimb", false);
                                player.character.canUseTool = true;
                                player.OnSurface();
                                OnExit(player);
                                OnLadderTop(player);
                        }
                }

                private void LadderTopReached (bool standOnLadder, ref Vector2 velocity)
                {
                        if (!standOnLadder && velocity.y > 0 && (playerCenter.y + velocity.y * Time.deltaTime) >= ladder.top)
                        {
                                velocity.y = 0;
                        }
                }

                private void Jump (AbilityManager player, ref Vector2 velocity)
                {
                        if (!player.jumpButtonActive)
                        {
                                return;
                        }
                        if (ladder.stopSideJump && ((player.world.box.bottomLeft.x <= (ladder.left + 1f)) || (player.world.box.bottomRight.x >= (ladder.right - 1f))))
                        {
                                jumpingFromLadder = true;
                        }
                        if (ladder.canJumpUp || (!ladder.stopSideJump && player.inputX != 0 && !player.world.onWall) || !ladder.ContainsY(playerTop))
                        {
                                jumpingFromLadder = true;
                        }
                        if (jumpingFromLadder)
                        {
                                velocity = new Vector2(velocity.x, jump);
                                player.signals.Set("ladderClimb", false);
                                player.CheckForAirJumps();
                                player.OnSurface(false);
                                fenceReverse.StopFlip();
                                jumpPoint = playerBottom;
                        }
                }

                private void GetPlayerPositions (AbilityManager player)
                {
                        playerTop = player.world.box.top;
                        playerBottom = player.world.box.bottom;
                        playerCenter = player.world.box.center;
                }

                public override void PostAIExecute (AbilityManager player)
                {
                        if (ladderRef != null && ladder != null && Time.deltaTime != 0)
                        {
                                jumpPoint += ladderVelocity.y;
                                Vector2 ladderVelocityAdjusted = Time.deltaTime != 0 ? ladderVelocity / Time.deltaTime : Vector2.zero;
                                player.world.MoveBasic(ref ladderVelocityAdjusted, Time.deltaTime);
                                ladderPosition = ladder.rawPosition;
                        }
                }

                public void OnExit (AbilityManager player)
                {
                        onExit.Invoke(ImpactPacket.impact.Set(exitWE, transform, player.world.boxCollider, player.world.position, null, player.world.box.down, player.playerDirection, 0));
                }

                public void OnLadderTop (AbilityManager player)
                {
                        onLadderTop.Invoke(ImpactPacket.impact.Set(ladderTopWE, transform, player.world.boxCollider, player.world.position, null, player.world.box.down, player.playerDirection, 0));
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField, HideInInspector] private bool enterFoldOut;
                [SerializeField, HideInInspector] private bool exitFoldOut;
                [SerializeField, HideInInspector] private bool entryFoldOut;
                [SerializeField, HideInInspector] private bool ladderTopFoldOut;
                public override bool OnInspector (SerializedObject controller, SerializedObject parent, string[] inputList, Color barColor, Color labelColor)
                {
                        if (Open(parent, "Ladder", barColor, labelColor))
                        {
                                int climb = parent.Enum("climb");
                                if (climb == 0)
                                {
                                        FoldOut.Box(4, FoldOut.boxColorLight, extraHeight: 5, offsetY: -2);
                                        {
                                                parent.Field("Climb", "climb");
                                                parent.DropDownDoubleList(inputList, "Up, Down", "up", "down");
                                                parent.FieldDouble("Climb Speed", "sideSpeed", "climbSpeed");
                                                Labels.FieldDoubleText("X", "Y");
                                                parent.FieldToggle("Auto Latch", "autoLatch");
                                        }
                                        if (FoldOut.FoldOutButton(parent.Get("entryFoldOut")))
                                        {
                                                Fields.EventFoldOutEffect(parent.Get("onEnter"), parent.Get("enterWE"), parent.Get("enterFoldOut"), "On Enter", color: FoldOut.boxColorLight);
                                                Fields.EventFoldOutEffect(parent.Get("onExit"), parent.Get("exitWE"), parent.Get("exitFoldOut"), "On Exit", color: FoldOut.boxColorLight);
                                                Fields.EventFoldOutEffect(parent.Get("onLadderTop"), parent.Get("ladderTopWE"), parent.Get("ladderTopFoldOut"), "On Ladder Top", color: FoldOut.boxColorLight);
                                        }

                                        FoldOut.Box(2, FoldOut.boxColorLight);
                                        {
                                                parent.Field("Jump", "jump");
                                                parent.Field("Jump Fall Point", "fallPoint");
                                        }
                                        Layout.VerticalSpacing(5);

                                        FoldOut.Box(1, FoldOut.boxColorLight);
                                        {
                                                parent.DropDownList(inputList, "Fence Flip", "fenceFlip");
                                        }
                                        Layout.VerticalSpacing(5);
                                }
                                else
                                {
                                        FoldOut.Box(4, FoldOut.boxColorLight, extraHeight: 5, offsetY: -2);
                                        {
                                                parent.Field("Climb", "climb");
                                                parent.Field("Climb Speed", "climbSpeed");
                                                parent.Field("Jump", "jump");
                                                parent.FieldToggle("Auto Latch", "autoLatch");
                                        }
                                        if (FoldOut.FoldOutButton(parent.Get("entryFoldOut")))
                                        {
                                                Fields.EventFoldOutEffect(parent.Get("onEnter"), parent.Get("enterWE"), parent.Get("enterFoldOut"), "On Enter", color: FoldOut.boxColorLight);
                                                Fields.EventFoldOutEffect(parent.Get("onExit"), parent.Get("exitWE"), parent.Get("exitFoldOut"), "On Exit", color: FoldOut.boxColorLight);
                                                Fields.EventFoldOutEffect(parent.Get("onLadderTop"), parent.Get("ladderTopWE"), parent.Get("ladderTopFoldOut"), "On Ladder Top", color: FoldOut.boxColorLight);
                                        }
                                }

                        }
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion

        }

        public enum LadderClimbType
        {
                Manual,
                Automatic
        }
}
