using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TwoBitMachines.FlareEngine
{
        [AddComponentMenu ("Flare Engine/一Weapons/ProjectileInventory")]
        public class ProjectileInventory : MonoBehaviour
        {
                [SerializeField] public List<ProjectileBase> list = new List<ProjectileBase> ( );
                [SerializeField] public bool enableUI;
                [SerializeField] public Image image;
                [SerializeField] public TextMeshProUGUI textMesh;
                [SerializeField] private ProjectileBase currentProjectile;

                #region ▀▄▀▄▀▄ Editor Variables ▄▀▄▀▄▀
                #if UNITY_EDITOR
                #pragma warning disable 0414
                [SerializeField] private bool add;
                [SerializeField] private bool foldOut;
                #pragma warning restore 0414
                #endif
                #endregion

                private void Awake ( )
                {
                        if (list.Count > 0) currentProjectile = list[0];
                        for (int i = 0; i < list.Count; i++) list[i].inventory = this;
                        SetUI ( );
                }

                public void SetUI ( )
                {
                        if (enableUI && currentProjectile != null)
                        {
                                if (image != null) image.fillAmount = currentProjectile.AmmoPercent ( );
                                if (textMesh != null) textMesh.text = String.Concat (currentProjectile.AmmoCount ( ).ToString ( ), " / ", currentProjectile.AmmoMax ( ).ToString ( ));
                        }
                }

                public ProjectileBase GetCurrentProjectile ( )
                {
                        return currentProjectile;
                }

                public ProjectileBase GetProjectile (string name)
                {
                        for (int i = 0; i < list.Count; i++)
                        {
                                if (list[i].projectileName == name)
                                {
                                        return list[i];
                                }
                        }
                        return null;
                }

                public bool SetProjectile (ProjectileBase projectile, string name)
                {
                        for (int i = 0; i < list.Count; i++)
                        {
                                if (list[i].projectileName == name)
                                {
                                        projectile = list[i];
                                        return true;
                                }
                        }
                        return false;
                }

                public bool ChangeProjectileAmmo (string name, float value)
                {
                        for (int i = 0; i < list.Count; i++)
                        {
                                if (list[i].projectileName == name)
                                {
                                        list[i].ChangeAmmo (value);
                                        return true;
                                }
                        }
                        return false;
                }

                public void ChangeProjectileAmmo (ItemEventData itemEventData)
                {
                        itemEventData.success = ChangeProjectileAmmo (itemEventData.genericString, itemEventData.genericFloat);
                }

        }
}