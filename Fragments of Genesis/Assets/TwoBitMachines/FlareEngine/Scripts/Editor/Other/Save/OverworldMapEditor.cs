using System.Collections.Generic;
using TwoBitMachines.Editors;
using UnityEditor;
using UnityEngine;

namespace TwoBitMachines.FlareEngine.Editors
{
        [CustomEditor(typeof(OverworldMap))]
        public class OverworldMapEditor : UnityEditor.Editor
        {
                private OverworldMap main;
                private SerializedObject so;
                public bool selectPathActive;
                public OverworldNode pathOrigin;
                private List<string> sceneNames = new List<string>();

                private void OnEnable ()
                {
                        main = target as OverworldMap;
                        so = serializedObject;
                        Layout.Initialize();
                        OverworldNode.icon = Icon.icon;

                        if (main.mapSavekey == "")
                        {
                                main.mapSavekey = System.Guid.NewGuid().ToString();
                        }
                        int sceneCount = Util.SceneCount();
                        sceneNames.Clear();
                        for (int i = 0; i < sceneCount; i++)
                        {
                                sceneNames.Add(Util.GetSceneName(i));
                        }

                        if (main.basicNodes == null)
                        {
                                main.basicNodes = new GameObject("Basic Nodes");
                                main.basicNodes.transform.parent = main.transform;
                        }
                }

                public override void OnInspectorGUI ()
                {
                        main.transform.localScale = Vector3.one;
                        Layout.Update();
                        Layout.VerticalSpacing(10);
                        so.Update();

                        Block.Box(2, Tint.Blue, extraHeight: 3);
                        {
                                so.FieldDouble_("Player", "player", "offset");
                                Block.HelperText("Offset");
                                so.Field_("Enter Button", "enterButton");
                        }

                        if (Block.ExtraFoldout(so, "foldOut"))
                        {
                                Fields.EventFoldOut(so.Get("onIdle"), so.Get("idleFoldOut"), "Player Idle", space: false);
                                Fields.EventFoldOut(so.Get("onMoving"), so.Get("movingFoldOut"), "Player Is Moving");
                                Fields.EventFoldOut(so.Get("onEnteringLevel"), so.Get("enteringLevelFoldOut"), "Player Is Entering Level");
                                Fields.EventFoldOut(so.Get("onSignalUnlock"), so.Get("signalUnlockFoldOut"), "Play Signal Unlock");

                                Fields.EventFoldOut(so.Get("onMoveBegin"), so.Get("moveBeginFoldOut"), "On Moving Begin");
                                Fields.EventFoldOut(so.Get("onMoveFailed"), so.Get("moveFailedFoldOut"), "On Moving Failed");
                                Fields.EventFoldOut(so.Get("onEnterSuccess"), so.Get("enterFoldOut"), "On Enter Success");
                                Fields.EventFoldOut(so.Get("onEnterFailed"), so.Get("enterFailedFoldOut"), "On Enter Failed");
                                Layout.VerticalSpacing();
                        }

                        Block.Box(3, Tint.BoxTwo);
                        {
                                SerializedProperty manual = so.Get("manual");
                                manual.FieldDouble_("Left, Right", "left", "right");
                                manual.FieldDouble_("Up, Down", "up", "down");
                                so.FieldDouble_("Tween", "tween", "speed");
                                Block.HelperText("Speed");
                        }

                        so.ApplyModifiedProperties();

                        if (main.list != null && main.list.Length >= 0 && main.editIndex < main.list.Length && main.list.Length > 0)
                        {
                                OverworldNode editNode = main.list[main.editIndex];
                                if (editNode != null)
                                {
                                        Block.Header().Style(Tint.Delete).Label("Edit Node:  " + main.editIndex, 12, false, FoldOut.titleColor).Button("Delete").Build();
                                        OverworldNodeEditor.UpdateNode(new SerializedObject(editNode), editNode, sceneNames, false);
                                        if (Header.SignalActive("Delete"))
                                        {
                                                DestroyImmediate(editNode.gameObject);
                                                main.editIndex = Mathf.Clamp(main.editIndex, 0, main.list.Length);
                                        }
                                }
                        }
                }

                public void OnSceneGUI ()
                {
                        main.transform.localScale = Vector3.one;
                        Mouse.Update();
                        CreateNode();
                        so.Update();

                        int mainIndex = so.Int("editIndex");
                        SerializedProperty list = so.Get("list");

                        if (main.transform.childCount != list.arraySize)
                        {
                                SetNodeChildren(list);
                        }

                        for (int i = 0; i < list.arraySize; i++)
                        {
                                SerializedProperty node = list.Element(i);
                                if (node.objectReferenceValue == null)
                                {
                                        SetNodeChildren(list);
                                        break;
                                }

                                // set save key
                                OverworldNode nodeRef = node.objectReferenceValue as OverworldNode;
                                SerializedObject nodeObject = new SerializedObject(node.objectReferenceValue);
                                nodeObject.Update();
                                {
                                        if (nodeObject.String("blockSaveKey") == "")
                                        {
                                                nodeObject.Get("blockSaveKey").stringValue = System.Guid.NewGuid().ToString();
                                        }
                                }
                                nodeObject.ApplyModifiedProperties();

                                // hide Flags
                                GameObject gameObject = nodeRef.gameObject;
                                bool isBasic = (OverworldNodeType) nodeObject.Enum("type") == OverworldNodeType.Basic;
                                if (gameObject.hideFlags == HideFlags.HideInHierarchy)
                                {
                                        gameObject.hideFlags = HideFlags.None;
                                }
                                if (isBasic && main.basicNodes != null && !main.basicNodes.transform.IsChildOf(nodeRef.transform))
                                {
                                        nodeRef.transform.parent = main.basicNodes.transform;
                                }
                                if (!isBasic && main.basicNodes != null && main.basicNodes.transform.IsChildOf(nodeRef.transform))
                                {
                                        nodeRef.transform.parent = main.transform;
                                }

                                // move transform
                                SerializedObject transformObject = new SerializedObject(nodeRef.transform);
                                transformObject.Update();
                                {
                                        Vector2 position = transformObject.FindProperty("m_LocalPosition").vector3Value;
                                        bool insideCircle = (position - (Vector2) Mouse.position).sqrMagnitude <= 0.75f * 0.75f;

                                        if (Mouse.down && insideCircle)
                                        {
                                                so.Get("editIndex").intValue = mainIndex = i;
                                        }
                                        if (!selectPathActive && Mouse.down && insideCircle && (position - (Vector2) Mouse.position).sqrMagnitude > 0.3f * 0.3f)
                                        {
                                                selectPathActive = true;
                                                pathOrigin = nodeRef;
                                        }
                                        if (selectPathActive && pathOrigin == nodeRef)
                                        {
                                                Draw.GLStart();
                                                Draw.GLCircle(position, 0.3f, Tint.White);
                                                Draw.GLCircle(position, 0.75f, Color.yellow);
                                                Draw.GLEnd();
                                        }
                                        else
                                        {
                                                if (selectPathActive)
                                                {
                                                        Color handleColor = Tint.Green;
                                                        if ((position - (Vector2) Mouse.position).sqrMagnitude <= 0.75f * 0.75f)
                                                        {
                                                                handleColor = Color.yellow;
                                                        }
                                                        Draw.GLStart();
                                                        Draw.GLCircle(position, 0.3f, Tint.White);
                                                        Draw.GLCircle(position, 0.75f, handleColor);
                                                        Draw.GLEnd();
                                                }
                                                else
                                                {
                                                        Handles.Label(position + Vector2.up * 1.5f - Vector2.right * 0.5f, nodeRef.nodeName);
                                                        position = SceneTools.MovePositionCircleHandle(position, Vector2.zero, mainIndex == i ? Tint.Delete * Tint.WarmGrey : Tint.Green, out bool changed, 0.5f, 0.75f);
                                                        if (!Mouse.holding && insideCircle && (position - (Vector2) Mouse.position).sqrMagnitude > 0.3f * 0.3f)
                                                        {
                                                                Draw.GLCircleInit(position, 0.75f, Color.yellow, 8);
                                                                SceneView.RepaintAll();
                                                        }
                                                        if (mainIndex == i)
                                                        {
                                                                Draw.GLTriangleTopInit(position + Vector2.up * 1.5f, 0.5f, Tint.Delete * Tint.WarmGrey);
                                                        }
                                                        transformObject.Get("m_LocalPosition").vector3Value = position;
                                                }
                                        }
                                }
                                transformObject.ApplyModifiedProperties();
                        }

                        so.ApplyModifiedProperties();

                        DrawLevelPaths(main, main.list, !selectPathActive);

                        if (selectPathActive)
                        {
                                if (pathOrigin == null)
                                {
                                        selectPathActive = false;
                                }
                                else if (Mouse.up)
                                {
                                        selectPathActive = false;
                                        SetReferencePath(main, main.list);
                                }
                                else
                                {
                                        Draw.GLStart();
                                        Draw.GLLine(pathOrigin.transform.position, Mouse.position, Color.yellow);
                                        Draw.GLEnd();
                                        SceneView.RepaintAll();
                                }
                        }

                        if (Event.current.type == EventType.Layout)
                        {
                                HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
                        }
                }

                private void SetNodeChildren (SerializedProperty list)
                {
                        list.arraySize = 0;
                        OverworldNode[] newList = main.gameObject.GetComponentsInChildren<OverworldNode>();
                        for (int i = 0; i < newList.Length; i++)
                        {
                                list.arraySize++;
                                list.LastElement().objectReferenceValue = newList[i];
                        }
                }

                private void CreateNode ()
                {
                        if (Mouse.rightDown)
                        {
                                GameObject newNode = new GameObject("Node");
                                newNode.transform.parent = main.transform;
                                newNode.transform.position = Mouse.position;
                                newNode.AddComponent<OverworldNode>();
                                Undo.RegisterCreatedObjectUndo(newNode, "Create Node");
                                EditorUtility.SetDirty(main);
                                if (Event.current.type != EventType.Layout)
                                {
                                        Event.current.Use();
                                }
                        }
                }

                public static void DrawLevelPaths (OverworldMap main, OverworldNode[] level, bool delete = false)
                {
                        Draw.GLStart();
                        for (int i = 0; i < level.Length; i++)
                        {
                                OverworldNode parentNode = level[i];
                                for (int j = 0; j < parentNode.path.Count; j++)
                                {
                                        OverworldNode childNode = parentNode.path[j];
                                        if (childNode == null)
                                        {
                                                parentNode.path.RemoveAt(j);
                                                EditorUtility.SetDirty(main);
                                                break;
                                        }
                                        bool isAblockedPath = parentNode.blockPath.Contains(childNode);
                                        bool nearLine = delete && MouseNeaerLine(parentNode.position, childNode.position, Mouse.position);
                                        Color color = nearLine ? Tint.Orange : isAblockedPath ? Tint.Delete : Tint.Green;
                                        Draw.GLLine(parentNode.position, childNode.position, color);

                                        if (nearLine && Mouse.down && parentNode.canBeLocked)
                                        {
                                                Mouse.down = false;
                                                if (parentNode.blockPath.Contains(childNode))
                                                {
                                                        childNode.blockPath.RemoveItem(parentNode);
                                                        parentNode.blockPath.RemoveItem(childNode);
                                                }
                                                else
                                                {
                                                        childNode.blockPath.AddItem(parentNode);
                                                        parentNode.blockPath.AddItem(childNode);
                                                }
                                                Event.current.Use();
                                                EditorUtility.SetDirty(main);
                                                break;
                                        }

                                        if (nearLine && (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Delete))
                                        {
                                                childNode.path.RemoveItem(parentNode);
                                                parentNode.path.RemoveItem(childNode);
                                                Event.current.Use();
                                                EditorUtility.SetDirty(main);
                                                break;
                                        }

                                        if (parentNode.isTeleport && parentNode.teleportToNode != null)
                                        {
                                                Draw.GLLine(parentNode.position, parentNode.teleportToNode.position, Tint.Blue);
                                        }
                                }
                        }
                        Draw.GLEnd();
                }

                public static bool MouseNeaerLine (Vector2 lineStart, Vector2 lineEnd, Vector2 point, float proximityThreshold = 0.5f)
                {
                        if ((point - lineStart).sqrMagnitude <= 1f || (point - lineEnd).sqrMagnitude <= 1f)
                        {
                                return false;
                        }

                        Vector2 lineDirection = lineEnd - lineStart;
                        float lineLengthSquared = lineDirection.sqrMagnitude;

                        if (lineLengthSquared < 0.0001f)
                        {
                                // If the line has very low length, treat it as a point.
                                return (point - lineStart).sqrMagnitude <= proximityThreshold * proximityThreshold;
                        }

                        float t = Mathf.Clamp01(Vector2.Dot(point - lineStart, lineDirection) / lineLengthSquared);

                        Vector2 closestPoint = lineStart + t * lineDirection;
                        float distanceSquared = (point - closestPoint).sqrMagnitude;
                        return distanceSquared <= proximityThreshold * proximityThreshold;
                }

                public void SetReferencePath (OverworldMap main, OverworldNode[] level)
                {
                        for (int i = 0; i < level.Length; i++)
                        {
                                Vector2 position = level[i].transform.position;
                                if (pathOrigin != level[i] && (position - (Vector2) Mouse.position).sqrMagnitude <= 0.75f * 0.75f)
                                {
                                        if (pathOrigin.path.Contains(level[i]) || level[i].path.Contains(pathOrigin))
                                        {
                                                pathOrigin.path.RemoveItem(level[i]);
                                                level[i].path.RemoveItem(pathOrigin);
                                        }
                                        else
                                        {
                                                pathOrigin.path.AddItem(level[i]);
                                                level[i].path.AddItem(pathOrigin);
                                        }
                                        EditorUtility.SetDirty(main);
                                        break;
                                }
                        }
                }

                [DrawGizmo(GizmoType.NotInSelectionHierarchy)]
                static void DrawWhenObjectIsNotSelected (OverworldMap main, GizmoType gizmoType)
                {
                        Draw.GLStart();
                        for (int i = 0; i < main.list.Length; i++)
                        {
                                Transform t = main.list[i].transform;
                                Draw.GLCircle(t.position, 0.75f, Color.green);
                        }
                        Draw.GLEnd();
                        DrawLevelPaths(main, main.list);
                }
        }
}
