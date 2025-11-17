#region Editor
#if UNITY_EDITOR
using TwoBitMachines.Editors;
using UnityEditor;
#endif
#endregion;
using UnityEngine;

namespace TwoBitMachines.FlareEngine.ThePlayer
{
        [System.Serializable]
        public class WallSlide //anim signals: wall, wallSlide, wallClimb, wallLeft, wallRight, wallSlideJump
        {
                [SerializeField] public SlideType slideType;
                [SerializeField] public SlideForceType slideForce;

                [SerializeField] public float onSlideRate;
                [SerializeField] public float slideTimer = 1f;
                [SerializeField] public float slideFriction = 2f;
                [SerializeField] public float climbUpSpeed = 5f;

                [SerializeField] public bool canClimbUp;
                [SerializeField] public bool blockJumpClimb;
                [SerializeField] public bool enableSlideTimer;

                [SerializeField] public int jumpLimit = 1;
                [SerializeField] public bool jumpingLimit;
                [SerializeField] private bool variableJump;

                [SerializeField] public Vector2 jumpUp = new Vector2(10f, 15f);
                [SerializeField] public Vector2 jumpAway = new Vector2(10f, 15f);

                [SerializeField] public string onSlideWE;
                [SerializeField] public string enterWE;
                [SerializeField] public string exitWE;
                [SerializeField] public UnityEventEffect onEnter;
                [SerializeField] public UnityEventEffect onExit;
                [SerializeField] public UnityEventEffect onSlide;

                [System.NonSerialized] public bool isJumping;
                [System.NonSerialized] private bool isSliding;
                [System.NonSerialized] private bool monitorJumpHeight;
                [System.NonSerialized] private bool jumpTimerEnabled;

                [System.NonSerialized] private const float stickTime = 0.2f;
                [System.NonSerialized] private float onSlideRateCounter;
                [System.NonSerialized] private float jumpStickCounter;
                [System.NonSerialized] private float slideCounter;
                [System.NonSerialized] private float stickCounter;
                [System.NonSerialized] private float extraCounter;
                [System.NonSerialized] private float velocityXRef;
                [System.NonSerialized] private float jumpPoint;

                [System.NonSerialized] private int jumpCounter;
                [System.NonSerialized] private int wallDirectionRef;

                [System.NonSerialized] private SlideJumpType jumpType;

                public void Reset (AbilityManager player)
                {
                        if (isSliding)
                        {
                                onExit.Invoke(ImpactPacket.impact.Set(exitWE, player.world.transform, player.world.boxCollider, player.world.position, null, player.world.box.down, player.playerDirection, 0));
                        }

                        ExitJump();
                        velocityXRef = 0;
                        jumpCounter = 0;
                        extraCounter = 0;
                        slideCounter = 0;
                        onSlideRateCounter = 10000f;
                        isSliding = false;
                        jumpTimerEnabled = false;
                        monitorJumpHeight = false;
                }

                private void ExitJump ()
                {
                        jumpStickCounter = 0;
                        stickCounter = 0;
                        isJumping = false;
                }

                public void Slide (Wall wall, AbilityManager player, ref Vector2 velocity)
                {
                        if (!player.world.onWall || isJumping || (jumpingLimit && jumpCounter > jumpLimit))
                        {
                                return;
                        }
                        if (!canClimbUp && velocity.y >= 0)
                        {
                                return;
                        }
                        if (player.world.missedAHorizontal && !DetectWall(player, wall.Direction(player)))
                        {
                                return;
                        }
                        if (enableSlideTimer && Clock.TimerExpired(ref slideCounter, slideTimer))
                        {
                                return;
                        }
                        if (Jump(player, wall.Direction(player), ref velocity))
                        {
                                jumpTimerEnabled = false;
                                extraCounter = 0;
                                return;
                        }
                        SlideDownWall(player, wall.Direction(player), ref velocity);
                }

                private void SlideDownWall (AbilityManager player, int wallDirection, ref Vector2 velocity)
                {
                        velocity.y = slideForce == SlideForceType.Velocity ? slideFriction * -1 : velocity.y * slideFriction;
                        velocity.x = slideType == SlideType.Automatic || StickTime(player, wallDirection) ? 0.25f * wallDirection : velocity.x;

                        if (canClimbUp && slideType == SlideType.Automatic && player.inputX != 0)
                        {
                                if (Compute.SameSign(player.inputX, wallDirection))
                                {
                                        velocity.y = climbUpSpeed;
                                        player.signals.Set("wallClimb"); // force climb up
                                }
                        }
                        if (!isSliding)
                        {
                                onEnter.Invoke(ImpactPacket.impact.Set(enterWE, player.world.transform, player.world.boxCollider, player.world.position, null, player.world.box.down, player.playerDirection, 0));
                        }
                        isSliding = true;
                        player.signals.Set("wall");
                        player.signals.Set("wallSlide");
                        player.signals.Set("wallLeft", wallDirection < 0);
                        player.signals.Set("wallRight", wallDirection > 0);
                        player.signals.ForceDirection(wallDirection);

                        if (onSlideRate > 0 && Clock.Timer(ref onSlideRateCounter, onSlideRate))
                        {
                                ImpactPacket impact = ImpactPacket.impact.Set(onSlideWE, player.world.transform, player.world.boxCollider, player.world.transform.position, null, player.world.box.right * wallDirection, player.playerDirection, 0);
                                onSlide.Invoke(impact);
                        }

                }

                private bool Jump (AbilityManager player, int wallDirection, ref Vector2 velocity)
                {
                        if (jumpTimerEnabled) // extra stick time, player jumped without input x and fails to jump away. Recheck.
                        {
                                if (player.inputX != 0 && !Compute.SameSign(player.inputX, wallDirection))
                                {
                                        JumpAway(player, wallDirection, ref velocity);
                                        return true;
                                }
                                if (Clock.Timer(ref extraCounter, stickTime))
                                {
                                        JumpFailed(player, wallDirection, ref velocity);
                                        return true;
                                }
                                return false;
                        }

                        if (!player.jumpButtonPressed || (jumpingLimit && jumpCounter++ >= jumpLimit))
                        {
                                return false; // not jumping
                        }
                        if (player.inputX == 0)
                        {
                                jumpTimerEnabled = true;
                                extraCounter = 0;
                                return false;
                        }
                        if (!Compute.SameSign(player.inputX, wallDirection))
                        {
                                JumpAway(player, wallDirection, ref velocity);
                        }
                        else
                        {
                                JumpUp(player, wallDirection, ref velocity);
                        }
                        return true;
                }

                private void JumpUp (AbilityManager player, int wallDirection, ref Vector2 velocity)
                {
                        velocity = new Vector2(jumpUp.x * -wallDirection, jumpUp.y);
                        velocityXRef = -wallDirection * jumpUp.x;
                        wallDirectionRef = wallDirection;
                        player.CheckForAirJumps();

                        isJumping = true;
                        monitorJumpHeight = variableJump;
                        jumpType = SlideJumpType.JumpUp;
                        jumpPoint = player.world.position.y - 0.5f;
                        player.world.mp.Follow();
                }

                private void JumpAway (AbilityManager player, int wallDirection, ref Vector2 velocity)
                {
                        velocity = new Vector2(Mathf.Abs(jumpAway.x) * -wallDirection, jumpAway.y);
                        velocityXRef = -wallDirection * jumpUp.x;
                        wallDirectionRef = wallDirection;
                        player.CheckForAirJumps();

                        isJumping = true;
                        monitorJumpHeight = variableJump;
                        jumpType = SlideJumpType.JumpAway;
                        jumpPoint = player.world.position.y - 0.5f;
                        player.world.mp.Launch(ref velocity);
                }

                private void JumpFailed (AbilityManager player, int wallDirection, ref Vector2 velocity)
                {
                        velocity = Vector2.zero;
                        velocityXRef = -wallDirection * jumpUp.x;
                        wallDirectionRef = wallDirection;

                        isJumping = false;
                        jumpType = SlideJumpType.Fall;
                        jumpPoint = player.world.position.y - 0.5f;
                        player.world.mp.Launch(ref velocity);
                }

                private bool StickTime (AbilityManager player, int wallDirection)
                {
                        if (slideType == SlideType.PressingInput && player.inputX == 0) //            if player is not pressing any x input, stick to wall for a bit in case players wants to jump
                        {
                                if (!Clock.Timer(ref stickCounter, stickTime))
                                {
                                        return true;
                                }
                        }
                        else
                        {
                                stickCounter = 0;
                        }

                        if (player.inputX != 0 && !Compute.SameSign(player.inputX, wallDirection)) // when player is moving away from wall, stick to wall for a bit in case player wants to jump
                        {
                                if (!Clock.Timer(ref jumpStickCounter, stickTime))
                                {
                                        return true;
                                }
                        }
                        else
                        {
                                jumpStickCounter = 0;
                        }
                        return false;
                }

                public void CompleteJump (AbilityManager player, ref Vector2 velocity)
                {
                        if (monitorJumpHeight && !player.jumpButtonHold && velocity.y > 0)
                        {
                                velocity.y *= 0.4f;
                                monitorJumpHeight = false;
                        }

                        if (!isJumping)
                                return;

                        if (player.ground || jumpType == SlideJumpType.Fall)
                        {
                                ExitJump();
                                return;
                        }
                        if (jumpType == SlideJumpType.JumpUp)
                        {
                                if ((!blockJumpClimb || player.world.position.y < jumpPoint) && player.world.onWall)
                                {
                                        ExitJump();
                                }
                                else if (!Compute.SameSign(player.inputX, wallDirectionRef) || player.inputX == 0)
                                {
                                        jumpType = SlideJumpType.JumpAway;
                                }
                                else // move away and then towards wall
                                {
                                        velocity.x = velocityXRef; // this will override impede change from ability walk
                                        velocityXRef = Mathf.Clamp(velocityXRef + player.speed * wallDirectionRef * Time.deltaTime * 5f, -player.speed, player.speed);
                                        player.signals.Set("wallSlideJump", true);
                                        player.signals.Set("wallLeft", wallDirectionRef < 0);
                                        player.signals.Set("wallRight", wallDirectionRef > 0);
                                        player.signals.ForceDirection(wallDirectionRef);
                                }
                        }
                        if (jumpType == SlideJumpType.JumpAway)
                        {
                                if ((!blockJumpClimb || player.world.position.y < jumpPoint) && player.world.onWall)
                                {
                                        ExitJump();
                                }
                        }
                }

                private bool DetectWall (AbilityManager player, int wallDirection)
                {
                        Vector2 origin = player.world.box.center;
                        Vector2 direction = player.world.box.right * wallDirection;
                        return Physics2D.Raycast(origin, direction, player.world.box.sizeX, WorldManager.collisionMask);
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField, HideInInspector] private bool eventsFoldOut;
                [SerializeField, HideInInspector] private bool onSlideFoldOut;
                [SerializeField, HideInInspector] private bool enterFoldOut;
                [SerializeField, HideInInspector] private bool exitFoldOut;

                public static void OnInspector (SerializedObject parent, int type)
                {
                        SerializedProperty wallSlide = parent.Get("wallSlide");

                        if (type == 0)
                        {
                                if (FoldOut.FoldOutButton(wallSlide.Get("eventsFoldOut")))
                                {
                                        Fields.EventFoldOutEffectAndRate(wallSlide.Get("onSlide"), wallSlide.Get("onSlideWE"), wallSlide.Get("onSlideRate"), wallSlide.Get("onSlideFoldOut"), "On Slide", color: FoldOut.boxColorLight);
                                        Fields.EventFoldOutEffect(wallSlide.Get("onEnter"), wallSlide.Get("enterWE"), wallSlide.Get("enterFoldOut"), "On Enter", color: FoldOut.boxColorLight);
                                        Fields.EventFoldOutEffect(wallSlide.Get("onExit"), wallSlide.Get("exitWE"), wallSlide.Get("exitFoldOut"), "On Exit", color: FoldOut.boxColorLight);
                                }

                                int slideType = wallSlide.Enum("slideType");
                                int height = slideType == 1 ? 1 : 0;
                                FoldOut.Box(3 + height, FoldOut.boxColorLight);
                                {
                                        wallSlide.Field("Slide When", "slideType");
                                        wallSlide.FieldDouble("Slide Speed", "slideFriction", "slideForce");
                                        wallSlide.FieldAndEnableHalf("Slide Timer", "slideTimer", "enableSlideTimer");
                                        wallSlide.FieldAndEnableHalf("Climb Up", "climbUpSpeed", "canClimbUp", execute: slideType == 1);
                                        Labels.FieldText("Speed", rightSpacing: 17, execute: slideType == 1);
                                }
                                Layout.VerticalSpacing(5);

                                FoldOut.Box(5, FoldOut.boxColorLight);
                                {
                                        wallSlide.Field("Jump Up", "jumpUp");
                                        wallSlide.Field("Jump Away", "jumpAway");
                                        wallSlide.FieldAndEnableHalf("Jump Limit", "jumpLimit", "jumpingLimit");
                                        wallSlide.FieldToggle("Block Jump Climb", "blockJumpClimb");
                                        wallSlide.FieldToggle("Variable Jump Height", "variableJump");
                                }
                                Layout.VerticalSpacing(5);
                        }
                }
#pragma warning restore 0414
#endif
                #endregion

        }

        public enum SlideType
        {
                PressingInput,
                Automatic
        }

        public enum SlideJumpType
        {
                JumpUp,
                JumpAway,
                Fall
        }

        public enum SlideForceType
        {
                Velocity,
                Acceleration
        }
}
