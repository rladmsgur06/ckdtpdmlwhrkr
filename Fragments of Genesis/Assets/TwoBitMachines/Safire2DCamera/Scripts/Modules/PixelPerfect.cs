#if UNITY_EDITOR
using TwoBitMachines.Editors;
using UnityEditor;
#endif
using UnityEngine;

namespace TwoBitMachines.Safire2DCamera
{
        [ExecuteInEditMode]
        public class PixelPerfect : MonoBehaviour
        {
                [SerializeField] public int PPU = 8;
                [SerializeField] public Color color = Color.black;
                [SerializeField] public Vector2Int resolution = new Vector2Int(320, 180);

                [SerializeField, HideInInspector] private float originSize = 11f;
                [SerializeField, HideInInspector] private Camera cameraRef;

                [System.NonSerialized] public float zoomScale = 1f; // set externally
                [System.NonSerialized] private int scale = 1;
                public int scaledWidth => resolution.x * scale;
                public int scaledHeight => resolution.y * scale;

                private void Awake ()
                {
                        zoomScale = 1f;
                        if (cameraRef == null)
                        {
                                cameraRef = this.gameObject.GetComponent<Camera>();
                                originSize = cameraRef.orthographicSize;
                        }
                        SetPixelCameraView(cameraRef);
                }

                private void SetPixelCameraView (Camera camera)
                {
                        if (camera == null)
                                return;

                        int yScale = Screen.height / resolution.y;
                        int xScale = Screen.width / resolution.x;
                        scale = Mathf.Max(1, Mathf.Min(yScale, xScale));
                        float x = (Screen.width - scaledWidth) * 0.5f;
                        float y = (Screen.height - scaledHeight) * 0.5f;
                        camera.pixelRect = new Rect(x, y, scaledWidth, scaledHeight);
                        camera.orthographicSize = Height(); //resolution.y * zoomScale) / (PPU * 2f);
                }

                public void SnapToPixelGrid ()
                {
                        float pixelGrid = 1f / (PPU * scale);
                        transform.position = Compute.Round(transform.position, pixelGrid);
                }

                public float SnapToPixelGrid (float value)
                {
                        float pixelGrid = 1f / (PPU * scale);
                        return Compute.Round(value, pixelGrid);
                }

                public float Height ()
                {
                        return SnapToPixelGrid((resolution.y * zoomScale) / (PPU * 2f));
                }

                public void Execute () // make sure this updates as the last script
                {
                        SetPixelCameraView(cameraRef);
                        SnapToPixelGrid();
                }

                private void OnPreRender ()
                {
                        GL.Clear(false, true, color);
                }

                private void OnRenderImage (RenderTexture source, RenderTexture destination)
                {
                        RenderTexture tempTexture = RenderTexture.GetTemporary(scaledWidth, scaledHeight);
                        tempTexture.filterMode = FilterMode.Point;
                        source.filterMode = FilterMode.Point;
                        Graphics.Blit(source, tempTexture);
                        Graphics.Blit(tempTexture, destination);
                        RenderTexture.ReleaseTemporary(tempTexture);
                }

                private void OnDisable ()
                {
                        if (cameraRef == null)
                                return;
                        cameraRef.rect = new Rect(0, 0, 1f, 1f);
                        cameraRef.orthographicSize = originSize;
                        cameraRef.ResetAspect();
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                public static void CustomInspector (SerializedObject parent, Color barColor, Color labelColor)
                {
                        parent.Update();
                        {
                                FoldOut.Box(2, Tint.Box, 0);
                                Fields.Start(parent, "PPU").F("PPU", S.H).F("color", S.Q).B("Wrench", S.Q, out bool pressed);
                                if (pressed)
                                        PixelPerfect.RoundAllObjects(parent.Int("PPU"));
                                parent.Field("Resolution", "resolution");
                                Layout.VerticalSpacing(5);
                        }
                        parent.ApplyModifiedProperties();
                }

                private void Update ()
                {
                        if (!EditorApplication.isPlaying)
                        {
                                Execute();
                                this.hideFlags = HideFlags.HideInInspector;
                        }

                }
                public static void RoundAllObjects (float PPU)
                {
                        Object[] list = Resources.FindObjectsOfTypeAll(typeof(GameObject));
                        float pixelGrid = 1f / PPU;
                        for (int i = 0; i < list.Length; i++)
                        {
                                GameObject o = (GameObject) list[i];
                                Vector3 newPosition = Round(o.transform.position, pixelGrid);
                                o.transform.position = Tooly.SetPosition(newPosition.x, newPosition.y, o.transform.position);
                        }
                        Debug.Log("Total objects snapped: " + list.Length);
                }

                private static Vector3 Round (Vector3 position, float pixelGrid)
                {
                        position.x = Mathf.RoundToInt(position.x / pixelGrid) * pixelGrid;
                        position.y = Mathf.RoundToInt(position.y / pixelGrid) * pixelGrid;
                        position.z = Mathf.RoundToInt(position.z / pixelGrid) * pixelGrid;
                        return position;
                }
#pragma warning restore 0414
#endif
                #endregion
        }

}
