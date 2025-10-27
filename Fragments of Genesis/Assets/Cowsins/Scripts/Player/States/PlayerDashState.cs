using UnityEngine;
namespace cowsins2D
{
    public class PlayerDashState : PlayerBaseState
    {
        PlayerStats playerStats;
        Rigidbody2D rb;
        public PlayerDashState(PlayerStates currentContext, PlayerStateFactory playerStateFactory)
            : base(currentContext, playerStateFactory)
        {
            playerStats = _ctx.PlayerStats;
            rb = _ctx.Rigidbody2D;
        }

        public override void EnterState()
        {
            player.InitializeDash();
        }

        public override void UpdateState()
        {
            if (!playerControl.Controllable) return;
            CheckSwitchState();
            player.PerformDash();
        }

        public override void FixedUpdateState() {}

        public override void ExitState() {}

        public override void CheckSwitchState()
        {
            if (player.LastOnGroundTime > 0 && !player.isDashing) SwitchState(_factory.Default());

            if (!player.isDashing) SwitchState(_factory.Default());

            if (playerStats.Health <= 0) SwitchState(_factory.Die());
        }
    }
}