using System;
using UnityEngine;

namespace cowsins2D
{
    [DefaultExecutionOrder(-1000)]
    public class InputManager : MonoBehaviour
    {
        public static InputManager Instance;
      
        public PlayerActions inputActions;

        public static PlayerInputs PlayerInputs;

        public event Action onDrop, onJump, onJumpCut, onDash, onOpenInventory, onPause;

        private void OnEnable()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else Destroy(this.gameObject);


            if (inputActions == null)  inputActions = new PlayerActions();
                
            // Initialize player inputs
            inputActions.Enable();

            inputActions.GameControls.Drop.started += ctx => onDrop?.Invoke();
            inputActions.GameControls.Jumping.started += ctx => onJump?.Invoke();
            inputActions.GameControls.Jumping.canceled += ctx => onJumpCut?.Invoke();
            inputActions.GameControls.Dash.started += ctx => onDash?.Invoke();
            inputActions.GameControls.OpenInventory.started += ctx => onOpenInventory?.Invoke();
            inputActions.GameControls.Pause.started += ctx => onPause?.Invoke();
        }
        private void OnDisable()
        {
            inputActions.Disable();
            inputActions.GameControls.Drop.started -= ctx => onDrop?.Invoke();
            inputActions.GameControls.Jumping.started -= ctx => onJump?.Invoke();
            inputActions.GameControls.Jumping.canceled -= ctx => onJumpCut?.Invoke();
            inputActions.GameControls.Dash.started -= ctx => onDash?.Invoke();
            inputActions.GameControls.OpenInventory.started -= ctx => onOpenInventory?.Invoke();
            inputActions.GameControls.Pause.started -= ctx => onPause?.Invoke();
        }

        // Update inputs in realtime
        private void Update() => PlayerInputs = ReceiveInputs();

        private PlayerInputs ReceiveInputs()
        {
            // Returns all the necessary inputs in-game
            return new PlayerInputs
            {
                HorizontalMovement = inputActions.GameControls.Movement.ReadValue<float>(),
                VerticalMovement = -inputActions.GameControls.VerticalMovement.ReadValue<float>(),
                AimDirection = inputActions.GameControls.Aiming.ReadValue<Vector2>(),
                Crouch = inputActions.GameControls.Crouch.IsPressed(),
                JumpingDown = inputActions.GameControls.Jumping.WasPressedThisFrame(),
                JumpingUp = inputActions.GameControls.Jumping.WasReleasedThisFrame(),
                Jump = inputActions.GameControls.Jumping.IsPressed(),
                Run = inputActions.GameControls.Sprint.IsPressed(),
                Interact = inputActions.GameControls.Interact.IsPressed(),
                OpenInventory = inputActions.GameControls.OpenInventory.WasPressedThisFrame(),
                MousePos = inputActions.GameControls.MousePosition.ReadValue<Vector2>(),
                Shoot = inputActions.GameControls.Shoot.WasPressedThisFrame(),
                ShootHold = inputActions.GameControls.Shoot.IsPressed(),
                Reload = inputActions.GameControls.Reload.IsPressed(),
                Dash = inputActions.GameControls.Dash.WasPressedThisFrame(),
                MouseWheel = inputActions.GameControls.MouseWheel.ReadValue<Vector2>(),
                UINavigation = inputActions.GameControls.UINavigation.ReadValue<Vector2>(),
                UISelect = inputActions.GameControls.UISelect.WasPressedThisFrame(),
                InventoryDrop = inputActions.GameControls.DropInventory.WasPressedThisFrame(),
                InventoryUse = inputActions.GameControls.UseInventory.WasPressedThisFrame(),
                Drop = inputActions.GameControls.Drop.WasPressedThisFrame(),
                NextWeapon = inputActions.GameControls.NextWeapon.WasPressedThisFrame(),
                PreviousWeapon = inputActions.GameControls.PreviousWeapon.WasPressedThisFrame(),
                Pausing = inputActions.GameControls.Pause.WasPressedThisFrame()
            };
        }
    }
}
