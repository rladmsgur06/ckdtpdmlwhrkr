using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace TwoBitMachines.FlareEngine
{
        public class InputRumble : MonoBehaviour
        {
                [System.NonSerialized] private PlayerInput playerInput;
                [System.NonSerialized] private RumblePattern rumbleType;

                [System.NonSerialized] private float rumbleDuration;
                [System.NonSerialized] private float pulseDuration;

                [System.NonSerialized] private float lowFrequencyMotor; // 0f off, 1f max
                [System.NonSerialized] private float highFrequencyMotor;

                [System.NonSerialized] private float lowStep;
                [System.NonSerialized] private float highStep;
                [System.NonSerialized] private float rumbleStep;
                [System.NonSerialized] private bool isMotorActive = false;

                // Unity MonoBehaviors
                private void Awake ( )
                {
                        playerInput = GetComponent<PlayerInput> ( );
                }

                public void Test ( )
                {
                        RumbleConstant (0.5f, 0.5f, 2f);
                }

                // Public Methods
                public void RumbleConstant (float low, float high, float duration)
                {
                        rumbleType = RumblePattern.Constant;
                        lowFrequencyMotor = low;
                        highFrequencyMotor = high;
                        rumbleDuration = Time.time + duration;
                }

                public void RumblePulse (float low, float high, float burstTime, float duration)
                {
                        rumbleType = RumblePattern.Pulse;
                        lowFrequencyMotor = low;
                        highFrequencyMotor = high;
                        rumbleStep = burstTime;
                        pulseDuration = Time.time + burstTime;
                        rumbleDuration = Time.time + duration;
                        isMotorActive = true;
                        var g = GetGamepad ( );
                        g?.SetMotorSpeeds (lowFrequencyMotor, highFrequencyMotor);
                }

                public void RumbleLinear (float lowStart, float lowEnd, float highStart, float highEnd, float duration)
                {
                        rumbleType = RumblePattern.Linear;
                        lowFrequencyMotor = lowStart;
                        highFrequencyMotor = highStart;
                        lowStep = (lowEnd - lowStart) / duration;
                        highStep = (highEnd - highStart) / duration;
                        rumbleDuration = Time.time + duration;
                }

                public void StopRumble ( )
                {
                        var gamepad = GetGamepad ( );
                        if (gamepad != null)
                        {
                                gamepad.SetMotorSpeeds (0, 0);
                        }
                }

                private void Update ( )
                {
                        if (Time.time > rumbleDuration)
                        {
                                StopRumble ( );
                                return;
                        }

                        var gamepad = GetGamepad ( );
                        if (gamepad == null)
                        {
                                return;
                        }
                        switch (rumbleType)
                        {
                                case RumblePattern.Constant:
                                        gamepad.SetMotorSpeeds (lowFrequencyMotor, highFrequencyMotor);
                                        break;

                                case RumblePattern.Pulse:

                                        if (Time.time > pulseDuration)
                                        {
                                                isMotorActive = !isMotorActive;
                                                pulseDuration = Time.time + rumbleStep;
                                                if (!isMotorActive)
                                                {
                                                        gamepad.SetMotorSpeeds (0, 0);
                                                }
                                                else
                                                {
                                                        gamepad.SetMotorSpeeds (lowFrequencyMotor, highFrequencyMotor);
                                                }
                                        }

                                        break;
                                case RumblePattern.Linear:
                                        gamepad.SetMotorSpeeds (lowFrequencyMotor, highFrequencyMotor);
                                        lowFrequencyMotor += (lowStep * Time.deltaTime);
                                        highFrequencyMotor += (highStep * Time.deltaTime);
                                        break;
                                default:
                                        break;
                        }
                }

                private void OnDestroy ( )
                {
                        StopAllCoroutines ( );
                        StopRumble ( );
                }

                private Gamepad GetGamepad ( )
                {
                        for (int i = 0; i < Gamepad.all.Count; i++)
                        {
                                Gamepad gamePad = Gamepad.all[i];
                                for (int j = 0; j < playerInput.devices.Count; j++)
                                {
                                        if (gamePad.deviceId == playerInput.devices[i].deviceId)
                                        {
                                                return gamePad;
                                        }
                                }
                        }
                        return null;
                        //return Gamepad.all.FirstOrDefault (g => playerInput.devices.Any (d => d.deviceId == g.deviceId));
                }
        }

        public enum RumblePattern
        {
                Constant,
                Pulse,
                Linear
        }

}