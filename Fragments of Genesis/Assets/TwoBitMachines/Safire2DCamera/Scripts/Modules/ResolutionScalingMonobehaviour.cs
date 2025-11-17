
using UnityEngine;

namespace TwoBitMachines.Safire2DCamera
{
        [ExecuteInEditMode]
        public class ResolutionScalingMonobehaviour : MonoBehaviour
        {
                [SerializeField, HideInInspector] public ResolutionType type;
                [SerializeField, HideInInspector] public Color color = Color.black;
                [SerializeField, HideInInspector] public float targetPPU = 8f;
                [SerializeField, HideInInspector] public float targetFOV = 100f;
                [SerializeField, HideInInspector] public Vector2 resolution = new Vector2(320f, 180f);
                [SerializeField, HideInInspector] public ResolutionScaling resolutionRef;
                [SerializeField, HideInInspector] public Camera cam;

                [System.NonSerialized] private bool letterbox;
                [System.NonSerialized] private bool isOrtho;
                [System.NonSerialized] private float targetPPURef;
                [System.NonSerialized] private float targetFOVRef;
                [System.NonSerialized] private Vector2 screenSize;
                [System.NonSerialized] private Vector2 resolutionSize;
                [System.NonSerialized] private Color colorRef;
                [System.NonSerialized] private Texture2D border;
                [System.NonSerialized] private ResolutionType typeRef;

                public enum ResolutionType
                {
                        AspectRatio,
                        Width,
                        Height
                }

                public void Update ()
                {
                        Execute();
                }

                public void Execute (bool enable = true)
                {
                        if (cam == null || (resolutionRef != null && !resolutionRef.enable))
                        {
                                return;
                        }
                        if (screenSize.x == Screen.width && screenSize.y == Screen.height && type == typeRef && isOrtho == cam.orthographic)
                        {
                                if (resolutionSize.x == resolution.x && resolutionSize.y == resolution.y && targetFOVRef == targetFOV && targetPPURef == targetPPU)
                                {
                                        return;
                                }
                        }

                        typeRef = type;
                        targetPPURef = targetPPU;
                        targetFOVRef = targetFOV;
                        isOrtho = cam.orthographic;
                        resolutionSize = resolution;
                        cam.rect = new Rect(0, 0, 1f, 1f);
                        screenSize = new Vector2(Screen.width, Screen.height);

                        float targetAspect = resolution.x / resolution.y;
                        float currentAspect = screenSize.x / screenSize.y;

                        if (type == ResolutionType.AspectRatio)
                        {
                                if (cam.orthographic)
                                {
                                        cam.orthographicSize = resolution.y * 0.5f / targetPPU;
                                }
                                else
                                {
                                        float distance = Mathf.Abs(cam.transform.position.z);
                                        float height = Mathf.Tan(targetFOV * 0.5f * Mathf.Deg2Rad) * distance;
                                        cam.fieldOfView = 2.0f * Mathf.Atan(height / distance) * Mathf.Rad2Deg;
                                }

                                Rect rect = new Rect(0, 0, 1f, 1f);
                                float scaleheight = currentAspect / targetAspect;
                                if (scaleheight < 1f)
                                {
                                        letterbox = true; // letterbox
                                        rect.height = scaleheight;
                                        rect.y = (1f - scaleheight) * 0.5f;
                                }
                                else
                                {
                                        letterbox = false; // pillarbox
                                        float scalewidth = 1f / scaleheight;
                                        rect.x = (1f - scalewidth) * 0.5f;
                                        rect.width = scalewidth;
                                }
                                cam.rect = rect;
                        }
                        else if (type == ResolutionType.Width)
                        {
                                if (cam.orthographic)
                                {
                                        float desiredHeight = resolution.x / currentAspect;
                                        cam.orthographicSize = desiredHeight * 0.5f / targetPPU;
                                }
                                else
                                {
                                        float distance = Mathf.Abs(cam.transform.position.z);
                                        float height = distance * Mathf.Tan(targetFOV * 0.5f * Mathf.Deg2Rad);
                                        cam.fieldOfView = 2.0f * Mathf.Atan((height * (targetAspect / currentAspect)) / distance) * Mathf.Rad2Deg;
                                }
                        }
                        else if (type == ResolutionType.Height)
                        {
                                if (cam.orthographic)
                                {
                                        cam.orthographicSize = resolution.y * 0.5f / targetPPU;
                                }
                                else
                                {
                                        float distance = Mathf.Abs(cam.transform.position.z);
                                        float height = Mathf.Tan(targetFOV * 0.5f * Mathf.Deg2Rad) * distance;
                                        cam.fieldOfView = 2.0f * Mathf.Atan(height / distance) * Mathf.Rad2Deg;
                                }
                        }

                }

                private void LateUpdate ()
                {
                        if (cam == null || type != ResolutionType.AspectRatio)
                        {
                                return;
                        }

                        if (border == null || color != colorRef)
                        {
                                border = new Texture2D(1, 1, TextureFormat.RGB24, false);
                                border.SetPixel(1, 1, (Color) color);
                                border.Apply();
                                colorRef = color;
                        }

                        GUI.depth = 10;
                        if (!letterbox)
                        {
                                GUI.DrawTexture(new Rect(0, 0, cam.pixelRect.x, cam.pixelRect.height), border);
                                GUI.DrawTexture(new Rect(cam.pixelRect.width + cam.pixelRect.x, 0, cam.pixelRect.width, cam.pixelRect.height), border);
                        }
                        else
                        {
                                GUI.DrawTexture(new Rect(0, 0, cam.pixelRect.width, cam.pixelRect.y), border);
                                GUI.DrawTexture(new Rect(0, Screen.height - cam.pixelRect.y, cam.pixelRect.width, cam.pixelRect.y), border);
                        }
                }
        }
}
