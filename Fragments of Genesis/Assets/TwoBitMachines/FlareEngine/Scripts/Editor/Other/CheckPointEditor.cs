using TwoBitMachines.Editors;
using UnityEditor;
using UnityEngine;

namespace TwoBitMachines.FlareEngine.Editors
{
        [CustomEditor(typeof(CheckPoint))]
        public class CheckPointEditor : UnityEditor.Editor
        {
                private CheckPoint main;
                private SerializedObject parent;

                private void OnEnable ()
                {
                        main = target as CheckPoint;
                        parent = serializedObject;
                        Layout.Initialize();
                }

                public override void OnInspectorGUI ()
                {
                        Layout.Update();
                        Layout.VerticalSpacing(10);
                        parent.Update();
                        {
                                if (FoldOut.Bar(parent, Tint.Blue).Label("Check Points", Color.white).BR().FoldOut())
                                {
                                        FoldOut.Box(4, FoldOut.boxColor, extraHeight: 5, offsetY: -2);

                                        bool delete = parent.FieldAndButton("Name", "checkPointName", "Delete");
                                        parent.Field("Type", "type");
                                        parent.Field("Button", "input");
                                        parent.FieldAndEnable("Default Index", "defaultIndex", "hasDefault");

                                        if (FoldOut.FoldOutButton(parent.Get("eventsFoldOut")))
                                        {
                                                Fields.EventFoldOut(parent.Get("onReset"), parent.Get("resetFoldOut"), "On Reset");
                                                Fields.EventFoldOut(parent.Get("onSave"), parent.Get("saveFoldOut"), "On Save");
                                        }

                                        if (delete)
                                        {
                                                WorldManagerEditor.DeleteSavedData(main.checkPointName);
                                        }

                                        SerializedProperty array = parent.Get("checkPoints");
                                        if (parent.ReadBool("add"))
                                        {
                                                array.arraySize++;
                                                SerializedProperty element = array.LastElement();
                                                element.Get("bounds").Get("position").vector2Value = SceneTools.SceneCenter(main.transform.position);
                                                element.Get("bounds").Get("size").vector2Value = new Vector2(5f, 5f);
                                                element.Get("index").intValue = array.arraySize - 1;
                                        }

                                        for (int i = 0; i < array.arraySize; i++)
                                        {
                                                SerializedProperty element = array.Element(i);
                                                bool open = FoldOut.Bar(element, FoldOut.boxColor, 25).Label(" Index: " + element.Int("index")).BR("delete", "Delete").FoldOut();
                                                ListReorder.Grip(parent, array, Bar.barStart.CenterRectHeight(), i, Tint.WarmWhite);

                                                if (open)
                                                {
                                                        FoldOut.Box(4, FoldOut.boxColor, extraHeight: 5, offsetY: -2);
                                                        {
                                                                element.Field("Index", "index");
                                                                element.Get("bounds").Field("Position", "position");
                                                                element.Field("Save", "saveType");
                                                                element.Field("On Reset", "playerDirection");
                                                        }

                                                        if (FoldOut.FoldOutButton(element.Get("eventsFoldOut")))
                                                        {
                                                                Fields.EventFoldOut(element.Get("onReset"), element.Get("resetFoldOut"), "On Reset");
                                                                Fields.EventFoldOut(element.Get("onSave"), element.Get("saveFoldOut"), "On Save");
                                                        }
                                                }

                                                if (element.Bool("delete"))
                                                {
                                                        array.DeleteArrayElement(i);
                                                        break;
                                                }

                                        }
                                }
                        }
                        parent.ApplyModifiedProperties();

                        Layout.VerticalSpacing(10);
                }

                private void OnSceneGUI ()
                {
                        Layout.Update();
                        for (int i = 0; i < main.checkPoints.Count; i++)
                        {
                                SimpleBounds bounds = main.checkPoints[i].bounds;
                                SceneTools.DrawAndModifyBounds(ref bounds.position, ref bounds.size, Tint.Orange);
                                SceneTools.Label(bounds.position + Vector2.up * (bounds.size.y + 1f), "Check Point: " + main.checkPoints[i].index, Tint.WarmWhite);
                        }
                }

                [DrawGizmo(GizmoType.NotInSelectionHierarchy)]
                public static void DrawFollowGizmos (CheckPoint main, GizmoType gizmoType)
                {
                        for (int i = 0; i < main.checkPoints.Count; i++)
                        {
                                SimpleBounds bounds = main.checkPoints[i].bounds;
                                SceneTools.Square(bounds.position, bounds.size, Tint.Orange);
                                SceneTools.Label(bounds.position + Vector2.up * (bounds.size.y + 1f), "Check Point: " + main.checkPoints[i].index, Tint.WarmWhite);
                        }
                }

        }
}
