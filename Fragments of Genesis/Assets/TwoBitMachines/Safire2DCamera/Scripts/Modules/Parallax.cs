#if UNITY_EDITOR
using TwoBitMachines.Editors;
using UnityEditor;
#endif
using System.Collections.Generic;
using UnityEngine;

namespace TwoBitMachines.Safire2DCamera
{
        [System.Serializable]
        public class Parallax
        {
                [SerializeField] public bool enable;
                [SerializeField] public List<ParallaxLayer> parallax = new List<ParallaxLayer>();

                [System.NonSerialized] private Transform transform;
                [System.NonSerialized] private Camera mainCamera;
                [System.NonSerialized] private Vector3 previousCameraPosition;

                public void Initialize (Camera camera)
                {
                        this.mainCamera = camera;
                        this.transform = camera.transform;
                        for (int i = 0; i < parallax.Count; i++)
                        {
                                parallax[i].Create();
                        }
                        previousCameraPosition = transform.position;
                        Reset();
                }

                public void Reset ()
                {
                        for (int j = 0; j < 5; j++) // get parallax to target quicker on scene start or layers will lag behind for a few frames
                                for (int i = 0; i < parallax.Count; i++)
                                {
                                        parallax[i].Execute(mainCamera, 0);
                                }
                }

                public void Execute (Camera camera)
                {
                        if (!enable)
                                return;
                        Vector3 delta = transform.position - previousCameraPosition;
                        previousCameraPosition = transform.position;
                        for (int i = 0; i < parallax.Count; i++)
                        {
                                parallax[i].Execute(camera, delta.x);
                        }
                }

                public void RefreshParallaxImage (int index)
                {
                        for (int i = 0; i < parallax.Count; i++)
                        {
                                if (i == index)
                                {
                                        parallax[i].RefreshImages();
                                        break;
                                }
                        }
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField, HideInInspector] public int signalIndex;
                [SerializeField, HideInInspector] public bool foldOut;
                [SerializeField, HideInInspector] public bool active;
                [SerializeField, HideInInspector] public bool close;
                [SerializeField, HideInInspector] public bool edit;
                [SerializeField, HideInInspector] public bool add;
                public static void CustomInspector (SerializedProperty parent, Color barColor, Color labelColor)
                {
                        if (!parent.Bool("edit"))
                                return;

                        if (Follow.Open(parent, "Infinite Parallax", barColor, labelColor, true))
                        {
                                GUI.enabled = parent.Bool("enable");

                                SerializedProperty array = parent.Get("parallax");

                                if (parent.ReadBool("add"))
                                {
                                        array.arraySize++;
                                }

                                if (array.arraySize == 0)
                                        Layout.VerticalSpacing(5);

                                for (int i = 0; i < array.arraySize; i++)
                                {
                                        SerializedProperty element = array.Element(i);
                                        FoldOut.BoxSingle(1, Tint.Blue, 0);
                                        {
                                                Fields.ConstructField();
                                                Fields.ConstructSpace(15);
                                                element.ConstructField("transform", S.FW * 0.32f);
                                                element.ConstructField("parallaxRate", S.FW * 0.68f - S.B2 - 20f, 5f);
                                                if (Fields.ConstructButton("Delete"))
                                                { array.DeleteArrayElement(i); break; }
                                                if (Fields.ConstructButton("Reopen"))
                                                { element.Toggle("open"); }
                                                ListReorder.Grip(parent, array, Layout.GetLastRect(20, 20), i, Tint.WarmWhite, yOffset: 2);
                                        }
                                        Layout.VerticalSpacing(2);

                                        if (!element.Bool("open"))
                                        { continue; }

                                        FoldOut.Box(1, FoldOut.boxColor, offsetY: -2);
                                        {
                                                element.Field("Auto Scroll", "autoScroll");
                                        }
                                        Layout.VerticalSpacing(3);

                                }
                                GUI.enabled = true;
                        }
                }
#pragma warning restore 0414
#endif
                #endregion
        }

        [System.Serializable]
        public class ParallaxLayer
        {
                [SerializeField] public Transform transform;
                [SerializeField] public Vector2 parallaxRate;
                [SerializeField] public float autoScroll;
                [SerializeField, HideInInspector] private bool open;

                [System.NonSerialized] private GameObject extendRight;
                [System.NonSerialized] private GameObject extendLeft;
                [System.NonSerialized] private float previousCenter;
                [System.NonSerialized] private float startPositionX;
                [System.NonSerialized] private float startPositionY;
                [System.NonSerialized] private float length;
                [System.NonSerialized] private float scroll;

                public void Create ()
                {
                        if (transform == null)
                                return;

                        SpriteRenderer renderer = transform.GetComponent<SpriteRenderer>();
                        if (renderer == null)
                                return;

                        startPositionX = transform.position.x;
                        startPositionY = transform.position.y;

                        extendRight = MonoBehaviour.Instantiate(transform.gameObject, Vector3.zero, Quaternion.identity);
                        extendLeft = MonoBehaviour.Instantiate(transform.gameObject, Vector3.zero, Quaternion.identity);
                        extendRight.transform.parent = transform;
                        extendLeft.transform.parent = transform;
                        extendRight.transform.localPosition = Vector3.zero;
                        extendLeft.transform.localPosition = Vector3.zero;

                        length = renderer.bounds.size.x;
                        float realLength = length / transform.localScale.x;

                        SetLocalPositionX(extendRight.transform, realLength);
                        SetLocalPositionX(extendLeft.transform, -realLength);
                        scroll = 0;
                }
                //* foreground is negative numbers larger than 1, regular is between 0-1
                public void Execute (Camera camera, float deltaX)
                {
                        if (transform == null)
                                return;

                        if (camera.orthographic)
                        {
                                if (autoScroll != 0)
                                {
                                        scroll += Time.deltaTime * autoScroll + ((1 - parallaxRate.x) * -deltaX);
                                        scroll = scroll > length ? 0 : scroll < -length ? 0 : scroll;
                                }
                                float cameraX = camera.transform.position.x;
                                float distanceX = cameraX * parallaxRate.x;
                                SetPositionX(transform, startPositionX + distanceX + scroll);

                                float cameraY = camera.transform.position.y;
                                float distanceY = cameraY * parallaxRate.y;
                                SetPositionY(transform, startPositionY + distanceY);

                                float limit = cameraX * (1f - parallaxRate.x);
                                if (limit > (startPositionX + length + scroll)) //                   right
                                {
                                        startPositionX += length;
                                }
                                else if (limit < (startPositionX - length + scroll)) //              left
                                {
                                        startPositionX -= length;
                                }
                        }
                        else // 3D camera
                        {
                                scroll = autoScroll != 0 ? Time.deltaTime * autoScroll - deltaX : 0;
                                SetPositionX(transform, transform.position.x + scroll);
                                Vector2 currentPosition = transform.position;
                                float centerX = camera.transform.position.x;
                                float velX = (centerX - previousCenter);
                                float width = FrustumWidth(camera) * 0.5f;
                                float right = centerX + width;
                                float left = centerX - width;
                                previousCenter = centerX;

                                if ((velX > 0 || scroll < 0) && right > (currentPosition.x + length)) //    right
                                {
                                        SetPositionX(transform, currentPosition.x + length);
                                }
                                else if ((velX < 0 || scroll > 0) && left < (currentPosition.x - length)) // left
                                {
                                        SetPositionX(transform, currentPosition.x - length);
                                }
                        }

                }

                public void RefreshImages ()
                {
                        SpriteRenderer renderer = transform.GetComponent<SpriteRenderer>();
                        extendRight.GetComponent<SpriteRenderer>().sprite = renderer.sprite;
                        extendLeft.GetComponent<SpriteRenderer>().sprite = renderer.sprite;
                }

                private void SetPositionX (Transform transform, float positionX)
                {
                        transform.position = new Vector3(positionX, transform.position.y, transform.position.z);
                }

                private void SetPositionY (Transform transform, float positionY)
                {
                        transform.position = new Vector3(transform.position.x, positionY, transform.position.z);
                }

                private void SetLocalPositionX (Transform transform, float positionX)
                {
                        if (transform == null)
                                return;
                        transform.localPosition = new Vector3(positionX, transform.localPosition.y, transform.localPosition.z);
                }

                private void SetLocalPositionY (Transform transform, float positionY)
                {
                        if (transform == null)
                                return;
                        transform.localPosition = new Vector3(transform.localPosition.x, positionY, transform.localPosition.z);
                }

                private float Distance (Camera cam) => Mathf.Abs(transform.position.z - cam.transform.position.z);

                private float FrustumHeight (Camera cam) => 2f * Distance(cam) * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);

                private float FrustumWidth (Camera cam) => FrustumHeight(cam) * cam.aspect;
        }
}
