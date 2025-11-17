using System;
using UnityEngine;

namespace TwoBitMachines.Safire2DCamera
{
        [System.Serializable]
        public static class Tooly
        {

                public static Vector3 SetPosition (float horizontal, float vertical, Vector3 original)
                {
                        return new Vector3(horizontal, vertical, original.z);
                }

                public static void SetPosition (float horizontal, float vertical, Transform original)
                {
                        original.position = new Vector3(horizontal, vertical, original.position.z);
                }

                public static void SetPosition (Vector2 position, Transform original)
                {
                        original.position = new Vector3(position.x, position.y, original.position.z);
                }

                public static Vector3 SetPosition (Vector3 position, Vector3 original)
                {

                        return new Vector3(position.x, position.y, original.z);
                }

                public static Vector3 SetPosition (Vector3 position, Transform original)
                {
                        return new Vector3(position.x, position.y, original.position.z);
                }

                public static Vector3 SetDepth (Vector3 position, float depth)
                {
                        return new Vector3(position.x, position.y, depth);
                }

        }

        [System.Serializable]
        public static class Cammy
        {
                public static float playerDepth;

                public static float Left (this Camera camera)
                {

                        return camera.transform.position.x - Width(camera);
                }

                public static float Right (this Camera camera)
                {

                        return camera.transform.position.x + Width(camera);
                }

                public static float Top (this Camera camera)
                {
                        return camera.transform.position.y + Height(camera);
                }

                public static float Bottom (this Camera camera)
                {
                        return camera.transform.position.y - Height(camera);
                }

                public static float Width (this Camera camera)
                {
                        return camera.orthographic ? camera.orthographicSize * camera.aspect : camera.Height() * camera.aspect;
                }

                public static float Height (this Camera camera)
                {
                        return camera.orthographic ? camera.orthographicSize : Mathf.Abs(playerDepth - camera.transform.position.z) * Mathf.Tan(camera.fieldOfView * 0.5f * Mathf.Deg2Rad);
                }

                public static Vector2 Size (this Camera camera)
                {
                        return new Vector2(camera.Width(), camera.Height());
                }

                public static float ShortestLength (this Camera camera)
                {
                        Vector2 size = camera.Size();
                        return size.x < size.y ? size.x : size.y;
                }
        }

}
