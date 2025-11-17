using UnityEngine;

namespace TwoBitMachines
{
        public static class QuadMesh
        {
                private static int[] tris = new int[6] { 0, 2, 1, 2, 3, 1 };

                private static Vector2[] uv = new Vector2[4]
                {
                        new Vector2 (0, 0),
                        new Vector2 (1, 0),
                        new Vector2 (0, 1),
                        new Vector2 (1, 1)
                };

                private static Vector3[] vertices = new Vector3[4]
                {
                        new Vector3 (0, 0, 0),
                        new Vector3 (1, 0, 0),
                        new Vector3 (0, 1, 0),
                        new Vector3 (1, 1, 0)
                };

                private static Vector3[] normals = new Vector3[4]
                {
                        -Vector3.forward, -Vector3.forward, -Vector3.forward, -Vector3.forward
                };

                public static Mesh Create ()
                {
                        Mesh mesh = new Mesh();
                        mesh.vertices = vertices;
                        mesh.triangles = tris;
                        mesh.normals = normals;
                        mesh.uv = uv;
                        return mesh;
                }

                public static void Create (Mesh mesh)
                {
                        mesh.vertices = vertices;
                        mesh.triangles = tris;
                        mesh.normals = normals;
                        mesh.uv = uv;
                }

                public static Mesh Create (float sizex, float sizey)
                {
                        Mesh mesh = new Mesh();
                        Vector3[] v = new Vector3[4]
                        {
                                new Vector3 (0, 0),
                                new Vector3 (sizex, 0),
                                new Vector3 (0, sizey),
                                new Vector3 (sizex, sizey)
                        };
                        mesh.vertices = v;
                        mesh.triangles = tris;
                        mesh.normals = normals;
                        mesh.uv = uv;
                        return mesh;
                }

        }

}
