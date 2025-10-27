using UnityEngine;
namespace cowsins2D
{
    public class PlayerStates : MonoBehaviour
    {
        PlayerBaseState _currentState;
        PlayerStateFactory _states;

        public PlayerBaseState CurrentState { get { return _currentState; } set { _currentState = value; } }
        public PlayerStateFactory _States { get { return _states; } set { _states = value; } }

        public PlayerMovement PlayerMovement { get; private set; }
        public PlayerStats PlayerStats { get; private set; }
        public PlayerControl PlayerControl { get; private set; }
        public PlayerAnimator PlayerAnimator { get; private set; }
        public Rigidbody2D Rigidbody2D { get; private set; }

        static PlayerStates _instance;
        public static PlayerStates instance
        {
            get
            {
                return _instance;
            }
        }

        private void Awake()
        {
            GetReferences();

            _instance = this;

            _states = new PlayerStateFactory(this);
            _currentState = _states.Default();
            _currentState.EnterState();
        }

        private void Update()
        {
            if (PlayerStats.Health <= 0) ForceChangeState(_States.Die());
            _currentState.UpdateState();
        }

        private void FixedUpdate()
        {
            _currentState.FixedUpdateState();
        }

        public void ForceChangeState(PlayerBaseState state)
        {
            CurrentState.ExitState();
            CurrentState = state;
            CurrentState.EnterState();
        }

        private void GetReferences()
        {
            PlayerMovement = GetComponent<PlayerMovement>();
            PlayerStats = GetComponent<PlayerStats>();
            PlayerControl = GetComponent<PlayerControl>();
            PlayerAnimator = GetComponent<PlayerAnimator>();
            Rigidbody2D = GetComponent<Rigidbody2D>();
        }
    }
}