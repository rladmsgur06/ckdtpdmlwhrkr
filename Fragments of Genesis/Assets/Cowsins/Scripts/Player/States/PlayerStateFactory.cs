using System.Runtime.CompilerServices;
using System.Xml;

namespace cowsins2D
{
    public class PlayerStateFactory
    {
        PlayerStates _context;

        // Cache Player States
        private readonly PlayerDefaultState _playerDefaultState;
        private readonly PlayerCrouchState _playerCrouchState;
        private readonly PlayerJumpState _playerJumpState;
        private readonly PlayerWallSlideState _playerWallSlideState;
        private readonly PlayerWallJumpState _playerWallJumpState;
        private readonly PlayerDashState _playerDashState;
        private readonly PlayerDieState _playerDieState;
        private readonly PlayerLadderState _playerLadderState;
        private readonly PlayerGlideState _playerGlideState;

        // When PlayerStateFactory is created, retrieve all PlayerStates and store them.
        public PlayerStateFactory(PlayerStates currentContext)
        {
            _context = currentContext;

            _playerDefaultState = new PlayerDefaultState(_context, this);
            _playerCrouchState = new PlayerCrouchState(_context, this);
            _playerJumpState = new PlayerJumpState(_context, this);
            _playerWallSlideState = new PlayerWallSlideState(_context, this);
            _playerWallJumpState = new PlayerWallJumpState(_context, this);
            _playerDashState = new PlayerDashState(_context, this);
            _playerDieState = new PlayerDieState(_context, this);
            _playerLadderState = new PlayerLadderState(_context, this);
            _playerGlideState = new PlayerGlideState(_context, this);
        }


        // Access Player States
        public PlayerBaseState Default() => _playerDefaultState;
        public PlayerBaseState Crouch() => _playerCrouchState;
        public PlayerBaseState Jump() => _playerJumpState;
        public PlayerBaseState WallSlide() => _playerWallSlideState;
        public PlayerBaseState WallJump() => _playerWallJumpState;
        public PlayerBaseState Dash() => _playerDashState;
        public PlayerBaseState Die() => _playerDieState;
        public PlayerBaseState Ladder() => _playerLadderState;
        public PlayerBaseState Glide() => _playerGlideState;
    }
}