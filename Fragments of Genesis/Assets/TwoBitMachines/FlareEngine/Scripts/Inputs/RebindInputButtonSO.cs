using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace TwoBitMachines.FlareEngine
{
        public class RebindInputButtonSO : MonoBehaviour
        {
                [SerializeField] public InputButtonSO inputButtonSO;
                [SerializeField] public TextMeshProUGUI buttonLabel;
                [SerializeField] public TextMeshProUGUI bindingLabel;
                [SerializeField] public bool overrideButtonLabel = true;
                [SerializeField] public InputType resetTypeTo;
                [SerializeField] public KeyCode resetKeyTo = KeyCode.None;
                [SerializeField] public InputMouse resetMouseTo = InputMouse.None;

                [SerializeField] public UnityEvent rebindStartEvent = new UnityEvent();
                [SerializeField] public UnityEvent rebindStopEvent = new UnityEvent();
                [SerializeField] public UpdateButtonBinding updateEvent = new UpdateButtonBinding();

                [System.NonSerialized] private bool binding = false;

#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField, HideInInspector] public bool startFoldOut;
                [SerializeField, HideInInspector] public bool stopFoldOut;
#endif

                void OnValidate ()
                {
                        UpdateDisplay();
                }
                private void Awake ()
                {
                        WorldManager.RegisterInput(inputButtonSO);
                }

                private void Start ()
                {
                        if (inputButtonSO != null)
                        {
                                inputButtonSO.RestoreSavedValues();
                                UpdateDisplay();
                        }
                }

                public void UpdateDisplay ()
                {
                        if (inputButtonSO != null && bindingLabel != null)
                        {
                                bindingLabel.SetText(inputButtonSO.InputName());
                        }
                        if (inputButtonSO != null && buttonLabel != null && overrideButtonLabel)
                        {
                                buttonLabel.SetText(inputButtonSO.buttonName);
                        }
                        if (inputButtonSO != null)
                        {
                                updateEvent?.Invoke(bindingLabel?.gameObject, inputButtonSO.InputName());
                        }
                }

                public void StartRebind ()
                {
                        if (inputButtonSO != null)
                        {
                                binding = true;
                                rebindStartEvent.Invoke();
                        }
                }

                public void ResetBinding ()
                {
                        if (inputButtonSO != null)
                        {
                                inputButtonSO.key = resetKeyTo;
                                inputButtonSO.mouse = resetMouseTo;
                                inputButtonSO.type = resetTypeTo;
                                inputButtonSO.OverrideKeyboardKey(resetKeyTo);
                                inputButtonSO.OverrideMouseKey((int) resetMouseTo);
                                PlayerPrefs.SetInt("TwoBitMachinesType" + inputButtonSO.buttonName, (int) resetTypeTo);
                        }
                        UpdateDisplay();
                }

                public void Update ()
                {
                        if (!binding || inputButtonSO == null)
                                return;

                        if (Input.GetKeyDown(KeyCode.Escape))
                        {
                                binding = false;
                                rebindStopEvent.Invoke();
                                return;
                        }

                        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
                        {
                                for (int i = 0; i < 3; i++)
                                {
                                        if (Input.GetMouseButtonDown(i))
                                        {
                                                inputButtonSO.OverrideMouseKey(i);
                                                PlayerPrefs.SetInt("TwoBitMachinesType" + inputButtonSO.buttonName, (int) InputType.Mouse);
                                                inputButtonSO.type = InputType.Mouse;
                                                rebindStopEvent.Invoke();
                                                UpdateDisplay();
                                                binding = false;
                                                return;
                                        }
                                }
                        }
                        else if (Input.anyKeyDown)
                        {
                                foreach (KeyCode keyCode in System.Enum.GetValues(typeof(KeyCode)))
                                {
                                        if (Input.GetKeyDown(keyCode) && keyCode != KeyCode.Escape)
                                        {
                                                inputButtonSO.OverrideKeyboardKey(keyCode);
                                                PlayerPrefs.SetInt("TwoBitMachinesType" + inputButtonSO.buttonName, (int) InputType.Keyboard);
                                                inputButtonSO.type = InputType.Keyboard;
                                                rebindStopEvent.Invoke();
                                                UpdateDisplay();
                                                binding = false;
                                                return;
                                        }
                                }
                        }
                }

        }

        [System.Serializable]
        public class UpdateButtonBinding : UnityEvent<GameObject, string> { }
}
