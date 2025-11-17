using System.Collections.Generic;
using UnityEngine;

namespace TwoBitMachines.MapSystem
{
        public static class MeshInfo
        {
                public static List<int> index = new List<int>();
                public static List<ushort> triangles = new List<ushort>();
                public static List<Color32> colors = new List<Color32>();
                public static List<Vector3> vertices = new List<Vector3>();

                public static void ClearTempLists ()
                {
                        index.Clear();
                        colors.Clear();
                        vertices.Clear();
                        triangles.Clear();
                }

                public static void AddMeshInfoTo (MeshData meshData, bool execute = true)
                {
                        if (!execute)
                                return;
                        meshData.vertices.AddRange(vertices);
                        meshData.triangles.AddRange(triangles);
                        meshData.colors.AddRange(colors);
                        meshData.shapeOffset += (ushort) vertices.Count;
                }

                public static void AddPoint (Vector2 point, Color color)
                {
                        vertices.Add(point);
                        colors.Add(color);
                }

                public static void AddTriangle (int baseIndex, int a, int b, int c)
                {
                        triangles.Add((ushort) (baseIndex + a));
                        triangles.Add((ushort) (baseIndex + b));
                        triangles.Add((ushort) (baseIndex + c));
                }
        }
}
