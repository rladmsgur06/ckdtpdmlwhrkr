using UnityEngine;

namespace cowsins2D
{
    public class PlayerDefaultState : PlayerBaseState
    {
        PlayerStats playerStats;
        public PlayerDefaultState(PlayerStates currentContext, PlayerStateFactory playerStateFactory)
            : base(currentContext, playerStateFactory)
        {
            playerStats = _ctx.PlayerStats;
        }

        public override void EnterState()
        {
            // Reset Jumping
            player.isJumping = false;

            InputManager.Instance.onJump += HandleJumpInput;
            InputManager.Instance.onDash += Dash;
        }

        public override void UpdateState()
        {
            player.CheckCollisions();
            player.HandleVelocities();

            // Avoid running the following code if the player is not controllable
            if (!playerControl.Controllable) return;
            player.CheckIfJumpingOrWallSliding();
            player.orientatePlayer?.Invoke();
            player.HandleFootsteps();
            CheckSwitchState();
        }

        public override void FixedUpdateState()
        {
            // Avoid running the following code if the player is not controllable
            if (!playerControl.Controllable) return;
            // Handles the player movement
            player.Movement();
        }

        public override void ExitState()
        {
            InputManager.Instance.onJump -= HandleJumpInput;
            InputManager.Instance.onDash -= Dash;
        }

        public override void CheckSwitchState()
        {
            if (player.ladderAvailable && InputManager.PlayerInputs.VerticalMovement > 0) SwitchState(_factory.Ladder());

            if(player.CheckIfPerformJump()) SwitchState(_factory.Jump());

            if (player.CheckSlideStatus()) SwitchState(_factory.WallSlide());

            if (InputManager.PlayerInputs.Crouch && player.AllowCrouch) SwitchState(_factory.Crouch());

            if (InputManager.PlayerInputs.Jump && player.currentJumps <= 0 && player.LastOnGroundTime <= 0 && player.LastPressedJumpTime > 0 && player.CanGlide) SwitchState(_factory.Glide());
        }
    }
}