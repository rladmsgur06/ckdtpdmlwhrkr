using UnityEngine;
namespace cowsins2D
{
    public class PlayerGlideState : PlayerBaseState
    {
        private PlayerStats playerStats;
        private Rigidbody2D rb;

        private float timer;
        public PlayerGlideState(PlayerStates currentContext, PlayerStateFactory playerStateFactory)
            : base(currentContext, playerStateFactory) {
            playerStats = _ctx.PlayerStats;
            rb = _ctx.Rigidbody2D;
        }

        public override void EnterState()
        {
            player.StartGlide();

            if (player.GlideDurationMethod != GlideDurationMethod.None)
                timer = player.MaximumGlideTime;

            InputManager.Instance.onJump += HandleJumpInput;
            InputManager.Instance.onDash += Dash;
        }

        public override void UpdateState()
        {
            if (!playerControl.Controllable) return;

            player.events.onStartGlide?.Invoke();

            player.GlideVerticalMovement();
            player.CheckCollisions();
            CheckSwitchState();

            if (player.HandleOrientationWhileGliding) player.orientatePlayer?.Invoke();

            if (player.GlideDurationMethod == GlideDurationMethod.None) return;

            RunGlideTimer();
        }

        public override void FixedUpdateState()
        {
            if (!playerControl.Controllable) return;
            player.Movement();

        }

        public override void ExitState()
        {
            player.StopGlide();
            InputManager.Instance.onJump -= HandleJumpInput;
            InputManager.Instance.onDash -= Dash;
        }

        public override void CheckSwitchState()
        {
            if (player.LastOnGroundTime > 0 || !InputManager.PlayerInputs.Jump) SwitchState(_factory.Default());

            if (player.GlideDurationMethod == GlideDurationMethod.None) return;

            if (timer <= 0) SwitchState(_factory.Default());
        }

        private void RunGlideTimer() => timer -= Time.deltaTime;

    }
}