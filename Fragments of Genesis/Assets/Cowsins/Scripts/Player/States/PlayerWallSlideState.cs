using UnityEngine;
namespace cowsins2D
{
    public class PlayerWallSlideState : PlayerBaseState
    {
        private PlayerStats playerStats;

        public PlayerWallSlideState(PlayerStates currentContext, PlayerStateFactory playerStateFactory)
            : base(currentContext, playerStateFactory) 
        {
            playerStats = _ctx.PlayerStats;
        }

        public override void EnterState()
        {
            if (player.WallSlidingResetsJumps) player.ResetJumpAmounts();
            if (player.isJumping) player.rb.velocity = Vector2.zero;

            player.wallSlideVFXTimer = player.WallSlideVFXInterval;

            InputManager.Instance.onJump += HandleJumpInput;
            InputManager.Instance.onDash += Dash;
        }

        public override void UpdateState()
        {
            player.HandleVelocities();

            if (!playerControl.Controllable) return;
            player.CheckCollisions(); 
            CheckSwitchState();
        }

        public override void FixedUpdateState()
        {
            if (!playerControl.Controllable) return;
            player.Movement();
            player.Slide();
        }

        public override void ExitState()
        {
            InputManager.Instance.onJump -= HandleJumpInput;
            InputManager.Instance.onDash -= Dash;
        }

        public override void CheckSwitchState()
        {
            if (player.isJumping && player.rb.velocity.y < 0)
            {
                player.JumpFall();
                SwitchState(_factory.Default());
            }
            if (!player.CheckSlideStatus()) SwitchState(_factory.Default());

            if (player.CheckIfPerformWallJump()) SwitchState(_factory.WallJump());
        }
    }
}