using System;
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
#if UNITY_EDITOR
using TwoBitMachines.Editors;
using UnityEditor;
#endif

namespace TwoBitMachines.FlareEngine
{
        [CreateAssetMenu(menuName = "FlareEngine/InputButtonSO")]
        public class InputButtonSO : ScriptableObject
        {
                [SerializeField] public string buttonName;
                [SerializeField] public InputType type;
                [SerializeField] public InputMouse mouse;
                [SerializeField] public KeyCode key = KeyCode.A;
                [SerializeField] public string axisName; // for input manager
                [SerializeField] public List<InputButtonSO> bindings = new List<InputButtonSO>();

                [SerializeField] public Axis axisToRead = Axis.X;
                [SerializeField] public float maxThreshold = 99;
                [SerializeField] public float minThreshold = -99;

                [System.NonSerialized] public bool inputHold;
                [System.NonSerialized] public bool inputPressed;
                [System.NonSerialized] public bool inputReleased;
                [System.NonSerialized] public bool axisPressed;
                [System.NonSerialized] public bool axisReleased;
                [System.NonSerialized] public int axisPressFrame;
                [System.NonSerialized] public float value;
                [System.NonSerialized] public Vector2 valueV2;
                [SerializeField, HideInInspector] public bool foldOut;
                public float axisValue => Input.GetAxisRaw(axisName);

                #region Read Button Values
                public bool Holding (bool checkBindings = true)
                {
                        if (inputHold)
                        {
                                return true;
                        }
                        else if (type == InputType.Keyboard && Input.GetKey(key))
                        {
                                return true;
                        }
                        else if (type == InputType.Mouse && Input.GetMouseButton((int) mouse))
                        {
                                return true;
                        }
                        else if (type == InputType.AxisNegative && axisValue < 0)
                        {
                                return true;
                        }
                        else if (type == InputType.AxisPositive && axisValue > 0)
                        {
                                return true;
                        }
                        if (!checkBindings)
                        {
                                return false;
                        }
                        for (int i = 0; i < bindings.Count; i++)
                        {
                                if (bindings[i] != null && bindings[i] != this && bindings[i].Holding(false))
                                {
                                        return true;
                                }
                        }
                        return false;
                }

                public bool Pressed (bool checkBindings = true)
                {
                        if (axisPressed && axisPressFrame != Time.frameCount && axisValue == 0)
                        {
                                axisPressed = false;
                                axisReleased = true;
                        }
                        if (inputPressed)
                        {
                                return true;
                        }
                        else if (type == InputType.Keyboard && Input.GetKeyDown(key))
                        {
                                return true;
                        }
                        else if (type == InputType.Mouse && Input.GetMouseButtonDown((int) mouse))
                        {
                                return true;
                        }
                        else if ((type == InputType.AxisNegative && axisValue < 0) || (type == InputType.AxisPositive && axisValue > 0))
                        {
                                if (axisPressed && axisPressFrame == Time.frameCount)
                                {
                                        return true;
                                }
                                if (!axisPressed)
                                {
                                        axisPressFrame = Time.frameCount;
                                        axisPressed = true;
                                        axisReleased = false;
                                        return true;
                                }
                        }
                        if (!checkBindings)
                        {
                                return false;
                        }
                        for (int i = 0; i < bindings.Count; i++)
                        {
                                if (bindings[i] != null && bindings[i] != this && bindings[i].Pressed(false))
                                {
                                        return true;
                                }
                        }
                        return false;
                }

                public bool Released (bool checkBindings = true)
                {
                        if (inputReleased)
                        {
                                return true;
                        }
                        else if (type == InputType.Keyboard && Input.GetKeyUp(key))
                        {
                                return true;
                        }
                        else if (type == InputType.Mouse && Input.GetMouseButtonUp((int) mouse))
                        {
                                return true;
                        }
                        else if (type == InputType.AxisNegative || type == InputType.AxisPositive)
                        {
                                if (!axisPressed && ((type == InputType.AxisNegative && axisValue < 0) || (type == InputType.AxisPositive && axisValue > 0)))
                                {
                                        axisPressFrame = Time.frameCount;
                                        axisPressed = true; // have to set here too, or else Released cant be used as a stand alone method
                                        axisReleased = false;
                                }
                                if (axisPressed && axisValue == 0)
                                {
                                        axisPressed = false;
                                        axisReleased = true;
                                        return true;
                                }
                                if (axisReleased)
                                {
                                        return true;
                                }
                        }
                        if (!checkBindings)
                        {
                                return false;
                        }
                        for (int i = 0; i < bindings.Count; i++)
                        {
                                if (bindings[i] != null && bindings[i] != this && bindings[i].Released(false))
                                {
                                        return true;
                                }
                        }
                        return false;
                }

                public bool Active (ButtonTrigger buttonTrigger)
                {
                        if (buttonTrigger == ButtonTrigger.OnPress)
                                return Pressed();
                        else if (buttonTrigger == ButtonTrigger.OnHold)
                                return Holding();
                        else if (buttonTrigger == ButtonTrigger.OnRelease)
                                return Released();
                        else if (buttonTrigger == ButtonTrigger.Always)
                                return true;
                        else if (buttonTrigger == ButtonTrigger.Never)
                                return false;
                        else
                                return Active();
                }

                public bool Active ()
                {
                        return inputPressed || inputHold; // these can only be read if they've been set true externally
                }

                public bool Any (ButtonTrigger buttonTrigger)
                {
                        return Pressed() || Holding() || Released() || (buttonTrigger == ButtonTrigger.Always);
                }

                public bool Any ()
                {
                        return Pressed() || Holding() || Released();
                }
                #endregion

                #region Set Button Values
                public void ButtonPressed ()
                {
                        inputPressed = true;
                        for (int i = 0; i < bindings.Count; i++)
                        {
                                if (bindings[i] != null)
                                {
                                        bindings[i].inputPressed = true;
                                }
                        }
                }

                public void ButtonHold ()
                {
                        inputHold = true;
                        for (int i = 0; i < bindings.Count; i++)
                        {
                                if (bindings[i] != null)
                                {
                                        bindings[i].inputHold = true;
                                }
                        }
                }

                public void ButtonReleased ()
                {
                        inputHold = false;
                        inputReleased = true; //cleared each frame by world manager
                        for (int i = 0; i < bindings.Count; i++)
                        {
                                if (bindings[i] == null)
                                        continue;
                                bindings[i].inputHold = false;
                                bindings[i].inputReleased = true;
                        }
                }

#if ENABLE_INPUT_SYSTEM
                public void InputPerformed (InputAction.CallbackContext context)
                {
                        if (context.valueType == typeof(Single))
                        {
                                if (context.performed)
                                {
                                        inputPressed = true;
                                }
                        }
                        else if (context.valueType == typeof(float))
                        {
                                value = context.ReadValue<float>();
                                inputPressed = value >= minThreshold && value <= maxThreshold;
                        }
                        else if (context.valueType == typeof(Vector2))
                        {
                                valueV2 = context.ReadValue<Vector2>();
                                float v2 = axisToRead == Axis.X ? valueV2.x : valueV2.y;
                                inputPressed = v2 >= minThreshold && v2 <= maxThreshold;
                        }
                }

                public void InputPerformedHold (InputAction.CallbackContext context)
                {
                        if (context.valueType == typeof(Single))
                        {
                                if (context.performed)
                                {
                                        inputHold = true;
                                }
                        }
                        else if (context.valueType == typeof(float))
                        {
                                value = context.ReadValue<float>();
                                inputHold = value >= minThreshold && value <= maxThreshold;
                        }
                        else if (context.valueType == typeof(Vector2))
                        {
                                valueV2 = context.ReadValue<Vector2>();
                                float v2 = axisToRead == Axis.X ? valueV2.x : valueV2.y;
                                inputHold = v2 >= minThreshold && v2 <= maxThreshold;
                        }
                }

                public void InputCancelled (InputAction.CallbackContext context)
                {
                        if (context.canceled)
                        {
                                inputHold = false;
                                inputReleased = true;
                        }
                }
#endif
                #endregion

                #region Override Button Values -- old input system only
                public void RestoreSavedValues ()
                {
                        key = (KeyCode) PlayerPrefs.GetInt("TwoBitMachinesButton" + buttonName, (int) key);
                        mouse = (InputMouse) PlayerPrefs.GetInt("TwoBitMachinesMouse" + buttonName, (int) mouse);
                        axisName = PlayerPrefs.GetString("TwoBitMachinesAxis" + buttonName, axisName);
                        type = (InputType) PlayerPrefs.GetInt("TwoBitMachinesType" + buttonName, (int) type);
                }

                public void DeleteSavedValues ()
                {
                        PlayerPrefs.DeleteKey("TwoBitMachinesButton" + buttonName);
                        PlayerPrefs.DeleteKey("TwoBitMachinesMouse" + buttonName);
                        PlayerPrefs.DeleteKey("TwoBitMachinesAxis" + buttonName);
                        PlayerPrefs.DeleteKey("TwoBitMachinesType" + buttonName);
                }

                public void OverrideKeyboardKey (KeyCode newKey)
                {
                        key = newKey;
                        PlayerPrefs.SetInt("TwoBitMachinesButton" + buttonName, (int) key);
                }

                public void OverrideMouseKey (int newKey)
                {
                        mouse = (InputMouse) newKey;
                        PlayerPrefs.SetInt("TwoBitMachinesMouse" + buttonName, newKey);
                }

                public void OverrideAxisName (string name)
                {
                        axisName = name;
                        PlayerPrefs.SetString("TwoBitMachinesAxis" + buttonName, name);
                }

                public string InputName ()
                {
                        if (type == InputType.Keyboard)
                        {
                                return key.ToString();
                        }
                        if (type == InputType.Mouse)
                        {
                                if (mouse == InputMouse.Left)
                                {
                                        return "LMB";
                                }
                                if (mouse == InputMouse.Right)
                                {
                                        return "RMB";
                                }
                                else
                                {
                                        return "MMB";
                                }
                        }
                        return axisName;
                }

                #endregion
        }

        public enum ButtonTrigger
        {
                OnHold,
                OnPress,
                OnRelease,
                Always,
                Never,
                Active
        }

        public enum InputMouse
        {
                Left,
                Right,
                Middle,
                None
        }

        public enum InputType
        {
                Keyboard,
                Mouse,
                AxisNegative,
                AxisPositive
        }

        public enum Axis
        {
                X,
                Y
        }
}
