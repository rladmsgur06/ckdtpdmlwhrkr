#if UNITY_EDITOR
using TwoBitMachines.Editors;
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.EventSystems;

namespace TwoBitMachines.Safire2DCamera
{
        [System.Serializable]
        public class Follow
        {
                [SerializeField] public bool enable = true;
                [SerializeField] public FollowType followType;
                [SerializeField] public float speed = 10f;
                [SerializeField] public float smoothX = 1f;
                [SerializeField] public float smoothY = 1f;
                [SerializeField] public float returnSmooth = 0.75f;
                [SerializeField] public bool useUnscaledTime;
                [SerializeField] public Vector2 offset;
                [SerializeField] public Vector2 autoScroll;

                [SerializeField] public Rails rails = new Rails();
                [SerializeField] public Regions regions = new Regions();
                [SerializeField] public DeadZone deadZone = new DeadZone();
                [SerializeField] public PushZone pushZone = new PushZone();
                [SerializeField] public LookAhead lookAhead = new LookAhead();
                [SerializeField] public ScreenZone screenZone = new ScreenZone();
                [SerializeField] public DetectWalls detectWalls = new DetectWalls();
                [SerializeField] public FollowBlocks followBlocks = new FollowBlocks();
                [SerializeField] public HighlightTarget highlightTarget = new HighlightTarget();

                [SerializeField] public UserPan userPan = new UserPan();
                [SerializeField] public UserZoom userZoom = new UserZoom();
                [SerializeField] public UserRotate userRotate = new UserRotate();

                [System.NonSerialized] public PixelPerfect pixelPerfect;
                [System.NonSerialized] public Transform targetTransform;
                [System.NonSerialized] public Transform cameraTransform;
                [System.NonSerialized] public Camera camera;
                [System.NonSerialized] public Rooms rooms;
                [System.NonSerialized] public Zoom zoom;
                [System.NonSerialized] public Peek peek;
                [System.NonSerialized] public PeekPlus peekPlus;

                [System.NonSerialized] public int roomSmoothX;
                [System.NonSerialized] public int roomSmoothY;
                [System.NonSerialized] public bool forceClampX;
                [System.NonSerialized] public bool forceClampY;
                [System.NonSerialized] public bool forceSmoothX;
                [System.NonSerialized] public bool forceSmoothY;
                [System.NonSerialized] public bool usingAutoRail;
                [System.NonSerialized] public bool pauseAutoscroll;
                [System.NonSerialized] public Vector3 previousTarget;

                public bool isUser => followType == FollowType.user;
                public Vector3 CameraPosition => cameraTransform.position;

                public void Initialize (Safire2DCamera main)
                {
                        this.targetTransform = main.targetTransform;
                        this.cameraTransform = main.transform;
                        this.pixelPerfect = main.pixelPerfect;
                        this.camera = main.cameraRef;
                        this.rooms = main.rooms;
                        this.zoom = main.zoom;
                        this.peek = main.peek;
                        this.peekPlus = main.peekPlus;

                        rails.Initialize();
                        deadZone.Set(TargetPosition());
                        lookAhead.Initialize(this);
                        detectWalls.Initialize(this);
                        followBlocks.Initialize();
                        userPan.Initialize(MousePosition());
                        userRotate.Initialize();
                        previousTarget = TargetPosition();
                        // regions, push zone, screen zone, highlight target, user zoom, no init
                }

                public void Reset ()
                {
                        rails.Reset();
                        regions.Reset();
                        lookAhead.Reset();
                        screenZone.Reset();
                        detectWalls.Reset();
                        followBlocks.Reset();
                        highlightTarget.Reset();
                        userZoom.Reset();
                        userPan.Reset(MousePosition());
                        userRotate.Reset(cameraTransform);
                        deadZone.Set(TargetPosition());
                        // push zone, screen zone, no reset
                        roomSmoothX = 0;
                        roomSmoothY = 0;
                        forceClampX = false;
                        forceClampY = false;
                        forceSmoothX = false;
                        forceSmoothY = false;
                        usingAutoRail = false;
                        pauseAutoscroll = false;
                        previousTarget = TargetPosition();
                }

                public void Execute ()
                {
                        if (!enable)
                                return;

                        usingAutoRail = false;
                        bool overUI = isUser ? IsPointerOverGameObject() : false;
                        Vector3 target = TargetPosition();
                        Vector3 realTarget = target;
                        target = detectWalls.Position(target, screenZone);
                        target = deadZone.Position(target, isUser);
                        target = lookAhead.Position(target, camera, isUser);
                        target = followBlocks.Position(target, this, isUser);
                        target = rooms.MultipleTarget(target, isUser);
                        target += regions.Offset(target, this, isUser);

                        float realSmoothX = forceSmoothX ? Mathf.Min(smoothX, returnSmooth) : forceClampX ? 1f : smoothX;
                        float realSmoothY = forceSmoothY ? Mathf.Min(smoothY, returnSmooth) : forceClampY ? 1f : smoothY;
                        forceClampX = forceClampY = false; // have to keep setting true

                        Vector3 newCameraPosition = Compute.LerpToTarget(cameraTransform.position, target, ref previousTarget, realSmoothX, realSmoothY, speed);
                        newCameraPosition += userPan.Velocity(cameraTransform.position, this, overUI);
                        newCameraPosition = rails.Position(newCameraPosition, cameraTransform.position, this);

                        Vector3 cameraVelocity = newCameraPosition - cameraTransform.position;

                        cameraVelocity = pushZone.Velocity(target, cameraVelocity, camera, isUser);
                        cameraVelocity += screenZone.Velocity(realTarget, camera, isUser);
                        cameraVelocity += peek.Velocity(this, isUser);
                        cameraVelocity += peekPlus.Velocity(this, isUser);

                        if (autoScroll.x != 0)
                                cameraVelocity.x = autoScroll.x * Time.deltaTime;
                        if (autoScroll.y != 0)
                                cameraVelocity.y = autoScroll.y * Time.deltaTime;
                        cameraVelocity = highlightTarget.Velocity(target, cameraVelocity, this);

                        SetCameraPosition(cameraTransform.position + cameraVelocity);

                        if (forceSmoothX && Mathf.Abs(cameraTransform.position.x - target.x) < 0.1f)
                        {
                                forceSmoothX = false;
                        }
                        if (forceSmoothY && Mathf.Abs(cameraTransform.position.y - target.y) < 0.1f)
                        {
                                forceSmoothY = false;
                        }

                        userZoom.Execute(zoom, this, overUI);
                        userRotate.Execute(this, overUI);
                        roomSmoothX = roomSmoothY = 0;
                }

                public void ForceTargetSmooth (bool x = true, bool y = true)
                {
                        if (x)
                        {
                                forceSmoothX = true;
                                forceClampX = false;
                        }
                        if (y)
                        {
                                forceSmoothY = true;
                                forceClampY = false;
                        }
                }

                public void ForceTargetClamp (bool x = true, bool y = true)
                {
                        if (x)
                        {
                                forceClampX = true;
                                forceSmoothX = false;
                        }
                        if (y)
                        {
                                forceClampY = true;
                                forceSmoothY = false;
                        }
                }

                public void ForceTargetClampConditional (bool x = true, bool y = true)
                {
                        if (x && !forceSmoothX)
                        {
                                forceClampX = true;
                        }
                        if (y && !forceSmoothY)
                        {
                                forceClampY = true;
                        }
                }

                public void SetCameraPosition (Vector3 newPosition)
                {
                        Tooly.SetPosition(newPosition.x, newPosition.y, cameraTransform);
                }

                public Vector3 TargetPosition ()
                {
                        if (followType == FollowType.Target && targetTransform != null)
                        {
                                return targetTransform.position + new Vector3(offset.x, offset.y, 0);
                        }
                        return cameraTransform.position;
                }

                public Vector2 MousePosition ()
                {
                        Vector3 mousePosition = Input.mousePosition;
                        if (!camera.orthographic)
                        {
                                mousePosition.z = zoom.currentCameraDepth;
                        }
                        return camera.ScreenToWorldPoint(mousePosition);
                }

                public Vector2 MousePositionRay ()
                {
                        Vector3 mousePosition = camera.ScreenPointToRay(Input.mousePosition).origin;
                        if (!camera.orthographic)
                        {
                                mousePosition.z = zoom.currentCameraDepth;
                        }
                        return mousePosition;
                }

                public Vector2 Touch ()
                {
                        Vector3 touchPosition = Input.GetTouch(0).position;
                        if (!camera.orthographic)
                        {
                                touchPosition.z = zoom.currentCameraDepth;
                        }
                        return camera.ScreenToWorldPoint(touchPosition);
                }

                public static bool IsPointerOverGameObject ()
                {
                        if (EventSystem.current == null)
                        {
                                return false;
                        }
                        bool overUI = EventSystem.current.IsPointerOverGameObject(); // Check mouse
                        for (int i = 0; i < Input.touchCount; i++) //                     Check touches
                        {
                                if (Input.GetTouch(i).phase == TouchPhase.Began && EventSystem.current.IsPointerOverGameObject(Input.GetTouch(i).fingerId))
                                {
                                        overUI = true;
                                }
                        }
                        return overUI;
                }

                #region ▀▄▀▄▀▄ Custom Inspector▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField, HideInInspector] public bool edit;
                [SerializeField, HideInInspector] public bool close;
                [SerializeField, HideInInspector] public bool foldOut;
                [SerializeField, HideInInspector] public bool view = true;
                public static void CustomInspector (SerializedProperty parent, bool isUser, Color barColor, Color labelColor)
                {
                        if (Open(parent, "Follow", barColor, labelColor, false, false, true))
                        {
                                GUI.enabled = parent.Bool("enable");
                                FoldOut.Box(5, Tint.Box);
                                parent.Field("Speed", "speed");
                                parent.Slider("Smooth X", "smoothX");
                                parent.Slider("Smooth Y", "smoothY");
                                parent.Slider("Return Smooth", "returnSmooth", min: 0.01f, max: 0.99f);
                                parent.FieldToggleAndEnable("User Unscaled Time", "useUnscaledTime");
                                Layout.VerticalSpacing(5);

                                FoldOut.Box(7, Tint.Box);
                                parent.Field("Offset", "offset");
                                parent.Get("deadZone").Field("Dead Zone", "size");
                                parent.Get("screenZone").Field("Screen Zone", "size");
                                parent.Field("Auto Scroll", "autoScroll");
                                DetectWalls(parent, parent.Get("detectWalls"));
                                PushZone(parent.Get("pushZone"));
                                Layout.VerticalSpacing(5);
                                GUI.enabled = true;

                                parent.Get("screenZone").ClampV2("size");
                        }
                }
                private static void DetectWalls (SerializedProperty follow, SerializedProperty detectWalls)
                {
                        int index = detectWalls.Enum("direction");
                        detectWalls.Field("Detect Walls", "direction", execute: index == 0);
                        detectWalls.FieldDouble("Detect Walls", "direction", "layerMask", execute: index > 0);
                }
                private static void PushZone (SerializedProperty pushZone)
                {
                        PushHorizontal activeX = (PushHorizontal) pushZone.Get("horizontal").enumValueIndex;
                        pushZone.Field("Push Zone X", "horizontal", execute: activeX == PushHorizontal.DontPush);
                        pushZone.FieldDouble("Push Zone X", "horizontal", "zoneX", execute: activeX != PushHorizontal.DontPush);
                        pushZone.Clamp("zoneX");

                        PushVertical activeY = (PushVertical) pushZone.Get("vertical").enumValueIndex;
                        pushZone.Field("Push Zone Y", "vertical", execute: activeY == PushVertical.DontPush);
                        pushZone.FieldDouble("Push Zone Y", "vertical", "zoneY", execute: activeY != PushVertical.DontPush);
                        pushZone.Clamp("zoneY");
                }
                public static bool Open (SerializedProperty parent, string name, Color barColor, Color labelColor, bool add = false, bool checkClose = true, bool canView = false)
                {
                        bool open = FoldOut.Bar(parent, barColor, -3)
                                .BL(on: Tint.PastelGreen, off: Tint.Box)
                                .SL(3).Label(name, labelColor)
                                .BR("close", "Minus", execute: checkClose)
                                .BR(execute: add && parent.Bool("foldOut") && parent.Bool("enable"))
                                .BR("view", "EyeOpen", execute: canView && parent.Bool("view"))
                                .BR("view", "EyeClosed", execute: canView && !parent.Bool("view"))
                                .FoldOut();
                        if (checkClose && parent.ReadBool("close"))
                        {
                                parent.SetFalse("close");
                                parent.SetFalse("enable");
                                parent.SetFalse("edit");
                        }
                        return open;
                }
                public static void Select (SerializedProperty array, int index)
                {
                        for (int i = 0; i < array.arraySize; i++)
                        {
                                array.Element(i).Get("select").intValue = -1;
                                if (index == i)
                                {
                                        array.Element(i).Get("select").intValue = i;
                                        SceneTools.MoveSceneCamera(array.Element(i));
                                }
                        }
                }
#pragma warning restore 0414
#endif
                #endregion

        }

        public enum FollowType
        {
                Target,
                user
        }
}
