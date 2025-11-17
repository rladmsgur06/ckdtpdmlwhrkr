#if UNITY_EDITOR
using TwoBitMachines.Editors;
using UnityEditor;
#endif
using UnityEngine;

namespace TwoBitMachines.Safire2DCamera
{

        [System.Serializable]
        public class UserPan
        {
                [SerializeField] public bool enable;
                [SerializeField] public bool useEdge;
                [SerializeField] public bool useTouch;
                [SerializeField] public bool useMouse;
                [SerializeField] public bool useKeyboard;
                [SerializeField] public bool useInputButtonSO;

                [SerializeField] public float bottomEdge = 0;
                [SerializeField] public float rightEdge = 0;
                [SerializeField] public float leftEdge = 0;
                [SerializeField] public float topEdge = 0;
                [SerializeField] public float boost = 1;

                [SerializeField] public KeyCode right = KeyCode.D;
                [SerializeField] public KeyCode down = KeyCode.S;
                [SerializeField] public KeyCode left = KeyCode.A;
                [SerializeField] public KeyCode up = KeyCode.W;

                [SerializeField] public InputButtonSO inputLeft;
                [SerializeField] public InputButtonSO inputRight;
                [SerializeField] public InputButtonSO inputUp;
                [SerializeField] public InputButtonSO inputDown;

                [SerializeField] public Pan pan;
                [SerializeField] public PanTouch panTouch;
                [SerializeField] public MouseButton mouseButton = MouseButton.Left;
                [SerializeField] public MouseButton holdButton = MouseButton.Right;

                [System.NonSerialized] public bool pause;
                [System.NonSerialized] private bool usingDesktop;
                [System.NonSerialized] private Vector3 mousePosition;
                [System.NonSerialized] public Vector2 velocityTarget;
                [System.NonSerialized] private Vector2 velocityCurrent;
                private bool mouseActive => useMouse && usingDesktop;

                public void Initialize (Vector2 mouse)
                {
                        usingDesktop = SystemInfo.deviceType == DeviceType.Desktop;
                }

                public void Reset (Vector2 mouse)
                {
                        pause = false;
                }

                public Vector3 Velocity (Vector2 cameraPosition, Follow follow, bool UI)
                {
                        if (!enable || pause)
                                return Vector2.zero;

                        if (useKeyboard)
                        {
                                Vector2 speed = Vector2.one * follow.speed;
                                if (Input.GetKey(left))
                                        velocityTarget.x = -speed.x;
                                if (Input.GetKey(right))
                                        velocityTarget.x = speed.x;
                                if (Input.GetKey(down))
                                        velocityTarget.y = -speed.y;
                                if (Input.GetKey(up))
                                        velocityTarget.y = speed.y;
                        }
                        if (useInputButtonSO)
                        {
                                Vector2 speed = Vector2.one * follow.speed;
                                if (inputLeft != null && inputLeft.Holding())
                                        velocityTarget.x = -speed.x;
                                if (inputRight != null && inputRight.Holding())
                                        velocityTarget.x = speed.x;
                                if (inputDown != null && inputDown.Holding())
                                        velocityTarget.y = -speed.y;
                                if (inputUp != null && inputUp.Holding())
                                        velocityTarget.y = speed.y;
                        }
                        if (useTouch && Input.touchCount == 1 && !UI && InsideCamera(follow, cameraPosition, follow.Touch()))
                        {
                                if (panTouch == PanTouch.Click && Input.GetTouch(0).phase == TouchPhase.Began)
                                {
                                        velocityTarget = follow.Touch() - cameraPosition;
                                }
                                if (panTouch == PanTouch.Drag && Input.GetTouch(0).phase == TouchPhase.Moved)
                                {
                                        velocityTarget = -Input.GetTouch(0).deltaPosition * follow.speed;
                                }
                                if (useEdge)
                                {
                                        velocityTarget = EdgePan(follow, follow.zoom, cameraPosition, follow.Touch()); // this should return a velocity
                                }
                        }
                        else if (mouseActive && !UI && Input.touchCount == 0 && InsideCamera(follow, cameraPosition, follow.MousePosition()))
                        {
                                if (pan == Pan.Drag && Input.GetMouseButton((int) mouseButton))
                                {
                                        if (Input.GetMouseButtonDown((int) mouseButton))
                                        {
                                                mousePosition = Input.mousePosition;
                                                return Vector2.zero;
                                        }
                                        if (Input.GetMouseButton((int) mouseButton))
                                        {
                                                Vector2 mouseDelta = Input.mousePosition - mousePosition;
                                                mousePosition = Input.mousePosition;
                                                velocityTarget = Vector2.zero;
                                                Vector2 velocity;

                                                if (follow.camera.orthographic)
                                                {
                                                        // Orthographic camera
                                                        float orthoSize = follow.camera.orthographicSize;
                                                        float aspectRatio = follow.camera.aspect;
                                                        velocity = new Vector2(
                                                            mouseDelta.x / Screen.width * orthoSize * 2 * aspectRatio,
                                                            mouseDelta.y / Screen.height * orthoSize * 2
                                                        );
                                                }
                                                else
                                                {
                                                        // Perspective camera
                                                        float distance = follow.camera.transform.position.z;
                                                        Vector3 screenPoint1 = new Vector3(Screen.width / 2, Screen.height / 2, distance);
                                                        Vector3 screenPoint2 = new Vector3(Screen.width / 2 + mouseDelta.x, Screen.height / 2 + mouseDelta.y, distance);
                                                        Vector3 worldPoint1 = follow.camera.ScreenToWorldPoint(screenPoint1);
                                                        Vector3 worldPoint2 = follow.camera.ScreenToWorldPoint(screenPoint2);
                                                        velocity = new Vector2(worldPoint2.x - worldPoint1.x, worldPoint2.y - worldPoint1.y);
                                                }
                                                return -velocity;
                                        }
                                }
                                if (pan == Pan.Click && Input.GetMouseButtonDown((int) mouseButton))
                                {
                                        velocityTarget = follow.MousePosition() - cameraPosition;
                                }
                                if (useEdge)
                                {
                                        velocityTarget = EdgePan(follow, follow.zoom, cameraPosition, follow.MousePosition());
                                }
                        }

                        float x = follow.useUnscaledTime ? Compute.LerpUnscaled(0, velocityTarget.x, follow.smoothX) : Compute.Lerp(0, velocityTarget.x, follow.smoothX);
                        float y = follow.useUnscaledTime ? Compute.LerpUnscaled(0, velocityTarget.y, follow.smoothY) : Compute.Lerp(0, velocityTarget.y, follow.smoothY);
                        Vector2 lerp = new Vector2(x, y);
                        velocityTarget -= lerp;
                        return lerp;
                }

                private Vector2 EdgePan (Follow follow, Zoom zoom, Vector3 cameraPosition, Vector3 mousePosition)
                {
                        Vector2 velocity = Vector2.zero;
                        float cameraHeight = zoom.realTimeHeight;
                        float cameraWidth = zoom.realTimeWidth;
                        float topBorder = cameraPosition.y + cameraHeight * (1 - topEdge);
                        float leftBorder = cameraPosition.x - cameraWidth * (1 - leftEdge);
                        float rightBorder = cameraPosition.x + cameraWidth * (1 - rightEdge);
                        float bottomBorder = cameraPosition.y - cameraHeight * (1 - bottomEdge);

                        if (mousePosition.y > topBorder && topEdge > 0)
                                velocity.y = boost * follow.speed;
                        if (mousePosition.x < leftBorder && leftEdge > 0)
                                velocity.x = -boost * follow.speed;
                        if (mousePosition.x > rightBorder && rightEdge > 0)
                                velocity.x = boost * follow.speed;
                        if (mousePosition.y < bottomBorder && bottomEdge > 0)
                                velocity.y = -boost * follow.speed;
                        return velocity;
                }

                public bool InsideCamera (Follow follow, Vector3 cameraPosition, Vector3 target)
                {
                        float width = follow.zoom.realTimeWidth;
                        float height = follow.zoom.realTimeHeight;
                        return (target.x >= cameraPosition.x - width && target.x <= cameraPosition.x + width && target.y >= cameraPosition.y - height && target.y <= cameraPosition.y + height);
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField, HideInInspector] public bool edit;
                [SerializeField, HideInInspector] public bool close;
                [SerializeField, HideInInspector] public bool foldOut;
                [SerializeField, HideInInspector] public bool touchFoldOut;
                [SerializeField, HideInInspector] public bool mouseFoldOut;
                [SerializeField, HideInInspector] public bool edgeFoldOut;
                [SerializeField, HideInInspector] public bool keyboardFoldOut;
                [SerializeField, HideInInspector] public bool inputFoldOut;
                public static void CustomInspector (SerializedProperty parent, Color barColor, Color labelColor)
                {
                        if (!parent.Bool("edit"))
                                return;

                        if (Follow.Open(parent, "User Pan", barColor, labelColor))
                        {
                                GUI.enabled = parent.Bool("enable");
                                bool guiCurrent = GUI.enabled;

                                if (FoldOut.Bar(parent, FoldOut.boxColor).Label("Pan Touch", FoldOut.titleColor).BRE("useTouch").FoldOut("touchFoldOut"))
                                {
                                        GUI.enabled = parent.Bool("useTouch") && guiCurrent;
                                        FoldOut.Box(1, color: FoldOut.boxColor, offsetY: -2);
                                        parent.Field("Type", "panTouch");
                                        Layout.VerticalSpacing(3);
                                        GUI.enabled = guiCurrent;
                                }
                                if (FoldOut.Bar(parent, FoldOut.boxColor).Label("Pan Mouse", FoldOut.titleColor).BRE("useMouse").FoldOut("mouseFoldOut"))
                                {
                                        GUI.enabled = parent.Bool("useMouse") && guiCurrent;
                                        FoldOut.Box(2, color: FoldOut.boxColor, offsetY: -2);
                                        parent.Field("Type", "pan");
                                        parent.Field("Button", "mouseButton");
                                        Layout.VerticalSpacing(3);
                                        GUI.enabled = guiCurrent;
                                }
                                if (FoldOut.Bar(parent, FoldOut.boxColor).Label("Pan Keyboard", FoldOut.titleColor).BRE("useKeyboard").FoldOut("keyboardFoldOut"))
                                {
                                        GUI.enabled = parent.Bool("useKeyboard") && guiCurrent;
                                        FoldOut.Box(4, color: FoldOut.boxColor, offsetY: -2);
                                        parent.Field("Left", "left");
                                        parent.Field("Right", "right");
                                        parent.Field("Up", "up");
                                        parent.Field("Down", "down");
                                        Layout.VerticalSpacing(3);
                                        GUI.enabled = guiCurrent;
                                }
                                if (FoldOut.Bar(parent, FoldOut.boxColor).Label("Pan InputButtonSO", FoldOut.titleColor).BRE("useInputButtonSO").FoldOut("inputFoldOut"))
                                {
                                        GUI.enabled = parent.Bool("useInputButtonSO") && guiCurrent;
                                        FoldOut.Box(4, color: FoldOut.boxColor, offsetY: -2);
                                        parent.Field("Left", "inputLeft");
                                        parent.Field("Right", "inputRight");
                                        parent.Field("Up", "inputUp");
                                        parent.Field("Down", "inputDown");
                                        Layout.VerticalSpacing(3);
                                        GUI.enabled = guiCurrent;
                                }
                                if (FoldOut.Bar(parent, FoldOut.boxColor).Label("Pan Edge", FoldOut.titleColor).BRE("useEdge").FoldOut("edgeFoldOut"))
                                {
                                        GUI.enabled = parent.Bool("useEdge") && guiCurrent;
                                        FoldOut.Box(5, color: FoldOut.boxColor, offsetY: -2);
                                        parent.Slider("Left", "leftEdge");
                                        parent.Slider("Right", "rightEdge");
                                        parent.Slider("Top", "topEdge");
                                        parent.Slider("Bottom", "bottomEdge");
                                        parent.Slider("Scale", "boost", 0.1f, 5f);
                                        Layout.VerticalSpacing(3);
                                        GUI.enabled = guiCurrent;
                                }
                                GUI.enabled = true;
                        };

                }
#pragma warning restore 0414
#endif
                #endregion
        }

        [System.Serializable]
        public class UserZoom
        {
                [SerializeField] public bool enable;
                [SerializeField] public float speed = 1;
                [SerializeField] public float smooth = 0.5f;
                [SerializeField] public float maxZoomIn = 1;
                [SerializeField] public float maxZoomOut = 2;

                [SerializeField] public bool useTouch;
                [SerializeField] public bool useMouse;
                [SerializeField] public bool useKeyboard;
                [SerializeField] public bool useInputButtonSO;
                [SerializeField] public bool saveZoom;

                [SerializeField] public KeyCode zoomIn = KeyCode.Q;
                [SerializeField] public KeyCode zoomOut = KeyCode.E;
                [SerializeField] public InputButtonSO inputIn;
                [SerializeField] public InputButtonSO inputOut;

                [System.NonSerialized] public bool pause;
                [System.NonSerialized] private float zoomScale = 0;
                private Touch tA => Input.GetTouch(0);
                private Touch tB => Input.GetTouch(1);

                public void Reset ()
                {
                        pause = false;
                        zoomScale = 0;
                }

                public void SaveZoomLevel ()
                {
                        if (saveZoom)
                        {
                                PlayerPrefs.SetFloat("Safire2dCameraZoomLevelSave", zoomScale);
                                PlayerPrefs.Save();
                        }
                }

                public void LoadZoomLevel (Zoom zoom, Follow follow)
                {
                        if (saveZoom)
                        {
                                zoomScale = PlayerPrefs.GetFloat("Safire2dCameraZoomLevelSave");
                                zoomScale = Mathf.Clamp(zoomScale, maxZoomIn, maxZoomOut);
                                zoom.ZoomCamera(zoomScale, true);
                                zoom.SetTempScale(zoomScale);
                        }
                }

                public void Execute (Zoom zoom, Follow follow, bool UI)
                {
                        if (!enable || pause)
                                return;

                        float time = follow.useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

                        if (useTouch && !UI && Input.touchCount == 2 && (Input.GetTouch(0).phase == TouchPhase.Moved && Input.GetTouch(1).phase == TouchPhase.Moved))
                        {
                                float delta = (tA.position - tB.position).magnitude - ((tA.position - tA.deltaPosition) - (tB.position - tB.deltaPosition)).magnitude; // current position - previous position
                                zoomScale += delta != 0 ? Mathf.Sign(delta) * speed * time : 0;
                        }

                        if (useMouse && !UI)
                        {
                                zoomScale -= Input.GetAxis("Mouse ScrollWheel") * speed; // get axis should already be smoothed 
                        }
                        if (useKeyboard)
                        {
                                zoomScale += (Input.GetKey(zoomIn) ? -speed * time : 0) + (Input.GetKey(zoomOut) ? speed * time : 0);
                        }
                        if (useInputButtonSO)
                        {
                                if (inputIn != null && inputIn.Holding())
                                        zoomScale += -speed * time;
                                if (inputOut != null && inputOut.Holding())
                                        zoomScale += speed * time;
                        }
                        if (zoomScale != 0)
                        {
                                zoom.Set(scale: zoomScale = Mathf.Clamp(zoomScale, maxZoomIn, maxZoomOut), timeScale: !follow.useUnscaledTime, speed: smooth, isTween: false);
                        }
                }

                public void ModifyKeyboard (KeyCode zoomIn, KeyCode zoomOut)
                {
                        this.zoomIn = zoomIn;
                        this.zoomOut = zoomOut;
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField, HideInInspector] public bool edit;
                [SerializeField, HideInInspector] public bool close;
                [SerializeField, HideInInspector] public bool foldOut;
                [SerializeField, HideInInspector] public bool extraFoldOut;
                [SerializeField, HideInInspector] public bool keyboardFoldOut;
                [SerializeField, HideInInspector] public bool inputFoldOut;
                public static void CustomInspector (SerializedProperty parent, Color barColor, Color labelColor)
                {
                        if (!parent.Bool("edit"))
                                return;

                        if (Follow.Open(parent, "User Zoom", barColor, labelColor))
                        {
                                GUI.enabled = parent.Bool("enable");
                                bool guiCurrent = GUI.enabled;

                                FoldOut.Box(4, color: FoldOut.boxColor, offsetY: -2);
                                parent.FieldDouble("Range", "maxZoomIn", "maxZoomOut");
                                Labels.FieldDoubleText("Zoom In", "Zoom Out", rightSpacing: 3);
                                parent.Field("Speed", "speed");
                                parent.Slider("Smooth", "smooth");
                                parent.FieldToggleAndEnable("Save Zoom Level", "saveZoom");
                                Layout.VerticalSpacing(3);

                                FoldOut.Bar(parent, FoldOut.boxColor).Label("Zoom Touch", FoldOut.titleColor).BRE("useTouch");
                                FoldOut.Bar(parent, FoldOut.boxColor).Label("Zoom Mouse", FoldOut.titleColor).BRE("useMouse");

                                if (FoldOut.Bar(parent, FoldOut.boxColor).Label("Zoom Keyboard", FoldOut.titleColor).BRE("useKeyboard").FoldOut("keyboardFoldOut"))
                                {
                                        GUI.enabled = parent.Bool("useKeyboard") && guiCurrent;
                                        FoldOut.Box(2, color: FoldOut.boxColor, offsetY: -2);
                                        parent.Field("Zoom In", "zoomIn");
                                        parent.Field("Zoom Out", "zoomOut");
                                        Layout.VerticalSpacing(3);
                                        GUI.enabled = guiCurrent;
                                }
                                if (FoldOut.Bar(parent, FoldOut.boxColor).Label("Zoom InputButtonSO", FoldOut.titleColor).BRE("useInputButtonSO").FoldOut("inputFoldOut"))
                                {
                                        GUI.enabled = parent.Bool("useInputButtonSO") && guiCurrent;
                                        FoldOut.Box(4, color: FoldOut.boxColor, offsetY: -2);
                                        parent.Field("Zoom In", "inputIn");
                                        parent.Field("Zoom Out", "inputOut");
                                        Layout.VerticalSpacing(3);
                                        GUI.enabled = guiCurrent;
                                }
                                GUI.enabled = true;
                        }

                }
#pragma warning restore 0414
#endif
                #endregion
        }

        [System.Serializable]
        public class UserRotate
        {
                [SerializeField] public bool enable;
                [SerializeField] public float speed = 10;
                [SerializeField] public float smooth = 0.25f;
                [SerializeField] public bool useTouch;
                [SerializeField] public bool useMouse;
                [SerializeField] public bool useKeyboard;
                [SerializeField] public bool useInputButtonSO;
                [SerializeField] public Axis1D2 touchAxis;
                [SerializeField] public MouseButton mouseButton;
                [SerializeField] public KeyCode rotateLeft = KeyCode.Z;
                [SerializeField] public KeyCode rotateRight = KeyCode.X;
                [SerializeField] public InputButtonSO inputIn;
                [SerializeField] public InputButtonSO inputOut;

                [System.NonSerialized] private float angle = 0;
                [System.NonSerialized] private float target = 0;
                [System.NonSerialized] private bool usingDesktop;
                [System.NonSerialized] public bool pause;


                public void Initialize ()
                {
                        usingDesktop = SystemInfo.deviceType == DeviceType.Desktop;
                }

                public void Reset (Transform camera)
                {
                        pause = false;
                        angle = target = 0;
                        if (enable)
                        {
                                Vector3 a = camera.localEulerAngles;
                                camera.localEulerAngles = new Vector3(a.x, a.y, 0);
                        }
                }

                public void Execute (Follow follow, bool overUI) // Execute all input inside LateUpate. Fixed Update can miss input signals.
                {
                        if (!enable || pause)
                                return;

                        float time = follow.useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

                        if (useTouch && !overUI && Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Moved)
                        {
                                target -= touchAxis == Axis1D2.Horizontal ? Input.GetTouch(0).deltaPosition.x * speed * time : Input.GetTouch(0).deltaPosition.y * speed * time; // do not multiply speed by factor, touch is already fast
                        }

                        if (useMouse && !overUI && usingDesktop && Input.GetMouseButton((int) mouseButton))
                        {
                                target -= Input.GetAxis("Mouse Y") * speed;
                        }

                        if (useKeyboard)
                        {
                                float boost = speed * time * 10f;
                                target += (Input.GetKey(rotateLeft) ? -boost : 0) + (Input.GetKey(rotateRight) ? boost : 0);
                        }
                        if (useInputButtonSO)
                        {
                                float boost = speed * time * 10f;
                                if (inputIn != null && inputIn.Holding())
                                        target += -boost;
                                if (inputOut != null && inputOut.Holding())
                                        target += boost;
                        }

                        if (useTouch || useMouse || useKeyboard)
                        {
                                angle = follow.useUnscaledTime ? Compute.LerpUnscaled(angle, target, smooth) : Compute.Lerp(angle, target, smooth);
                                Vector3 a = follow.cameraTransform.transform.localEulerAngles;
                                follow.cameraTransform.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
                                follow.cameraTransform.transform.localEulerAngles = Tooly.SetPosition(a.x, a.y, follow.cameraTransform.transform.localEulerAngles); // Do not override other angles, reset them.
                        }
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField, HideInInspector] public bool edit;
                [SerializeField, HideInInspector] public bool close;
                [SerializeField, HideInInspector] public bool foldOut;
                [SerializeField, HideInInspector] public bool mouseFoldOut;
                [SerializeField, HideInInspector] public bool touchFoldOut;
                [SerializeField, HideInInspector] public bool keyboardFoldOut;
                [SerializeField, HideInInspector] public bool inputFoldOut;

                public static void CustomInspector (SerializedProperty parent, Color barColor, Color labelColor)
                {
                        if (!parent.Bool("edit"))
                                return;

                        if (Follow.Open(parent, "User Rotate", barColor, labelColor))
                        {
                                GUI.enabled = parent.Bool("enable");
                                bool guiCurrent = GUI.enabled;

                                FoldOut.Box(2, color: FoldOut.boxColor, offsetY: -2);
                                parent.Field("Speed", "speed");
                                parent.Slider("Smooth", "smooth");
                                Layout.VerticalSpacing(3);

                                if (FoldOut.Bar(parent, FoldOut.boxColor).Label("Rotate Touch", FoldOut.titleColor).BRE("useTouch").FoldOut("touchFoldOut"))
                                {
                                        GUI.enabled = parent.Bool("useTouch") && guiCurrent;
                                        FoldOut.Box(1, color: FoldOut.boxColor, offsetY: -2);
                                        parent.Field("Axis", "touchAxis");
                                        Layout.VerticalSpacing(3);
                                        GUI.enabled = guiCurrent;
                                }
                                if (FoldOut.Bar(parent, FoldOut.boxColor).Label("Rotate Mouse", FoldOut.titleColor).BRE("useMouse").FoldOut("mouseFoldOut"))
                                {
                                        GUI.enabled = parent.Bool("useMouse") && guiCurrent;
                                        FoldOut.Box(1, color: FoldOut.boxColor, offsetY: -2);
                                        parent.Field("Button", "mouseButton");
                                        Layout.VerticalSpacing(3);
                                        GUI.enabled = guiCurrent;
                                }
                                if (FoldOut.Bar(parent, FoldOut.boxColor).Label("Rotate Keyboard", FoldOut.titleColor).BRE("useKeyboard").FoldOut("keyboardFoldOut"))
                                {
                                        GUI.enabled = parent.Bool("useKeyboard") && guiCurrent;
                                        FoldOut.Box(2, color: FoldOut.boxColor, offsetY: -2);
                                        parent.Field("Left", "rotateLeft");
                                        parent.Field("Right", "rotateRight");
                                        Layout.VerticalSpacing(3);
                                        GUI.enabled = guiCurrent;
                                }
                                if (FoldOut.Bar(parent, FoldOut.boxColor).Label("Rotate InputButtonSO", FoldOut.titleColor).BRE("useInputButtonSO").FoldOut("inputFoldOut"))
                                {
                                        GUI.enabled = parent.Bool("useInputButtonSO") && guiCurrent;
                                        FoldOut.Box(4, color: FoldOut.boxColor, offsetY: -2);
                                        parent.Field("Left", "inputIn");
                                        parent.Field("Right", "inputOut");
                                        Layout.VerticalSpacing(3);
                                        GUI.enabled = guiCurrent;
                                }
                                GUI.enabled = true;
                        }
                }
#pragma warning restore 0414
#endif
                #endregion
        }

        public enum Pan
        {
                Click,
                Drag
        }

        public enum PanTouch
        {
                Click,
                Drag
        }

        public enum MouseButton
        {
                Left,
                Right,
                Middle,
                None
        }

        public enum Axis1D2
        {
                Horizontal,
                Vertical
        }
}
