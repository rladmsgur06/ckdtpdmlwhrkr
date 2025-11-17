#if UNITY_EDITOR
using TwoBitMachines.Editors;
using UnityEditor;
#endif
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace TwoBitMachines.Safire2DCamera
{
        [System.Serializable]
        public class Rooms
        {
                [SerializeField] public bool enable;
                [SerializeField] public List<Room> rooms = new List<Room>();

                [System.NonSerialized] public bool ignoreClamps;
                [System.NonSerialized] private bool transition;
                [System.NonSerialized] private bool inTwoRooms;
                [System.NonSerialized] private int index = -1;
                [System.NonSerialized] private int previousIndex = -1;

                [System.NonSerialized] private Zoom zoom;
                [System.NonSerialized] private Follow follow;
                [System.NonSerialized] private Camera camera;
                [System.NonSerialized] private bool clampedX;
                [System.NonSerialized] private bool clampedY;

                private bool roomExists => index >= 0 && index < rooms.Count;

                public void Initialize (Camera camera, Follow follow, Zoom zoom)
                {
                        this.follow = follow;
                        this.camera = camera;
                        this.zoom = zoom;
                        this.index = -1;
                        this.previousIndex = -1;
                        for (int i = 0; i < rooms.Count; i++)
                        {
                                rooms[i].bounds.Initialize();
                                rooms[i].multipleTargets.Initialize(zoom, rooms[i].bounds);
                        }
                        Reset();
                }

                public void Reset ()
                {
                        if (roomExists)
                                rooms[index].onExit.Invoke();
                        if (zoom != null)
                                zoom.lockZoom = false;
                        ignoreClamps = false;
                        transition = false;
                        inTwoRooms = false;
                        index = -1;
                        previousIndex = -1;
                        ImmediateEnter(camera, follow.TargetPosition());
                }

                public void RestrictTarget ()
                {
                        if (enable && roomExists && !transition && follow.targetTransform != null)
                        {
                                rooms[index].RestrictTarget(follow.targetTransform);
                        }
                }

                public bool Execute (Vector3 previousCamera)
                {
                        if (!enable || rooms.Count == 0)
                                return false;

                        if (TransitionActive(previousCamera))
                                return true;

                        Vector2 targetPosition = follow.TargetPosition();
                        int roomsFound = 0;
                        int newRoomIndex = -1;

                        for (int i = 0; i < rooms.Count; i++)
                        {
                                if (rooms[i].bounds.DetectBounds(targetPosition))
                                {
                                        roomsFound++;
                                        newRoomIndex = i == index ? newRoomIndex : i;
                                }
                        }
                        if (!inTwoRooms && roomsFound > 0 && newRoomIndex != index && newRoomIndex != -1) // changing rooms
                        {
                                if (roomExists)
                                        rooms[index].onExit.Invoke();
                                if (roomExists)
                                        inTwoRooms = rooms[index].bounds.Overlap(rooms[newRoomIndex].bounds);
                                previousIndex = index;
                                index = newRoomIndex;
                                rooms[newRoomIndex].onEnter.Invoke();
                                transition = rooms[newRoomIndex].SetTransitionPoint(targetPosition, camera.transform, follow, zoom);
                                follow.lookAhead.RoomExit();
                        }
                        if (roomsFound == 1)
                        {
                                inTwoRooms = false;
                        }
                        if (roomsFound == 0 && roomExists && rooms[index].canExit) // exiting room, no more rooms
                        {
                                zoom.lockZoom = false;
                                if (rooms[index].canZoom || rooms[index].multipleTargets.enable)
                                        zoom.Set(1f, 1f);
                                rooms[index].onExit.Invoke();
                                rooms[index].ForceSmooth(targetPosition, follow);
                                inTwoRooms = false;
                                index = -1;
                                follow.lookAhead.RoomExit();
                        }
                        return TransitionActive(previousCamera);
                }

                public void ImmediateEnter (Camera camera, Vector2 characterPosition)
                {
                        if (!enable)
                                return;
                        for (int i = 0; i < rooms.Count; i++)
                        {
                                if (rooms[i].bounds.DetectBounds(characterPosition))
                                {
                                        EnterRoom(rooms[i], characterPosition, i);
                                        return;
                                }
                        }
                }

                public void EnterRoom (Room room, Vector2 characterPosition, int i)
                {
                        index = i;
                        room.onEnter.Invoke();
                        room.onTransitionComplete.Invoke();
                        room.SetTransitionPoint(characterPosition, camera.transform, follow, zoom);
                        if (room.canZoom)
                                zoom.Set(room.ZoomValue(zoom), 0.001f);
                        camera.transform.position = room.transitionPoint;
                        transition = zoom.lockZoom = false;
                }

                public bool TransitionActive (Vector3 previousCamera)
                {
                        if (!enable || !transition || !roomExists)
                        {
                                return false;
                        }
                        Vector2 targetPosition = follow.TargetPosition();
                        bool exit = false;
                        if (rooms[index].canExit && !rooms[index].bounds.DetectBounds(targetPosition))
                        {
                                exit = true;
                        }
                        for (int i = 0; i < rooms.Count; i++)
                        {
                                if (index != i && previousIndex != i && rooms[i].bounds.DetectBounds(targetPosition))
                                {
                                        exit = true;
                                        break;
                                }
                        }
                        if (exit)
                        {
                                transition = zoom.lockZoom = false;
                                if (rooms[index].canZoom || rooms[index].multipleTargets.enable)
                                        zoom.Set(1f, 1f);
                                rooms[index].onExit.Invoke();
                                follow.ForceTargetSmooth();
                                index = -1;
                                follow.lookAhead.RoomExit();
                                return false;
                        }
                        if (rooms[index].Transition(camera, previousCamera, zoom, follow))
                        {
                                return transition = zoom.lockZoom = false;
                        }
                        return true;
                }

                public void Clamp (Camera camera, Vector3 previousCamera)
                {
                        if (!enable || transition || ignoreClamps || rooms.Count == 0 || !roomExists)
                                return;
                        rooms[index].Clamp(camera, previousCamera);
                }

                public Vector2 MultipleTarget (Vector2 target, bool isUser)
                {
                        if (!enable || transition || !roomExists || !rooms[index].multipleTargets.enable || isUser)
                                return target;
                        return rooms[index].multipleTargets.FollowMultipleTargets(target, ignoreClamps);
                }

                public Vector2 RoomEntryPoint (Vector2 target, Transform camera)
                {
                        if (!enable || !roomExists)
                                return target;
                        rooms[index].SetTransitionPoint(target, camera, follow, zoom, false);
                        return rooms[index].transitionPoint;
                }

                public Vector3 RoomTarget (Vector3 target, Follow follow)
                {
                        Vector3 roomTarget = RoomEntryPoint(target, follow.cameraTransform);
                        if (Mathf.Abs(follow.cameraTransform.position.x - roomTarget.x) < 0.25f && Mathf.Abs(follow.cameraTransform.position.y - roomTarget.y) < 0.25f)
                        {
                                ignoreClamps = false;
                        }
                        return ignoreClamps ? roomTarget : target;
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField, HideInInspector] public bool add;
                [SerializeField, HideInInspector] public bool edit;
                [SerializeField, HideInInspector] public bool close;
                [SerializeField, HideInInspector] public bool foldOut;
                [SerializeField, HideInInspector] public bool view = true;
                public static void CustomInspector (SerializedProperty parent, Color barColor, Color labelColor)
                {
                        if (!parent.Bool("edit"))
                                return;

                        if (Follow.Open(parent, "Rooms", barColor, labelColor, true, canView: true))
                        {
                                GUI.enabled = parent.Bool("enable");
                                SerializedProperty array = parent.Get("rooms");

                                if (parent.ReadBool("add"))
                                {
                                        array.arraySize++;
                                        EditorTools.ClearProperty(array.LastElement());
                                        array.LastElement().Get("bounds").Get("position").vector2Value = SceneTools.SceneCenter(Vector2.zero);
                                        array.LastElement().Get("bounds").Get("size").vector2Value = new Vector2(10f, 5f);
                                        array.LastElement().Get("bounds").Get("detectTop").floatValue = 0;
                                        array.LastElement().Get("bounds").Get("detectLeft").floatValue = 0;
                                        array.LastElement().Get("bounds").Get("detectRight").floatValue = 0;
                                        array.LastElement().Get("bounds").Get("detectBottom").floatValue = 0;
                                        array.LastElement().Get("transitionTime").floatValue = 1f;
                                        array.LastElement().SetTrue("canExit");
                                        array.LastElement().SetTrue("clampTop");
                                        array.LastElement().SetTrue("clampLeft");
                                        array.LastElement().SetTrue("clampRight");
                                        array.LastElement().SetTrue("clampBottom");
                                }

                                if (array.arraySize == 0)
                                        Layout.VerticalSpacing(5);

                                for (int i = 0; i < array.arraySize; i++)
                                {
                                        SerializedProperty element = array.Element(i);

                                        if (CustomInspectorRoom(element, array, i))
                                        {
                                                break;
                                        }
                                }
                                GUI.enabled = true;
                        }
                }

                public static bool CustomInspectorRoom (SerializedProperty element, SerializedProperty array, int index)
                {
                        FoldOut.BoxSingle(1, Tint.Blue, 0);
                        {
                                Fields.ConstructField();
                                element.ConstructField("name", S.FW - S.B3 - 5f, 5f);
                                if (array != null && Fields.ConstructButton("Delete"))
                                {
                                        array.DeleteArrayElement(index);
                                        return true;
                                }
                                if (array != null && Fields.ConstructButton("Target"))
                                {
                                        Follow.Select(array, index);
                                }
                                if (Fields.ConstructButton("Reopen"))
                                {
                                        element.Toggle("open");
                                }
                        }
                        Layout.VerticalSpacing(2);

                        if (!element.Bool("open"))
                                return false;

                        FoldOut.Box(7, Tint.Blue, extraHeight: 5, offsetY: -2);
                        {
                                Fields.ConstructField();
                                Fields.ConstructString("Boundary", Layout.labelWidth - 4, 4);
                                if (Fields.ConstructButton(element.Bool("clampLeft") ? "LeftSolid" : "LeftBroken", Layout.quint))
                                { element.Toggle("clampLeft"); }
                                if (Fields.ConstructButton(element.Bool("clampRight") ? "RightSolid" : "RightBroken", Layout.quint))
                                { element.Toggle("clampRight"); }
                                if (Fields.ConstructButton(element.Bool("clampTop") ? "TopSolid" : "TopBroken", Layout.quint))
                                { element.Toggle("clampTop"); }
                                if (Fields.ConstructButton(element.Bool("clampBottom") ? "BottomSolid" : "BottomBroken", Layout.quint))
                                { element.Toggle("clampBottom"); }
                                if (element.Bool("canExit") && Fields.ConstructButton("RoomExit", Layout.quint))
                                { element.Toggle("canExit"); }
                                if (!element.Bool("canExit") && Fields.ConstructButton("RoomClosed", Layout.quint))
                                { element.Toggle("canExit"); }

                                element.FieldDouble("Transition", "transitionTime", "delay");
                                Labels.FieldDoubleText("Duration", "Delay", rightSpacing: 3);
                                element.Field("Tween", "tween");
                                element.FieldAndEnable("zoomScale", "zoomValue", "canZoom", titleIsField: true);
                                element.FieldToggleAndEnable("Restrict", "restrict");
                                element.FieldToggleAndEnable("Hold Target", "holdTarget");
                                element.FieldToggleAndEnable("Lazy Clamp", "lazyClamp");
                        }

                        bool eventOpen = FoldOut.FoldOutButton(element.Get("eventsFoldOut"));
                        Fields.EventFoldOut(element.Get("onEnter"), element.Get("enterFoldOut"), "On Enter", color: Tint.Blue, execute: eventOpen);
                        Fields.EventFoldOut(element.Get("onExit"), element.Get("exitFoldOut"), "On Exit", color: Tint.Blue, execute: eventOpen);
                        Fields.EventFoldOut(element.Get("onTransitionStart"), element.Get("startFoldOut"), "On Transition Start", color: Tint.Blue, execute: eventOpen);
                        Fields.EventFoldOut(element.Get("onTransitionComplete"), element.Get("completeFoldOut"), "On Transition Complete", color: Tint.Blue, execute: eventOpen);
                        MultipleTargets.CustomInspector(element.Get("multipleTargets"), Tint.Blue);

                        return false;
                }

                public static void DrawRooms (Safire2DCamera main)
                {
                        if (!main.rooms.view || !main.rooms.enable)
                                return;

                        for (int i = 0; i < main.rooms.rooms.Count; i++)
                        {
                                Room room = main.rooms.rooms[i];
                                DrawRooms(room, main, i);
                        }
                }

                public static void DrawRooms (Room room, Safire2DCamera main, int index, bool standAlone = false)
                {
                        Bounds bounds = room.bounds;
                        bounds.position = Compute.Round(bounds.position, 0.25f);
                        bounds.size = Compute.Round(bounds.size, 0.05f);

                        DrawRoom(room, ref bounds.position, ref bounds.size, room.select == index ? Tint.PastelGreen : Tint.Blue, 0.5f, standAlone);
                        SceneTools.ModifyPercentH(ref bounds.detectLeft, bounds.center, bounds.size, bounds.detectLeft == 0 ? Tint.PastelGreen : Tint.Delete, -1f, 0.25f);
                        SceneTools.ModifyPercentH(ref bounds.detectRight, bounds.center, bounds.size, bounds.detectRight == 0 ? Tint.PastelGreen : Tint.Delete, 1f, 0.25f);
                        SceneTools.ModifyPercentV(ref bounds.detectTop, bounds.center, bounds.size, bounds.detectTop == 0 ? Tint.PastelGreen : Tint.Delete, 1f, 0.25f);
                        SceneTools.ModifyPercentV(ref bounds.detectBottom, bounds.center, bounds.size, bounds.detectBottom == 0 ? Tint.PastelGreen : Tint.Delete, -1f, 0.25f);

                        if (SceneTools.UpperRightButton(bounds.position, bounds.size, Tint.PastelGreen, 0.5f))
                        {
                                if (Camera.main != null)
                                        bounds.size = Compute.CameraSize(Camera.main);
                        }
                        if (main != null && Mouse.down && bounds.DetectRaw(Mouse.position))
                        {
                                for (int j = 0; j < main.rooms.rooms.Count; j++)
                                {
                                        main.rooms.rooms[j].select = -1;
                                        main.rooms.rooms[j].open = false;
                                }
                                room.select = index;
                                room.open = true;
                        }
                        if (room.name != "")
                        {
                                SceneTools.Label(bounds.position + Vector2.up * (bounds.size.y + 1f), room.name, Tint.WarmWhite);
                        }

                }

                public static void DrawRoom (Room room, ref Vector2 position, ref Vector2 size, Color color, float handleSize = 0.25f, bool standAlone = false)
                {
                        SceneTools.DrawAndModifyBounds(ref position, ref size, color, handleSize, false, move: !standAlone);

                        Vector2 tL = position + Vector2.up * size.y;
                        Vector2 tR = tL + Vector2.right * size.x;
                        Vector2 bR = tR + Vector2.down * size.y;

                        Color previousColor = Handles.color;
                        Handles.color = color;
                        if (room.clampLeft)
                                Handles.DrawLine(position, tL);
                        else
                                Handles.DrawDottedLine(position, tL, 16f);

                        if (room.clampTop)
                                Handles.DrawLine(tL, tR);
                        else
                                Handles.DrawDottedLine(tL, tR, 16f);

                        if (room.clampRight)
                                Handles.DrawLine(tR, bR);
                        else
                                Handles.DrawDottedLine(tR, bR, 16f);

                        if (room.clampBottom)
                                Handles.DrawLine(position, bR);
                        else
                                Handles.DrawDottedLine(position, bR, 16f);
                        Handles.color = previousColor;
                }
#pragma warning restore 0414
#endif
                #endregion
        }

        [System.Serializable]
        public class Room
        {
                [SerializeField] public string name;
                [SerializeField] public bool canExit = true;
                [SerializeField] public bool clampTop = true;
                [SerializeField] public bool clampLeft = true;
                [SerializeField] public bool clampRight = true;
                [SerializeField] public bool clampBottom = true;
                [SerializeField] public bool holdTarget = false;
                [SerializeField] public bool lazyClamp = false;
                [SerializeField] public float transitionTime = 1f;
                [SerializeField] public float delay;
                [SerializeField] public Tween tween;
                [SerializeField] public MultipleTargets multipleTargets = new MultipleTargets();

                [SerializeField] public bool restrict;
                [SerializeField] public bool canZoom;
                [SerializeField] public float zoomValue = 1f;
                [SerializeField] public ZoomScale zoomScale;

                [SerializeField] public UnityEvent onExit;
                [SerializeField] public UnityEvent onEnter;
                [SerializeField] public UnityEvent onTransitionComplete;
                [SerializeField] public UnityEvent onTransitionStart;
                [SerializeField, HideInInspector] public Bounds bounds = new Bounds();

                [System.NonSerialized] public Vector3 transitionPoint;
                [System.NonSerialized] private bool transitionX;
                [System.NonSerialized] private bool transitionY;
                [System.NonSerialized] private bool transitionStart;
                [System.NonSerialized] private bool insideRestrictArea;
                [System.NonSerialized] private bool lazyClampLeft;
                [System.NonSerialized] private bool lazyClampRight;
                [System.NonSerialized] private bool lazyClampTop;
                [System.NonSerialized] private bool lazyClampBottom;
                [System.NonSerialized] private float transitionCounter;
                [System.NonSerialized] private float delayCounter;
                [System.NonSerialized] private Vector3 startPoint;
                [System.NonSerialized] private Vector3 targetEntry;
                [System.NonSerialized] private static bool isClampedX;
                [System.NonSerialized] private static bool isClampedY;

                private bool canClampLeft => (clampLeft && !lazyClamp) || lazyClampLeft;
                private bool canClampRight => (clampRight && !lazyClamp) || lazyClampRight;
                private bool canClampTop => (clampTop && !lazyClamp) || lazyClampTop;
                private bool canClampBottom => (clampBottom && !lazyClamp) || lazyClampBottom;
                private bool allClamped => canClampLeft && canClampRight && canClampTop && canClampBottom;

                #region ▀▄▀▄▀▄ Editor Variables ▄▀▄▀▄▀ 
#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField, HideInInspector] private bool eventsFoldOut;
                [SerializeField, HideInInspector] private bool completeFoldOut;
                [SerializeField, HideInInspector] private bool startFoldOut;
                [SerializeField, HideInInspector] private bool enterFoldOut;
                [SerializeField, HideInInspector] private bool exitFoldOut;
                [SerializeField, HideInInspector] public bool open = true;
                [SerializeField, HideInInspector] public int select = -1;
#pragma warning restore 0414
#endif
                #endregion

                public bool SetTransitionPoint (Vector2 targetEntryPoint, Transform camera, Follow follow, Zoom zoom, bool setZoom = true)
                {
                        bool foundX = false;
                        bool foundY = false;
                        lazyClampLeft = false;
                        lazyClampRight = false;
                        lazyClampTop = false;
                        lazyClampBottom = false;
                        transitionX = false;
                        transitionY = false;
                        insideRestrictArea = false;
                        transitionStart = false;
                        transitionCounter = 0;
                        delayCounter = 0;
                        float scale = ZoomValue(zoom);
                        scale = scale <= 0 ? 1f : scale;
                        startPoint = camera.position;
                        targetEntry = targetEntryPoint;
                        transitionPoint.z = camera.position.z;
                        transitionPoint.x = targetEntryPoint.x;
                        transitionPoint.y = targetEntryPoint.y;
                        float camSizeX = !canZoom ? zoom.realTimeWidth : zoom.originalWidth * scale;
                        float camSizeY = !canZoom ? zoom.realTimeHeight : zoom.originalHeight * scale;

                        if (lazyClamp)
                        {
                                if (isClampedY)
                                        follow.ForceTargetSmooth(false, true);
                                if (isClampedX)
                                        follow.ForceTargetSmooth(true, false);
                                if (setZoom)
                                        SetZoom(zoom, delay == 0);
                                return false;
                        }

                        if (clampLeft && (targetEntryPoint.x - camSizeX) <= bounds.left)
                        {
                                transitionPoint.x = bounds.left + camSizeX;
                                foundX = true;
                        }
                        if (clampRight && (targetEntryPoint.x + camSizeX) >= bounds.right)
                        {
                                transitionPoint.x = bounds.right - camSizeX;
                                foundX = true;
                        }
                        if ((clampRight && clampLeft) && camSizeX >= bounds.width)
                        {
                                transitionPoint.x = bounds.center.x;
                                foundX = true;
                        }
                        if (clampBottom && (targetEntryPoint.y - camSizeY) <= bounds.bottom)
                        {
                                transitionPoint.y = bounds.bottom + camSizeY;
                                foundY = true;
                        }
                        if (clampTop && (targetEntryPoint.y + camSizeY) >= bounds.top)
                        {
                                transitionPoint.y = bounds.top - camSizeY;
                                foundY = true;
                        }
                        if ((clampTop && clampBottom) && camSizeY >= bounds.height)
                        {
                                transitionPoint.y = bounds.center.y;
                                foundY = true;
                        }

                        transitionX = transitionPoint.x != targetEntryPoint.x || foundX;
                        transitionY = transitionPoint.y != targetEntryPoint.y || foundY;

                        if (!transitionY && isClampedY)
                                follow.ForceTargetSmooth(false, true);
                        if (!transitionX && isClampedX)
                                follow.ForceTargetSmooth(true, false);
                        if (setZoom)
                                SetZoom(zoom, delay == 0);

                        return transitionX || transitionY;
                }

                public bool Transition (Camera camera, Vector3 previousCamera, Zoom zoom, Follow follow)
                {
                        if (holdTarget)
                        {
                                follow.targetTransform.position = targetEntry - new Vector3(follow.offset.x, follow.offset.y);
                                ;
                        }
                        if (delay > 0 && !Clock.TimerExpired(ref delayCounter, delay))
                        {
                                camera.transform.position = Tooly.SetPosition(startPoint, camera.transform);
                                return false;
                        }
                        if (!transitionStart)
                        {
                                transitionStart = true;
                                onTransitionStart.Invoke();
                        }
                        SetZoom(zoom, delay > 0);

                        transitionCounter += Time.deltaTime;
                        float ease = EasingFunction.Run(tween, transitionCounter, transitionTime);
                        float x = Mathf.Lerp(startPoint.x, transitionPoint.x, ease);
                        float y = Mathf.Lerp(startPoint.y, transitionPoint.y, ease);

                        if (!transitionX || !transitionY)
                        {
                                follow.Execute(); //                                          keep following target
                                Clamp(camera, previousCamera, !transitionX, !transitionY); //  still need to clamp
                        }

                        follow.SetCameraPosition(new Vector3(transitionX ? x : camera.transform.position.x, transitionY ? y : camera.transform.position.y));
                        if (transitionCounter >= transitionTime)
                                onTransitionComplete.Invoke();
                        if (transitionCounter >= transitionTime)
                                follow.ForceTargetSmooth();
                        return transitionCounter >= transitionTime;
                }

                public void Clamp (Camera camera, Vector3 previousCamera, bool clampX = true, bool clampY = true)
                {
                        isClampedX = false;
                        isClampedY = false;
                        Vector3 camCenter = camera.transform.position;
                        Vector3 origin = camCenter;

                        if (clampX)
                        {
                                Vector2 velocity = camCenter - previousCamera;
                                float camSizeX = camera.Width();
                                //float buffer = 2.5f;
                                // float rate = 0.98f;

                                if (lazyClamp)
                                {
                                        if (clampLeft && !lazyClampLeft && (camCenter.x - camSizeX) >= bounds.left)
                                        {
                                                lazyClampLeft = true;
                                        }
                                        if (clampRight && !lazyClampRight && (camCenter.x + camSizeX) <= bounds.right)
                                        {
                                                lazyClampRight = true;
                                        }
                                }

                                // if (canClampLeft && velocity.x < 0 && (camCenter.x - camSizeX) < bounds.left + buffer)
                                //         camCenter.x = Compute.Lerp(camCenter.x - velocity.x , bounds.left + camSizeX , rate);
                                // else if (canClampRight && velocity.x > 0 && (camCenter.x + camSizeX) > bounds.right - buffer)
                                //         camCenter.x = Compute.Lerp(camCenter.x - velocity.x , bounds.right - camSizeX , rate);

                                if (canClampLeft && (camCenter.x - camSizeX) < bounds.left)
                                        camCenter.x = bounds.left + camSizeX;
                                if (canClampRight && (camCenter.x + camSizeX) > bounds.right)
                                        camCenter.x = bounds.right - camSizeX;
                                if (canClampLeft && canClampRight && camSizeX > bounds.width)
                                        camCenter.x = bounds.center.x;
                        }

                        if (clampY)
                        {
                                float camSizeY = camera.Height();

                                if (lazyClamp)
                                {
                                        if (clampTop && !lazyClampTop && (camCenter.y + camSizeY) <= bounds.top)
                                        {
                                                lazyClampTop = true;
                                        }
                                        if (clampBottom && !lazyClampBottom && (camCenter.y - camSizeY) >= bounds.bottom)
                                        {
                                                lazyClampBottom = true;
                                        }
                                }

                                if (canClampTop && (camCenter.y + camSizeY) > bounds.top)
                                        camCenter.y = bounds.top - camSizeY;
                                if (canClampBottom && (camCenter.y - camSizeY) < bounds.bottom)
                                        camCenter.y = bounds.bottom + camSizeY;
                                if (canClampTop && canClampBottom && camSizeY > bounds.height)
                                        camCenter.y = bounds.center.y;
                        }
                        isClampedX = camCenter.x != origin.x;
                        isClampedY = camCenter.y != origin.y;
                        camera.transform.position = camCenter;
                }

                public float ZoomValue (Zoom zoom)
                {
                        return canZoom ? (zoomScale == ZoomScale.Zoom ? zoomValue : zoomScale == ZoomScale.MatchHeight ? bounds.height / zoom.originalHeight : bounds.width / zoom.originalWidth) : 1f;
                }

                public void SetZoom (Zoom zoom, bool execute = true)
                {
                        if (canZoom && execute && ZoomValue(zoom) > 0)
                        {
                                zoom.Set(ZoomValue(zoom), transitionTime, run: true); // will not reset zoom when locked
                                zoom.lockZoom = true;
                        }
                }

                public void RestrictTarget (Transform target)
                {
                        if (!restrict)
                                return;
                        Vector2 p = target.position;

                        if (!insideRestrictArea && bounds.DetectBounds(p, 1f))
                        {
                                insideRestrictArea = true;
                        }

                        if (!insideRestrictArea)
                                return;

                        if (canClampTop && p.y > (bounds.top - 1f))
                                target.position = Tooly.SetPosition(p.x, bounds.top - 1f, p);
                        if (canClampLeft && p.x < (bounds.left + 1f))
                                target.position = Tooly.SetPosition(bounds.left + 1f, p.y, p);
                        if (canClampRight && p.x > (bounds.right - 1f))
                                target.position = Tooly.SetPosition(bounds.right - 1f, p.y, p);
                        if (canClampBottom && p.y < (bounds.bottom + 1f))
                                target.position = Tooly.SetPosition(p.x, bounds.bottom + 1f, p);
                }

                public void ForceSmooth (Vector2 target, Follow follow)
                {
                        if (allClamped)
                        {
                                follow.ForceTargetSmooth();
                                return;
                        }
                        if (target.x < bounds.left && canClampLeft)
                        {
                                follow.ForceTargetSmooth();
                        }
                        if (target.x > bounds.right && canClampRight)
                        {
                                follow.ForceTargetSmooth();
                        }
                        if (target.y < bounds.bottom && canClampBottom)
                        {
                                follow.ForceTargetSmooth();
                        }
                        if (target.y > bounds.top && canClampTop)
                        {
                                follow.ForceTargetSmooth();
                        }
                }
        }

        public enum ZoomScale
        {
                Zoom,
                MatchHeight,
                MatchWidth
        }
}
