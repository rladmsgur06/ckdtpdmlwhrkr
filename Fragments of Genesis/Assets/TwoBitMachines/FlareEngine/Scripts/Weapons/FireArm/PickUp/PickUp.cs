using UnityEngine;

namespace TwoBitMachines.FlareEngine
{
        public class PickUp : MonoBehaviour
        {
                [System.NonSerialized] public Tool tool;
                public bool isSwapping => tool.pickUpType == Tool.PickUpType.SetAndSwap;

                public void OnTriggerEnter2D (Collider2D other)
                {
                        if (tool != null && Compute.ContainsLayer(tool.pickUpLayer , other.gameObject.layer))
                        {
                                Character character = other.gameObject.GetComponent<Character>();
                                if (character != null && (!isSwapping || Time.time > character.timeSwap))
                                {
                                        character.timeSwap = Time.time + 0.7f;
                                        SetTool(character);
                                }
                        }
                }

                private void SetTool (Character character)
                {
                        if (tool.pickUpType == Tool.PickUpType.Set)
                        {
                                character.RegisterToolAndSetAsChild(tool);
                                character.ActivateThisToolOnly(tool.toolName);
                                character.onToolSet.Invoke();
                        }
                        else if (tool.pickUpType == Tool.PickUpType.SetIdle)
                        {
                                character.RegisterToolAndSetAsChild(tool);
                                character.onToolSet.Invoke();
                                gameObject.SetActive(false);
                        }
                        if (tool.pickUpType == Tool.PickUpType.SetAndSwap)
                        {
                                GameObject currentTool = character.ReturnActiveTool();

                                if (currentTool != null)
                                {
                                        character.RemoveActiveTool();
                                        currentTool.transform.parent = null;
                                        currentTool.transform.eulerAngles = Vector3.zero;
                                        currentTool.transform.position = tool.transform.position;
                                }
                                character.RegisterToolAndSetAsChild(tool);
                                character.ActivateThisToolOnly(tool.toolName);
                                character.onToolSet.Invoke();
                        }
                }
        }
}
