using UnityEngine;
namespace cowsins2D
{
    public class PlayerJumpState : PlayerBaseState
    {
        private PlayerStats playerStats;
        public PlayerJumpState(PlayerStates currentContext, PlayerStateFactory playerStateFactory)
            : base(currentContext, playerStateFactory)
        {
            playerStats = _ctx.PlayerStats; 
        }

        public override void EnterState()
        {
            player.Jump();
            player.ReduceJumpAmount();
            player.transform.up = Vector3.up;

            InputManager.Instance.onJumpCut += HandleJumpCutInput;
            InputManager.Instance.onDash += Dash;
        }

        public override void UpdateState()
        {
            player.HandleVelocities();

            if (!playerControl.Controllable)
            {
                SwitchState(_factory.Default());
                return;
            }
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
            if (player.ladderAvailable && InputManager.PlayerInputs.VerticalMovement != 0) SwitchState(_factory.Ladder());
            
            if (player.isJumping && player.rb.velocity.y < 0)
            {
                player.JumpFall();
                SwitchState(_factory.Default());
            }
            if (player.CheckIfPerformJump()) SwitchState(_factory.Jump());
            
            if (player.IsGrounded) SwitchState(_factory.Default());
        }
    }
}