using UnityEngine;
using UnityEngine.Events;

namespace TwoBitMachines.FlareEngine
{
        [System.Serializable]
        public class Ammunition
        {
                [SerializeField] public AmmoType type = AmmoType.Infinite;
                [SerializeField] public float ammunition = 10f;
                [SerializeField] public float max = 100f;
                [SerializeField] public bool save = false;
                [SerializeField] public string saveKey = "";
                [SerializeField] public UnityEvent onAmmoEmpty;
                [SerializeField] public UnityEvent onAmmoReload;
                [SerializeField] private SaveFloat saveFloat = new SaveFloat();

                [System.NonSerialized] public float available = 0;

                #region ▀▄▀▄▀▄ Editor Variables ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField] private bool reloadFoldOut = false;
                [SerializeField] private bool emptyFoldOut = false;
#pragma warning restore 0414
#endif
                #endregion

                public bool Consume (int rate, ProjectileInventory inventory)
                {
                        if (type != AmmoType.Infinite)
                        {
                                if (ammunition <= 0)
                                {
                                        if (inventory != null)
                                                inventory.SetUI();
                                        return false;
                                }
                                else if (type == AmmoType.Discrete)
                                {
                                        available = Mathf.Min(ammunition, (float) rate);
                                        ammunition = Mathf.Clamp(ammunition - rate, 0, max);
                                        if (ammunition <= 0)
                                        {
                                                onAmmoEmpty.Invoke();
                                        }
                                }
                                else
                                {
                                        ammunition = Mathf.Clamp(ammunition - Time.deltaTime, 0, max);
                                        if (ammunition <= 0)
                                        {
                                                onAmmoEmpty.Invoke();
                                        }
                                }
                                if (inventory != null)
                                        inventory.SetUI();
                        }
                        else
                        {
                                available = rate;
                        }
                        return true;
                }

                public bool EnoughAmmo ()
                {
                        if (type == AmmoType.Infinite)
                        {
                                return true;
                        }
                        if (type == AmmoType.Discrete)
                        {
                                return ammunition > 0;
                        }
                        else
                        {
                                return ammunition > 0;
                        }
                }

                public void RestoreValue ()
                {
                        if (save)
                        {
                                saveFloat.value = ammunition;
                                ammunition = Storage.Load<SaveFloat>(saveFloat, WorldManager.saveFolder, saveKey).value;
                        }
                }

                public void Save ()
                {
                        if (save)
                        {
                                saveFloat.value = ammunition;
                                Storage.Save(saveFloat, WorldManager.saveFolder, saveKey);
                        }
                }

        }

}
