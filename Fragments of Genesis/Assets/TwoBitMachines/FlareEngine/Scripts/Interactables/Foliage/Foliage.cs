using System.Collections.Generic;
using UnityEngine;

namespace TwoBitMachines.FlareEngine.Interactables
{
        [AddComponentMenu("Flare Engine/一Interactables/Foliage")]
        public class Foliage : MonoBehaviour
        {
                [SerializeField] public FoliageJobSetup foliage = new FoliageJobSetup();
                [SerializeField] public FoliageTextureArray textureArray = new FoliageTextureArray();
                [System.NonSerialized] public static List<Foliage> foliages = new List<Foliage>();

                #region ▀▄▀▄▀▄ Editor Variables ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField] private int index = 0;
                [SerializeField] private bool foldOut;
                [SerializeField] private bool foldOutPaint;
                [SerializeField] private bool createTexture;
                [SerializeField] private float deleteRadius = 1;
                [SerializeField] private float randomDensity = 1;
                [SerializeField] public FoliageBrush brushes = 0;

                public void DrawMeshes (UnityEditor.SceneView sceneView)
                {
                        if (Application.isPlaying)
                        {
                                return;
                        }
                        if (textureArray.property == null)
                        {
                                textureArray.CreatePropertyBlock();
                        }
                        if (textureArray.property != null && textureArray.mesh != null)
                        {
                                textureArray.DrawMeshes();
                        }
                }

                public void DrawMeshesOnPause (UnityEditor.SceneView sceneView)
                {
                        if (textureArray.property == null)
                        {
                                textureArray.CreatePropertyBlock();
                        }
                        if (textureArray.property != null && textureArray.mesh != null)
                        {
                                textureArray.DrawMeshes();
                        }
                }
#pragma warning restore 0414
#endif
                #endregion

                public void Create ()
                {
                        if (textureArray.textures.Count == 0)
                        {
                                textureArray.ClearDataLists();
                        }
                        if (textureArray.GroupTextures())
                        {
                                textureArray.SetInstanceDataArrays();
                                textureArray.CreatePropertyBlock();
                                textureArray.DrawMeshes();
                        }
                }

                private void Start ()
                {
                        textureArray.Initialize();
                        foliage.InitializeJob(this, textureArray.foliage, textureArray.textures);
                }

                private void OnEnable ()
                {
                        if (!foliages.Contains(this))
                        {
                                foliages.Add(this);
                        }
                }
                private void OnDisable ()
                {
                        if (foliages.Contains(this))
                        {
                                foliages.Remove(this);
                        }
                }

                public static void RunFoliage ()
                {
                        for (int i = 0; i < foliages.Count; i++)
                        {
                                if (foliages[i].enabled && foliages[i].gameObject.activeInHierarchy)
                                {
                                        foliages[i].Execute();
                                }
                        }
                }

                public static void LateUpdateFoliage ()
                {
                        for (int i = 0; i < foliages.Count; i++)
                        {
                                foliages[i].foliage.CompleteJob(foliages[i]);
                        }
                }

                public static void DrawFoliage ()
                {
                        for (int i = 0; i < foliages.Count; i++)
                        {
                                foliages[i].textureArray.DrawMeshes();
                        }
                }

                private void Execute () // must execute after all characters have been updated
                {
                        if (Time.deltaTime != 0)
                        {
                                foliage.CreateJob(this);
                        }
                }

                private void OnDestroy ()
                {
                        foliage.OnDestroy();
                }
        }

}
