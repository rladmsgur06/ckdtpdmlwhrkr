using System.Collections.Generic;
using UnityEngine;

namespace TwoBitMachines.FlareEngine
{
        [System.Serializable]
        public class UserInputs
        {
                [SerializeField] public List<InputButtonSO> inputSO = new List<InputButtonSO>();

                [System.NonSerialized] public Dictionary<string, InputButtonSO> inputs = new Dictionary<string, InputButtonSO>();
                [System.NonSerialized] public bool block = false;
                [System.NonSerialized] public bool previousBlock = false;

                public void Initialize ()
                {
                        for (int i = 0; i < inputSO.Count; i++)
                        {
                                if (inputSO[i] == null)
                                        continue;
                                WorldManager.RegisterInput(inputSO[i]);
                                if (!inputs.ContainsKey(inputSO[i].buttonName))
                                {
                                        inputs.Add(inputSO[i].buttonName, inputSO[i]);
                                }
                        }
                }

                public bool Holding (string name)
                {
                        if (!block && inputs.TryGetValue(name, out InputButtonSO button))
                        {
                                return button.Holding();
                        }
                        return false;
                }

                public bool Pressed (string name)
                {
                        if (!block && inputs.TryGetValue(name, out InputButtonSO button))
                        {
                                return button.Pressed();
                        }
                        return false;
                }

                public bool PressedUnblocked (string name)
                {
                        if (inputs.TryGetValue(name, out InputButtonSO button))
                        {
                                return button.Pressed();
                        }
                        return false;
                }

                public bool Released (string name)
                {
                        if (!block && inputs.TryGetValue(name, out InputButtonSO button))
                        {
                                return button.Released();
                        }
                        return false;
                }

                public float Value (string name)
                {
                        if (!block && inputs.TryGetValue(name, out InputButtonSO button))
                        {
                                return button.value;
                        }
                        return 0;
                }

                public bool Active (string name)
                {
                        if (!block && inputs.TryGetValue(name, out InputButtonSO button))
                        {
                                return button.Active();
                        }
                        return false;
                }

                public bool Active (ButtonTrigger buttonTrigger, string buttonName)
                {
                        if (!block && inputs.TryGetValue(buttonName, out InputButtonSO button))
                        {
                                return button.Active(buttonTrigger);
                        }
                        return false;
                }

                public bool Exists (string name)
                {
                        return inputs.ContainsKey(name);
                }

                public void RememberBlock (bool value)
                {
                        if (value)
                        {
                                previousBlock = block;
                                block = true;
                        }
                        else
                        {
                                block = previousBlock;
                                previousBlock = false;
                        }

                }

                public void Block (bool value)
                {
                        block = value;
                        previousBlock = false;
                }

                public void ClearInputs ()
                {
                        for (int i = 0; i < inputSO.Count; i++)
                        {
                                if (inputSO[i] == null)
                                        continue;
                                inputSO[i].DeleteSavedValues();
                        }
                }

        }

}
