using System.Collections.Generic;
using TwoBitMachines.Editors;
using UnityEngine;
using UnityEditor;

namespace TwoBitMachines.MapSystem
{
        public class SceneControls
        {
                private static float scale = 0.25f;
                private static float snapSize = 0.25f;
                private static float handleSize = 1f;
                private static float sqrHandleSize => handleSize * handleSize;
                private static bool ignoreControlOffsets;
                public static List<Vector2> debugPoints = new List<Vector2>();

                public static void EditRoomOffset (Map main, SerializedProperty room, SerializedProperty roomList, Room roomClass, Zone zoneClass, SerializedObject so)
                {
                        SerializedProperty offset = room.Get("offset");
                        SerializedProperty pointList = room.Get("pointList");
                        SerializedProperty lineList = room.Get("lineList");
                        Vector2 center = Compute.Round(Center(roomClass.pointList), 0.5f) + offset.vector2Value;

                        snapSize = main.map.snapSize;
                        handleSize = main.map.gizmoSize;

                        if (MoveHandle.MoveID(0, 0, center - Vector2.right * handleSize * 2f, Tint.Green, handleSize * 2f, snapSize, out Vector2 newPosition))
                        {
                                Vector2 move = newPosition + Vector2.right * handleSize * 2f - center;
                                offset.vector2Value += move;

                                so.ApplyModifiedProperties();
                                Undo.RecordObject(main, "Move Mesh");
                                main.SetMeshRef();
                                main.map.UpdateMeshEditorShowAll(main.mesh, main.meshFilter, main.meshRenderer);
                                SceneView.RepaintAll();
                                return;
                        }
                        if (MoveHandle.MoveID(0, 1, center + Vector2.right * handleSize * 2f, Tint.Delete, handleSize * 2f, snapSize, out Vector2 newPositionAll))
                        {
                                Vector2 move = newPositionAll - Vector2.right * handleSize * 2f - center;
                                for (int i = 0; i < roomList.arraySize; i++)
                                {
                                        roomList.Element(i).Get("offset").vector2Value += move;
                                }
                                so.ApplyModifiedProperties();
                                Undo.RecordObject(main, "Move Mesh");
                                main.SetMeshRef();
                                main.map.UpdateMeshEditorShowAll(main.mesh, main.meshFilter, main.meshRenderer);
                                SceneView.RepaintAll();
                                return;
                        }

                        if (Event.current.type == EventType.MouseDown)
                        {
                                for (int i = 0; i < zoneClass.roomList.Count; i++)
                                {
                                        if (i != zoneClass.roomIndex && MapSO.IsPointInPoly(zoneClass.roomList[i].pointList, zoneClass.roomList[i].offset, Mouse.mousePosition))
                                        {
                                                Undo.RecordObject(main, "Set Index");
                                                zoneClass.roomIndex = i;
                                                Event.current.Use();
                                                break;
                                        }
                                }
                        }
                }

                public static void EditRoom (Map map, SerializedProperty room, Room roomClass, Zone zoneClass, SerializedObject so)
                {
                        SerializedProperty pointList = room.Get("pointList");
                        SerializedProperty lineList = room.Get("lineList");
                        SerializedProperty pointIndex = room.Get("pointIndex");
                        SerializedProperty lineIndex = room.Get("lineIndex");
                        SerializedProperty playerIcon = room.Get("playerIcon");

                        snapSize = map.map.snapSize;
                        handleSize = map.map.gizmoSize;

                        AddDeletePoint(map, pointList, roomClass, zoneClass, so);
                        AddDeleteLine(map, lineList, roomClass, zoneClass, so);
                        PathUtil.cull = true;
                        if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.C)
                        {
                                playerIcon.vector2Value = Center(roomClass.pointList);
                                Event.current.Use();
                                SceneView.RepaintAll();
                                return;
                        }

                        playerIcon.vector2Value = SceneTools.MovePositionCircleHandle(playerIcon.vector2Value, Vector2.zero, Tint.Blue, out bool changedPlayerIcon, Handles.CircleHandleCap, snap: snapSize, handleSize: (handleSize - 0.075f));

                        if (Event.current.type == EventType.MouseUp)
                        {
                                ignoreControlOffsets = false;
                        }
                        // move point
                        int totalPoints = pointList.arraySize;
                        for (int i = 0; i < pointList.arraySize; i++)
                        {
                                SerializedProperty previous = pointList.Previous(i);
                                SerializedProperty point = pointList.Element(i);
                                SerializedProperty next = pointList.Next(i);
                                Vector2 position = point.Get("position").vector2Value;

                                Vector2 leftDir = (previous.Get("position").vector2Value - position).normalized;
                                Vector2 rightDir = (next.Get("position").vector2Value - position).normalized;
                                leftDir = pointList.arraySize <= 2 ? -rightDir : leftDir;
                                rightDir = pointList.arraySize <= 2 ? -leftDir : rightDir;
                                leftDir = leftDir.Rotate(45f);
                                rightDir = rightDir.Rotate(-45f);

                                bool selected = pointIndex.intValue == i;
                                Color color = selected ? Color.red : Color.green;

                                if (Mouse.MouseDown(false) && ((Vector2) Mouse.position - position).magnitude <= handleSize && i != pointIndex.intValue)
                                {
                                        pointIndex.intValue = i;
                                        SceneView.RepaintAll();
                                }
                                if (i == 0)
                                {
                                        SceneTools.Circle(position, handleSize * 0.4f, color);
                                }

                                if (MoveHandle.MoveID(i, 0, position, color, handleSize - 0.075f, snapSize, out Vector2 newPosition))
                                {
                                        Vector2 move = newPosition - point.Get("position").vector2Value;
                                        point.Get("position").vector2Value = newPosition;
                                        if (i == 0 && Event.current.alt)
                                        {
                                                for (int j = 1; j < pointList.arraySize; j++)
                                                {
                                                        pointList.Element(j).Get("position").vector2Value += move;
                                                }
                                                for (int j = 0; j < lineList.arraySize; j++)
                                                {
                                                        lineList.Element(j).Get("position").vector2Value += move;
                                                }
                                        }
                                        RecalculateRoom(so, map, roomClass, zoneClass);
                                        playerIcon.vector2Value = Center(roomClass.pointList);
                                        return;
                                }

                                if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.I && i == pointIndex.intValue)
                                {
                                        point.Toggle("invisible");
                                        RecalculateRoom(so, map, roomClass, zoneClass);
                                        Event.current.Use();
                                        return;
                                }

                                if (!selected || ignoreControlOffsets)
                                {
                                        continue;
                                }

                                SceneTools.Circle(position + leftDir * handleSize, handleSize * scale, Tint.White);
                                SceneTools.Circle(position + rightDir * handleSize, handleSize * scale, Tint.White);
                                SceneTools.Line(position + leftDir * handleSize, position + point.Get("offsetEnd").vector2Value + leftDir * handleSize, Color.green);
                                SceneTools.Line(position + rightDir * handleSize, position + point.Get("offsetStart").vector2Value + rightDir * handleSize, Color.green);
                                Vector2 controlEnd = SceneTools.MovePositionCircleHandle(position + point.Get("offsetEnd").vector2Value + leftDir * handleSize, Vector2.zero, Tint.Blue, out bool changedLeft, Handles.CircleHandleCap, snap: 0.01f, handleSize: handleSize * scale);
                                Vector2 controlStart = SceneTools.MovePositionCircleHandle(position + point.Get("offsetStart").vector2Value + rightDir * handleSize, Vector2.zero, Tint.Blue, out bool changedRight, Handles.CircleHandleCap, snap: 0.01f, handleSize: handleSize * scale);

                                if ((controlEnd - (position + leftDir * handleSize)).magnitude < handleSize * scale)
                                {
                                        point.Get("offsetEnd").vector2Value = Vector2.zero;
                                }

                                if ((controlStart - (position + rightDir * handleSize)).magnitude < handleSize * scale)
                                {
                                        point.Get("offsetStart").vector2Value = Vector2.zero;
                                }

                                if (changedLeft)
                                {
                                        point.Get("offsetEnd").vector2Value = controlEnd - leftDir * handleSize - position;
                                }

                                if (changedRight)
                                {
                                        point.Get("offsetStart").vector2Value = controlStart - rightDir * handleSize - position;
                                }

                                if (changedLeft || changedRight)
                                {
                                        RecalculateRoom(so, map, roomClass, zoneClass);
                                        return;
                                }
                        }

                        for (int i = 0; i < lineList.arraySize; i++)
                        {
                                SerializedProperty previous = lineList.Previous(i);
                                SerializedProperty point = lineList.Element(i);
                                SerializedProperty next = lineList.Next(i);
                                Vector2 position = point.Get("position").vector2Value;

                                Vector2 leftDir = -(previous.Get("position").vector2Value - position).normalized;
                                Vector2 rightDir = -(next.Get("position").vector2Value - position).normalized;
                                leftDir = leftDir.Rotate(45f);
                                rightDir = rightDir.Rotate(-45f);

                                bool even = i % 2 == 0;
                                bool selected = pointIndex.intValue == i;
                                bool changedLeft = false;
                                bool changedRight = false;
                                Color color = selected ? Color.red : Color.green;

                                if (Mouse.MouseDown(false) && ((Vector2) Mouse.position - position).magnitude <= handleSize && i != pointIndex.intValue)
                                {
                                        pointIndex.intValue = i;
                                        SceneView.RepaintAll();
                                }

                                if (even)
                                {
                                        SceneTools.Circle(position, handleSize * 0.4f, color);
                                }

                                if (MoveHandle.MoveID(totalPoints + 1 + i, 0, position, color, handleSize - 0.075f, snapSize, out Vector2 newPosition))
                                {
                                        Vector2 move = newPosition - point.Get("position").vector2Value;
                                        point.Get("position").vector2Value = newPosition;
                                        if (even)
                                        {
                                                next.Get("position").vector2Value += move;
                                        }
                                        RecalculateRoom(so, map, roomClass, zoneClass);
                                        return;
                                }

                                if (!selected || ignoreControlOffsets)
                                {
                                        continue;
                                }

                                if (even)
                                {
                                        SceneTools.Circle(position + rightDir * handleSize, handleSize * scale, Tint.White);
                                        SceneTools.Line(position + rightDir * handleSize, position + point.Get("offsetStart").vector2Value + rightDir * handleSize, Color.green);
                                        Vector2 controlStart = SceneTools.MovePositionCircleHandle(position + point.Get("offsetStart").vector2Value + rightDir * handleSize, Vector2.zero, Tint.Blue, out changedRight, Handles.CircleHandleCap, snap: 0.01f, handleSize: handleSize * scale);

                                        if ((controlStart - (position + rightDir * handleSize)).magnitude < handleSize * scale)
                                        {
                                                point.Get("offsetStart").vector2Value = Vector2.zero;
                                        }
                                        if (changedRight)
                                        {
                                                point.Get("offsetStart").vector2Value = controlStart - rightDir * handleSize - position;
                                        }
                                }
                                else
                                {

                                        SceneTools.Circle(position + leftDir * handleSize, handleSize * scale, Tint.White);
                                        SceneTools.Line(position + leftDir * handleSize, position + point.Get("offsetEnd").vector2Value + leftDir * handleSize, Color.green);
                                        Vector2 controlEnd = SceneTools.MovePositionCircleHandle(position + point.Get("offsetEnd").vector2Value + leftDir * handleSize, Vector2.zero, Tint.Blue, out changedLeft, Handles.CircleHandleCap, snap: 0.01f, handleSize: handleSize * scale);
                                        if ((controlEnd - (position + leftDir * handleSize)).magnitude < handleSize * scale)
                                        {
                                                point.Get("offsetEnd").vector2Value = Vector2.zero;
                                        }
                                        if (changedLeft)
                                        {
                                                point.Get("offsetEnd").vector2Value = controlEnd - leftDir * handleSize - position;
                                        }
                                }

                                if (changedLeft || changedRight)
                                {
                                        RecalculateRoom(so, map, roomClass, zoneClass);
                                        return;
                                }
                        }

                        if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
                        {
                                for (int i = 0; i < zoneClass.roomList.Count; i++)
                                {
                                        if (i != zoneClass.roomIndex && MapSO.IsPointInPoly(zoneClass.roomList[i].pointList, Mouse.mousePosition))
                                        {
                                                Undo.RecordObject(map, "Set Index");
                                                zoneClass.roomIndex = i;
                                                Event.current.Use();
                                                break;
                                        }
                                }
                        }
                }

                private static void AddDeletePoint (Map map, SerializedProperty pointList, Room roomClass, Zone zoneClass, SerializedObject so)
                {
                        if (!Event.current.control && Mouse.MouseRightDown())
                        {
                                PathUtil.cull = false;

                                //delete point
                                for (int i = 0; i < roomClass.pointList.Count; i++)
                                {
                                        if (((Vector2) Mouse.position - roomClass.pointList[i].position).sqrMagnitude <= sqrHandleSize)
                                        {
                                                pointList.DeleteArrayElement(i);
                                                RecalculateRoom(so, map, roomClass, zoneClass);
                                                return;
                                        }
                                }

                                // insert new point into path
                                for (int i = 0; i < roomClass.pointList.Count; i++)
                                {
                                        int nextIndex = i >= roomClass.pointList.Count - 1 ? 0 : i + 1;
                                        MeshInfo.ClearTempLists();
                                        PathUtil.CreatePath(roomClass.pointList, i, nextIndex, zoneClass.thickness, 0.35f, createLine: false);

                                        for (int j = 0; j < PathUtil.tempVertices.Count; j++)
                                        {
                                                // Debug.Log(((Vector2) Mouse.position - PathUtil.tempVertices[j]).magnitude);
                                                if (((Vector2) Mouse.position - PathUtil.tempVertices[j]).sqrMagnitude <= sqrHandleSize)
                                                {
                                                        pointList.InsertArrayElementAtIndex(i + 1);
                                                        SerializedProperty newPoint = pointList.Element(i + 1);
                                                        newPoint.Get("position").vector2Value = Mouse.position;
                                                        RecalculateRoom(so, map, roomClass, zoneClass);
                                                        return;
                                                }
                                        }
                                }

                                // add new point
                                pointList.arraySize++;
                                pointList.LastElement().Get("position").vector2Value = Compute.Round(Mouse.position, snapSize);
                                RecalculateRoom(so, map, roomClass, zoneClass);
                                return;
                        }
                }

                private static void AddDeleteLine (Map map, SerializedProperty lineList, Room roomClass, Zone zoneClass, SerializedObject so)
                {
                        if (Event.current.control && Mouse.MouseRightDown())
                        {
                                PathUtil.cull = false;

                                //delete point
                                for (int i = 0; i < roomClass.lineList.Count; i++)
                                {
                                        if (((Vector2) Mouse.position - roomClass.lineList[i].position).sqrMagnitude <= sqrHandleSize)
                                        {
                                                if (i % 2 == 0)
                                                {
                                                        lineList.DeleteArrayElement(i);
                                                        lineList.DeleteArrayElement(i);
                                                }
                                                else
                                                {
                                                        lineList.DeleteArrayElement(i);
                                                        lineList.DeleteArrayElement(i - 1);
                                                }

                                                RecalculateRoom(so, map, roomClass, zoneClass);
                                                return;
                                        }
                                }

                                // add new point
                                lineList.arraySize++;
                                lineList.LastElement().Get("position").vector2Value = Compute.Round(Mouse.position, snapSize);
                                lineList.arraySize++;
                                lineList.LastElement().Get("position").vector2Value = Compute.Round(Mouse.position, snapSize) + Vector3.up * handleSize * 2f;
                                RecalculateRoom(so, map, roomClass, zoneClass);
                                return;
                        }
                }

                public static void RecalculateRoom (SerializedObject so, Map main, Room roomClass, Zone zoneClass)
                {
                        so.ApplyModifiedProperties();
                        GenerateRoomMesh.ResetMesh(roomClass, zoneClass);
                        main.SetMeshRef();
                        main.map.UpdateMeshEditor(main.mesh, main.meshFilter, main.meshRenderer);
                        SceneView.RepaintAll();
                }

                public static void RecalculateZone (Zone zoneClass)
                {
                        for (int i = 0; i < zoneClass.roomList.Count; i++)
                        {
                                GenerateRoomMesh.ResetMesh(zoneClass.roomList[i], zoneClass);
                        }
                        SceneView.RepaintAll();
                }

                public static Vector2 Center (List<Point> pointList)
                {
                        if (pointList.Count < 3)// Need at least 3 points to define a polygon
                        {
                                return CenterEstimate(pointList);
                        }

                        float signedArea = 0f;
                        Vector2 centroid = Vector2.zero;

                        for (int i = 0; i < pointList.Count; i++)
                        {
                                int j = (i + 1) % pointList.Count;
                                float x1 = pointList[i].position.x;
                                float y1 = pointList[i].position.y;
                                float x2 = pointList[j].position.x;
                                float y2 = pointList[j].position.y;

                                float area = (x1 * y2) - (x2 * y1);
                                signedArea += area;

                                centroid.x += (x1 + x2) * area;
                                centroid.y += (y1 + y2) * area;
                        }

                        signedArea *= 0.5f;
                        centroid.x /= (6f * signedArea);
                        centroid.y /= (6f * signedArea);

                        return centroid;
                }

                public static Vector2 CenterEstimate (List<Point> pointList)
                {
                        Vector2 center = Vector2.zero;
                        for (int i = 0; i < pointList.Count; i++)
                        {
                                center += pointList[i].position;
                        }
                        return pointList.Count == 0 ? Vector2.zero : center / pointList.Count;
                }
        }
}
