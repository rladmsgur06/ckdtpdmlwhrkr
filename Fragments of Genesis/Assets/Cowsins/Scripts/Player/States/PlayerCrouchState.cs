using UnityEngine;

namespace cowsins2D
{
    public class PlayerCrouchState : PlayerBaseState
    {
        private PlayerStats playerStats;
        private Rigidbody2D rb;

        private bool slide;
        private float slideTimer;
        public PlayerCrouchState(PlayerStates currentContext, PlayerStateFactory playerStateFactory)
            : base(currentContext, playerStateFactory)
        {
            playerStats = _ctx.PlayerStats;
            rb = _ctx.Rigidbody2D;
        }

        public override void EnterState()
        {
            if (rb.velocity.magnitude > player.WalkSpeed && player.currentSpeed > player.WalkSpeed && (player.LastOnGroundTime > 0 || player.LastOnGroundTime <= 0 && player.CanCrouchSlideMidAir))
            {
                slide = true;
                slideTimer = player.CrouchSlideDuration;
            }
            player.StartCrouch();

            InputManager.Instance.onJump += HandleJumpInput;
            InputManager.Instance.onJumpCut += HandleJumpCutInput;
            InputManager.Instance.onDash += Dash;
        }

        public override void UpdateState()
        {
            if (!playerControl.Controllable) return;
            player.CheckCollisions();
            player.orientatePlayer?.Invoke();
            CheckSwitchState();

            player.events.onCrouched?.Invoke();
            if(rb.velocity.magnitude > .1f) player.events.onCrouchWalking?.Invoke();
            else player.events.onCrouchedIdle?.Invoke();

            if (!slide) return;
            slideTimer -= Time.deltaTime;

            if (!InputManager.PlayerInputs.Crouch || slideTimer <= 0) slide = false;
            else player.CrouchSlide();
        }

        public override void FixedUpdateState()
        {
            if (!playerControl.Controllable) return;
            player.Movement();
        }

        public override void ExitState() { 
            player.StopCrouch();

            InputManager.Instance.onJump -= HandleJumpInput;
            InputManager.Instance.onJumpCut -= HandleJumpCutInput;
            InputManager.Instance.onDash -= Dash;
        }

        public override void CheckSwitchState()
        {
            if (player.CheckCeiling() || PauseMenu.isPaused) return;

            if (player.CheckIfPerformJump()) SwitchState(_factory.Jump());

            if (player.isJumping && rb.velocity.y < 0)
            {
                player.JumpFall();
                SwitchState(_factory.Default());
            }

            if (!InputManager.PlayerInputs.Crouch) SwitchState(_factory.Default());
        }
    }
}