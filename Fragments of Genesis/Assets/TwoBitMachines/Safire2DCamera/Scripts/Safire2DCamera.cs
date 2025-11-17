using System.Collections.Generic;
using UnityEngine;

namespace TwoBitMachines.Safire2DCamera
{
        [AddComponentMenu("Safire2DCamera")]
        [RequireComponent(typeof(Camera))]
        public class Safire2DCamera : MonoBehaviour
        {
                [SerializeField] public Transform targetTransform;
                [SerializeField, HideInInspector] public Camera cameraRef;

                [SerializeField] public Zoom zoom = new Zoom();
                [SerializeField] public Peek peek = new Peek();
                [SerializeField] public Rooms rooms = new Rooms();
                [SerializeField] public Shakes shake = new Shakes();
                [SerializeField] public Follow follow = new Follow();
                [SerializeField] public Parallax parallax = new Parallax();
                [SerializeField] public PeekPlus peekPlus = new PeekPlus();
                [SerializeField] public ParallaxFinite parallaxFinite = new ParallaxFinite();
                [SerializeField] public ResolutionScaling resolutionScaling = new ResolutionScaling();
                [SerializeField] public SpeedZoom speedZoom = new SpeedZoom();
                [SerializeField] public Cinematics cinematics = new Cinematics();
                [SerializeField] public SlowMotion timeManager = new SlowMotion();
                [SerializeField] public WorldBounds worldBounds = new WorldBounds();
                [SerializeField] public WorldClamp worldClamp = new WorldClamp();

                [SerializeField] public PixelPerfect pixelPerfect;
                [SerializeField] public bool enablePixelPerfect = false;
                [SerializeField, HideInInspector] public bool pixelFoldOut = false;

                [SerializeField] public ZoomTrigger zoomTrigger = new ZoomTrigger();
                [SerializeField] public BasicTrigger basicTrigger = new BasicTrigger();
                [SerializeField] public SlowMotionTrigger slowMotionTrigger = new SlowMotionTrigger();

                [System.NonSerialized] public static List<Safire2DCamera> cameras = new List<Safire2DCamera>();
                [System.NonSerialized] private Vector3 previousCamera;
                [System.NonSerialized] bool pauseFollow = false;

                public static Safire2DCamera mainCamera;

                private void OnEnable ()
                {
                        if (!cameras.Contains(this))
                        {
                                cameras.Add(this);
                        }
                }

                private void OnDestroy ()
                {
                        if (cameras.Contains(this))
                        {
                                cameras.Remove(this);
                        }
                        follow.userZoom.SaveZoomLevel();
                }

                public static void ResetCameras ()
                {
                        for (int i = 0; i < cameras.Count; i++)
                        {
                                cameras[i]?.ResetAll();
                        }
                }

                public static void ForceFollowSmooth (Transform target)
                {
                        for (int i = 0; i < cameras.Count; i++)
                        {
                                if (cameras[i] != null && cameras[i].targetTransform == target)
                                {
                                        cameras[i].ForceFollowSmooth();
                                        return;
                                }
                        }
                }

                private void Awake ()
                {
                        cameraRef = cameraRef == null ? GetComponent<Camera>() : cameraRef;
                        if (cameraRef == null)
                        {
                                enabled = false;
                                Debug.LogWarning("Safire 2D Camera: main camera is missing. Initialization failed.");
                                return;
                        }
                        if (follow.followType == FollowType.Target && targetTransform == null)
                        {
                                Debug.LogWarning("Safire 2D Camera: target transform is missing.");
                                follow.followType = FollowType.user;
                        }

                        resolutionScaling.Execute(cameraRef);
                        GetPlayerDepth();
                        timeManager.Initialize();
                        worldClamp.Initialize(cameraRef);
                        zoom.Initialize(this);
                        speedZoom.Initialize(follow, zoom);
                        follow.Initialize(this);
                        CenterCameraOnTarget();
                        rooms.Initialize(cameraRef, follow, zoom);

                        cinematics.Initialize(this);
                        shake.Initialize(transform);
                        parallax.Initialize(cameraRef);
                        parallaxFinite.Initialize();
                        gameObject.AddComponent<Wiggle>();
                        zoomTrigger.Initialize(zoom);
                        slowMotionTrigger.Initialize(timeManager);
                        basicTrigger.Initialize();
                        pauseFollow = false;
                        previousCamera = transform.position;
                        mainCamera = this;
                        follow.userZoom.LoadZoomLevel(zoom, follow);
                }

                public void ResetAll ()
                {
                        zoom.Reset();
                        timeManager.Reset();
                        follow.Reset();
                        peek.Reset();
                        peekPlus.Reset();
                        cinematics.Reset();
                        parallax.Reset();
                        parallaxFinite.Reset();
                        speedZoom.Reset();
                        CenterCameraOnTarget();
                        shake.Reset(); // comes after camera set on target
                        rooms.Reset();
                        worldBounds.Reset();
                        worldClamp.Clamp(transform.position);
                        previousCamera = transform.position;
                        pauseFollow = false;
                }

                private void LateUpdate ()
                {
                        CameraUpdate();
                }

                /// <summary> Update all camera modules. </summary>
                public void CameraUpdate ()
                {
                        shake.RestoreCameraValues();

                        GetPlayerDepth();
                        timeManager.Execute();
                        speedZoom.Execute();
                        zoomTrigger.Execute(follow);
                        zoom.Execute();
                        rooms.RestrictTarget();
                        worldBounds.Exit(follow);

                        if (!cinematics.Execute() && !pauseFollow && !rooms.Execute(previousCamera))
                        {
                                follow.Execute();
                                worldBounds.Clamp(cameraRef, previousCamera, follow);
                                rooms.Clamp(cameraRef, previousCamera);
                                worldClamp.Clamp(previousCamera);
                                peek.Velocity(cameraRef, follow);
                                peekPlus.Velocity(cameraRef, follow);
                        }

                        slowMotionTrigger.Execute(follow);
                        basicTrigger.Execute(follow);
                        previousCamera = transform.position;
                        shake.Execute();
                        parallax.Execute(cameraRef);
                        parallaxFinite.Execute(cameraRef, follow);
                        pixelPerfect?.Execute();
                }

                private void GetPlayerDepth ()
                {
                        if (follow.followType == FollowType.Target && targetTransform != null)
                        {
                                Cammy.playerDepth = zoom.playerDepth = targetTransform.position.z;
                        }
                        else
                        {
                                Cammy.playerDepth = zoom.playerDepth = 0;
                        }
                }

                #region methods

                /// <summary>Enable a Module:
                /// Cinematics,  Follow, Peek, LookAhead, FollowBlocks, SpeedZoom, HighlightTarget, Regions, Rooms,
                /// Rails, Shake, Parallax, WorldClamp, BasicTrigger, ZoomTrigger, SlowMotionTrigger, UserPan, UserZoom, UserRotate.
                ///</summary>
                public void ModuleEnable (string moduleName)
                {
                        Module(moduleName, true);
                }

                /// <summary>Disable a Module:
                /// Cinematics,  Follow, Peek, LookAhead, FollowBlocks, SpeedZoom, HighlightTarget, Regions, Rooms,
                /// Rails, Shake, Parallax, WorldClamp, BasicTrigger, ZoomTrigger, SlowMotionTrigger, UserPan, UserZoom, UserRotate.
                ///</summary>
                public void ModuleDisable (string moduleName)
                {
                        Module(moduleName, false);
                }

                /// <summary>Enable or Disable a Module:
                /// Cinematics,  Follow, Peek, LookAhead, FollowBlocks, SpeedZoom, HighlightTarget, Regions, Rooms,
                /// Rails, Shake, Parallax, WorldClamp, BasicTrigger, ZoomTrigger, SlowMotionTrigger, UserPan, UserZoom, UserRotate.
                ///</summary>
                public void Module (string moduleName, bool enable)
                {
                        if (moduleName == "Follow")
                                follow.enable = enable;
                        else if (moduleName == "Rooms")
                                rooms.enable = enable;
                        else if (moduleName == "Peek")
                                peek.enable = enable;
                        else if (moduleName == "PeekPlus")
                                peekPlus.enable = enable;
                        else if (moduleName == "LookAhead")
                                follow.lookAhead.enable = enable;
                        else if (moduleName == "SpeedZoom")
                                speedZoom.enable = enable;
                        else if (moduleName == "FollowBlocks")
                                follow.followBlocks.enable = enable;
                        else if (moduleName == "HighlightTarget")
                                follow.highlightTarget.enable = enable;
                        else if (moduleName == "Cinematics")
                                cinematics.enable = enable;
                        else if (moduleName == "Rails")
                                follow.rails.enable = enable;
                        else if (moduleName == "Regions")
                                follow.regions.enable = enable;
                        else if (moduleName == "WorldClamp")
                                worldClamp.enable = enable;
                        else if (moduleName == "WorldBounds")
                                worldBounds.enable = enable;
                        else if (moduleName == "Shake")
                                shake.enable = enable;
                        else if (moduleName == "Parallax")
                                parallax.enable = enable;
                        else if (moduleName == "ParallaxFinite")
                                parallaxFinite.enable = enable;
                        else if (moduleName == "ZoomTrigger")
                                zoomTrigger.enable = enable;
                        else if (moduleName == "SlowMotionTrigger")
                                slowMotionTrigger.enable = enable;
                        else if (moduleName == "BasicTrigger")
                                basicTrigger.enable = enable;
                        else if (moduleName == "UserPan")
                                follow.userPan.enable = enable;
                        else if (moduleName == "UserZoom")
                                follow.userZoom.enable = enable;
                        else if (moduleName == "UserRotate")
                                follow.userRotate.enable = enable;

                        if (moduleName == "Rooms")
                                ForceFollowSmooth();
                }

                public void PauseFollowMechanics (bool enable)
                {
                        pauseFollow = enable;
                }

                public void RefreshParallaxImage (int index)
                {
                        parallax.RefreshParallaxImage(index);
                }

                /// <summary> Change the target transform.</summary>
                public void ChangeTargetTransform (Transform newTransform)
                {
                        targetTransform = newTransform;
                        follow.targetTransform = newTransform;
                        // do a camera refresh here
                }

                /// <summary> Center camera on target.</summary>
                public void CenterCameraOnTarget ()
                {
                        follow.SetCameraPosition(follow.TargetPosition());
                }

                /// <summary> Set the camera position.</summary>
                public void SetCameraPosition (Vector3 position)
                {
                        if (follow.cameraTransform != null)
                        {
                                follow.SetCameraPosition(position);
                                follow.previousTarget = follow.TargetPosition();
                                shake.previousPosition = this.transform.position;
                                shake.previousAngle = this.transform.eulerAngles;
                        }
                        else
                        {
                                this.transform.position = new Vector3(position.x, position.y, this.transform.position.z);
                                follow.previousTarget = this.transform.position;
                                shake.previousPosition = this.transform.position;
                                shake.previousAngle = this.transform.eulerAngles;
                        }
                }

                /// <summary> The speed by which the camera follows the target.</summary>
                public void SetFollowSpeed (float speed)
                {
                        follow.speed = Mathf.Clamp(speed, 0.0001f, Mathf.Infinity);
                }

                /// <summary> The smoothness by which the camera follows the target.</summary>
                public void SetFollowSmooth (Vector2 smooth)
                {
                        follow.smoothX = Mathf.Clamp(smooth.x, 0, 1);
                        follow.smoothY = Mathf.Clamp(smooth.y, 0, 1);
                }

                /// <summary> The smoothness by which the camera follows the target.</summary>
                public void ForceFollowSmooth ()
                {
                        follow.ForceTargetSmooth();
                }

                /// <summary> Set Autoscroll speed.</summary>
                public void SetAutoScrollSpeed (Vector2 speed)
                {
                        follow.autoScroll = speed;
                }

                /// <summary> Pause Autoscroll.</summary>
                public void PauseAutoScroll (bool enable)
                {
                        follow.pauseAutoscroll = enable;
                }

                /// <summary> Set Detect Walls: 0 == None, 1 == IgnoreGravity, 2 == DetectWalls </summary>
                public void DetectWallsSet (int key)
                {
                        if (key == 0)
                        {
                                follow.ForceTargetSmooth();
                        }
                        follow.detectWalls.Set(key);
                }

                /// <summary> Enable a room's restriction option. </summary>
                public void RoomRestrict (string roomName)
                {
                        for (int i = 0; i < rooms.rooms.Count; i++)
                        {
                                if (rooms.rooms[i].name == roomName)
                                {
                                        rooms.rooms[i].restrict = true;
                                        break;
                                }
                        }
                }

                public void WorldBoundsPause (int index, bool value)
                {
                        worldBounds.Pause(index, value);
                }

                /// <summary> Disable a room's restriction option. </summary>
                public void RoomUnrestrict (string roomName)
                {
                        for (int i = 0; i < rooms.rooms.Count; i++)
                        {
                                if (rooms.rooms[i].name == roomName)
                                {
                                        rooms.rooms[i].restrict = false;
                                        break;
                                }
                        }
                }

                /// <summary> Add a transform to the multiple target's list. </summary>
                public void RoomAddMultipleTarget (string roomName, Transform transform)
                {
                        for (int i = 0; i < rooms.rooms.Count; i++)
                        {
                                if (rooms.rooms[i].name == roomName)
                                {
                                        rooms.rooms[i].multipleTargets.AddTarget(transform, Vector2.zero);
                                        break;
                                }
                        }
                }

                /// <summary> Remove a transform from the multiple target's list. </summary>
                public void RoomRemoveMultipleTarget (string roomName, Transform transform)
                {
                        for (int i = 0; i < rooms.rooms.Count; i++)
                        {
                                if (rooms.rooms[i].name == roomName)
                                {
                                        rooms.rooms[i].multipleTargets.RemoveTarget(transform);
                                        break;
                                }
                        }
                }

                /// <summary> Start a cinematic sequence.</summary>
                public void CinematicTrigger (string cinematicName)
                {
                        cinematics.TriggerCinematic(cinematicName, this.transform.position);
                }

                /// <summary> If the cinematic sequence has been halted, move to the next target in the cinematic sequence.</summary>
                public void CinematicNextTarget ()
                {
                        cinematics.NextCinematicTarget();
                }

                public void CinematicPause (string cinematicName)
                {
                        cinematics.CinematicPause(cinematicName);
                }

                /// <summary>Pause or unpause an active auto rail.</summary>
                public void RailPause (bool value)
                {
                        follow.rails.RailPause(value);
                }

                /// <summary> Shake the camera. </summary>
                public void Shake (string shakeName)
                {
                        shake.Shake(shakeName);
                }

                public void ShakeSmall ()
                {
                        shake.Set(ShakeType.Random, 1f, 0.1f, 0.5f, false, TimeScale.TimeScale, Vector2.one);
                }

                public void Shake ()
                {
                        shake.Set(ShakeType.Random, 1f, 0.25f, 1f, false, TimeScale.TimeScale, Vector2.one);
                }

                /// <summary> Create a Random shake.  </summary>
                public void ShakeRandom (float duration, Vector3 Amplitude, float strength = 1, float speed = 1, bool useTimeScale = true, bool constant = false)
                {
                        shake.Set(ShakeType.Random, Mathf.Clamp(speed, 0, 1), Mathf.Clamp(strength, 0, 1), Mathf.Clamp(duration, 0, Mathf.Infinity), constant, useTimeScale ? TimeScale.TimeScale : TimeScale.TimeUnscaled, Amplitude);
                }

                /// <summary> Create a Perlin shake. The shake algorithm will be random but smooth. </summary>
                public void ShakePerlin (float duration, Vector3 Amplitude, float strength = 1, float speed = 1, bool useTimeScale = true, bool constant = false)
                {
                        shake.Set(ShakeType.Perlin, Mathf.Clamp(speed, 0, 1), Mathf.Clamp(strength, 0, 1), Mathf.Clamp(duration, 0, Mathf.Infinity), constant, useTimeScale ? TimeScale.TimeScale : TimeScale.TimeUnscaled, Amplitude);
                }

                /// <summary> Create a Sine shake. The shake algorithm will be sinusoidal. </summary>
                public void ShakeSine (float duration, Vector3 Amplitude, float strength = 1, float speed = 1, bool useTimeScale = true, bool constant = false)
                {
                        shake.Set(ShakeType.Sine, Mathf.Clamp(speed, 0, 1), Mathf.Clamp(strength, 0, 1), Mathf.Clamp(duration, 0, Mathf.Infinity), constant, useTimeScale ? TimeScale.TimeScale : TimeScale.TimeUnscaled, Amplitude);
                }

                /// <summary> Create a One-Shot shake. This is a quick shake, displacing the camera in the specified direction. It's basically a recoil shake for bullets. </summary>
                public void ShakeOneShot (float duration, Vector2 direction, float strength = 1, bool useTimeScale = true)
                {
                        shake.Set(ShakeType.OneShot, 1, strength, Mathf.Clamp(duration, 0, Mathf.Infinity), false, useTimeScale ? TimeScale.TimeScale : TimeScale.TimeUnscaled, direction);
                }

                /// <summary>
                /// A Single shake should be used for frequent shakes with short durations such as bullet impacts. Numerous shakes can still occur -- it is only called Single because the shakes are grouped together and executed concurrently in a single state.
                /// To work effectively, the shakes in this group should have strengths much lower than 1.
                /// </summary>
                public void ShakeIsSingle (float duration, Vector3 Amplitude, float strength = 0.1f, float speed = 1, bool useTimeScale = true)
                {
                        shake.Set(ShakeType.SingleShake, Mathf.Clamp(speed, 0, 1), Mathf.Clamp(strength, 0, Mathf.Infinity), Mathf.Clamp(duration, 0, Mathf.Infinity), false, useTimeScale ? TimeScale.TimeScale : TimeScale.TimeUnscaled, Amplitude);
                }

                /// <summary> Stop the current constant shake.</summary>
                public void TurnOffConstantShake ()
                {
                        shake.TurnOffConstantShake();
                }

                /// <summary>Stop all shakes. </summary>
                public void TurnOffAllShakes ()
                {
                        shake.TurnOffAllShakes();
                }

                public void PeekUp ()
                {
                        peek.PeekUp();
                }
                public void PeekDown ()
                {
                        peek.PeekDown();
                }

                public void PeekLeft ()
                {
                        peek.PeekLeft();
                }

                public void PeekRight ()
                {
                        peek.PeekRight();
                }

                public void PeekDirection (Vector2 direction)
                {
                        peek.PeekDirection(direction);
                }

                /// <summary>Zoom the camera.</summary>
                public void Zoom (float scale, float duration)
                {
                        zoom.Set(scale: scale, duration: duration);
                }

                /// <summary>Zoom the camera with a duration of 1 second.</summary>
                public void Zoom (float scale)
                {
                        zoom.Set(scale: scale, duration: 1f);
                }

                /// <summary>
                /// Add slow motion to your game. During this process, Time.timeScale will gradually increase to one.
                /// If set to constant, you need to manually turn off slow motion with TurnOffSlowMotion ().
                /// </summary>
                public void SlowMotion (float intensity, float duration, bool constant = false)
                {
                        timeManager.Set(Mathf.Clamp(intensity, 0, 1f), duration, constant);
                }

                /// <summary>Turn off slow motion.</summary>
                public void TurnOffSlowMotion ()
                {
                        timeManager.Reset();
                }

                public void SlowMotionHitImpact (float intensity)
                {
                        timeManager.Set(Mathf.Clamp(intensity, 0, 1f), 0.75f, false);
                }

                /// <summary> Camera control will switch from the player to the specified position and will follow with the specified speed.</summary>
                public void HighlightTarget (string name)
                {
                        follow.highlightTarget.Trigger(name, follow);
                }

                /// <summary> Camera control will switch from the player to the specified position and will follow with the specified speed.</summary>
                public void HighlightTarget (Transform transform, float duration, float speed, float yOffset = 0, float range = 0, bool stay = false, bool ignoreClamps = true)
                {
                        follow.highlightTarget.Set(TargetTypes.Transform, transform, Vector3.zero, duration, speed, yOffset, range, stay, ignoreClamps, follow);
                }

                /// <summary> Camera control will switch from the player to the specified position and will follow with the specified speed.</summary>
                public void HighlightTarget (Vector2 position, float duration, float speed, float yOffset = 0, float range = 0, bool stay = false, bool ignoreClamps = true)
                {
                        follow.highlightTarget.Set(TargetTypes.Position, null, position, duration, speed, yOffset, range, stay, ignoreClamps, follow);
                }

                public void HighlightTargetTerminate ()
                {
                        follow.highlightTarget.Reset();
                        follow.ForceTargetSmooth();
                }

                public void HighlightTargetTerminateOnTargetMove ()
                {
                        follow.highlightTarget.ExitOnMove();
                }

                /// <summary>Pause user. This only works if the camera engine is being directly controlled by the user.</summary>
                public void PauseUser (bool pausePan, bool pauseZoom, bool pauseRotate)
                {
                        follow.userPan.pause = pausePan;
                        follow.userZoom.pause = pauseZoom;
                        follow.userRotate.pause = pauseRotate;
                }

                /// <summary>Change the user keyboard settings for pan movement. </summary> 
                public void UserPanKeyboard (KeyCode left, KeyCode right, KeyCode up, KeyCode down)
                {
                        follow.userPan.up = up;
                        follow.userPan.down = down;
                        follow.userPan.left = left;
                        follow.userPan.right = right;
                }

                /// <summary> Change the user mouse settings for pan movement.</summary> 
                public void UserPanMouse (Pan panType, MouseButton mouseButton, MouseButton holdButton)
                {
                        follow.userPan.pan = panType;
                        follow.userPan.mouseButton = mouseButton;
                        follow.userPan.holdButton = holdButton;
                }

                /// <summary> Pan camera horizontally.</summary> 
                public void UserPanHorizontal (float direction)
                {
                        Vector2 speed = Vector2.one * follow.speed;
                        follow.userPan.velocityTarget.x = speed.x * direction;
                }

                /// <summary> Pan camera vertically.</summary> 
                public void UserPanVertical (float direction)
                {
                        Vector2 speed = Vector2.one * follow.speed;
                        follow.userPan.velocityTarget.y = speed.y * direction;
                }

                /// <summary>Change the user keyboard settings for zooming.</summary> 
                public void UserZoomKeyboard (KeyCode zoomInKey, KeyCode zoomOutKey)
                {
                        follow.userZoom.ModifyKeyboard(zoomInKey, zoomOutKey);
                }

                /// <summary> Change the user keyboard settings for camera rotation. </summary> 
                public void UserRotateKeyboard (KeyCode rotateLeftKey, KeyCode rotateRightKey)
                {
                        follow.userRotate.rotateLeft = rotateLeftKey;
                        follow.userRotate.rotateRight = rotateRightKey;
                }

                /// <summary>Camera is reset to original size.</summary>
                public void ZoomReset ()
                {
                        zoom.Reset();
                }

                public void ZoomSetToOne ()
                {
                        zoom.Set(1f, 1f);
                }

                #endregion
        }
}
