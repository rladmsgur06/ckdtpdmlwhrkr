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
        public class WallClimb // anim signals: wall, wallHold, wallSlide, wallClimb
        {
                [SerializeField] public ClimbType climbType;
                [SerializeField] private HoldType holdType;
                [SerializeField] public float onClimbRate = 0.1f;
                [SerializeField] public string worldEffect;

                [SerializeField] private string hold = "None";
                [SerializeField] private string climbUp = "Up";
                [SerializeField] private string climbDown = "Down";

                [SerializeField] private float climbUpSpeed = 2f;
                [SerializeField] private float climbDownSpeed = 2f;

                [SerializeField] private float jumpUp;
                [SerializeField] private float jumpAway = 10;
                [SerializeField] private bool jumpingLimit;
                [SerializeField] private bool jumpOnInputChange;
                [SerializeField] private bool variableJump;
                [SerializeField] private int jumpLimit = 1;

                [SerializeField] private WallSlideType slideDown;
                [SerializeField] private WallSlideSpeedType slideDownSpeed;
                [SerializeField] private float slideVelocity = 5;
                [SerializeField] private float slideAccelerate = 5;
                [SerializeField] private float slideRate = 0.1f;
                [SerializeField] private bool climbUpOnly;
                [SerializeField] private bool enableClimbTimer;
                [SerializeField] private float climbTimer = 1f;
                [SerializeField] private float fallTimer = 1f;

                [SerializeField] public string enterWE;
                [SerializeField] public string exitWE;
                [SerializeField] public string slideWE;
                [SerializeField] public UnityEventEffect onEnter;
                [SerializeField] public UnityEventEffect onExit;
                [SerializeField] public UnityEventEffect onClimb;
                [SerializeField] public UnityEventEffect onSlide;
                [SerializeField] public UnityEventFloat onClimbLimit;
                [SerializeField] public UnityEventFloat onFallLimit;

                [System.NonSerialized] private bool upJump;
                [System.NonSerialized] private bool autoCornerJump;
                [System.NonSerialized] private bool fallToCorner;
                [System.NonSerialized] private bool monitorJumpHeight;

                [System.NonSerialized] public bool clearBarrierY;
                [System.NonSerialized] private bool isHolding;
                [System.NonSerialized] private bool stickActive;
                [System.NonSerialized] private float barrierY;
                [System.NonSerialized] private float barrierSign;
                [System.NonSerialized] private float slideStamp;
                [System.NonSerialized] private float slideAccel;
                [System.NonSerialized] private float slideRateCounter;
                [System.NonSerialized] private float onClimbRateCounter;
                [System.NonSerialized] private float stickTimeCounter;
                [System.NonSerialized] private float stickInputTimeCounter;
                [System.NonSerialized] private float climbCounter;
                [System.NonSerialized] private float fallCounter;
                [System.NonSerialized] private float cornerTopY;
                [System.NonSerialized] private int jumpCounter;
                [System.NonSerialized] private int wallDirRef;
                [System.NonSerialized] private Vector2 jumpToVelocity;

                public bool isJumping => fallToCorner || autoCornerJump || upJump;
                public bool cantClimb => enableClimbTimer && climbCounter >= climbTimer;

                public void Reset (AbilityManager player)
                {
                        if (isHolding)
                        {
                                OnExit(player);
                        }
                        ResetJumpSignals();
                        ResetStickTime();
                        climbCounter = 0;
                        fallCounter = 0;
                        jumpCounter = 0;
                        isHolding = false;
                        clearBarrierY = false;
                        monitorJumpHeight = false;
                }

                public void OnExit (AbilityManager player)
                {
                        onExit.Invoke(ImpactPacket.impact.Set(exitWE, player.world.transform, player.world.boxCollider, player.world.position, null, player.world.box.down, player.playerDirection, 0));
                }

                private void ResetJumpSignals ()
                {
                        upJump = false;
                        fallToCorner = false;
                        autoCornerJump = false;
                }

                private void ResetStickTime ()
                {
                        slideRateCounter = slideRate;
                        onClimbRateCounter = onClimbRate;
                        stickInputTimeCounter = 0;
                        stickTimeCounter = 0;
                        stickActive = false;
                }

                public bool ClearBarrierY (AbilityManager player)     // prevent wall spamming
                {
                        if (!clearBarrierY)
                        {
                                return false;
                        }
                        if (player.inputX != 0 && !Compute.SameSign(barrierSign, player.inputX))
                        {
                                clearBarrierY = false;
                        }
                        else if (player.world.position.y <= barrierY || WallGap(player, player.world.box))
                        {
                                clearBarrierY = false;
                        }
                        return clearBarrierY;
                }

                private bool WallGap (AbilityManager player, BoxInfo box)
                {
                        Vector2 corner = barrierSign > 0 ? box.bottomRight - box.skinX : box.bottomLeft + box.skinX;
                        for (int i = 0; i < box.rays.x; i++)
                        {
                                Vector2 origin = corner + box.up * box.spacing.y * i;
                                RaycastHit2D hit = Physics2D.Raycast(origin, box.right * barrierSign, 0.25f, player.world.collisionLayer);
                                if (!hit)
                                        return true;
                        }
                        return false;
                }

                public void Climb (Wall wall, AbilityManager player, ref Vector2 velocity)
                {
                        if (!player.world.onWall || clearBarrierY || isJumping || (jumpingLimit && jumpCounter > jumpLimit) || ClimbTimer(player))
                        {
                                return;
                        }
                        if (holdType == HoldType.HoldButton && !player.inputs.Holding(hold))
                        {
                                return;
                        }
                        if (holdType == HoldType.Automatic && !isHolding && velocity.y > 0)
                        {
                                velocity.y *= 0.9f; //                                        slow down velocity y if crashing into wall before latching on
                                return;
                        }

                        int wallDirection = wall.Direction(player);

                        if (player.ground && player.inputX != 0 && !Compute.SameSign(player.inputX, wallDirection))
                        {
                                return;
                        }

                        if (!isHolding && !DetectWall(player, wallDirection, velocity.x)) //  do not start climbing until player is below corner
                        {
                                if (velocity.y > 0)
                                        return; //                                            no wall detected, but player is jumping over platform, skip wall climb
                                wallDirRef = wallDirection;
                                fallToCorner = true;
                                return;
                        }

                        if (Jump(player, wall, player.inputX, wallDirection, ref velocity))
                        {
                                return;
                        }
                        ClimbWall(player, wallDirection, ref velocity);
                }

                private bool ClimbTimer (AbilityManager player)
                {
                        if (isJumping || player.ground)
                        {
                                return false;
                        }
                        if (enableClimbTimer && Clock.TimerExpired(ref climbCounter, climbTimer))
                        {
                                if (fallTimer > 0 && Clock.TimerExpired(ref fallCounter, fallTimer))
                                {
                                        return true;
                                }
                                if (fallTimer > 0)
                                {
                                        onFallLimit.Invoke(fallCounter / fallTimer);
                                }
                        }
                        if (enableClimbTimer && climbTimer > 0)
                        {
                                float percent = climbCounter / climbTimer;
                                if (percent >= 0.70f)
                                {
                                        onClimbLimit.Invoke(percent);
                                }
                        }
                        return false;
                }

                private bool Jump (AbilityManager player, Wall wall, float inputX, int wallDirection, ref Vector2 velocity)
                {
                        if (jumpOnInputChange)
                        {
                                if (isHolding && !stickActive && (!Compute.SameSign(inputX, wallDirection) || inputX == 0))
                                {
                                        stickActive = true;
                                        stickInputTimeCounter = Time.time;
                                }
                                if (stickActive)
                                {
                                        if (Time.time >= (stickInputTimeCounter + 0.1f))
                                        {
                                                ResetStickTime();
                                        }
                                        if (inputX != 0 && !Compute.SameSign(inputX, wallDirection))
                                        {
                                                velocity = new Vector2(velocity.x, jumpAway);
                                                player.world.mp.Launch(ref velocity);
                                                player.CheckForAirJumps();
                                                ResetStickTime();
                                        }
                                        else
                                        {
                                                isHolding = true;
                                                velocity.x = 0.25f * wallDirection;
                                        }
                                        return true;
                                }
                        }

                        if (StickTime(player, inputX) && (!jumpingLimit || jumpCounter++ < jumpLimit))
                        {
                                if (inputX == 0 || Compute.SameSign(inputX, wallDirection)) // jump up
                                {
                                        velocity = new Vector2(0, jumpUp);
                                        wallDirRef = wallDirection;
                                        player.world.mp.Launch(ref velocity);
                                        player.CheckForAirJumps();
                                        ResetStickTime();
                                        upJump = true;
                                        monitorJumpHeight = variableJump;
                                        barrierSign = wallDirection;
                                        barrierY = player.world.position.y;
                                        clearBarrierY = true; // must reset means the player can only jump up once to prevent wall jump spaming
                                        OnExit(player);
                                        return true;
                                }
                                else // jump away
                                {
                                        velocity = new Vector2(velocity.x, jumpAway);
                                        player.world.mp.Launch(ref velocity);
                                        player.CheckForAirJumps();
                                        ResetStickTime();
                                        monitorJumpHeight = variableJump;
                                        OnExit(player);
                                        return true;
                                }
                        }

                        if (player.world.missedAHorizontal && FindCornerForAutomaticJump(player.world.box, wallDirection))
                        {
                                Vector2 jumpTo = new Vector2(player.world.position.x + (player.world.box.sizeX) * wallDirection, cornerTopY);
                                velocity = Compute.ArchObject(player.world.box.bottomCenter, jumpTo, 0.25f, player.gravity.gravity);
                                player.world.mp.Follow();
                                jumpToVelocity = velocity;
                                wallDirRef = wallDirection;
                                autoCornerJump = true;
                                return true;
                        }
                        return false;
                }

                private bool StickTime (AbilityManager player, float inputX)
                {
                        if (player.jumpButtonPressed)
                        {
                                if (inputX != 0)
                                        return true; //                       can jump immediately if player input x is not zero
                                stickActive = true;
                        }
                        return stickActive && Clock.Timer(ref stickTimeCounter, 0.1f);
                }

                private void ClimbWall (AbilityManager player, int wallDirection, ref Vector2 velocity)
                {
                        float climbY = cantClimb ? 0 : Inputs(player, wallDirection);
                        if (climbY != 0)
                        {
                                slideStamp = Time.time + 0.15f;
                        }

                        if (climbUpOnly && climbY <= 0)
                        {
                                return;
                        }

                        velocity.y = climbY;
                        velocity.x = 0.25f * wallDirection; // stay on wall

                        bool sliding = false;
                        if (slideDown != WallSlideType.None)
                        {
                                if (slideDown == WallSlideType.OnNoInput && climbY == 0 && Time.time >= slideStamp)
                                {
                                        sliding = true;
                                }
                                if (slideDown == WallSlideType.OnNoInputUp && climbY <= 0)
                                {
                                        sliding = true;
                                }
                                if (climbY > 0)
                                {
                                        slideAccel = 0;
                                }
                                if (sliding)
                                {
                                        if (slideDownSpeed == WallSlideSpeedType.Velocity)
                                        {
                                                velocity.y = -Mathf.Abs(slideVelocity == 0 ? 1f : slideVelocity);
                                        }
                                        else
                                        {
                                                slideAccel -= Time.deltaTime * slideAccelerate;
                                                slideAccel = player.ground ? 0 : slideAccel < -30f ? -30f : slideAccel;
                                                velocity.y = -0.5f + slideAccel;
                                        }
                                        if (slideRate > 0 && Clock.Timer(ref slideRateCounter, slideRate))
                                        {
                                                ImpactPacket impact = ImpactPacket.impact.Set(slideWE, player.world.transform, player.world.boxCollider, player.world.position, null, player.world.box.right * wallDirection, player.playerDirection, 0);
                                                onSlide.Invoke(impact);
                                        }
                                }
                        }

                        if (!isHolding)
                        {
                                ResetStickTime();
                                onEnter.Invoke(ImpactPacket.impact.Set(enterWE, player.world.transform, player.world.boxCollider, player.world.position, null, player.world.box.right * wallDirection, player.playerDirection, 0));
                        }
                        if ((!isHolding || velocity.y != 0) && onClimbRate > 0 && Clock.Timer(ref onClimbRateCounter, onClimbRate))
                        {
                                ImpactPacket impact = ImpactPacket.impact.Set(worldEffect, player.world.transform, player.world.boxCollider, player.world.position, null, player.world.box.right * wallDirection, player.playerDirection, 0);
                                onClimb.Invoke(impact);
                        }

                        isHolding = true;
                        player.signals.Set("wall");
                        player.signals.Set("wallSlide", sliding);
                        player.signals.Set("wallLeft", wallDirection < 0);
                        player.signals.Set("wallRight", wallDirection > 0);
                        player.signals.Set("wallHold", velocity.y == 0);
                        player.signals.Set("wallClimb", !sliding && velocity.y != 0);
                        player.signals.ForceDirection(wallDirection);

                }

                public float Inputs (AbilityManager player, int wallDirection)
                {
                        if (climbType == ClimbType.None)
                        {
                                return 0;
                        }

                        bool useButtons = climbType == ClimbType.Button;
                        bool up = player.inputs.Holding(useButtons ? climbUp : "Left");
                        bool down = player.inputs.Holding(useButtons ? climbDown : "Right");

                        if (!useButtons && !up && player.inputX < 0)
                        {
                                up = true;
                        }

                        if (!useButtons && !down && player.inputX > 0)
                        {
                                down = true;
                        }

                        if (useButtons || wallDirection == -1)
                        {
                                return (up ? climbUpSpeed : 0) + (down ? -climbDownSpeed : 0);
                        }
                        else
                        {
                                return (up ? -climbDownSpeed : 0) + (down ? climbUpSpeed : 0);
                        }
                }

                public void CompleteJump (AbilityManager player, ref Vector2 velocity)
                {
                        if (monitorJumpHeight && !player.jumpButtonHold && velocity.y > 0)
                        {
                                velocity.y *= 0.4f;
                                monitorJumpHeight = false;
                        }

                        if (fallToCorner)
                        {
                                if (player.ground || !Compute.SameSign(player.inputX, wallDirRef) || DetectWall(player, wallDirRef, velocity.x))
                                {
                                        ResetJumpSignals();
                                }
                        }
                        else if (upJump)
                        {
                                if (player.ground || velocity.y < 0 || (player.inputX != 0 && !Compute.SameSign(player.inputX, wallDirRef)))
                                {
                                        ResetJumpSignals();
                                }
                        }
                        else if (autoCornerJump)
                        {
                                player.signals.Set("autoCornerJump");
                                if (player.ground || Wall.CheckForGround(player.world.box, jumpToVelocity.x, velocity.y) || (player.inputX != 0 && !Compute.SameSign(player.inputX, wallDirRef)))
                                {
                                        ResetJumpSignals();
                                }
                                else
                                {
                                        velocity.x = jumpToVelocity.x;
                                }
                        }
                }

                private bool FindCornerForAutomaticJump (BoxInfo box, int wallDirection)
                {
                        float shift = -box.size.y * 0.25f; // jump automatically to platform top if player is at least 25% above corner
                        Vector2 origin = box.TopExactCorner(wallDirection) + box.up * shift + box.right * 0.1f * wallDirection;
                        RaycastHit2D hit = Physics2D.Raycast(origin, box.down, box.size.y + shift, WorldManager.collisionMask);

                        if (!hit || hit.distance == 0)
                        {
                                return false;
                        }
                        cornerTopY = hit.point.y;
                        return true;
                }

                private bool DetectWall (AbilityManager player, int wallDirection, float velX)
                {
                        Vector2 origin = player.world.box.TopExactCorner(wallDirection);
                        Vector2 direction = player.world.box.right * wallDirection;
                        float length = Mathf.Abs(velX * Time.deltaTime);
                        return Physics2D.Raycast(origin, direction, length, WorldManager.collisionMask);
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField, HideInInspector] private bool climbLimitFoldOut;
                [SerializeField, HideInInspector] private bool fallLimitFoldOut;
                [SerializeField, HideInInspector] private bool onClimbFoldOut;
                [SerializeField, HideInInspector] private bool slideFoldOut;
                [SerializeField, HideInInspector] private bool eventsFoldOut;
                [SerializeField, HideInInspector] private bool enterFoldOut;
                [SerializeField, HideInInspector] private bool exitFoldOut;

                public static void OnInspector (SerializedObject parent, string[] inputList, int type)
                {
                        SerializedProperty wallClimb = parent.Get("wallClimb");

                        if (type == 1)
                        {
                                if (FoldOut.FoldOutButton(wallClimb.Get("eventsFoldOut")))
                                {
                                        Fields.EventFoldOutEffectAndRate(wallClimb.Get("onClimb"), wallClimb.Get("worldEffect"), wallClimb.Get("onClimbRate"), wallClimb.Get("onClimbFoldOut"), "On Climb", color: Tint.Blue);
                                        Fields.EventFoldOutEffectAndRate(wallClimb.Get("onSlide"), wallClimb.Get("slideWE"), wallClimb.Get("slideRate"), wallClimb.Get("slideFoldOut"), "On Slide", color: Tint.Blue);
                                        Fields.EventFoldOutEffect(wallClimb.Get("onEnter"), wallClimb.Get("enterWE"), wallClimb.Get("enterFoldOut"), "On Enter", color: Tint.Blue);
                                        Fields.EventFoldOutEffect(wallClimb.Get("onExit"), wallClimb.Get("exitWE"), wallClimb.Get("exitFoldOut"), "On Exit", color: Tint.Blue);
                                        Fields.EventFoldOut(wallClimb.Get("onClimbLimit"), wallClimb.Get("climbLimitFoldOut"), "On Climb Limit", color: Tint.Blue);
                                        Fields.EventFoldOut(wallClimb.Get("onFallLimit"), wallClimb.Get("fallLimitFoldOut"), "On Fall Limit", color: Tint.Blue);

                                }

                                int holdType = wallClimb.Enum("holdType");
                                int climbType = wallClimb.Enum("climbType");
                                int slideType = wallClimb.Enum("slideDown");
                                int slideSpeed = wallClimb.Enum("slideDownSpeed");
                                int height = climbType == 0 ? 1 : 0;
                                FoldOut.Box(2 + height, FoldOut.boxColorLight);
                                {
                                        wallClimb.Field("Buttons", "climbType");
                                        wallClimb.DropDownDoubleList(inputList, "Up, Down", "climbUp", "climbDown", execute: wallClimb.Enum("climbType") == 0);
                                        wallClimb.Field("Hold Wall", "holdType", execute: holdType == 0);
                                        wallClimb.FieldAndDropDownList(inputList, "Hold Wall", "holdType", "hold", execute: holdType == 1);
                                }
                                Layout.VerticalSpacing(5);

                                height = slideType == 0 ? 0 : 1;
                                FoldOut.Box(4 + height, FoldOut.boxColorLight);
                                {
                                        wallClimb.FieldDouble("Speed", "climbUpSpeed", "climbDownSpeed");
                                        Labels.FieldDoubleText("Up", "Down", rightSpacing: 3);
                                        wallClimb.FieldDoubleAndEnableHalf("Climb Limit", "climbTimer", "fallTimer", "enableClimbTimer");
                                        Labels.FieldDoubleText("Climb", "Fall", rightSpacing: Layout.boolWidth + 6);
                                        wallClimb.Field("Slide Down", "slideDown");
                                        wallClimb.FieldDouble("Slide Speed", "slideDownSpeed", "slideVelocity", execute: slideType > 0 && slideSpeed == 0);
                                        wallClimb.FieldDouble("Slide Speed", "slideDownSpeed", "slideAccelerate", execute: slideType > 0 && slideSpeed == 1);
                                        wallClimb.FieldToggle("Climb Up Only", "climbUpOnly");
                                }
                                Layout.VerticalSpacing(5);

                                FoldOut.Box(5, FoldOut.boxColorLight);
                                {
                                        wallClimb.Field("Jump Up", "jumpUp");
                                        wallClimb.Field("Jump Away", "jumpAway");
                                        wallClimb.FieldAndEnableHalf("Jump Limit", "jumpLimit", "jumpingLimit");
                                        wallClimb.FieldToggle("Jump On X Input", "jumpOnInputChange");
                                        wallClimb.FieldToggle("Variable Jump Height", "variableJump");
                                }
                                Layout.VerticalSpacing(5);
                        }

                }

#pragma warning restore 0414
#endif
                #endregion

        }

        public enum ClimbType
        {
                Button,
                PlayerDirection,
                None
        }

        public enum HoldType
        {
                Automatic,
                HoldButton
        }

        public enum WallSlideType
        {
                None,
                OnNoInput,
                OnNoInputUp
        }

        public enum WallSlideSpeedType
        {
                Velocity,
                Accelerate
        }
}
