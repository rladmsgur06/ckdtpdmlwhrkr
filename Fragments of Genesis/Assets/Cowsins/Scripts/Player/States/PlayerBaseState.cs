namespace cowsins2D
{
    public abstract class PlayerBaseState
    {
        protected PlayerStates _ctx;
        protected PlayerStateFactory _factory;
        protected PlayerControl playerControl;
        protected PlayerMovement player;

        public PlayerBaseState(PlayerStates currentContext, PlayerStateFactory playerStateFactory)
        {
            _ctx = currentContext;
            _factory = playerStateFactory;
            player = _ctx.PlayerMovement;
            playerControl = _ctx.PlayerControl;
        }

        public abstract void EnterState();

        public abstract void UpdateState();

        public abstract void FixedUpdateState();

        public abstract void ExitState();

        public abstract void CheckSwitchState();
        void UpdateStates() { }

        protected void SwitchState(PlayerBaseState newState)
        {
            ExitState();

            newState.EnterState();

            _ctx.CurrentState = newState;
        }

        protected void HandleJumpInput()
        {
            if (!playerControl.Controllable) return;

            player.OnJumpInput();
        }

        protected void HandleJumpCutInput()
        {
            if (!playerControl.Controllable) return;

            player.OnJumpUpInput();
        }

        protected void Dash()
        {
            if (!playerControl.Controllable) return;

            if (player.CanDash()) SwitchState(_factory.Dash());
        }
    }
}