using UnityEngine;

namespace TwoBitMachines.FlareEngine.Interactables
{
        [System.Serializable]
        public class WaterMesh
        {
                [SerializeField] private Texture2D texture2D;
                [SerializeField] private Material material;
                [SerializeField] private Vector3[] vertices;
                [SerializeField] private Mesh mesh;

                public bool readyToCreate => material != null;

                public void Create (Water waters, int length, float particleLength)
                {
                        vertices = new Vector3[length * 2];
                        Vector2[] uv = new Vector2[vertices.Length];

                        for (int index = 0, up = 0; up < 2; up++)
                        {
                                for (int x = 0; x < length; x++, index++)
                                {
                                        vertices[index] = Vector3.up * waters.size.y * up + Vector3.right * particleLength * x;
                                        uv[index] = new Vector2((float) x / waters.size.x, (float) up);
                                }
                        }

                        int[] triangles = new int[(length - 1) * 6];
                        for (int t = 0, x = 0; x < length - 1; x++, t += 6)
                        {
                                triangles[t + 0] = x;
                                triangles[t + 1] = triangles[t + 4] = length + x;
                                triangles[t + 2] = triangles[t + 3] = x + 1;
                                triangles[t + 5] = length + x + 1;
                        }

                        //* Generate mesh
                        MeshRenderer meshRenderer = waters.gameObject.GetComponent<MeshRenderer>();
                        MeshFilter meshFilter = waters.gameObject.GetComponent<MeshFilter>();

                        if (meshRenderer == null || meshFilter == null || material == null)
                        {
                                return;
                        }

                        meshRenderer.material = material;
                        if (texture2D != null)
                        {
                                material.SetTexture("_MainTex", texture2D);
                        }
                        mesh = new Mesh();
                        mesh.vertices = vertices;
                        mesh.triangles = triangles;
                        mesh.uv = uv;
                        mesh.RecalculateNormals();
                        mesh.RecalculateBounds();
                        meshFilter.mesh = mesh;
                }

                public void UpdateMeshVertices (WaveProperties wave, float topY, Vector2 size)
                {
                        if (mesh == null || vertices == null)
                        {
                                return;
                        }

                        int length = vertices.Length / 2;
                        for (int w = 0, i = length; i < vertices.Length; i++, w++)
                        {
                                vertices[i].y = size.y + (wave.wave[w].y - topY); //                   Size.y is acting as the local position for the top height
                        }
                        mesh.vertices = vertices;
                }

        }

}
