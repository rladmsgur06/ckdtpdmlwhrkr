using UnityEngine;

namespace cowsins2D
{
    public class PlayerLadderState : PlayerBaseState
    {
        private PlayerStats playerStats;
        private PlayerAnimator anim;
        private Rigidbody2D rb;

        public PlayerLadderState(PlayerStates currentContext, PlayerStateFactory playerStateFactory)
            : base(currentContext, playerStateFactory)
        {
            player = _ctx.PlayerMovement;
            playerControl = _ctx.PlayerControl;
            playerStats = _ctx.PlayerStats; 
            anim = _ctx.PlayerAnimator;
            rb = _ctx.Rigidbody2D;  
        }

        public override void EnterState()
        {
            if (player.isGliding) player.StopGlide();
            
            player.SetGravityScale(0);
        }

        public override void UpdateState()
        {
            player.CheckCollisions();
            if (!playerControl.Controllable) return;
            player.LadderVelocity();
            CheckSwitchState();

            if (rb.velocity.magnitude < .1f) player.events.onIdleLadder?.Invoke();
            else player.events.onMovingLadder?.Invoke();
        }

        public override void FixedUpdateState()
        {
            if (!playerControl.Controllable) return;
            player.Movement();
            player.VerticalMovement();
        }

        public override void ExitState() {}

        public override void CheckSwitchState()
        {
            if (!player.ladderAvailable || (player.IsGrounded && InputManager.PlayerInputs.VerticalMovement <= 0)) SwitchState(_factory.Default());
        }
    }
}