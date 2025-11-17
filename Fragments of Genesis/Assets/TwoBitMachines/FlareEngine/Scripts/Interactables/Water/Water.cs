using System.Collections.Generic;
using UnityEngine;

namespace TwoBitMachines.FlareEngine.Interactables
{
        [AddComponentMenu ("Flare Engine/一Interactables/Water")]
        public class Water : MonoBehaviour
        {
                [SerializeField] public WaterType type;
                [SerializeField] public SwimType swimType;
                [SerializeField] public SwitchSwimTypes canSwitch;
                [SerializeField] public int particles = 20;
                [SerializeField] public bool createOnAwake;

                [SerializeField] public Vector2 size = new Vector2 (20f, 5f);
                [SerializeField] public WaterMesh waterMesh = new WaterMesh ( );
                [SerializeField] public WaterBatch waterBatch = new WaterBatch ( );
                [SerializeField] public WaveProperties wave = new WaveProperties ( );

                [SerializeField] private bool isSquare;
                [SerializeField] private float topY;
                [SerializeField] private float bottomY;
                [SerializeField] private float particleLength;
                [SerializeField] private Vector2 topHeight;

                [System.NonSerialized] private Rect searchRect = new Rect ( );
                public static List<Water> water = new List<Water> ( );

                #region ▀▄▀▄▀▄ Editor Variables ▄▀▄▀▄▀
                #if UNITY_EDITOR
                #pragma warning disable 0414
                [SerializeField, HideInInspector] private bool foldOut = false;
                [SerializeField, HideInInspector] private bool bodyFoldOut = false;

                public void DrawBatches (UnityEditor.SceneView sceneView)
                {
                        if (!waterBatch.canRender)
                        {
                                return;
                        }
                        if (waterBatch.propertyBlockNull)
                        {
                                waterBatch.CreatePropertyBlock ( );
                        }
                        if (wave.wave != null && isSquare)
                        {
                                waterBatch.SetWaterBatchHeights (wave, topY, size.y, 0);
                                waterBatch.BatchAndRender ( );
                        }
                }
                #pragma warning restore 0414
                #endif
                #endregion

                public void CreateWaves ( )
                {
                        particles = particles < 1 ? 1 : particles;
                        particleLength = size.x == 0 ? 1f : size.x / particles;
                        int length = particles + 1;

                        wave.Create (this, length, particleLength);
                        topHeight = Vector2.up * (transform.position.y + size.y * 2f); // extend for searching
                        bottomY = transform.position.y;
                        topY = wave.dynamicWave[0].y;

                        if (type == WaterType.Square)
                        {
                                waterBatch.Create (length - 1, particleLength, bottomY, wave.dynamicWave);
                                isSquare = true;
                        }
                        else
                        {
                                waterMesh.Create (this, length, particleLength);
                                isSquare = false;
                        }
                }

                public static void ResetWaves ( )
                {
                        for (int i = 0; i < water.Count; i++)
                        {
                                if (water[i] != null) water[i].ResetAll ( );
                        }
                }
                private void Awake ( )
                {
                        if (createOnAwake)
                        {
                                CreateWaves ( );
                        }

                        searchRect.Set (transform.position.x, transform.position.y - 1f, size.x, size.y * 2f); // subtract 1 from bottom to detect player even if sitting at the bottom

                        if (isSquare)
                        {
                                waterBatch.CreatePropertyBlock ( );
                        }
                }
                private void OnEnable ( )
                {
                        if (!water.Contains (this))
                        {
                                water.Add (this);
                        }
                        if (isSquare)
                        {
                                waterBatch.OnEnable ( );
                        }
                }
                private void OnDisable ( )
                {
                        if (water.Contains (this))
                        {
                                water.Remove (this);
                        }
                        if (isSquare)
                        {
                                waterBatch.OnDisable ( );
                        }
                }
                private void FixedUpdate ( )
                {
                        float phase = wave.Execute (this, topY, size.y);

                        if (isSquare)
                        {
                                waterBatch.SetWaterBatchHeights (wave, topY, size.y, phase);
                        }
                        else
                        {
                                waterMesh.UpdateMeshVertices (wave, topY, size);
                        }
                }

                public void ResetAll ( )
                {
                        wave.Reset (topY);
                }

                public bool FoundWater (Vector2 entryPoint, bool wasInWater, ref Vector2 waveTopPoint, ref int index, bool force = false)
                {
                        if (!(wasInWater || force || searchRect.Contains (entryPoint))) //                               force is for AI (AI will always find the water)
                        {
                                return false;
                        }

                        entryPoint = wasInWater ? new Vector2 (entryPoint.x, bottomY) : entryPoint;
                        int near = Mathf.RoundToInt (Mathf.Abs (entryPoint.x - transform.position.x) / particleLength); //approximate landing point
                        near = Mathf.Clamp (near, 1, wave.wave.Length - 1);

                        for (index = near - 1; index < near + 1; index++)
                        {
                                if (index < 0 || index >= wave.wave.Length - 1)
                                {
                                        return false;
                                }
                                if (Compute.LineIntersection (entryPoint, new Vector2 (entryPoint.x, topHeight.y), wave.wave[index].position, wave.wave[index + 1].position, out Vector2 intersectPoint))
                                {
                                        waveTopPoint = intersectPoint;
                                        return true;
                                }
                        }
                        return false;
                }

                public void ApplyImpact (int index, float impact, int splashRange = 4)
                {
                        wave.ApplyImpact (index, impact, splashRange);
                }

        }
}

namespace TwoBitMachines.FlareEngine
{
        public enum WaterType
        {
                Square,
                Round
        }

        public enum SwimType
        {
                Float,
                Swim
        }

        public enum SwitchSwimTypes
        {
                No,
                Yes
        }
}