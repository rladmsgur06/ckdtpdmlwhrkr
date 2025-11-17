#region
#if UNITY_EDITOR
using TwoBitMachines.Editors;
using UnityEditor;
#endif
#endregion
using UnityEngine;

namespace TwoBitMachines.FlareEngine.ThePlayer
{
        [System.Serializable]
        public class Walk
        {
                [SerializeField] public bool walk = true;
                [SerializeField] public float speed = 10f;
                [SerializeField] public float smooth = 1f;
                [SerializeField] public float impedeChange; // prevent player from changing direction while in air

                [SerializeField] public RunType runType;
                [SerializeField] public bool run;
                [SerializeField] public bool runLimit;
                [SerializeField] public float runTimeLimit = 3f;
                [SerializeField] public float runCoolDown = 5f;
                [SerializeField] public float runRechargeRate = 0.5f;
                [SerializeField] public bool smoothIntoRun;
                [SerializeField] public bool buttonIsLeftRight;
                [SerializeField] public bool runFromStop;
                [SerializeField] public float speedBoost = 1f;
                [SerializeField] public float tapTime;
                [SerializeField] public float lerpRunTime;
                [SerializeField] public float lerpWalkTime;
                [SerializeField] public float runThreshold;
                [SerializeField] public float runJumpBoost;
                [SerializeField] public string runButton;
                [SerializeField] public string groundHitWE;
                [SerializeField] public string notOnGroundWE;
                [SerializeField] public string walkingOnGroundWE;
                [SerializeField] public string directionChangedWE;
                [SerializeField] public UnityEventEffect onGroundHit;
                [SerializeField] public UnityEventEffect onNotOnGround;
                [SerializeField] public UnityEventEffect onWalkingOnGround;
                [SerializeField] public UnityEventEffect onDirectionChanged;
                [SerializeField] public UnityEventFloat onRunTimeLimit;
                [SerializeField] public UnityEventFloat onCoolDown;

                [System.NonSerialized] public bool isRunning;
                [System.NonSerialized] private float inputX;
                [System.NonSerialized] public float runSmoothInVelocity;
                [System.NonSerialized] private float smoothIntoRunCounter;
                [System.NonSerialized] private float lerpStop;
                [System.NonSerialized] private float thresholdCounter;
                [System.NonSerialized] private float firstTapInputX;
                [System.NonSerialized] private float onGroundCounter;
                [System.NonSerialized] private bool stoppedRunning;

                [System.NonSerialized] private RunLimit runLimitState;
                [System.NonSerialized] private float runLimitCounter;
                [System.NonSerialized] private float runCoolDownCounter;
                [System.NonSerialized] private bool coolDownRun;

                [System.NonSerialized] public float externalVelX = 0;
                [System.NonSerialized] private bool toggleMode = false;
                [System.NonSerialized] private bool doubleTapMode = false;
                [System.NonSerialized] private bool switchTimeActive = false;
                [System.NonSerialized] private bool keepPlayerDirection = false;

                [System.NonSerialized] private float velocityX = 0;
                [System.NonSerialized] private float tapTimer = 0;
                [System.NonSerialized] private float tapCounter = 0;
                [System.NonSerialized] private float switchCounter = 0;
                [System.NonSerialized] private int easeInWalkDirection = 0;

                public void Reset ()
                {
                        lerpStop = 0;
                        velocityX = 0;
                        switchCounter = 0;
                        firstTapInputX = 0;
                        thresholdCounter = 0;
                        runCoolDownCounter = 0;
                        easeInWalkDirection = 0;
                        runSmoothInVelocity = 0;
                        smoothIntoRunCounter = 0;
                        runLimitCounter = runTimeLimit;

                        walk = true;
                        isRunning = false;
                        toggleMode = false;
                        coolDownRun = false;
                        doubleTapMode = false;
                        stoppedRunning = false;
                        switchTimeActive = false;
                        keepPlayerDirection = false;
                }

                public void Initialize ()
                {
                        runLimitCounter = runTimeLimit;
                }

                public void Execute (AbilityManager player, WorldCollision world, ref Vector2 velocity)
                {
                        GetUserInput(player, velocity);
                        HorizontalMove(player, ref velocity);
                        PlayerRun(player, world, ref velocity);
                        ImpedeChange(player, ref velocity);

                        player.speed = speed;
                        player.inputX = inputX;
                        velocityX = velocity.x; // need to hold a reference to velocity x at start of frame, hover will use this
                        player.velocityX = velocity.x;
                        player.jumpBoost = isRunning && runJumpBoost > 0 ? runJumpBoost : 1f;
                        int previousDirection = player.playerDirection;

                        if (!walk)
                        {
                                velocity.x = 0;
                                velocityX = 0;
                                player.speed = 0;
                                player.velocityX = 0;
                                player.inputX = inputX;
                        }
                        if (!keepPlayerDirection && velocity.x < 0)
                        {
                                player.playerDirection = -1;
                        }
                        if (!keepPlayerDirection && velocity.x > 0)
                        {
                                player.playerDirection = 1;
                        }

                        if (previousDirection != player.playerDirection)
                        {
                                onDirectionChanged.Invoke(ImpactPacket.impact.Set(directionChangedWE, world.transform, world.box.collider, player.world.position, null, -world.box.right * player.playerDirection, player.playerDirection, 0));
                        }
                        if (!player.world.wasOnGround && player.world.onGround)
                        {
                                onGroundHit.Invoke(ImpactPacket.impact.Set(groundHitWE, world.transform, world.box.collider, player.world.position, null, world.box.up, player.playerDirection, 0));
                        }
                        if (player.world.wasOnGround && !player.world.onGround)
                        {
                                onNotOnGround.Invoke(ImpactPacket.impact.Set(notOnGroundWE, world.transform, world.box.collider, player.world.position, null, world.box.down, player.playerDirection, 0));
                        }
                        if (Clock.TimerExpired(ref onGroundCounter, 0.15f) && player.world.onGround && velocity.x != 0)
                        {
                                onWalkingOnGround.Invoke(ImpactPacket.impact.Set(walkingOnGroundWE, world.transform, world.box.collider, player.world.position, null, -world.box.right * player.playerDirection, player.playerDirection, 0));
                                onGroundCounter = 0;
                        }
                }

                private void GetUserInput (AbilityManager player, Vector2 velocity)
                {
                        bool left = player.inputs.Holding("Left");
                        bool right = player.inputs.Holding("Right");

                        #region Debug
#if UNITY_EDITOR
                        if (WorldManager.viewDebugger)
                        {
                                if (useRightOnly)
                                        right = true;
                                if (useRightOnly)
                                        left = false;
                                if (useLeftOnly)
                                        left = true;
                                if (useLeftOnly)
                                        right = false;
                        }
#endif
                        #endregion

                        inputX = (left ? -1f : 0) + (right ? 1f : 0);
                }

                public void HorizontalMove (AbilityManager player, ref Vector2 velocity)
                {
                        if (smooth > 0 && smooth < 1f)
                        {
                                velocity.x = Compute.Lerp(velocityX, inputX * speed + externalVelX, smooth);
                                velocity.x = Mathf.Abs(velocity.x) < 0.2f ? 0 : velocity.x; // clamp or vel will continue to decrease without stopping
                        }
                        else
                        {
                                velocity.x = inputX * speed + externalVelX;
                        }
                        externalVelX = 0;

                        // if (player.world.mp.launching && velocity.y > 0)
                        // {
                        //         velocity.x *= 0.1f; //if being launched by moving platform, do not add to momentum
                        // }
                }

                private void PlayerRun (AbilityManager player, WorldCollision world, ref Vector2 velocity)
                {
                        if (!run)
                        {
                                return;
                        }

                        keepPlayerDirection = false;
                        bool checkForRun = true;

                        if (runLimit)
                        {
                                if (coolDownRun)
                                {
                                        isRunning = false;
                                        onCoolDown.Invoke(runCoolDown - runCoolDownCounter);
                                        if (Clock.Timer(ref runCoolDownCounter, runCoolDown))
                                        {
                                                coolDownRun = false;
                                        }
                                        else
                                        {
                                                return;
                                        }
                                }
                                if (!isRunning)
                                {
                                        runLimitCounter = Mathf.Clamp(runLimitCounter + runRechargeRate * Time.deltaTime, 0, runTimeLimit);
                                }
                                if (isRunning && Clock.TimerReverse(ref runLimitCounter, 0))
                                {
                                        RunDeactivate(world, velocity);
                                        runCoolDownCounter = 0;
                                        checkForRun = false;
                                        coolDownRun = true;
                                }
                                onRunTimeLimit.Invoke(runLimitCounter);
                        }

                        if (RunInputReleased(player, world)) //stop running
                        {
                                RunDeactivate(world, velocity);
                                checkForRun = false;
                        }
                        if (stoppedRunning && smoothIntoRun && !isRunning)
                        {
                                RunToWalkSmoothIn(player, ref velocity);
                        }

                        if (checkForRun && RunInputActive(player, world)) // dont re-enter run on same frame as exit
                        {
                                ApplyRunVelocity(player, ref velocity);
                        }
                }

                private bool RunInputReleased (AbilityManager player, WorldCollision world)
                {
                        float inputXr = runFromStop && this.inputX == 0 ? player.playerDirection : this.inputX;
                        if (!toggleMode && (inputXr == 0 || world.onWall))
                        {
                                return true;
                        }
                        if (runType == RunType.Hold && !player.inputs.Holding(runButton))
                        {
                                return true;
                        }
                        if (doubleTapMode && !player.inputs.Holding(runButton))
                        {
                                return true;
                        }
                        if (toggleMode)
                        {
                                if (player.inputs.Pressed(runButton) || world.onWall)
                                {
                                        return true;
                                }
                                if (inputX == 0 && !switchTimeActive)
                                {
                                        switchTimeActive = true;
                                        switchCounter = 0;
                                }
                                if (switchTimeActive && Clock.Timer(ref switchCounter, 0.2f) && inputX == 0)
                                {
                                        return true;
                                }
                        }
                        return false;
                }

                private void RunDeactivate (WorldCollision world, Vector2 velocity)
                {
                        if (velocity.x != 0)
                        {
                                tapCounter = tapTimer = 0;
                        }
                        if (isRunning)
                        {
                                stoppedRunning = true;
                                lerpStop = 0;
                        }
                        if (world.leftWall || world.rightWall)
                        {
                                stoppedRunning = false; // let player continue at normal speed if hit wall, or it will feel stuck for a moment during smoothIn.
                                runSmoothInVelocity = 0;
                        }

                        isRunning = false;
                        toggleMode = false;
                        doubleTapMode = false;
                        switchTimeActive = false;
                        switchCounter = 0;
                        thresholdCounter = 0;
                        smoothIntoRunCounter = 0;
                }

                private void RunToWalkSmoothIn (AbilityManager player, ref Vector2 velocity)
                {
                        if (runSmoothInVelocity != 0)
                        {
                                velocity.x = Compute.Lerp(runSmoothInVelocity, inputX * speed, lerpWalkTime, ref lerpStop);
                                if (inputX != 0)
                                {
                                        easeInWalkDirection = (int) Mathf.Sign(inputX);
                                }
                        }
                        if (lerpStop >= lerpWalkTime || runSmoothInVelocity == 0 || velocity.x == 0)
                        {
                                stoppedRunning = false;
                        }
                        else if (easeInWalkDirection != 0)
                        {
                                keepPlayerDirection = true;
                                player.playerDirection = easeInWalkDirection;
                                player.signals.ForceDirection(easeInWalkDirection);
                        }
                }

                private bool RunInputActive (AbilityManager player, WorldCollision world)
                {
                        if (runType == RunType.Hold)
                        {
                                if ((player.ground || isRunning) && player.inputs.Holding(runButton))
                                {
                                        return true;
                                }
                        }
                        else if (runType == RunType.TimeThreshold)
                        {
                                if (!isRunning && !world.onGround)
                                {
                                        thresholdCounter = 0;
                                }
                                if (inputX != 0 && Clock.TimerExpired(ref thresholdCounter, runThreshold))
                                {
                                        return true;
                                }
                        }
                        else if (runType == RunType.StickThreshold)
                        {
                                if (inputX != 0)
                                {
                                        float left = player.inputs.Value("Left");
                                        float right = player.inputs.Value("Right");
                                        if (left >= runThreshold || right >= runThreshold)
                                        {
                                                return true;
                                        }
                                }
                        }
                        else if (runType == RunType.Toggle)
                        {
                                if (toggleMode)
                                {
                                        return true;
                                }
                                if (player.ground && player.inputs.Pressed(runButton))
                                {
                                        switchCounter = 0;
                                        toggleMode = true;
                                        return true;
                                }
                        }
                        else
                        {
                                if (doubleTapMode)
                                {
                                        return true;
                                }

                                if (player.ground && player.inputs.Pressed(runButton))
                                {
                                        if (tapCounter == 0)
                                        {
                                                tapTimer = Time.time + tapTime;
                                                firstTapInputX = inputX;
                                                tapCounter = 1;
                                        }
                                        else if (Time.time <= tapTimer)
                                        {
                                                if (buttonIsLeftRight && firstTapInputX != inputX)
                                                {
                                                        tapCounter = firstTapInputX = 0;
                                                        doubleTapMode = false;
                                                }
                                                else
                                                {
                                                        tapCounter = 0;
                                                        doubleTapMode = true;
                                                        return true;
                                                }
                                        }
                                }

                                if (tapCounter == 1 && Time.time > tapTimer)
                                {
                                        tapCounter = firstTapInputX = 0;
                                        doubleTapMode = false;
                                }
                        }
                        return false;
                }

                private void ApplyRunVelocity (AbilityManager player, ref Vector2 velocity)
                {
                        float inputXr = runFromStop && this.inputX == 0 ? player.playerDirection : this.inputX;
                        if (smoothIntoRun)
                        {
                                velocity.x = runSmoothInVelocity = Compute.Lerp(velocity.x, inputXr * speed * speedBoost, lerpRunTime, ref smoothIntoRunCounter);
                        }
                        else
                        {
                                velocity.x = inputXr * speed * speedBoost;
                        }
                        isRunning = true;
                        easeInWalkDirection = 0;
                        player.signals.Set("running");
                }

                private void ImpedeChange (AbilityManager player, ref Vector2 velocity)
                {
                        if (player.ground)
                        {
                                player.UpdateVelocityGround();
                        }
                        if (impedeChange > 0 && !player.ground)
                        {
                                if (player.airMomentumActive && inputX == 0)
                                {
                                        return;
                                }
                                player.velocityOnGround = Mathf.MoveTowards(player.velocityOnGround, velocity.x, Time.deltaTime * (5f - impedeChange) * 10f); // 5 is the max value for airResistance, invert value
                                float scale = run ? speedBoost : 1f;
                                velocity.x = Mathf.Clamp(player.velocityOnGround, -speed * scale, speed * scale);
                        }
                }

                public void Run (bool value)
                {
                        run = value;
                }

                public enum RunLimit
                {
                        CanRun,
                        Charging,
                        CoolDown
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField, HideInInspector] private bool runFoldOut;
                [SerializeField, HideInInspector] private bool runLimitFoldOut;
                [SerializeField, HideInInspector] private bool onRunTimeLimitFoldOut;
                [SerializeField, HideInInspector] private bool onCoolDownFoldOut;
                [SerializeField, HideInInspector] private bool useRightOnly;
                [SerializeField, HideInInspector] private bool useLeftOnly;
                [SerializeField, HideInInspector] private bool foldOut;
                [SerializeField, HideInInspector] private bool eventFoldOut;
                [SerializeField, HideInInspector] private bool groundFoldOut;
                [SerializeField, HideInInspector] private bool notGroundFoldOut;
                [SerializeField, HideInInspector] private bool directionFoldOut;
                [SerializeField, HideInInspector] private bool walkingOnGroundFoldOut;
                [SerializeField, HideInInspector] private bool editTransitions;

                public static void OnInspector (SerializedProperty parent, string[] inputList, Color barColor, Color labelColor)
                {
                        if (FoldOut.Bar(parent, barColor).Label("Walk", labelColor).FoldOut())
                        {
                                FoldOut.Box(3, FoldOut.boxColorLight, extraHeight: 5, offsetY: -2);
                                {
                                        parent.Field("Speed", "speed");
                                        parent.Slider("Smooth", "smooth", min: 0.95f);
                                        parent.Slider("Damp Air Change", "impedeChange", 0, 5f);
                                }

                                if (FoldOut.FoldOutButton(parent.Get("eventFoldOut")))
                                {
                                        Fields.EventFoldOutEffect(parent.Get("onGroundHit"), parent.Get("groundHitWE"), parent.Get("groundFoldOut"), "Ground Hit", color: FoldOut.boxColorLight);
                                        Fields.EventFoldOutEffect(parent.Get("onNotOnGround"), parent.Get("notOnGroundWE"), parent.Get("notGroundFoldOut"), "Not On Ground", color: FoldOut.boxColorLight);
                                        Fields.EventFoldOutEffect(parent.Get("onWalkingOnGround"), parent.Get("walkingOnGroundWE"), parent.Get("walkingOnGroundFoldOut"), "Walking On Ground", color: FoldOut.boxColorLight);
                                        Fields.EventFoldOutEffect(parent.Get("onDirectionChanged"), parent.Get("directionChangedWE"), parent.Get("directionFoldOut"), "Direction Changed", color: FoldOut.boxColorLight);
                                }

                                if (FoldOut.Bar(parent, FoldOut.boxColorLight).Label("Run", FoldOut.titleColor, false).BRE("run").FoldOut("runFoldOut"))
                                {
                                        bool smoothIntoRun = parent.Bool("smoothIntoRun");
                                        int runType = parent.Enum("runType");
                                        int height = runType == 2 ? 2 : 0;
                                        if (smoothIntoRun)
                                                height += 1;

                                        FoldOut.Box(5 + height, FoldOut.boxColorLight, offsetY: -2);
                                        {
                                                GUI.enabled = parent.Bool("run");
                                                {
                                                        parent.FieldAndDropDownList(inputList, "Type", "runType", "runButton", execute: runType < 3);
                                                        parent.FieldDouble("Type", "runType", "runThreshold", execute: runType >= 3);
                                                        parent.Field("Speed Boost", "speedBoost");
                                                        parent.Field("Jump Boost", "runJumpBoost");
                                                        parent.FieldAndEnable("Ease Into Run", "lerpRunTime", "smoothIntoRun");
                                                        Labels.FieldText("Ease Time", rightSpacing: Layout.boolWidth + 4);
                                                        parent.Field("Ease Into Walk", "lerpWalkTime", execute: smoothIntoRun);
                                                        parent.Field("Tap Threshold", "tapTime", execute: runType == 2);
                                                        parent.FieldToggle("Button Is Left,Right", "buttonIsLeftRight", execute: runType == 2);
                                                        parent.FieldToggleAndEnable("Run From Stop", "runFromStop");
                                                }
                                                GUI.enabled = true;
                                        }
                                        Layout.VerticalSpacing(3);
                                }

                                if (parent.Bool("run") && FoldOut.Bar(parent, FoldOut.boxColorLight).Label("Run Time Limit", FoldOut.titleColor, false).BRE("runLimit").FoldOut("runLimitFoldOut"))
                                {
                                        FoldOut.Box(3, FoldOut.boxColorLight, offsetY: -2);
                                        {
                                                GUI.enabled = parent.Bool("runLimit");
                                                {
                                                        parent.Field("Time Limit", "runTimeLimit");
                                                        parent.Field("Recharge Rate", "runRechargeRate");
                                                        parent.Field("Cool Down", "runCoolDown");
                                                }
                                                GUI.enabled = true;
                                        }
                                        Layout.VerticalSpacing(3);

                                        Fields.EventFoldOut(parent.Get("onRunTimeLimit"), parent.Get("onRunTimeLimitFoldOut"), "On Run Time Limit", color: FoldOut.boxColorLight);
                                        Fields.EventFoldOut(parent.Get("onCoolDown"), parent.Get("onCoolDownFoldOut"), "On Run Cool Down", color: FoldOut.boxColorLight);

                                }

                                #region Debug
#if UNITY_EDITOR
                                if (WorldManager.viewDebugger)
                                {
                                        FoldOut.Box(2, FoldOut.boxColorLight);
                                        {
                                                parent.FieldToggle("Editor Only, Right", "useRightOnly");
                                                parent.FieldToggle("Editor Only, Left", "useLeftOnly");
                                        }
                                        Layout.VerticalSpacing(5);
                                }
#endif
                                #endregion
                        }
                }
#pragma warning restore 0414
#endif
                #endregion
        }

        public enum RunType
        {
                Hold,
                Toggle,
                DoubleTap,
                TimeThreshold,
                StickThreshold
        }
}
