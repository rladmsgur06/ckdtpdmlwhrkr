using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace TwoBitMachines.FlareEngine
{
        public partial class Character
        {
                [SerializeField] public bool canUseTool = true;
                [SerializeField] public UnityEvent onToolSet;

                [System.NonSerialized] public float timeSwap;
                [System.NonSerialized] public ThePlayer.Melee melee;
                [System.NonSerialized] public List<Tool> tools = new List<Tool>();
                [HideInInspector] public Vector2 setVelocity;

                public void ActivateTool (string toolName)
                {
                        for (int i = 0; i < tools.Count; i++)
                        {
                                if (tools[i] != null && tools[i].gameObject != null && tools[i].toolName == toolName)
                                {
                                        tools[i].ResetAll();
                                        tools[i].gameObject.SetActive(true);
                                        return;
                                }
                        }
                }

                public GameObject ReturnActiveTool ()
                {
                        for (int i = 0; i < tools.Count; i++)
                        {
                                if (tools[i] != null && tools[i].gameObject != null && tools[i].gameObject.activeInHierarchy)
                                {
                                        return tools[i].gameObject;
                                }
                        }
                        return null;
                }

                public void RemoveActiveTool ()
                {
                        for (int i = 0; i < tools.Count; i++)
                        {
                                if (tools[i] != null && tools[i].gameObject != null && tools[i].gameObject.activeInHierarchy)
                                {
                                        RemoveTool(tools[i]);
                                        return;
                                }
                        }
                }

                public void ActivateThisToolOnly (string toolName)
                {
                        DeactivateAllTools();
                        ActivateTool(toolName);
                }

                public void DeactivateTool (string toolName)
                {
                        for (int i = 0; i < tools.Count; i++)
                        {
                                if (tools[i] != null && tools[i].gameObject != null && tools[i].toolName == toolName)
                                {
                                        tools[i].ResetAll();
                                        tools[i].gameObject.SetActive(false);
                                        return;
                                }
                        }
                }

                public bool EquipmentIsActive ()
                {
                        for (int i = 0; i < tools.Count; i++)
                        {
                                if (tools[i] != null && tools[i].gameObject != null && tools[i].gameObject.activeInHierarchy)
                                {
                                        return true;
                                }
                        }
                        return false;
                }

                public void RegisterTool (Tool tool)
                {
                        if (tool != null && !tools.Contains(tool))
                        {
                                tools.Add(tool);
                        }
                }

                public void RegisterToolAndSetAsChild (Tool tool)
                {
                        if (tool != null && !tools.Contains(tool))
                        {
                                tool.transform.parent = transform;
                                tools.Add(tool);
                        }
                }

                public void RemoveTool (Tool tool)
                {
                        tools.Remove(tool);
                }

                public void ResetAll ()
                {
                        for (int i = 0; i < tools.Count; i++)
                        {
                                if (tools[i] != null)
                                {
                                        tools[i].ResetAll();
                                }
                        }
                }

                public void ToggleTool (string toolName)
                {
                        for (int i = 0; i < tools.Count; i++)
                        {
                                if (tools[i] != null && tools[i].gameObject != null && tools[i].toolName == toolName)
                                {
                                        tools[i].ResetAll();
                                        tools[i].gameObject.SetActive(!tools[i].gameObject.activeInHierarchy);
                                        return;
                                }
                        }
                }

                public void ToggleThisToolOnly (string toolName)
                {
                        DeactivateAllToolsExcept(toolName);
                        ToggleTool(toolName);
                }

                public void DeactivateAllTools ()
                {
                        for (int i = 0; i < tools.Count; i++)
                        {
                                if (tools[i] != null && tools[i].gameObject != null)
                                {
                                        tools[i].ResetAll();
                                        tools[i].gameObject.SetActive(false);
                                }
                        }
                }

                public bool ToolIsActive (string toolName)
                {
                        for (int i = 0; i < tools.Count; i++)
                        {
                                if (tools[i] != null && tools[i].gameObject != null && tools[i].toolName == toolName)
                                {
                                        return tools[i].gameObject.activeInHierarchy;
                                }
                        }
                        return false;
                }

                public void DeactivateAllToolsExcept (string toolName)
                {
                        for (int i = 0; i < tools.Count; i++)
                        {
                                if (tools[i] != null && tools[i].gameObject != null && tools[i].toolName != toolName)
                                {
                                        tools[i].ResetAll();
                                        tools[i].gameObject.SetActive(false);
                                }
                        }
                }

                public bool ToggleOrActivateOnly (bool toggleTool, string toolName)
                {
                        if (toggleTool)
                        {
                                ToggleThisToolOnly(toolName);
                        }
                        else if (ToolIsActive(toolName)) // if already active, do not activate again or else it will reset its angle
                        {
                                DeactivateAllToolsExcept(toolName);
                        }
                        else
                        {
                                ActivateThisToolOnly(toolName);
                        }
                        return true;
                }

                public bool ToggleOrActivate (bool toggleTool, string toolName)
                {
                        if (toggleTool)
                        {
                                ToggleTool(toolName);
                        }
                        else if (!ToolIsActive(toolName)) // if already active, do not activate again or else it will reset its angle
                        {
                                ActivateTool(toolName);
                        }
                        return true;
                }

                public void ToggleOrActivateOnly (ItemEventData itemEventData)
                {
                        itemEventData.success = ToggleOrActivateOnly(itemEventData.toggle, itemEventData.genericString);
                }

                public void ToggleOrActivate (ItemEventData itemEventData)
                {
                        itemEventData.success = ToggleOrActivate(itemEventData.toggle, itemEventData.genericString);
                }

                public void ToggleOrActivateOnlyAndToggleMelee (ItemEventData itemEventData)
                {
                        itemEventData.success = ToggleOrActivateOnly(itemEventData.toggle, itemEventData.genericString);
                        if (melee == null)
                        {
                                melee = this.gameObject.GetComponent<TwoBitMachines.FlareEngine.ThePlayer.Melee>();
                        }
                        if (melee != null)
                        {
                                melee.Pause(EquipmentIsActive());
                        }
                }

                public void ToggleOrActivateAndToggleMelee (ItemEventData itemEventData)
                {
                        itemEventData.success = ToggleOrActivate(itemEventData.toggle, itemEventData.genericString);
                        if (melee == null)
                        {
                                melee = this.gameObject.GetComponent<TwoBitMachines.FlareEngine.ThePlayer.Melee>();
                        }
                        if (melee != null)
                        {
                                melee.Pause(EquipmentIsActive());
                        }
                }

                public void ChangeFirearmProjectile (ItemEventData itemEventData)
                {
                        for (int i = 0; i < tools.Count; i++)
                        {
                                if (tools[i] != null && tools[i].gameObject != null && tools[i].gameObject.activeInHierarchy)
                                {
                                        tools[i].ChangeFirearmProjectile(itemEventData);
                                        return;
                                }
                        }
                }

        }
}
