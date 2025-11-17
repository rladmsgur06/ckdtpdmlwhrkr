using TwoBitMachines.Editors;
using UnityEditor;
using UnityEngine;

namespace TwoBitMachines.Safire2DCamera.Editors
{
        [CustomEditor(typeof(Safire2DCamera))]
        public class CameraEngineEditor : UnityEditor.Editor
        {
                private Safire2DCamera main;
                private SerializedObject parent;
                public static Vector3 V = Vector3.up;
                public static Vector3 H = Vector3.right;

                private string[] modules = new string[]
                {
                                    "cinematics",
                                    "followBlocks",
                                    "highlightTarget",
                                    "lookAhead",
                                    "parallax",
                                    "parallaxFinite",
                                    "peek",
                                    "peekPlus",
                                    "rails",
                                    "regions",
                                    "rooms",
                                    "speedZoom",
                                    "shake",
                                    "userPan",
                                    "userRotate",
                                    "userZoom",
                                    "zoomTrigger",
                                    "slowMotionTrigger",
                                    "basicTrigger",
                                    "worldBounds",
                                    "worldClamp",
                                    "resolutionScaling"
                };

                private void OnEnable ()
                {
                        main = target as Safire2DCamera;
                        parent = serializedObject;
                        Layout.Initialize();

                        MonoScript monoScript = MonoScript.FromMonoBehaviour(main); // Execute script last.
                        if (MonoImporter.GetExecutionOrder(monoScript) != 20000)
                                MonoImporter.SetExecutionOrder(monoScript, 20000);

                }

                public override void OnInspectorGUI ()
                {
                        Layout.Update();
                        Layout.VerticalSpacing(10);

                        if (main.cameraRef == null)
                        {
                                main.cameraRef = main.gameObject.GetComponent<Camera>();
                        }

                        parent.Update();
                        {
                                Color barColor = Tint.BoxTwo;
                                Color labelColor = Color.black;
                                SerializedProperty follow = parent.Get("follow");

                                int type = follow.Enum("followType");
                                int height = type == 0 ? 1 : 0;
                                int is3D = main.cameraRef != null && !main.cameraRef.orthographic ? 1 : 0;

                                FoldOut.Box(1 + height + is3D, barColor);
                                {
                                        follow.Field("Follow", "followType", bold: true);
                                        parent.Field("", "targetTransform", execute: height == 1);
                                        parent.Get("zoom").Field("3D Zoom", "type", execute: is3D == 1);
                                }
                                Layout.VerticalSpacing(20);

                                Follow.CustomInspector(follow, type == 1, barColor, labelColor);
                                Peek.CustomInspector(parent.Get("peek"), barColor, labelColor);
                                PeekPlus.CustomInspector(parent.Get("peekPlus"), barColor, labelColor);
                                LookAhead.CustomInspector(follow.Get("lookAhead"), barColor, labelColor);
                                Rails.CustomInspector(follow.Get("rails"), barColor, labelColor, main);
                                FollowBlocks.CustomInspector(follow.Get("followBlocks"), barColor, labelColor);
                                HighlightTarget.CustomInspector(follow.Get("highlightTarget"), barColor, labelColor);
                                Regions.CustomInspector(follow.Get("regions"), barColor, labelColor);
                                Rooms.CustomInspector(parent.Get("rooms"), barColor, labelColor);
                                Cinematics.CustomInspector(parent.Get("cinematics"), barColor, labelColor);
                                Shakes.CustomInspector(parent.Get("shake"), barColor, labelColor, main.shake, main);
                                Parallax.CustomInspector(parent.Get("parallax"), barColor, labelColor);
                                ParallaxFinite.CustomInspector(parent.Get("parallaxFinite"), barColor, labelColor);
                                SpeedZoom.CustomInspector(parent.Get("speedZoom"), barColor, labelColor);

                                UserPan.CustomInspector(follow.Get("userPan"), barColor, labelColor);
                                UserZoom.CustomInspector(follow.Get("userZoom"), barColor, labelColor);
                                UserRotate.CustomInspector(follow.Get("userRotate"), barColor, labelColor);
                                WorldBounds.CustomInspector(parent.Get("worldBounds"), barColor, labelColor);
                                WorldClamp.CustomInspector(parent.Get("worldClamp"), barColor, labelColor);
                                ZoomTrigger.CustomInspector(parent.Get("zoomTrigger"), barColor, labelColor);
                                SlowMotionTrigger.CustomInspector(parent.Get("slowMotionTrigger"), barColor, labelColor);
                                BasicTrigger.CustomInspector(parent.Get("basicTrigger"), barColor, labelColor);
                                ResolutionScaling.CustomInspector(parent.Get("resolutionScaling"), barColor, labelColor, main);
                                PixelPerfectBar(Tint.Orange, Tint.WarmWhite);
                                CreateDragAndDropArea();
                        }
                        parent.ApplyModifiedProperties();
                }

                private void OnSceneGUI ()
                {
                        if (main.cameraRef == null)
                                main.cameraRef = main.GetComponent<Camera>();
                        if (main.cameraRef == null)
                                return;

                        parent.Update();
                        DrawModules(main, this, true);
                        DrawFollow(main);
                        parent.ApplyModifiedProperties();
                }

                public static void DrawModules (Safire2DCamera main, UnityEditor.Editor editor, bool drawHandles)
                {
                        if (!drawHandles)
                                SceneTools.blockHandles = true;
                        Layout.Update();
                        Layout.Update();
                        {
                                Color previousColor = Handles.color;
                                Handles.color = Color.white;
                                Regions.DrawTrigger(main, editor);
                                BasicTrigger.DrawTrigger(main);
                                SlowMotionTrigger.DrawTrigger(main);
                                ZoomTrigger.DrawTrigger(main);
                                FollowBlocks.DrawTrigger(main);
                                Cinematics.DrawCinematics(main, editor);
                                Rails.DrawRails(main, editor);
                                Rooms.DrawRooms(main);
                                WorldBounds.DrawBounds(main, editor);
                                WorldClamp.DrawTrigger(main);
                                Handles.color = previousColor;
                        }
                        if (Event.current.type == EventType.Layout)
                        {
                                HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive)); // keep object selected, default control
                        }
                        SceneTools.blockHandles = false;
                }

                private void PixelPerfectBar (Color barColor, Color labelColor)
                {

                        bool foldOut = FoldOut.Bar(parent, barColor, -3)
                                    .BL("enablePixelPerfect", on: Tint.PastelGreen, off: Tint.Box)
                                    .SL(3)
                                    .Label("Pixel Perfect", labelColor)
                                    .FoldOut("pixelFoldOut");

                        if (!parent.Bool("enablePixelPerfect") && main.GetComponent<PixelPerfect>() != null)
                        {
                                parent.Get("pixelPerfect").objectReferenceValue = null;
                                DestroyImmediate(main.GetComponent<PixelPerfect>());
                        }
                        if (parent.Bool("enablePixelPerfect"))
                        {
                                if (main.GetComponent<PixelPerfect>() == null)
                                {
                                        PixelPerfect pixelPerfect = main.transform.gameObject.AddComponent<PixelPerfect>();
                                        parent.Get("pixelPerfect").objectReferenceValue = pixelPerfect;
                                        pixelPerfect.hideFlags = HideFlags.HideInInspector;
                                }
                                if (foldOut)
                                {
                                        PixelPerfect.CustomInspector(new SerializedObject(parent.Get("pixelPerfect").objectReferenceValue), barColor, labelColor);
                                }
                        }
                }

                private void CreateDragAndDropArea ()
                {
                        Layout.VerticalSpacing(2);
                        Rect dropArea = Layout.CreateRectAndDraw(Layout.longInfoWidth, 27, offsetX: -11, texture: Icon.Get("RoomExit"), color: Color.white);
                        Labels.Centered(dropArea, "MODULE +", Color.white, fontSize: 11, shiftY: 0);
                        if (dropArea.ContainsMouseDown(false))
                        {
                                GenericMenu menu = new GenericMenu();
                                for (int i = 0; i < modules.Length; i++)
                                {
                                        if (!ModuleEnabled(modules[i]))
                                        {
                                                menu.AddItem(new GUIContent(Util.ToProperCase(modules[i])), false, AddModule, modules[i]);
                                        }
                                }
                                menu.ShowAsContext();
                        }
                }

                public void AddModule (object obj)
                {
                        string module = (string) obj;
                        serializedObject.Update();
                        SerializedProperty property = this.serializedObject.Get(module);
                        if (property != null)
                        {
                                property.SetTrue("edit");
                        }
                        else
                        {
                                property = this.serializedObject.Get("follow").Get(module);
                                if (property != null)
                                        property.SetTrue("edit");
                        }
                        serializedObject.ApplyModifiedProperties();
                }

                public bool ModuleEnabled (string module)
                {
                        SerializedProperty property = this.serializedObject.Get(module);
                        if (property != null)
                                return property.Bool("edit");
                        property = this.serializedObject.Get("follow").Get(module);
                        return property != null ? property.Bool("edit") : false;
                }

                public static void DrawFollow (Safire2DCamera main)
                {
                        Vector3 cameraPosition = Tooly.SetDepth(main.cameraRef.transform.position, 0);
                        float height = main.cameraRef.Height();
                        float width = main.cameraRef.Width();

                        // Position and Dead Zone
                        if (main.follow.view && main.follow.enable && main.targetTransform != null && main.follow.followType == FollowType.Target)
                        {
                                main.follow.targetTransform = main.targetTransform;
                                Vector3 playerPosition = main.targetTransform.position + (Vector3) main.follow.offset;
                                Vector3 deadZone = Compute.Abs(main.follow.deadZone.size);
                                Handles.color = Color.red;
                                if (deadZone.x != 0 || deadZone.y != 0)
                                {
                                        if (!EditorApplication.isPlaying)
                                        {
                                                Handles.DrawWireCube(playerPosition, Tooly.SetPosition(deadZone.x, deadZone.y, deadZone));
                                        }
                                        else
                                        {
                                                if (main.follow.deadZone.originalSize != main.follow.deadZone.size)
                                                        main.follow.deadZone.Set(main.follow.TargetPosition());
                                                Vector2 oldPlayerPosition = playerPosition;
                                                playerPosition = main.follow.deadZone.center;
                                                playerPosition.x = main.follow.deadZone.size.x == 0 ? oldPlayerPosition.x : playerPosition.x;
                                                playerPosition.y = main.follow.deadZone.size.y == 0 ? oldPlayerPosition.y : playerPosition.y;
                                                Handles.DrawWireCube(playerPosition, Tooly.SetPosition(deadZone.x, deadZone.y, deadZone));
                                        }
                                }
                                SceneTools.Circle(playerPosition, 0.1f);
                        }

                        // Screen Zone
                        if (main.follow.view && main.follow.enable && main.follow.screenZone.size != Vector2.zero)
                        {
                                Vector2 screenZone = main.follow.screenZone.size;
                                Handles.color = Color.red;
                                float offsetY = screenZone.y == 0 ? 0 : (1 - screenZone.y) * height;
                                float offsetX = screenZone.x == 0 ? 0 : (1 - screenZone.x) * width;
                                if (screenZone.x > 0)
                                        SceneTools.DottedLine(cameraPosition + V * (height - offsetY) + H * width * screenZone.x, cameraPosition - V * (height - offsetY) + H * width * screenZone.x);
                                if (screenZone.x > 0)
                                        SceneTools.DottedLine(cameraPosition + V * (height - offsetY) - H * width * screenZone.x, cameraPosition - V * (height - offsetY) - H * width * screenZone.x);
                                if (screenZone.y > 0)
                                        SceneTools.DottedLine(cameraPosition - V * height * screenZone.y - H * (width - offsetX), cameraPosition - V * height * screenZone.y + H * (width - offsetX));
                                if (screenZone.y > 0)
                                        SceneTools.DottedLine(cameraPosition + V * height * screenZone.y - H * (width - offsetX), cameraPosition + V * height * screenZone.y + H * (width - offsetX));
                        }

                        //Push Zone
                        if (main.follow.view && main.follow.enable && (main.follow.pushZone.enabledX || main.follow.pushZone.enabledY))
                        {
                                Handles.color = Color.magenta;
                                float distanceX = main.follow.pushZone.zoneX;
                                float distanceY = main.follow.pushZone.zoneY;
                                if (main.follow.pushZone.right)
                                {
                                        SceneTools.DottedLine(cameraPosition + V * height + H * (width * distanceX), cameraPosition - V * height + H * (width * distanceX));
                                }
                                if (main.follow.pushZone.left)
                                {
                                        SceneTools.DottedLine(cameraPosition + V * height - H * (width * distanceX), cameraPosition - V * height - H * (width * distanceX));
                                }
                                if (main.follow.pushZone.up)
                                {
                                        SceneTools.DottedLine(cameraPosition + V * (height * distanceY) - H * width, cameraPosition + V * (height * distanceY) + H * width);
                                }
                                if (main.follow.pushZone.down)
                                {
                                        SceneTools.DottedLine(cameraPosition - V * (height * distanceY) - H * width, cameraPosition - V * (height * distanceY) + H * width);
                                }
                        }

                        //Look Ahead
                        if (main.follow.lookAhead.enable && main.follow.lookAhead.view && main.follow.enable)
                        {
                                Handles.color = Color.blue;
                                float distanceY = main.follow.lookAhead.distanceY;
                                float distanceX = main.follow.lookAhead.distanceX;

                                if (main.follow.lookAhead.zoneType == ZoneShape.Circle)
                                {
                                        float shortestDistance = width < height ? width : height;
                                        float magnitude = shortestDistance * main.follow.lookAhead.radius;
                                        SceneTools.WireDisc(cameraPosition, Vector3.forward, magnitude, Color.blue);
                                }
                                else
                                {
                                        if (distanceX > 0)
                                                SceneTools.Line(cameraPosition + V * height + H * width * distanceX, cameraPosition - V * height + H * width * distanceX);
                                        if (distanceX > 0)
                                                SceneTools.Line(cameraPosition + V * height - H * width * distanceX, cameraPosition - V * height - H * width * distanceX);
                                        if (distanceY > 0)
                                                SceneTools.Line(cameraPosition - V * height * distanceY - H * width, cameraPosition - V * height * distanceY + H * width);
                                        if (distanceY > 0)
                                                SceneTools.Line(cameraPosition + V * height * distanceY - H * width, cameraPosition + V * height * distanceY + H * width);
                                }
                        }
                }

                [DrawGizmo(GizmoType.NotInSelectionHierarchy)]
                public static void DrawFollowGizmos (Safire2DCamera main, GizmoType gizmoType)
                {
                        DrawModules(main, null, false);
                        if (main.cameraRef == null)
                                main.cameraRef = main.GetComponent<Camera>();
                        if (main.cameraRef == null || !main.follow.enable)
                                return; // || Camera.current.name == "SceneCamera" || Camera.current.name != main.camera.name 

                        if (main.follow.followType != FollowType.Target)
                        {
                                if (EditorApplication.isPlaying)
                                {
                                        Handles.color = Color.red;
                                        SceneTools.Circle(main.follow.CameraPosition, 0.1f);
                                }
                                return;
                        }
                        DrawFollow(main);
                }

        }

}
