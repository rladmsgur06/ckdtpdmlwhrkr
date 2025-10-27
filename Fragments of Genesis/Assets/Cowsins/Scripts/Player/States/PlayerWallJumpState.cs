using UnityEngine;
namespace cowsins2D
{
    public class PlayerWallJumpState : PlayerBaseState
    {
        PlayerStats playerStats;
        public PlayerWallJumpState(PlayerStates currentContext, PlayerStateFactory playerStateFactory)
            : base(currentContext, playerStateFactory)
        {
            playerStats = _ctx.PlayerStats;
        }
        public override void EnterState()
        {
            player.ReduceJumpAmount();
            player.SetGravityScale(0);

            InputManager.Instance.onJumpCut += HandleJumpCutInput;
            InputManager.Instance.onDash += Dash;
        }

        public override void UpdateState()
        {
            player.CheckCollisions();
            player.HandleVelocities();

            if (!playerControl.Controllable) return;
            player.orientatePlayer?.Invoke();
            CheckSwitchState();
        }

        public override void FixedUpdateState()
        {
            if (!playerControl.Controllable) return;
            player.Movement();
        }

        public override void ExitState()
        {
            InputManager.Instance.onJumpCut -= HandleJumpCutInput;
            InputManager.Instance.onDash -= Dash;
        }

        public override void CheckSwitchState()
        {
            if (player.ladderAvailable && InputManager.PlayerInputs.VerticalMovement > 0) SwitchState(_factory.Ladder());

            if (player.CheckSlideStatus()) SwitchState(_factory.WallSlide());

            if (player.isJumping && player.rb.velocity.y < 0)
            {
                player.JumpFall();
                SwitchState(_factory.Default());
            }

            if (player.LastOnGroundTime > 0) SwitchState(_factory.Default());

            if (Time.time - player.wallJumpStartTime > player.WallJumpTime)
            {
                player.StopWallJump();
                SwitchState(_factory.Default());
            }
        }
    }
}