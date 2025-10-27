using UnityEngine;
namespace cowsins2D
{
    public class PlayerDieState : PlayerBaseState
    {
        PlayerStats stats;
        Rigidbody2D rb;
        public PlayerDieState(PlayerStates currentContext, PlayerStateFactory playerStateFactory)
            : base(currentContext, playerStateFactory)
        {
            stats = _ctx.PlayerStats;
            rb = _ctx.Rigidbody2D;
        }
        public override void EnterState()
        {
            rb.velocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        public override void UpdateState() { }

        public override void FixedUpdateState() {}

        public override void ExitState()
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
        }

        public override void CheckSwitchState() {}
    }
}