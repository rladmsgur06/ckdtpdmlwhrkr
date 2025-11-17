#region 
#if UNITY_EDITOR
using TwoBitMachines.Editors;
using UnityEditor;
#endif
#endregion
using UnityEngine;

namespace TwoBitMachines.FlareEngine.ThePlayer
{
        //* To prevent interference, abilities that contain a jump force must not include Jump ability as an exception.
        //* Those abilities will also send a signal to Jump ability incase air jump needs to be checked.
        [AddComponentMenu("")]
        public class Jump : Ability
        {
                [SerializeField] public int airJumps;
                [SerializeField] public float airMomentum = 0;
                [SerializeField] public float jumpBuffer = 0.25f;
                [SerializeField] public float coyoteTime = 0.05f;
                [SerializeField] public float airJumpScale = 1f;
                [SerializeField] public float glideBoost = 0.15f;
                [SerializeField] public float jumpHeight;
                [SerializeField] public float minJumpHeight;
                [SerializeField] public float glideScale;
                [SerializeField] public bool jumpFromFall;
                [SerializeField] public bool glideImmediately;
                [SerializeField] public string glideButton;
                [SerializeField] public string airJumpWE;
                [SerializeField] public string jumpWE;
                [SerializeField] public UnityEventEffect onJump;
                [SerializeField] public UnityEventEffect onAirJump;
                [SerializeField] public ButtonTrigger buttonTrigger;

                [System.NonSerialized] private int airJumpCount;
                [System.NonSerialized] private float bufferTimer;
                [System.NonSerialized] private float coyoteTimer;
                [System.NonSerialized] private float maxJumpVel;
                [System.NonSerialized] public float minJumpVel;

                [System.NonSerialized] private bool airGliding;
                [System.NonSerialized] private bool groundFall;
                [System.NonSerialized] private bool checkForFloat;
                [System.NonSerialized] private bool checkForMinJump;
                [System.NonSerialized] private bool checkForAirJump;
                [System.NonSerialized] private bool checkForAirMomentum;
                [System.NonSerialized] private bool jumpRefreshed;
                [System.NonSerialized] private bool checkRepress;
                [System.NonSerialized] private float jumpInputXRef;
                [System.NonSerialized] private Gravity gravity;
                [System.NonSerialized] private Player player;

                private bool buffer => jumpBuffer > 0 && bufferTimer >= Time.time;
                private bool coyote => coyoteTime > 0 && coyoteTimer >= Time.time;

                public override void Initialize (Player player)
                {
                        this.player = player;
                        gravity = player.abilities.gravity;
                        maxJumpVel = gravity.jumpVelocity;
                        minJumpVel = Mathf.Sqrt(Mathf.Abs(gravity.gravity) * minJumpHeight * 2f);
                        player.abilities.maxJumpVel = maxJumpVel;
                        player.abilities.minJumpVel = minJumpVel;
                }

                public void SetNewJumpHeight (float newJumpHeight)
                {
                        if (gravity != null & player != null)
                        {
                                gravity.jumpHeight = newJumpHeight;
                                gravity.Initialize();
                                maxJumpVel = gravity.jumpVelocity;
                                minJumpVel = Mathf.Sqrt(Mathf.Abs(gravity.gravity) * minJumpHeight * 2f);
                                player.abilities.maxJumpVel = maxJumpVel;
                                player.abilities.minJumpVel = minJumpVel;
                        }
                }

                public override void Reset (AbilityManager player)
                {
                        coyoteTimer = 0;
                        bufferTimer = 0;
                        airJumpCount = 0;
                        airGliding = false;
                        groundFall = false;
                        checkForFloat = false;
                        checkForMinJump = false;
                        checkForAirJump = false;
                        checkForAirMomentum = false;
                        player.airMomentumActive = false;
                        checkRepress = false;
                        jumpRefreshed = false;
                }

                public override bool TurnOffAbility (AbilityManager player)
                {
                        Reset(player);
                        return true;
                }

                public override void EarlyExecute (AbilityManager player, ref Vector2 velocity)
                {
                        if (player.checkForAirJumps) // Has been set externally
                        {
                                if (player.checkForMomentum && airMomentum > 0 && Mathf.Abs(velocity.x) > 0)
                                {
                                        checkForAirMomentum = true;
                                }
                                airJumpCount = player.setAirJump;
                                checkForFloat = glideScale > 0;
                                player.setAirJump = 0;
                                checkRepress = true;
                                checkForAirJump = true;
                                jumpRefreshed = false;
                                player.checkForAirJumps = false;
                                player.checkForMomentum = false;
                                jumpInputXRef = player.inputX;
                        }
                        if (player.ground || player.world.onWall)
                        {
                                player.dashAirJumpCheck = false;
                        }
                        player.airJumpCount = airJumpCount;
                }

                public override bool IsAbilityRequired (AbilityManager player, ref Vector2 velocity)
                {
                        player.jumpButtonActive = player.inputs.Active(buttonTrigger, "Jump"); //           read these values every frame
                        player.world.holdingJump = player.jumpButtonHold;
                        player.airMomentumActive = false;

                        #region Debug, Force Jump
#if UNITY_EDITOR
                        if (WorldManager.viewDebugger)
                        {
                                player.jumpButtonActive = player.jumpButtonActive || forceJump;
                        }
#endif
                        #endregion

                        if (player.world.onGround && player.holdingDown && player.jumpButtonPressed && player.world.verticalCollider != null && player.world.verticalCollider is EdgeCollider2D)
                        {
                                return false;
                        }

                        JumpBuffer(player);
                        JumpFromFall(player);
                        CoyoteTimer(player, velocity.y);

                        return !pause && (player.jumpButtonActive || checkForMinJump || checkForAirJump || checkForFloat || checkForAirMomentum);
                }

                public override void ExecuteAbility (AbilityManager player, ref Vector2 velocity, bool isRunningAsException = false)
                {
                        if ((player.ground || coyote) && (player.jumpButtonActive || buffer))
                        {
                                velocity.y = maxJumpVel * player.jumpBoost;
                                airJumpCount = 0;
                                bufferTimer = 0;
                                coyoteTimer = 0;

                                airGliding = false;
                                player.world.hasJumped = true;
                                player.hasJumped = true;
                                checkForMinJump = true;
                                checkForAirJump = true;
                                checkRepress = true;
                                jumpRefreshed = false;
                                checkForFloat = glideScale > 0;
                                jumpInputXRef = player.inputX;
                                checkForAirMomentum = airMomentum > 0 && Mathf.Abs(velocity.x) > 0;
                                onJump.Invoke(ImpactPacket.impact.Set(jumpWE, transform, player.world.boxCollider, player.world.position, null, player.world.box.down, player.playerDirection, 0));
                                return;
                        }
                        if ((checkForAirJump || checkForFloat || checkForMinJump || checkForAirMomentum) && player.ground)
                        {
                                checkForAirJump = checkForAirMomentum = checkForFloat = checkForMinJump = false;
                        }
                        if (checkForMinJump)
                        {
                                if (player.jumpButtonReleased && minJumpHeight > 0 && velocity.y > minJumpVel)
                                {
                                        velocity.y = minJumpVel;
                                }
                                if (player.jumpButtonReleased)
                                {
                                        checkForMinJump = false;
                                }
                        }
                        if (checkForAirMomentum && !player.world.onWall && (player.inputX == 0 || Compute.SameSign(player.inputX, jumpInputXRef)))
                        {
                                player.airMomentumActive = true;
                                velocity.x = player.velocityOnGround * airMomentum;
                                if (player.world.onWall)
                                {
                                        player.velocityOnGround = 0;
                                }
                        }
                        if (checkForAirJump)
                        {
                                if (player.jumpButtonPressed && airJumpCount < airJumps && airJumpCount++ < airJumps) // double check to keep airJump signal integrity
                                {
                                        player.dashAirJumpCheck = true;
                                        velocity.y = maxJumpVel * airJumpScale;
                                        onAirJump.Invoke(ImpactPacket.impact.Set(airJumpWE, this.transform, player.world.boxCollider, player.world.position, null, player.world.box.down, player.playerDirection, 0));
                                        airGliding = false;
                                }
                                if (player.ground)
                                {
                                        checkForAirJump = false;
                                }
                                else if (airJumpCount > 0)
                                {
                                        if (airJumpCount == 1)
                                                player.signals.Set("airJump");
                                        else if (airJumpCount == 2)
                                                player.signals.Set("airJump1");
                                        else if (airJumpCount == 3)
                                                player.signals.Set("airJump2");
                                        else if (airJumpCount == 4)
                                                player.signals.Set("airJump3");
                                }
                        }
                        // if (isRunningAsException)
                        // {
                        //         checkForFloat = airGliding = false;
                        // }
                        if (checkRepress && !player.jumpButtonHold)
                        {
                                jumpRefreshed = true;
                        }
                        if ((velocity.y < 0 || (airGliding && velocity.y < player.maxJumpVel * glideBoost)) && checkForFloat && (airJumpCount >= airJumps || glideImmediately) && player.inputs.Holding(glideButton) && jumpRefreshed)
                        {
                                velocity.y = !airGliding ? player.maxJumpVel * glideBoost : velocity.y - player.gravityEffect * glideScale;
                                airGliding = true;
                                player.signals.Set("airGlide");
                        }
                }

                public void GlideImmediately (bool value)
                {
                        glideImmediately = value;
                }

                private void JumpBuffer (AbilityManager player)
                {
                        if (jumpBuffer > 0 && player.jumpButtonActive && !player.ground)
                        {
                                bufferTimer = Time.time + jumpBuffer;
                        }
                }

                private void JumpFromFall (AbilityManager player)
                {
                        if (!jumpFromFall)
                                return;

                        if (player.ground)
                        {
                                groundFall = true;
                        }
                        if (!player.ground && groundFall && !checkForMinJump && !checkForAirJump && !checkForFloat)
                        {
                                airJumpCount = 0;
                                groundFall = false;
                                airGliding = false;
                                checkForAirJump = true;
                                checkForFloat = glideScale > 0;
                        }
                }

                private void CoyoteTimer (AbilityManager player, float velocityY)
                {
                        if (coyoteTime > 0 && velocityY <= 0 && !player.world.onGround && player.world.wasOnGround)
                        {
                                coyoteTimer = Time.time + coyoteTime;
                        }
                }

                public void SetAirJump (int value)
                {
                        airJumps = value;
                }

                public void SetAirJumpCurrentCount (int value)
                {
                        airJumpCount = value;
                }

                public void SetAirJumpScale (float value)
                {
                        airJumpScale = value;
                }

                public void SetAirGlide (float value)
                {
                        glideScale = value;
                }

                public bool IsAirGliding ()
                {
                        return airGliding;
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField, HideInInspector] private bool airJumpFoldOut;
                [SerializeField, HideInInspector] private bool eventFoldOut;
                [SerializeField, HideInInspector] private bool jumpFoldOut;
                [SerializeField, HideInInspector] private bool forceJump;

                public override bool OnInspector (SerializedObject controller, SerializedObject parent, string[] inputList, Color barColor, Color labelColor)
                {
                        if (Open(parent, "Jump", barColor, labelColor))
                        {
                                FoldOut.Box(4, FoldOut.boxColorLight, extraHeight: 5, offsetY: -2);
                                {
                                        SerializedProperty gravity = controller.Get("abilities").Get("gravity");
                                        SerializedProperty jumpHeight = parent.Get("jumpHeight");
                                        jumpHeight.floatValue = gravity.Get("jumpHeight").floatValue;
                                        parent.Field("Button Trigger", "buttonTrigger");

                                        parent.FieldDouble("Jump Height", "jumpHeight", "minJumpHeight");
                                        Labels.FieldDoubleText("Max", "Min");

                                        gravity.Field("Jump Time", "jumpTime");
                                        parent.FieldDouble("Air Jumps", "airJumps", "airJumpScale");
                                        Labels.FieldDoubleText("Jumps", "Scale", rightSpacing: 1);
                                        gravity.Get("jumpHeight").floatValue = jumpHeight.floatValue;
                                }

                                if (FoldOut.FoldOutButton(parent.Get("eventFoldOut")))
                                {
                                        Fields.EventFoldOutEffect(parent.Get("onJump"), parent.Get("jumpWE"), parent.Get("jumpFoldOut"), "On Jump", color: FoldOut.boxColorLight);
                                        Fields.EventFoldOutEffect(parent.Get("onAirJump"), parent.Get("airJumpWE"), parent.Get("airJumpFoldOut"), "On Air Jumps", color: FoldOut.boxColorLight);
                                }

                                FoldOut.Box(2, FoldOut.boxColorLight);
                                {
                                        parent.FieldDouble("Jump Buffers", "jumpBuffer", "coyoteTime");
                                        Labels.FieldDoubleText("Jump", "Coyote", rightSpacing: 1);

                                        parent.Field("Jump Momentum", "airMomentum");
                                        Labels.FieldText("Scale", rightSpacing: 1);
                                }
                                Layout.VerticalSpacing(5);

                                FoldOut.Box(4, FoldOut.boxColorLight);
                                {
                                        parent.DropDownListAndField(inputList, "Air Glide", "glideButton", "glideScale");
                                        Labels.FieldText("Scale", rightSpacing: 1);
                                        parent.Clamp("glideScale");
                                        parent.Field("Air Glide Boost", "glideBoost");
                                        parent.FieldToggle("Air Glide From Fall", "jumpFromFall");
                                        parent.FieldToggle("Air Glide Immediately", "glideImmediately");
                                }
                                Layout.VerticalSpacing(5);

                                #region Debug
#if UNITY_EDITOR
                                if (WorldManager.viewDebugger)
                                {
                                        FoldOut.Box(1, FoldOut.boxColorLight);
                                        parent.Field("Editor Only, Force jump", "forceJump");
                                        Layout.VerticalSpacing(5);
                                }
#endif
                                #endregion
                        }
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion
        }
}
