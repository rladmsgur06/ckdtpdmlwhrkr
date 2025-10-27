using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using System.Collections;

namespace cowsins2D
{
    [System.Serializable]
    public enum JumpMethod
    {
        Default, HoldToJumpHigher
    }

    [System.Serializable]
    public enum PlayerOrientationMethod
    {
        None,
        HorizontalInput,
        AimBased,
        Mixed
    }
    [System.Serializable]
    public enum TurnMethod
    {
        Simple,
        Smooth
    }

    [System.Serializable]
    public enum DashMethod
    {
        None,
        Default,
        AimBased,
        OrientationBased,
        HorizontalAimBased
    }
    [System.Serializable]
    public enum GlideDurationMethod
    {
        None,
        TimeBased
    }
    public class PlayerMovement : MonoBehaviour
    {
        [System.Serializable]
        public class Events
        {
            public UnityEvent onIdle, onWalking, onRunning, onTurn,
                onJump, onStartFall, onLand,
                onWallJump,
                onStartWallSliding, onWallSliding,
                onStartCrouch, onCrouched, onCrouchedIdle, onCrouchWalking, onStopCrouch,
                onStartDash,
                onIdleLadder, onMovingLadder,
                onStartGlide, onGliding, onStopGliding;
        }

        [System.Serializable]
        public class Sounds
        {
            public AudioClip jumpSFX, wallJumpSFX, dashSFX, startGlideSFX, startCrouchSFX;
        }

        [SerializeField, Tooltip("Reference to the player graphics.")] private Transform graphics;

        [Tooltip("Gravity strength."), SerializeField] private float gravityScale;
        [Tooltip("Gravity added. ( As a multiplier ).")] [SerializeField] private float fallGravityMult;
        [Tooltip("Limit the maximum fall speed.")] [SerializeField] private float maxFallSpeed;
        [Tooltip("Limit the maximum vertical speed ( going upwards )")] [SerializeField] private float maxUpwardsSpeed;

        [Tooltip("If enabled, the player will not be able to walk, just to run.")]
        [SerializeField] private bool autoRun;
        [Tooltip("Speed while walking."), SerializeField] private float walkSpeed;
        [Tooltip("Speed while running."), SerializeField] private float runSpeed;
        [Tooltip("Speed while crouching."), SerializeField] private float crouchSpeed;
        [Tooltip("Speed horizontally while in a ladder."), SerializeField] private float horizontalLadderSpeed;
        [Tooltip("Speed to climb a ladder."), SerializeField] private float verticalLadderSpeed;
        [Tooltip("Capacity to gain speed and reach the maximum speed."), SerializeField] private float runAcceleration;
        [Tooltip("Capacity to lose speed and go idle."), SerializeField] private float runDecceleration;
        [Tooltip("Capacity to gain speed and reach the maximum speed mid-air.")][Range(0f, 1), SerializeField] private float airAcceleration;
        [Tooltip("Capacity to lose speed mid-air."), Range(0f, 1), SerializeField] private float airDeceleration;
        [Tooltip("Maximum floor angle to consider as a walkable area.")] [SerializeField] private float maxFloorAngle;

        [Tooltip("Configures the way the jump works. Default: Press to jump. You are able to hold. Hold to Jump Higher: Press = Tiny jump. Hold = Higher jumps depending on the held time.")]
        [SerializeField] private JumpMethod jumpMethod;
        [Tooltip("How many jumps you can do before landing. 1 is the default value assigned.")] [SerializeField, Min(0)] private int amountOfJumps;
        [Tooltip("How tall you will jump.")] [SerializeField] private float jumpForce;

        [Header("JUMP CUSTOMIZATION")]
        [Tooltip("Adjust the stiffness of the jump. The higher this value is the sharper it will perform. " +
            "Usually, sharp jumps are more responsive than smooth jumps, but it all depends on the style you are looking for.")]
        [SerializeField] private float apexReachSharpness;
        [Tooltip("Jump Hang is the movement at the apex of the jump. This adjusts the gravity of the jump in that point."), Range(0f, 1), SerializeField] private float jumpHangGravityMult;
        [Tooltip("Enable jump hang depending on the current velocity, so it gets activated before reaching the apex.")] [SerializeField] private float jumpHangTimeThreshold;
        [Tooltip("Acceleration multiplier on the apex of the jump.")]
        
        [SerializeField] private float jumpHangAccelerationMult;
        [Tooltip("Speed multiplier on the apex of the jump.")] [SerializeField] private float jumpHangMaxSpeedMult;
        [Tooltip("When falling, you can press “S” to fall faster. Adjust the multiplier here.")] [SerializeField] private float fastFallMultiplier;

        [SerializeField] private bool allowWallJump;
        [Tooltip("Impulse of the wall jump.")] [SerializeField] private Vector2 wallJumpForce;
        [SerializeField, Tooltip("If true, Inputs will be canceled for a short period of time after wall jumping, not allowing player to move")] private bool cancelInputsOnWallJump;
        [Tooltip("Duration of the wall jump before going back to default state."), Range(0f, 1.5f), SerializeField] private float wallJumpTime;

        [SerializeField] private bool allowSlide;

        [SerializeField, Tooltip("If disabled, the player will stick to the wall even if no Horizontal Input is applied.")] private bool requireHorizontalInputToSlide = false;

        [Tooltip("Enable to reset the amount of jumps when wall sliding.")]
        [SerializeField] private bool wallSlidingResetsJumps;
        [Tooltip("Velocity of wall sliding."), SerializeField] private float wallSlideSpeed;
        [Tooltip("Interval in seconds to display Wall slide visual effects"), SerializeField] private float wallSlideVFXInterval;

        [Tooltip("GameObject that represents the visual effects for sliding.")] [SerializeField] private GameObject wallSlideVFX;

        [Tooltip("Configure the behaviour of the dash. None: Disables dashing. Default: Based on Horizontal movement. " +
            "AimBased: Dash wherever you are aiming. HorizontalAimBased: Dash Horizontally based on the aim. "), SerializeField] private DashMethod dashMethod;
        [Tooltip("How many dashes you have available.")]
        [SerializeField, Min(0)] private int amountOfDashes;

        [Tooltip("Time in seconds to being able to use dashes again.")] [SerializeField] private float dashCooldown;

        [Tooltip("Gravity when dashing.")] [SerializeField] private float dashGravityScale;

        [Tooltip("Duration in seconds of the dash.")] [SerializeField] private float dashDuration;

        [Tooltip("Travel velocity when dashing.")] [SerializeField] private float dashSpeed;

        [Tooltip("When you stopped dashing, time to being able to perform a new dash ( to avoid dash clipping )")] [SerializeField, Min(.1f)] private float dashInterval;

        [Tooltip("If enabled, the player won´t be able to receive damage while dashing."), SerializeField] private bool invincibleWhileDashing;

        [SerializeField] private bool canGlide;

        [Tooltip("Speed of movement when gliding.")] [SerializeField] private float glideSpeed;

        [Tooltip("How fast the player goes downwards when gliding.")] [SerializeField] private float glideGravity;

        [Tooltip("Allows dash if gliding.")] [SerializeField] private bool canDashWhileGliding;

        [Tooltip("Adjusts the duration method for the gliding. None: Infinite gliding. " +
            "TimeBased: Adjusts the duration based on a timer in seconds."), SerializeField] private GlideDurationMethod glideDurationMethod;

        [Tooltip("Duration of the glide in seconds."), SerializeField] private float maximumGlideTime;

        [Tooltip("Allow the player to orientate ( based on the current orientation method ) while gliding."), SerializeField] private bool handleOrientationWhileGliding;

        [Tooltip("Allow the player to crouch."), SerializeField] private bool allowCrouch;

        public Vector2 crouchScaleMultiplier = new Vector2(0.75f, 0.4f);

        [SerializeField, Range(-1,1)] private float crouchCollisionOffset = -.2f;

        [Tooltip("Allow the player to crouch while airborne."), SerializeField] private bool canCrouchSlideMidAir;

        [Tooltip("if the player has enough speed momentum when crouching, it will perform a slide. Adjust the force of that slide.")] [SerializeField] private float crouchSlideSpeed;

        [Tooltip("Adjust the duration of the crouch slide."), SerializeField] private float crouchSlideDuration;


        //Stamina
        [Tooltip("You will lose stamina on performing actions when true."), SerializeField] private bool usesStamina;

        [Tooltip("Minimum stamina required to being able to run again."), SerializeField] private float minStaminaRequiredToRun;

        [Tooltip("Max amount of stamina."), SerializeField] private float maxStamina;

        [SerializeField, Min(1), Tooltip("Speed to regenerate the stamina. ")] private float staminaRegenMultiplier;


        [Tooltip("Amount of stamina lost on jumping."), SerializeField]
        private float staminaLossOnJump;

        [Tooltip("Amount of stamina lost on jumping."), SerializeField]
        private float staminaLossOnWallJump;

        [Tooltip("Amount of stamina lost on sliding."), SerializeField]
        private float staminaLossOnCrouchSlide;

        private bool canRun = true;

        [Header("OTHERS")]
        [Tooltip("Maximum height allowed for a surface to be allowed as a step.")] [SerializeField] private float stepHeight;

        [Tooltip("Handles the way the player is oriented. None: The player cannot orientate itself. HorizontalInput: The player orientates based on the A & D inputs. " +
            "Aim Based, the player looks at the crosshair. Mixed: Mix between Aim Based and HorizontalInput. ")]
        [SerializeField] private PlayerOrientationMethod playerOrientationMethod;

        [SerializeField, Tooltip("")] private TurnMethod turnMethod;

        // Smooth Turn
        [SerializeField] private float turnDuration = 0.2f;

        [SerializeField, Range(-180,180)] private float rightRotation = -90;

        [SerializeField, Range(-180, 180)] private float leftRotation = 90;

        [Tooltip("Damage applied on an enemy when landing on it.")] [SerializeField] private float landOnEnemyDamage;
        [Tooltip("Upwards impulse applied to the player when landing on an enemy.")] [SerializeField] private float landOnEnemyImpulse;

        [Header("SOUNDS"), SerializeField] private Sounds sounds;

        [Tooltip("Stores the SurfaceEffects for each surface.")] [SerializeField, Header("FOOTSTEPS")] private List<SurfaceEffect> step = new List<SurfaceEffect>();

        [Tooltip("Speed at which the footsteps are played."), SerializeField] private float footstepsInterval;

        [Tooltip("Volume at which the footsteps are played.")] [SerializeField] private float footstepsVolume;

        [Tooltip("Volume at which the land SFX is played.")] [SerializeField] private float landVolume;

        [Tooltip("Camera Shake to apply to the main camera on landing"), SerializeField, Range(0, 2)] private float landCameraShake = .35f;

        [Tooltip("Coyote Jump allows the player to jump responsively, even when leaving a ground or surface, there is still a timing that allows the player to jump." +
            "Adjust this number to configure the way that works.")]
        [Range(0.01f, 0.5f), SerializeField] private float coyoteTime;
        [Tooltip("Allows for a more responsive jump."), Range(0.01f, 0.5f), SerializeField] private float jumpInputBufferTime;

        public Events events;
        
        public delegate void OrientatePlayer();
        public OrientatePlayer orientatePlayer;

        public delegate void OnTurn();
        public OnTurn onTurn;

        private Dictionary<int, int> sortedGroundLayer = new Dictionary<int, int>();
        private Vector2 dashDirection;

        #region ACCESSORS

        public Transform Graphics => graphics;

        private bool isTurning = false;
        public TurnMethod _TurnMethod => turnMethod;
        private Quaternion RightRotationQuaternion => Quaternion.Euler(0f, rightRotation, 0f);
        private Quaternion LeftRotationQuaternion => Quaternion.Euler(0f, leftRotation, 0f);
        public bool facingRight { get; private set; } = true;

        public float currentSpeed { get; private set; }
        public float WalkSpeed => walkSpeed;
        public float RunSpeed => runSpeed;
        public bool UsesStamina => usesStamina;
        public float MaxStamina => maxStamina;
        public float stamina { get; private set; }
        public bool AllowCrouch => allowCrouch;
        public bool CanCrouchSlideMidAir => canCrouchSlideMidAir;
        public float CrouchSlideDuration => crouchSlideDuration;

        public bool IsGrounded { get { return LastOnGroundTime > 0; } }

        public bool IsFalling { get { return !IsGrounded && rb.velocity.y <= 0; } }

        // This variable allows for external scripts such as GravityZone.cs to modify the gravity scale of the player.
        [HideInInspector] public bool externalGravityScale = false;

        public float groundAngle { get; private set; }
        public int currentJumps { get; private set; }
        public bool isJumping;
        public bool isWallJumping { get; private set; }
        public bool isWallSliding { get; private set; }

        public float wallSlideVFXTimer;

        public bool staminaAllowsJump { get; private set; }

        public bool isCrouching { get; private set; } = false;

        public float LastOnGroundTime { get; private set; }
        public bool LastOnWallTime;
        public bool WallRight;
        public bool WallLeft;

        public bool AllowSlide => allowSlide;
        public bool WallSlidingResetsJumps => wallSlidingResetsJumps;
        public float WallSlideVFXInterval => wallSlideVFXInterval;
        public bool AllowWallJump => allowWallJump;
        public float WallJumpTime => wallJumpTime;

        public bool isGliding { get; private set; } = false;
        public bool CanGlide => canGlide;
        public GlideDurationMethod GlideDurationMethod => glideDurationMethod;
        public float MaximumGlideTime => maximumGlideTime;
        public bool HandleOrientationWhileGliding => handleOrientationWhileGliding;

        public DashMethod DashMethod => dashMethod;
        public int AmountOfDashes => amountOfDashes;
        public bool isDashing { get; private set; } = false;
        public bool canDash { get; private set; } = true;
        public int currentDashes { get; private set; }
        public bool InvincibleWhileDashing => invincibleWhileDashing;

        public bool ladderAvailable { get; private set; } = false;

        private bool jumpCut;
        private bool isJumpFalling; // Tracks Falling after jumping only
        private bool isFalling; // Tracks Falling generally.

        public float wallJumpStartTime { get; private set; }
        private int lastWallJumpDir;

        public int currentLayer { get; private set; }

        private float footstepsTimer;

        private CapsuleCollider2D playerCol;

        private Vector2 colliderSize;
        ContactFilter2D filteredWhatIsGround = new ContactFilter2D();

        #endregion

        #region INPUT PARAMETERS
        public float LastPressedJumpTime { get; private set; }
        public float LastPressedDashTime { get; private set; }

        #endregion

        #region CHECK PARAMETERS
        [Header("Checks")]
        [SerializeField] private Vector3 groundCheckOffset;
        [SerializeField] private Vector2 groundCheckSize = new Vector2(0.49f, 0.03f);
        [Space(10)]
        [SerializeField] private Vector3 ceilingCheckOffset;
        [SerializeField] private Vector2 ceilingCheckSize = new Vector2(0.49f, 0.03f);
        [Space(10)]
        [SerializeField] private Vector3 leftSideCheckOffset;
        [SerializeField] private Vector3 rightSideCheckOffset;
        [SerializeField] private Vector2 _wallCheckSize = new Vector2(0.5f, 1f);

        [SerializeField] private float groundRaycastOffset = 0.5f;
        [SerializeField] private float groundRayLength = 0.1f;

        public Vector3 GroundCheckOffset => groundCheckOffset;
        public Vector2 GroundCheckSize => groundCheckSize;
        #endregion

        #region LAYERS & TAGS
        [Header("Layers & Tags")]
        [SerializeField] private LayerMask whatIsGround;
        public LayerMask WhatIsGround => whatIsGround; 
        #endregion

        #region COMPONENTS
        public Rigidbody2D rb { get; private set; }
        private PlayerControl playerControl;
        private PlayerStats playerStats; 
        private WeaponController weaponController;
        private PlayerMultipliers playerMultipliers;
        private CameraShake cameraShake;
        #endregion

        private void Awake()
        {
            // Initial references and settings
            rb = GetComponent<Rigidbody2D>();
            playerControl = GetComponent<PlayerControl>();
            weaponController = GetComponent<WeaponController>();
            playerCol = GetComponent<CapsuleCollider2D>();
            playerStats = GetComponent<PlayerStats>();
            playerMultipliers = GetComponent<PlayerMultipliers>();
            cameraShake = GetComponent<CameraShake>();
            colliderSize = playerCol.size;
            footstepsTimer = footstepsInterval;

            filteredWhatIsGround = new ContactFilter2D();
            filteredWhatIsGround.SetLayerMask(whatIsGround);
            filteredWhatIsGround.useTriggers = false;
        }

        private void Start()
        {
            // Set the events to use them later
            GatherEvents();
            // Detects all the available ground layers to display appropriate VFX and play the right SFX based on the current surface
            SortGroundLayer();

            ResetStamina();

            // Set the dashes to the max amount
            currentDashes = amountOfDashes;

            LastOnGroundTime = coyoteTime;
        }
        private void Update()
        {
            UpdateTimers();
            ApplyGravity();
            CheckFalling();
        }

        private void FixedUpdate()
        {
            // Stop from sliding without wanting to.
            PreventUnvoluntarySliding();
            // Handles Stamina
            Stamina();
        }

        private void UpdateTimers()
        {
            float deltaTime = Time.deltaTime;
            LastOnGroundTime -= deltaTime;
            LastPressedJumpTime -= deltaTime;
            LastPressedDashTime -= deltaTime;
        }

        public void HorizontalOrientation()
        {
            // Orientate the player horizontally
            if (InputManager.PlayerInputs.HorizontalMovement != 0)
                SetOrientation(InputManager.PlayerInputs.HorizontalMovement > 0);
        }

        // Orientate the player horizontally based on where the player is aiming
        public void AimBasedOrientation()
        {
            Vector3 crosshairPos = Crosshair.Instance.transform.position;
            Vector2 dir = DeviceDetection.Instance.mode == DeviceDetection.InputMode.Controller ? -Gamepad.current.rightStick.ReadValue().normalized : crosshairPos - transform.position;

            if (dir.x != 0) SetOrientation(dir.x > 0);
        }

        // Auxiliar variables
        Vector3 oldPos = Vector3.zero;

        // It mixes Horizontal and Aim Based Orientations.
        private void MixedOrientation()
        {
            Vector3 crosshairPos = Crosshair.Instance.transform.position;
            if (InputManager.PlayerInputs.HorizontalMovement == 0 && oldPos != crosshairPos)
            {
                AimBasedOrientation();
                return;
            }
            HorizontalOrientation();
            oldPos = crosshairPos;
        }

        public void ReduceJumpAmount() => currentJumps--;

        public void ResetJumpAmounts() => currentJumps = amountOfJumps;

        public void CheckCollisions()
        {
            // Get a reference for the actual ground check position
            Vector2 groundCheckBoxPosition = transform.position + groundCheckOffset;

            // Check if there is a collision running.
            Collider2D[] results = new Collider2D[1];
            int hitCount = Physics2D.OverlapBox(groundCheckBoxPosition, groundCheckSize, 0, filteredWhatIsGround, results);
            Collider2D groundHit = hitCount > 0 ? results[0] : null;

            if (groundHit != null && !groundHit.CompareTag("Ladder"))
            {
                // Determine the ground angle to orientate the player properly.
                groundAngle = Vector2.Angle(groundHit.transform.up, Vector3.up);

                if (groundAngle < maxFloorAngle)
                {
                    transform.up = groundHit.transform.up;
                }

                // If the player is grounded but it was not grounded the previous frame, Land.
                if (!IsGrounded)
                {
                    Land();
                }
                
                // Handle coyote jump and reset jumps when grounded.
                LastOnGroundTime = coyoteTime;
                currentJumps = amountOfJumps;

                // Determines if colliding with an enemy
                Collider2D hit;
                if ((hit = Physics2D.OverlapBox(groundCheckBoxPosition, groundCheckSize, 0, whatIsGround)) != null)
                {
                    if (hit.TryGetComponent<IDamageable>(out var damageable))
                    {
                        // Apply damage to the enemy
                        damageable.Damage(landOnEnemyDamage);
                        // Reset the velocity to avoid weird movement behaviour
                        rb.velocity = Vector2.zero;
                        // Apply a force vertically
                        rb.AddForce(transform.up * landOnEnemyImpulse, ForceMode2D.Impulse);
                    }
                }
            }
            else
            {
                transform.up = Vector3.up;
            }

            WallLeft = DetectWallOnSide(false);
            WallRight = DetectWallOnSide(true);

            LastOnWallTime = WallRight || WallLeft;
        }
        private bool DetectWallOnSide(bool right)
        {
            Vector2 origin = transform.position;
            Vector2 dir = right ? transform.right : Vector2.left;

            Vector2 topOrigin = origin + new Vector2(0, groundRaycastOffset);
            Vector2 bottomOrigin = origin - new Vector2(0, groundRaycastOffset);

            RaycastHit2D[] topHits = new RaycastHit2D[1];
            RaycastHit2D[] bottomHits = new RaycastHit2D[1];

            int topHitCount = Physics2D.Raycast(topOrigin, dir, filteredWhatIsGround, topHits, groundRayLength);
            int bottomHitCount = Physics2D.Raycast(bottomOrigin, dir, filteredWhatIsGround, bottomHits, groundRayLength);

            Debug.DrawRay(topOrigin, dir * groundRayLength, Color.red);
            Debug.DrawRay(bottomOrigin, dir * groundRayLength, Color.red);

            return topHitCount > 0 && bottomHitCount > 0;
        }

        private void PreventUnvoluntarySliding()
        {
            // Removes gravity when grounded and idle on a slope to prevent sliding down.
            if (groundAngle < maxFloorAngle && IsGrounded && InputManager.PlayerInputs.HorizontalMovement == 0 && !isJumping && !isDashing && !ladderAvailable)
            {
                rb.gravityScale = 0;
                rb.velocity = Vector2.zero;
            }
        }
        private void SortGroundLayer()
        {
            int sortedIndex = 0;
            for (int i = 0; i < 32; i++)
            {
                if (whatIsGround == (whatIsGround | (1 << i)))
                {
                    sortedGroundLayer[i] = sortedIndex;
                    sortedIndex++;
                }
            }
        }

        public bool CheckCeiling()
        {
            Collider2D hit = Physics2D.OverlapBox(transform.position + ceilingCheckOffset, ceilingCheckSize, 0, whatIsGround);

            // Check if there is a ceiling above before uncrouching
            return hit != null && !hit.CompareTag("Ladder");
        }

        public bool CheckIfPerformJump()
        {
            // If all the conditions are met, the player is able to jump.
            if (CanJump() && LastPressedJumpTime > 0 && currentJumps > 0)
            {
                rb.velocity = Vector2.zero;
                isJumping = true;
                isWallJumping = false;
                jumpCut = jumpMethod == JumpMethod.Default ? true : false;
                isJumpFalling = false;
                return true;
            }
            return false;
        }

        public bool CheckIfPerformWallJump()
        {
            // If all the conditions are met, the player is able to wall jump.
            if (CanWallJump() && LastPressedJumpTime > 0 && allowWallJump)
            {
                isWallJumping = true;
                isJumping = false;
                isWallSliding = false;
                jumpCut = false;
                isJumpFalling = false;
                InputManager.PlayerInputs.HorizontalMovement = 0;
                onTurn?.Invoke();
                wallJumpStartTime = Time.time;
                lastWallJumpDir = (WallRight) ? -1 : 1;
                WallJump(lastWallJumpDir);
                return true;
            }
            return false;
        }

        public void StopWallJump() => isWallJumping = false;
        public void CheckIfJumpingOrWallSliding()
        {
            if (IsGrounded && !isJumping && !isWallJumping)
            {
                jumpCut = false;
                isJumpFalling = false;
            }
        }


        public bool CheckSlideStatus()
        {
            // Determine wether the player is sliding or not
            if (CanSlide())
            {
                if(!isWallSliding) events.onStartWallSliding?.Invoke();

                isWallSliding = true;

                events.onWallSliding?.Invoke();
                return true;
            }
            else
            {
                isWallSliding = false;
                return false;
            }
        }

        private void ApplyGravity()
        {
            // Return if we want other scripts to manage the gravity scale of our player.
            if (externalGravityScale || ladderAvailable) return;

            if (!isDashing && !isGliding)
            {
                if (isWallSliding && !isJumping)
                {
                    SetGravityScale(0);
                }
                else if (jumpCut)
                {
                    if (InputManager.PlayerInputs.Crouch && !isCrouching) SetGravityScale(gravityScale * fastFallMultiplier);
                    else SetGravityScale(gravityScale * apexReachSharpness);
                    rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -maxFallSpeed));
                }
                else if ((isJumping || isWallJumping || isJumpFalling) && Mathf.Abs(rb.velocity.y) < jumpHangTimeThreshold)
                {
                    if (InputManager.PlayerInputs.Crouch && !isCrouching) SetGravityScale(gravityScale * fastFallMultiplier);
                    else SetGravityScale(gravityScale * jumpHangGravityMult);
                }
                else if (rb.velocity.y < 0)
                {
                    if (InputManager.PlayerInputs.Crouch && !isCrouching) SetGravityScale(gravityScale * fastFallMultiplier);
                    else SetGravityScale(gravityScale * fallGravityMult);
                    rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -maxFallSpeed));
                }
                else
                {
                    SetGravityScale(gravityScale);
                }
            }
            else
            {
                SetGravityScale(0);
            }
        }

        private void CheckFalling()
        {
            if (rb.velocity.y < 0 && !IsGrounded)
            {
                if(isFalling) return;

                isFalling = true;
                events.onStartFall?.Invoke();
            }
            else isFalling = false;
        }
        #region INPUT CALLBACKS

        public void OnJumpInput()
        {
            LastPressedJumpTime = jumpInputBufferTime;
        }

        public void OnJumpUpInput()
        {
            if (CanJumpCut() || CanWallJumpCut())
                jumpCut = true;
        }
        #endregion

        #region GENERAL METHODS
        public void SetGravityScale(float scale)
        {
            rb.gravityScale = scale;
        }
        #endregion

        //MOVEMENT METHODS
        #region RUN METHODS

        public void HandleVelocities()
        {
            currentSpeed = autoRun ? runSpeed : (InputManager.PlayerInputs.Run && (canRun || !usesStamina) ? runSpeed : walkSpeed);
            currentSpeed *= playerMultipliers.speedModifier;

            currentSpeed *= weaponController == null || weaponController.weapon == null ? 1 : 1 / weaponController.weapon.weight;

            // Handle Events
            if (isWallSliding || isGliding || IsFalling || isJumping || isCrouching || ladderAvailable || !IsGrounded) return;

            if (rb.velocity.magnitude < .1f && !isFalling && !isJumping)
            {
                if(!playerStats.IsDead) events.onIdle?.Invoke();
                return;
            }
            // Walk Anim
            if (currentSpeed <= walkSpeed && rb.velocity.magnitude > 0.5f)
                events.onWalking?.Invoke();
            else if (currentSpeed > walkSpeed)
                events.onRunning?.Invoke();
        }

        public void LadderVelocity()
        {
            currentSpeed = horizontalLadderSpeed * playerMultipliers.speedModifier;
        }
        private float GetSpeedModifier()
        {
            if (currentLayer >= 0 && currentLayer < step.Count)
            {
                return step[currentLayer].speedModifier;
            }
            return 1f; // Default fallback modifier
        }

        public void Movement()
        {
            if (isWallJumping && allowWallJump && cancelInputsOnWallJump) return;

            // Determine the actual speed at which the player should move
            float targetSpeed = InputManager.PlayerInputs.HorizontalMovement * currentSpeed * GetSpeedModifier();

            // Calculate capacity of gaining speed
            float accelRate = (IsGrounded) ? (Mathf.Abs(targetSpeed) > 0.01f ? runAcceleration : runDecceleration) : (Mathf.Abs(targetSpeed) > 0.01f ? runAcceleration * airAcceleration : runDecceleration * airDeceleration);

            // Acceleration and speed change while airborne for more satisfying movement styles
            if ((isJumping || isWallJumping || isJumpFalling) && Mathf.Abs(rb.velocity.y) < jumpHangTimeThreshold && !isCrouching && !isGliding)
            {
                accelRate *= jumpHangAccelerationMult;
                targetSpeed *= jumpHangMaxSpeedMult;
            }

            float speedDif = targetSpeed - rb.velocity.x;
            float movement = speedDif * accelRate;

            // Apply the forces
            // Prevent movement force if there's a wall in the intended direction
            if ((movement < 0 && !WallLeft) || (movement > 0 && !WallRight))
            {
                rb.AddForce(movement * Vector2.right, ForceMode2D.Force);
            }
        }

        // Used for ladders, mainly
        // It allows moving vertically using W & S ( for the default provided inputs by 2D Platformer Engine )
        public void VerticalMovement()
        {
            rb.velocity = InputManager.PlayerInputs.VerticalMovement * verticalLadderSpeed * Vector2.up * playerMultipliers.speedModifier;
        }

        // Orientate the player wether right, or left side.
        private void Turn()
        {
            graphics.localScale = new Vector3(-graphics.localScale.x, graphics.localScale.y, graphics.localScale.z);
            facingRight = !facingRight;

            events.onTurn?.Invoke();
        }

        public void SmoothTurn()
        {
            if (!isTurning)
            {
                StartCoroutine(RotateCharacter());
            }
        }

        private IEnumerator RotateCharacter()
        {
            isTurning = true;

            Quaternion startRot = graphics.rotation;
            Quaternion endRot = facingRight ? LeftRotationQuaternion : RightRotationQuaternion;

            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / turnDuration;
                graphics.rotation = Quaternion.Slerp(startRot, endRot, t);
                yield return null;
            }

            graphics.rotation = endRot;
            facingRight = !facingRight;
            isTurning = false;

            events.onTurn?.Invoke();
        }

        public void StartCrouch()
        {
            // Set everything necessary at start crouch.
            // this is only called the movement the player starts crouching
            isCrouching = true;

            if (currentSpeed > walkSpeed) stamina -= staminaLossOnCrouchSlide;

            currentSpeed = crouchSpeed;

            events.onStartCrouch?.Invoke();
            SoundManager.Instance.PlaySound(sounds.startCrouchSFX, 1);

            playerCol.size = colliderSize * crouchScaleMultiplier;
            playerCol.offset = new Vector2(playerCol.offset.x, playerCol.offset.y + crouchCollisionOffset);
        }
        public void StopCrouch()
        {
            // Set everything necessary at stoop crouch.
            // this is only called the movement the player stops crouching
            isCrouching = false;
            transform.localScale = Vector3.one;

            events.onStopCrouch?.Invoke();

            playerCol.size = colliderSize;
            playerCol.offset = Vector3.zero;
        }
        public void CrouchSlide()
        {
            // Apply a force in the direction of movement that simulates a slide.
            rb.AddForce(transform.right * InputManager.PlayerInputs.HorizontalMovement * crouchSlideSpeed * 1000 * Time.deltaTime);
        }
        #endregion

        #region JUMP METHODS
        public void Jump()
        {
            // Reset timers
            LastPressedJumpTime = 0;
            LastOnGroundTime = 0;

            // Calculate and apply forces
            float force = jumpForce * playerMultipliers.jumpHeightModifier - Mathf.Max(rb.velocity.y, 0);
            rb.AddForce(Vector2.up * force, ForceMode2D.Impulse);

            // Remove Stamina
            stamina -= staminaLossOnJump;

            // Play SFX
            SoundManager.Instance.PlaySound(sounds.jumpSFX, 1);

            events.onJump?.Invoke();
        }
        public void JumpFall()
        {
            // Reset variables
            if(isJumping) events.onStartFall?.Invoke();
            isJumping = false;
            isJumpFalling = true;
        }

        private void WallJump(int dir)
        {
            // Reset timers
            LastPressedJumpTime = 0;
            LastOnGroundTime = 0;
            WallRight = false;
            WallLeft = false;

            // Calculate force to apply on wall jumping
            Vector2 force = new Vector2(wallJumpForce.x * dir, wallJumpForce.y);

            if (Mathf.Sign(rb.velocity.x) != Mathf.Sign(force.x))
                force.x -= rb.velocity.x;

            force.y -= Mathf.Max(rb.velocity.y, 0);

            // Apply force
            rb.velocity = new Vector2(0, 0);
            rb.AddForce(force, ForceMode2D.Impulse);

            // Reduce stamina
            stamina -= staminaLossOnWallJump;

            // Play sounds and events
            SoundManager.Instance.PlaySound(sounds.wallJumpSFX, 1);

            events.onWallJump?.Invoke();
            events.onJump?.Invoke();
        }
        #endregion
        #region OTHER MOVEMENT METHODS

        private Vector3 wallSlideSide;

        public Vector3 WallSlideSlide => wallSlideSide;

        public void Slide()
        {
            // Remove the remaining upwards velocity to prevent upwards sliding
            if (rb.velocity.y > 0)
            {
                rb.AddForce(-rb.velocity.y * Vector2.up, ForceMode2D.Impulse);
            }

            rb.velocity += new Vector2(0, -wallSlideSpeed);

            // Handles VFX
            wallSlideVFXTimer -= Time.deltaTime;

            if (wallSlideVFXTimer > 0) return;

            wallSlideVFXTimer = wallSlideVFXInterval;

            wallSlideSide = facingRight ? leftSideCheckOffset : rightSideCheckOffset;
            PoolManager.Instance.GetFromPool(wallSlideVFX, transform.position + wallSlideSide, Quaternion.identity);
        }

        public void StartGlide()
        {
            // Only called once when the player starts to glide
            isGliding = true;
            // Sets the speed
            currentSpeed = glideSpeed;
            // Stops the player for one frame to avoid weird movement afterwards
            rb.velocity = Vector2.zero;

            events.onStartGlide?.Invoke();
            SoundManager.Instance.PlaySound(sounds.startGlideSFX, 1);
        }

        public void StopGlide()
        {
            // Only called once when the player stops gliding
            isGliding = false;

            events.onStopGliding?.Invoke();
        }

        public void GlideVerticalMovement()
        {
            // this acts as lower gravity when gliding
            rb.velocity += new Vector2(0, -glideGravity) * Time.deltaTime;
        }

        public void HandleFootsteps()
        {
            // If the player is idle, no footstep should be played, but the timer must be reset
            if (InputManager.PlayerInputs.HorizontalMovement == 0 || rb.velocity.magnitude < .1f)
            {
                footstepsTimer = footstepsInterval;
                return;
            }

            // If the player is moving calculate an interval within footsteps
            float multiplierRate = currentSpeed < runSpeed ? 1 : 1.5f;
            footstepsTimer -= Time.deltaTime * multiplierRate;

            // Are we able to play a footstep?
            if (!IsGrounded || footstepsTimer > 0) return;

            // Reset timer
            footstepsTimer = footstepsInterval;

            // Play sounds and Effects
            if (IsCurrentLayerValid())
            {
                if(step[currentLayer].stepSFX) SoundManager.Instance.PlaySound(step[currentLayer].stepSFX, footstepsVolume);
                if(step[currentLayer].stepVFX) PoolManager.Instance.GetFromPool(step[currentLayer].stepVFX, transform.position + groundCheckOffset, Quaternion.identity);
            }  
        }

        private void Land()
        {
            // Play sounds and spawn VFX when  the player lands on a surface
            if(IsCurrentLayerValid())
            {
                if (step[currentLayer].stepSFX) SoundManager.Instance.PlaySound(step[currentLayer].landSFX, landVolume);

                if (step[currentLayer].stepSFX) PoolManager.Instance.GetFromPool(step[currentLayer].landVFX, transform.position + groundCheckOffset, Quaternion.identity);
            }

            events.onLand?.Invoke();

            cameraShake.Shake(landCameraShake, 5, 1, 1);
        }
        public bool CanDash()
        {
            // Returns true if the player can Dash
            return canDash && dashMethod != DashMethod.None && currentDashes > 0 && (!isGliding || isGliding && canDashWhileGliding);
        }

        public void InitializeDash()
        {
            // Only called once when the player starts to dash
            // make sure this only gets called once
            CancelInvoke(nameof(ResetDash));
            canDash = false;
            isDashing = true;
            dashDirection = GetDashDirection();
            Invoke(nameof(ResetDash), dashDuration);
            UIController.Instance.RemoveDash();
            currentDashes--;

            events.onStartDash?.Invoke();
            SoundManager.Instance.PlaySound(sounds.dashSFX, 1);
        }

        public void PerformDash()
        {
            // Called everyframe the dash is being performed
            rb.velocity = new Vector2(dashDirection.x, dashDirection.y).normalized * dashSpeed;
        }

        private Vector2 GetDashDirection()
        {
            Vector3 crosshairPosition = Crosshair.Instance.transform.position;

            // Determine the dash direction based on the user variables.
            switch(dashMethod)
            {
                case DashMethod.None:
                    return Vector2.zero;

                case DashMethod.Default:
                    return InputManager.PlayerInputs.HorizontalMovement == 0 ? 
                        transform.right 
                        : new Vector2(InputManager.PlayerInputs.HorizontalMovement, 0).normalized;

                case DashMethod.AimBased:
                    return (crosshairPosition - transform.position).normalized;

                case DashMethod.OrientationBased:
                    return facingRight ? new Vector2(1, 0) : new Vector2(-1,0);

                case DashMethod.HorizontalAimBased:
                    Vector3 dir = (crosshairPosition - transform.position);

                    if (dir.x >= 0) return new Vector2(1, 0);
                    else return new Vector2(-1, 0);

                default: return Vector2.zero;
            }            
        }

        private void ResetDash()
        {
            // Enable dash and cooldown for the dash
            isDashing = false;
            rb.gravityScale = gravityScale;
            rb.velocity = Vector3.zero;
            Invoke(nameof(EnableDash), dashInterval);
            Invoke(nameof(CoolDash), dashCooldown);
        }

        private void EnableDash() => canDash = true;

        private void CoolDash()
        {
            currentDashes++;
            UIController.Instance.GainDash();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Check if the player is inside the range of a ladder
            if (!other.CompareTag("Ladder")) return;
            
            ladderAvailable = true;
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            // Check if the player is outside the range of a ladder
            if (!other.CompareTag("Ladder")) return;

            ladderAvailable = false;
        }
        #endregion


        #region UTILITIES
        private bool IsCurrentLayerValid()
        {
            return currentLayer >= 0 && currentLayer < step.Count;
        }

        // Handle turning
        public void SetOrientation(bool isMovingRight)
        {
            if (isWallJumping) return;

            if (isMovingRight != facingRight)
                onTurn?.Invoke();
        }

        private bool CanJump()
        {
            return (IsGrounded || amountOfJumps > 0) && (!usesStamina || usesStamina && staminaAllowsJump);
        }

        private bool CanWallJump()
        {
            bool wallIsTallEnough = false;

            if (WallLeft) wallIsTallEnough = Physics2D.Raycast(transform.position + ceilingCheckOffset, Vector2.left, whatIsGround);
            else if (WallRight) wallIsTallEnough = Physics2D.Raycast(transform.position + ceilingCheckOffset, Vector2.right, whatIsGround);

            bool jumpInputBuffered = LastPressedJumpTime > 0;
            bool touchingWallRecently = LastOnWallTime;
            bool notRepeatedSameWallJump = !isWallJumping || (WallRight && lastWallJumpDir == 1) || (WallLeft && lastWallJumpDir == -1);
            bool canUseStamina = !usesStamina || (usesStamina && staminaAllowsJump);

            return wallIsTallEnough && jumpInputBuffered && touchingWallRecently && !IsGrounded && notRepeatedSameWallJump && canUseStamina;
        }

        private bool CanJumpCut()
        {
            return isJumping && rb.velocity.y > 0;
        }

        private bool CanWallJumpCut()
        {
            return isWallJumping && rb.velocity.y > 0;
        }

        public bool CanSlide()
        {
            bool isWallContact = WallLeft || WallRight;
            bool isMovingTowardWall = (WallLeft && InputManager.PlayerInputs.HorizontalMovement < 0) ||
                (WallRight && InputManager.PlayerInputs.HorizontalMovement > 0);

            bool wallSlideCondition = requireHorizontalInputToSlide ? isMovingTowardWall : true;

            return !ladderAvailable && allowSlide && LastOnWallTime && !isJumping && !isWallJumping && !IsGrounded && wallSlideCondition && isWallContact; 
        }

        #endregion
        #region Stamina
        private void Stamina()
        {
            // Check if we def wanna use stamina
            if (!usesStamina || !playerControl.Controllable) return;

            float oldStamina = stamina; // Store stamina before we change its value

            // We ran out of stamina
            if (stamina <= 0)
            {
                canRun = false;
                staminaAllowsJump = false;
                stamina = 0;
            }

            // Wait for stamina to regenerate up to the min value allowed to start running and jumping again
            if (stamina >= minStaminaRequiredToRun)
            {
                canRun = true; staminaAllowsJump = true;
            }

            // Regen stamina
            if (stamina < maxStamina)
            {
                if (currentSpeed <= walkSpeed)
                    stamina += Time.deltaTime * staminaRegenMultiplier;
            }

            // Lose stamina
            if (currentSpeed == runSpeed && canRun && !isWallSliding && InputManager.PlayerInputs.HorizontalMovement != 0) stamina -= Time.deltaTime;


            if (oldStamina != stamina)
                UIController.Instance.EnableStaminaSlider(true);
            else
                UIController.Instance.EnableStaminaSlider(false);
        }

        void ResetStamina() => stamina = maxStamina;

        #endregion
        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Step"))
            {
                ContactPoint2D contact = collision.contacts[0];

                // Check if collision is happening from the sides
                if (contact.normal.y < 0.5f)
                {
                    // Move the player up by the step height
                    transform.position += Vector3.up * stepHeight;
                }
            }
        }

        private void GatherEvents()
        {
            switch (playerOrientationMethod)
            {
                case PlayerOrientationMethod.None: orientatePlayer = null; break;
                case PlayerOrientationMethod.HorizontalInput: orientatePlayer = HorizontalOrientation; break;
                case PlayerOrientationMethod.AimBased: orientatePlayer = AimBasedOrientation; break;
                case PlayerOrientationMethod.Mixed: orientatePlayer = MixedOrientation; break;
            }

            switch(turnMethod)
            {
                case TurnMethod.Simple: onTurn = Turn; break;
                case TurnMethod.Smooth: onTurn = SmoothTurn; break; 
            }
        }
        #region EDITOR METHODS
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position + groundCheckOffset, groundCheckSize);
            Gizmos.DrawWireCube(transform.position + ceilingCheckOffset, ceilingCheckSize);
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(transform.position + leftSideCheckOffset, _wallCheckSize);
            Gizmos.DrawWireCube(transform.position + rightSideCheckOffset, _wallCheckSize);
        }
        #endregion
    }
}