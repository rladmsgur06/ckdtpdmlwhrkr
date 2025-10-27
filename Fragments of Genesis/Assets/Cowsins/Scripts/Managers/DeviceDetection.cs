using UnityEngine;
using UnityEngine.InputSystem;
namespace cowsins2D {
public class DeviceDetection : MonoBehaviour
{
    public enum InputMode
    {
        Keyboard, Controller, Mobile
    }
    public InputMode mode { get; private set; }

    public static DeviceDetection Instance;

    bool controllerInputReceived = false;

    [SerializeField] private GameObject touchInputUI; 

    private void Awake()
    {
        // Makes sure there is only once instance of this script in the game
        if (Instance == null) Instance = this;

    }
    private void Update() => DetectInputs(); 

    public void DetectInputs()
    {
        // Detects if any input has been received from the keyboard or the mouse
        bool KeyboardInputReceived = Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame || (Mouse.current.delta.ReadValue() != Vector2.zero || Mouse.current.leftButton.isPressed || Mouse.current.rightButton.isPressed || Mouse.current.middleButton.isPressed);
            
        if (KeyboardInputReceived) controllerInputReceived = false;
        else
        {
                // If no input was received from the keyboard, check from the controller
                Gamepad gamepad = Gamepad.current;
                if (gamepad != null)
                {
                    // Loop through each control on the gamepads to see if that is pressed
                    foreach (InputControl control in gamepad.allControls)
                    {
                        if (control.IsPressed())
                        {
                            controllerInputReceived = true;
                            break;
                        }
                    }
                }
        }
        

        // Update the current device detected accordingly
        if (controllerInputReceived) mode = InputMode.Controller;
        else if (KeyboardInputReceived)  mode = InputMode.Keyboard;
    }
}
}