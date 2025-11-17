using System.Collections.Generic;
using TwoBitMachines.Editors;
using TwoBitMachines.MapSystem;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEngine;

namespace TwoBitMachines.FlareEngine.Editors
{
        [CustomEditor(typeof(Map))]
        public class MapEditor : UnityEditor.Editor
        {
                private Map main;
                private SerializedObject so;
                public string inputName = "Name";
                public static List<Vector2> debugPoints = new List<Vector2>();
                private List<string> sceneNames = new List<string>();
                public static SaveOptions save = new SaveOptions();

                private void OnEnable ()
                {
                        main = target as Map;
                        GetZone();
                        Layout.Initialize();
                        Undo.undoRedoPerformed += OnUndoRedoPerformed;

                        int sceneCount = Util.SceneCount();
                        sceneNames.Clear();
                        for (int i = 0; i < sceneCount; i++)
                        {
                                sceneNames.Add(Util.GetSceneName(i));
                        }
                }

                private void OnDisable ()
                {
                        Undo.undoRedoPerformed -= OnUndoRedoPerformed;
                }

                private void OnUndoRedoPerformed ()
                {
                        ResetMesh(false);
                }

                private void GetZone ()
                {
                        string currentSceneName = SceneManager.GetActiveScene().name;
                        for (int i = 0; i < main.map.zoneList.Count; i++)
                        {
                                if (main.map.zoneList[i].sceneName == currentSceneName)
                                {
                                        main.map.zoneIndex = i;
                                        return;
                                }
                        }
                }

                public override void OnInspectorGUI ()
                {
                        Layout.Update();
                        Layout.VerticalSpacing(10);

                        if (main.map == null)
                        {
                                serializedObject.Update();
                                Block.Box(1, Tint.Blue);
                                {
                                        serializedObject.Field_("Map", "map");
                                }
                                serializedObject.ApplyModifiedProperties();
                                return;
                        }
                        if (so == null || so.targetObject != main.map)
                        {
                                so = new SerializedObject(main.map);
                        }

                        so.Update();
                        serializedObject.Update();

                        SerializedProperty zoneList = so.Get("zoneList");
                        SerializedProperty zoneIndex = so.Get("zoneIndex");
                        SerializedProperty eventList = serializedObject.Get("eventList");
                        MapEditMode editMode = (MapEditMode) so.Enum("editMode");

                        serializedObject.Update();
                        EditorGUI.BeginChangeCheck();

                        Block.Box(2, Tint.Orange, extraHeight: 5);
                        {
                                serializedObject.FieldDouble_("Player, Icon", "player", "playerIcon");
                                serializedObject.Field_("On Open Object", "mapObject");
                        }

                        if (Block.ExtraFoldout(serializedObject, "eventFoldOut"))
                        {
                                Fields.EventFoldOut(serializedObject.Get("onOpen"), serializedObject.Get("openFoldOut"), "On Open", color: FoldOut.boxColorLight);
                                Fields.EventFoldOut(serializedObject.Get("onClose"), serializedObject.Get("closeFoldOut"), "On Close", color: FoldOut.boxColorLight);
                                Fields.EventFoldOut(serializedObject.Get("onRoomUnlocked"), serializedObject.Get("roomUnlockedFoldOut"), "On Room Unlocked", color: FoldOut.boxColorLight);
                                Layout.VerticalSpacing();
                        }

                        if (EditorGUI.EndChangeCheck())
                        {
                                serializedObject.ApplyModifiedProperties();
                        }

                        Block.Box(4, Tint.PurpleDark);
                        {
                                EditorGUI.BeginChangeCheck();
                                if (so.FieldAndButton_("Edit Mode", "editMode", "Add", toolTip: "Create Zone"))
                                {
                                        zoneList.arraySize++;
                                        SerializedProperty zone = zoneList.LastElement();
                                        EditorTools.ClearProperty(zone);
                                        zone.Get("name").stringValue = "New Zone";
                                        zone.Get("thickness").floatValue = 0.20f;
                                        zone.Get("outline").colorValue = new Color32(182, 221, 251, 255);
                                        zone.Get("background").colorValue = new Color32(47, 57, 67, 255);
                                        zoneIndex.intValue = zoneList.arraySize - 1;
                                }
                                if (EditorGUI.EndChangeCheck())
                                {
                                        ResetMesh(false);
                                }
                                if (so.FieldAndButton_("Map Name", "mapName", "Delete", toolTip: "Delete Map Saved Data"))
                                {
                                        DeleteSavedMapData();
                                }
                                so.FieldDouble_("On Open Input", "openInput", "pauseGame");
                                so.FieldAndEnable_("Track Player", "playerOffsetY", "trackPlayerRealTime");
                                Block.HelperText("Y Offset", rightSpacing: 19);
                        }

                        if (editMode == MapEditMode.EditZones)
                        {
                                Block.Box(2, Tint.PurpleDark);
                                {
                                        so.Slider_("Snap Size", "snapSize", round: 0.001f);
                                        so.Slider_("Gizmo Size", "gizmoSize", round: 0.01f);
                                }
                        }

                        for (int i = 0; i < zoneList.arraySize; i++)
                        {
                                if (zoneIndex.intValue == i || i == zoneList.arraySize - 1)
                                {
                                        if (editMode == MapEditMode.EditZones)
                                        {
                                                Zone(zoneList, eventList, i);
                                        }
                                        else
                                        {
                                                EditMapZone(zoneList, i);
                                        }
                                        break;
                                }
                        }

                        so.ApplyModifiedProperties();
                        Layout.VerticalSpacing(10);
                }

                private void EditMapZone (SerializedProperty zoneList, int i)
                {
                        SerializedProperty zone = i < zoneList.arraySize ? zoneList.Element(i) : null;

                        if (zone == null)
                                return;
                        string verts = " , Verts: " + main.map.meshData.vertices.Count.ToString();
                        Block.Header(zone).Style(Tint.PurpleDark)
                             .Label(zone.String("name") + verts, bold: true, color: Tint.White)
                             .Button("ArrowDown", ID: "Choose Zone", tooltip: "Choose Zone").Build();

                        if (Header.SignalActive("Choose Zone"))
                        {
                                ZoneContextMenu();
                                return;
                        }

                        SerializedProperty roomList = zone.Get("roomList");
                        for (int j = 0; j < roomList.arraySize; j++)
                        {
                                if (EditMapRoom(zone, roomList, j))
                                {
                                        break;
                                }
                        }
                }

                private bool EditMapRoom (SerializedProperty zone, SerializedProperty roomList, int i)
                {
                        SerializedProperty room = roomList.Element(i);
                        SerializedProperty roomIndex = zone.Get("roomIndex");
                        bool selected = roomIndex.intValue == i;

                        Block.Header(room).StylePlain(selected ? Tint.PurpleDark : Tint.SoftDark)
                                       .MouseDown(() => { roomIndex.intValue = i; Repaint(); })
                                       .Label("Room " + i.ToString()).Build();

                        return false;
                }

                private void Zone (SerializedProperty zoneList, SerializedProperty eventList, int i)
                {
                        SerializedProperty zone = zoneList.Element(i);

                        EditorGUI.BeginChangeCheck();
                        SerializedProperty eventRoomList = eventList.Get(i).Get("list");
                        if (EditorGUI.EndChangeCheck())
                        {
                                serializedObject.ApplyModifiedProperties();
                        }

                        string verts = " , Verts: " + main.map.meshData.vertices.Count.ToString();
                        Block.Header(zone).Style(Tint.PurpleDark)
                             .Label(zone.String("name") + verts, bold: true, color: Tint.White)
                             .HiddenButton("deleteAsk", "Delete")
                             .Toggle("deleteAsk", "DeleteAsk")
                             .Button(tooltip: "Add Room")
                             .Button("ArrowDown", ID: "Choose Zone", tooltip: "Choose Zone").Build();

                        Block.BoxPlain(5, Tint.PurpleDark, noGap: true);
                        {
                                zone.Field_("Zone Name", "name");
                                zone.DropDownList_(sceneNames, "For Scene", "sceneName");

                                EditorGUI.BeginChangeCheck();

                                zone.FieldDouble_("Colors", "outline", "background");
                                zone.Slider_("Thickness ", "thickness", 0.01f, 2f);
                                zone.Slider_("Resolution ", "resolution", 0.01f, 1f);
                        }

                        if (EditorGUI.EndChangeCheck())
                        {
                                ResetMesh();
                        }

                        SerializedProperty roomList = zone.Get("roomList");

                        if (Header.SignalActive("Add"))
                        {
                                roomList.arraySize++;
                                EditorTools.ClearProperty(roomList.LastElement());
                                zone.Get("roomIndex").intValue = roomList.arraySize - 1;
                                return;
                        }
                        if (Header.SignalActive("Choose Zone"))
                        {
                                ZoneContextMenu();
                                return;
                        }
                        if (Header.SignalActive("Delete"))
                        {
                                zoneList.DeleteArrayElement(i);
                                so.Get("zoneIndex").intValue = Mathf.Clamp(so.Get("zoneIndex").intValue - 1, 0, zoneList.arraySize - 1);
                                ResetMesh(false);
                                return;
                        }

                        for (int j = 0; j < roomList.arraySize; j++)
                        {
                                if (Room(zone, roomList, eventRoomList, j))
                                {
                                        break;
                                }
                        }
                }

                private bool Room (SerializedProperty zone, SerializedProperty roomList, SerializedProperty eventRoomList, int i)
                {
                        SerializedProperty room = roomList.Element(i);
                        SerializedProperty opacity = room.Get("opacity");
                        SerializedProperty roomIndex = zone.Get("roomIndex");

                        EditorGUI.BeginChangeCheck();
                        SerializedProperty events = eventRoomList.Get(i).Get("list");
                        if (EditorGUI.EndChangeCheck())
                        {
                                serializedObject.ApplyModifiedProperties();
                        }


                        bool previousOpacity = opacity.boolValue;
                        bool selected = roomIndex.intValue == i;

                        Header header = Block.Header(room).StylePlain(selected ? Tint.PurpleDark : Tint.SoftDark)
                                        .MouseDown(() => { roomIndex.intValue = i; Repaint(); })
                                        .Grip(this, roomList, i)
                                        .Label("Room " + i.ToString())
                                        .HiddenButton("deleteAsk", "Delete", execute: selected)
                                        .Toggle("deleteAsk", "DeleteAsk", execute: selected)
                                        .DropArrow("arrowFoldOut", execute: selected)
                                        .Toggle("opacity", opacity.boolValue ? "EyeClosed" : "EyeOpen")
                                        .Toggle("visible", room.Bool("visible") ? "LockOpenGreen" : "LockRed");

                        if (header.Build())
                        {
                                if (Header.SignalActive("Delete"))
                                {
                                        roomList.DeleteArrayElement(i);
                                        eventRoomList.Remove(i);
                                        serializedObject.ApplyModifiedProperties();
                                        return true;
                                }
                        }

                        if (Header.SignalActive("GripUsed"))
                        {
                                eventRoomList.Switch(BlockGrip.sourceUsed, BlockGrip.destinationUsed);
                                roomIndex.intValue = BlockGrip.destinationUsed;
                                serializedObject.ApplyModifiedProperties();
                        }

                        if (previousOpacity != opacity.boolValue)
                        {
                                ResetMesh(false);
                                return true;
                        }

                        if (selected && room.Bool("arrowFoldOut"))
                        {
                                Block.BoxPlain(1, Tint.PurpleDark, noGap: true);
                                {
                                        EditorGUI.BeginChangeCheck();
                                        room.FieldToggleAndEnable_("Round Ends", "useRoundEnds", toggleOffset: 13);
                                        if (EditorGUI.EndChangeCheck())
                                        {
                                                ResetMesh();
                                                return true;
                                        }
                                }
                                EditorGUI.BeginChangeCheck();
                                Block.BoxArray(events, Tint.PurpleDark, 23, false, 1, "Room Item", (height, index) =>
                                {
                                        Block.Header(events.Element(index))
                                        .BoxRect(Tint.PurpleDark, leftSpace: 5, height: height)
                                        .Field(events.Element(index))
                                        .ArrayButtons().BuildGet()
                                        .ReadArrayButtons(events, index);
                                });
                                if (EditorGUI.EndChangeCheck())
                                {
                                        serializedObject.ApplyModifiedProperties();
                                }

                        }

                        return false;
                }

                private void OnSceneGUI ()
                {
                        if (main.map == null)
                                return;

                        if (so == null || so.targetObject != main.map)
                        {
                                so = new SerializedObject(main.map);
                        }

                        so.Update();
                        Mouse.Update();

                        SerializedProperty zoneList = so.Get("zoneList");
                        SerializedProperty zoneIndex = so.Get("zoneIndex");
                        MapEditMode editMode = (MapEditMode) so.Enum("editMode");

                        if (zoneIndex.intValue >= 0 && zoneIndex.intValue < zoneList.arraySize)
                        {
                                SerializedProperty zone = zoneList.Element(zoneIndex.intValue);
                                SerializedProperty roomList = zone.Get("roomList");
                                SerializedProperty roomIndex = zone.Get("roomIndex");
                                Zone zoneClass = main.map.zoneList[main.map.zoneIndex];
                                if (roomIndex.intValue >= 0 && roomIndex.intValue < roomList.arraySize)
                                {
                                        Room roomClass = zoneClass.roomList[zoneClass.roomIndex];
                                        if (!roomClass.opacity)
                                        {
                                                if (editMode == MapEditMode.EditZones)
                                                {
                                                        SceneControls.EditRoom(main, roomList.Element(roomIndex.intValue), roomClass, zoneClass, so);
                                                }
                                                else
                                                {
                                                        SceneControls.EditRoomOffset(main, roomList.Element(roomIndex.intValue), roomList, roomClass, zoneClass, so);
                                                }
                                        }
                                }
                        }
                        so.ApplyModifiedProperties();
                        if (Event.current.type == EventType.Layout)
                        {
                                HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive)); // keep object selected, default control
                        }
                }

                public void ResetMesh (bool recalculateZone = true)
                {
                        main.map.zoneIndex = Mathf.Clamp(main.map.zoneIndex, 0, main.map.zoneList.Count - 1);
                        so.ApplyModifiedProperties();
                        if (recalculateZone)
                        {
                                PathUtil.cull = true;
                                Zone zoneClass = main.map.zoneList[main.map.zoneIndex];
                                SceneControls.RecalculateZone(zoneClass);
                        }

                        main.SetMeshRef();
                        if (main.map.editMode == MapEditMode.EditZones)
                        {
                                main.map.UpdateMeshEditor(main.mesh, main.meshFilter, main.meshRenderer);
                        }
                        else
                        {
                                main.map.UpdateMeshEditorShowAll(main.mesh, main.meshFilter, main.meshRenderer);
                        }
                        SceneView.RepaintAll();
                }

                [DrawGizmo(GizmoType.NotInSelectionHierarchy | GizmoType.InSelectionHierarchy)]
                public static void DrawFollowGizmos (Map main, GizmoType gizmoType)
                {
                        if (Application.isPlaying)
                                return;

                        Draw.GLStart();
                        for (int i = 0; i < debugPoints.Count; i++)
                        {
                                Draw.GLCircle(debugPoints[i], 0.15f, Color.red);
                        }
                        Draw.GLEnd();

                        Draw.GLStart();
                        for (int i = 0; i < SceneControls.debugPoints.Count; i++)
                        {
                                Draw.GLCircle(SceneControls.debugPoints[i], 0.15f, Color.red);
                        }
                        Draw.GLEnd();
                }

                private void ZoneContextMenu ()
                {
                        GenericMenu menu = new GenericMenu();
                        for (int i = 0; i < main.map.zoneList.Count; i++)
                        {
                                menu.AddItem(new GUIContent(main.map.zoneList[i].name), main.map.zoneIndex == i, ChooseContextMenu, i);
                        }
                        menu.ShowAsContext();
                }

                private void ChooseContextMenu (object obj)
                {
                        int index = (int) obj;
                        main.map.zoneIndex = index;
                        ResetMesh();
                }

                public void DeleteSavedMapData ()
                {
                        FlareEngine.SaveOptions.Load(ref save);
                        string saveFolder = save.RetrieveSaveFolder();
                        Storage.Delete(saveFolder, main.map.key);
                        Debug.Log("Deleted Map Saved Data");
                }
        }
}
