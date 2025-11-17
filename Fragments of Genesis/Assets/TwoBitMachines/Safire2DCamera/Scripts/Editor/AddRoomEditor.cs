using TwoBitMachines.Editors;
using UnityEditor;
using UnityEngine;

namespace TwoBitMachines.Safire2DCamera.Editors
{
        [CustomEditor(typeof(AddRoom))]
        public class AddRoomEditor : UnityEditor.Editor
        {
                private AddRoom room;
                private SerializedObject parent;

                private void OnEnable ()
                {
                        room = target as AddRoom;
                        parent = serializedObject;
                        Layout.Initialize();
                }

                public override void OnInspectorGUI ()
                {
                        Layout.Update();
                        Layout.VerticalSpacing(10);

                        parent.Update();
                        {
                                SerializedProperty room = parent.Get("room");
                                FoldOut.Box(1, FoldOut.boxColor);
                                {
                                        parent.Field("Safire Camera", "safireCamera");
                                }
                                Layout.VerticalSpacing(5);
                                Rooms.CustomInspectorRoom(room, null, 0);
                        }
                        parent.ApplyModifiedProperties();
                }

                void OnSceneGUI ()
                {
                        room.room.bounds.position = room.transform.position;
                        Rooms.DrawRooms(room.room, null, -1, standAlone: true);
                }

                [DrawGizmo(GizmoType.NonSelected | GizmoType.NotInSelectionHierarchy)]
                static void DrawWhenObjectIsNotSelected (AddRoom room, GizmoType gizmoType)
                {
                        SceneTools.blockHandles = true;
                        room.room.bounds.position = room.transform.position;
                        Rooms.DrawRooms(room.room, null, -1, standAlone: true);
                        SceneTools.blockHandles = false;
                }
        }
}
