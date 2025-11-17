using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TwoBitMachines.FlareEngine.Interactables
{
        [Serializable]
        public class WaterBatch
        {
                [SerializeField] private float crestTaper = 0;
                [SerializeField] private float crestThickness = 0.5f;
                [SerializeField] private float bottomOffset = 1f;
                [SerializeField] private float bottomSpeed = 1;
                [SerializeField] private int phaseShift = 15;
                [SerializeField] private Color crestColor = new Color (250f / 255f, 250f / 255f, 250f / 255f, 240f / 255f);
                [SerializeField] private Color bodyColor = new Color (16f / 255f, 250f / 255f, 255f / 255f, 240f / 255f);
                [SerializeField] private Color bottomColor = new Color (0f / 255f, 219f / 255f, 238f / 255f, 240f / 255f);

                [SerializeField] private Mesh mesh;
                [SerializeField] private Material material;
                [SerializeField] private List<Vector4> colors = new List<Vector4> ( );
                [SerializeField] private List<WaterWave> waves = new List<WaterWave> ( );
                [SerializeField] private List<Matrix4x4> tempData = new List<Matrix4x4> ( );

                private MaterialPropertyBlock propertyBlock; // not serializable
                public static List<WaterBatch> waterBatch = new List<WaterBatch> ( );
                public bool propertyBlockNull => propertyBlock == null;
                public bool canRender => colors != null && colors.Count > 0 && waves != null && waves.Count > 0;

                public void Create (int particles, float particleLength, float bottomY, Point[] point)
                {
                        waves.Clear ( );
                        mesh = QuadMesh.Create ( );

                        if (material == null)
                        {
                                // string path = "Assets/TwoBitMachines/FlareEngine/AssetsFolder/Materials/Water.mat";
                                //(Material) AssetDatabase.LoadAssetAtPath (path, typeof (Material));
                                material = MonoBehaviour.Instantiate (Resources.Load ("Water") as Material);
                                if (material != null) material.mainTexture = Texture2D.whiteTexture;
                                if (material != null) material.enableInstancing = true;
                        }
                        colors = new List<Vector4> ( );

                        AddWave (particles, particleLength, bottomY, point, bodyColor, colors);
                        AddWave (particles, particleLength, bottomY, point, bottomColor, colors);
                        AddWave (particles, particleLength, bottomY, point, crestColor, colors);
                        propertyBlock = new MaterialPropertyBlock ( );
                        propertyBlock.SetVectorArray ("_Color", colors);
                }

                private void AddWave (int particles, float particleLength, float bottomY, Point[] point, Color color, List<Vector4> colors)
                {
                        for (int i = 0; i < particles; i++)
                        {
                                WaterWave waveParticle = new WaterWave ( ) { scale = new Vector3 (particleLength, 1, 1), position = new Vector3 (point[i].x, bottomY, 0), bottomY = bottomY };
                                colors.Add (color);
                                waves.Add (waveParticle);
                        }
                }

                public void OnEnable ( )
                {
                        if (!waterBatch.Contains (this))
                        {
                                waterBatch.Add (this);
                        }
                }

                public void OnDisable ( )
                {
                        if (waterBatch.Contains (this))
                        {
                                waterBatch.Remove (this);
                        }
                }

                public void CreatePropertyBlock ( )
                {
                        propertyBlock = new MaterialPropertyBlock ( );
                        propertyBlock.SetVectorArray ("_Color", colors);
                }

                public static void Run ( )
                {
                        if (!SystemInfo.supportsInstancing)
                        {
                                return;
                        }

                        for (int i = waterBatch.Count - 1; i >= 0; i--)
                        {
                                if (waterBatch[i] != null)
                                {
                                        waterBatch[i].BatchAndRender ( );
                                }
                        }
                }

                public void BatchAndRender ( )
                {
                        if (mesh == null || material == null)
                        {
                                return;
                        }

                        int count = waves.Count;
                        for (int i = 0; i < count; i += 1023)
                        {
                                tempData.Clear ( );
                                if (i + 1023 < count)
                                {
                                        for (int ii = 0; ii < 1023; ii++)
                                        {
                                                tempData.Add (waves[i + ii].TransformData ( ));
                                        }
                                        Graphics.DrawMeshInstanced (mesh, 0, material, tempData, propertyBlock);
                                }
                                else //  Last batch
                                {
                                        for (int ii = 0; ii < count - i; ii++)
                                        {
                                                tempData.Add (waves[i + ii].TransformData ( ));
                                        }
                                        Graphics.DrawMeshInstanced (mesh, 0, material, tempData, propertyBlock); // Graphics.DrawMeshInstaNced does not work in openGl2
                                }
                        }
                }

                public void SetWaterBatchHeights (WaveProperties wave, float topY, float height, float phase)
                {
                        float amplitude = wave.amplitude;
                        float bottomTopY = topY - amplitude;
                        float baseLine = topY - height;
                        int count = waves.Count / 3;

                        for (int i = 0; i < count; i++)
                        {
                                WaterWave body = waves[i];
                                body.scale = new Vector2 (body.scale.x, Mathf.Abs (wave.wave[i].y - body.bottomY));

                                WaterWave crest = waves[count * 2 + i];
                                crest.position = body.position + Vector3.up * body.scale.y;
                                float percent = amplitude <= 0 ? 0 : Mathf.Clamp ((wave.wave[i].y - bottomTopY) / (amplitude * 2f), 1f - crestTaper, 1f);
                                crest.scale = new Vector2 (body.scale.x, crestThickness * percent);

                                WaterWave bottom = waves[count + i];
                                float phaseY = topY + Compute.SineWave (phase * (i + phaseShift), bottomSpeed, wave.amplitude);
                                float newBaseY = wave.JoinWaves (topY, baseLine, phaseY, wave.dynamicWave[i].position.y);
                                float scaleY = Math.Abs (newBaseY - body.bottomY) - bottomOffset;
                                bottom.scale = new Vector2 (body.scale.x, scaleY < 0 ? 0 : scaleY);
                        }
                }
        }

        [Serializable]
        public class WaterWave
        {
                [SerializeField] public float bottomY;
                [SerializeField] public Vector3 scale;
                [SerializeField] public Vector3 position;

                public Matrix4x4 TransformData ( )
                {
                        return Matrix4x4.TRS (position, Quaternion.identity, scale);
                }
        }
}