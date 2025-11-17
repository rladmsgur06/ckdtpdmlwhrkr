using System.Collections.Generic;
using UnityEngine;

namespace TwoBitMachines.MapSystem
{
        [System.Serializable]
        public class MeshData
        {
                [SerializeField] public ushort shapeOffset = 0;
                [SerializeField] public List<ushort> triangles = new List<ushort>();
                [SerializeField] public List<Color32> colors = new List<Color32>();
                [SerializeField] public List<Vector3> vertices = new List<Vector3>();

                public void Clear ()
                {
                        shapeOffset = 0;
                        colors.Clear();
                        vertices.Clear();
                        triangles.Clear();
                }

                public void Add (MeshData meshData)
                {
                        colors.AddRange(meshData.colors);
                        vertices.AddRange(meshData.vertices);
                        int count = meshData.triangles.Count;
                        for (int i = 0; i < count; i++)
                        {
                                triangles.Add((ushort) (shapeOffset + meshData.triangles[i]));
                        }
                        shapeOffset += (ushort) meshData.vertices.Count;
                }

                public void Add (MeshData meshData, Vector3 offset)
                {
                        int triangleCount = meshData.triangles.Count;
                        int verticeCount = meshData.vertices.Count;

                        colors.AddRange(meshData.colors);

                        for (int i = 0; i < verticeCount; i++)
                        {
                                vertices.Add(meshData.vertices[i] + offset);
                        }
                        for (int i = 0; i < triangleCount; i++)
                        {
                                triangles.Add((ushort) (shapeOffset + meshData.triangles[i]));
                        }
                        shapeOffset += (ushort) meshData.vertices.Count;
                }

                public void Set (Mesh mesh, MeshFilter meshFilter = null)
                {
                        if (mesh == null)
                        {
                                mesh = new Mesh();
                        }

                        mesh.Clear(); // avoid setting error
                        mesh.SetVertices(vertices, 0, vertices.Count, UnityEngine.Rendering.MeshUpdateFlags.DontRecalculateBounds);
                        mesh.SetTriangles(triangles, submesh: 0, calculateBounds: false);
                        mesh.SetColors(colors);

                        if (meshFilter != null)
                        {
                                // mesh.bounds = boundaryShape.Bounds();
                                meshFilter.mesh = mesh;
                        }
                }

                public void ChangeMeshColor (Color color)
                {
                        for (int i = 0; i < colors.Count; i++)
                        {
                                colors[i] = color;
                        }
                }

                public MeshData Copy (MeshData copyData)
                {
                        Clear();
                        vertices.AddRange(copyData.vertices);
                        triangles.AddRange(copyData.triangles);
                        colors.AddRange(copyData.colors);
                        return this;
                }

        }
}
