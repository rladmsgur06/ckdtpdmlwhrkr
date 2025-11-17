using TwoBitMachines.Editors;
using TwoBitMachines.FlareEngine.Interactables;
using UnityEditor;
using UnityEngine;

namespace TwoBitMachines.FlareEngine.Editors
{
        [CustomEditor(typeof(HighJump))]
        public class HighJumpEditor : UnityEditor.Editor
        {
                private HighJump main;
                private SerializedObject parent;

                private void OnEnable ()
                {
                        main = target as HighJump;
                        parent = serializedObject;
                        Layout.Initialize();
                        main.collider2DRef = main.GetComponent<Collider2D>();
                }

                public override void OnInspectorGUI ()
                {
                        Layout.Update();
                        Layout.VerticalSpacing(10);


                        parent.Update();
                        if (FoldOut.Bar(parent, Tint.Blue).Label("High Jump", Color.white).FoldOut())
                        {
                                int type = parent.Enum("type");
                                FoldOut.Box(2, FoldOut.boxColor, extraHeight: 5, offsetY: -2);
                                parent.Field("Type", "type");
                                parent.Field("Force", "force", execute: type <= 1);
                                if (type == 3)
                                        parent.Slider("Friction", "force");
                                parent.FieldDouble("Force", "force", "timer", execute: type == 2);
                                if (type == 2)
                                        Labels.FieldText("Time");

                                if (type == 0 && FoldOut.FoldOutButton(parent.Get("eventFoldOut")))
                                {
                                        Fields.EventFoldOutEffect(parent.Get("onTrampoline"), parent.Get("trampolineWE"), parent.Get("trampolineFoldOut"), "On Trampoline");
                                }
                                if (type == 1 && FoldOut.FoldOutButton(parent.Get("eventFoldOut")))
                                {
                                        Fields.EventFoldOutEffectAndRate(parent.Get("onWind"), parent.Get("windWE"), parent.Get("windRate"), parent.Get("windFoldOut"), "On Wind");
                                }
                                if (type == 2 && FoldOut.FoldOutButton(parent.Get("eventFoldOut")))
                                {
                                        Fields.EventFoldOutEffect(parent.Get("onSpeedBoost"), parent.Get("speedBoostWE"), parent.Get("speedBoostFoldOut"), "On Speed Boost");
                                }
                                if (type == 3 && FoldOut.FoldOutButton(parent.Get("eventFoldOut")))
                                {
                                        Fields.EventFoldOutEffectAndRate(parent.Get("onSlowDown"), parent.Get("slowDownWE"), parent.Get("slowRate"), parent.Get("slowDownFoldOut"), "On Slow Down");
                                }
                        }
                        parent.ApplyModifiedProperties();
                        Layout.VerticalSpacing(10);
                }

                public void OnSceneGUI ()
                {
                        main.collider2DRef = main.GetComponent<Collider2D>();
                        if (!Application.isPlaying)
                        {
                                main.transform.position = Compute.Round(main.transform.position, 0.25f);
                        }

                        if (main.collider2DRef != null)
                        {
                                main.collider2DRef.isTrigger = true;
                                //  main.transform.rotation = Handles.RotationHandle(main.transform.rotation, main.transform.position);

                                if (main.type == HighJumpType.SpeedBoost || main.type == HighJumpType.SlowDown)
                                {
                                        float width = Mathf.Min(main.collider2DRef.bounds.size.x * 0.25f, 1.5f);
                                        main.transform.rotation = Handles.Disc(main.transform.rotation, main.transform.position, Vector3.forward, width, false, 0f);
                                }

                                if (main.collider2DRef is BoxCollider2D)
                                {
                                        SceneTools.ResizeBoxCollider2D(main.collider2DRef as BoxCollider2D);
                                        if (GUI.changed)
                                        {
                                                EditorUtility.SetDirty(target);
                                        }
                                }
                                return;
                        }

                        if (Application.isPlaying)
                        {
                                Draw.Square(main.bounds.rawPosition, main.bounds.size, Tint.Blue);
                                return;
                        }

                        parent.Update();
                        {
                                Vector3 newPosition = main.transform.position;

                                if (main.bounds.position == Vector2.zero)
                                {
                                        main.bounds.position = main.transform.position - (Vector3) main.bounds.size * 0.5f;
                                }

                                if ((main.oldPosition.x != newPosition.x || main.oldPosition.y != newPosition.y) && !Application.isPlaying) // && TwoBitMachines.Editors.Mouse.ctrl)
                                {
                                        main.bounds.position += (Vector2) (newPosition - main.oldPosition);
                                        main.oldPosition = newPosition;
                                }
                                else
                                {
                                        if (!Application.isPlaying)
                                                main.bounds.MoveRaw(main.transform.position);
                                        SceneTools.DrawAndModifyBounds(ref main.bounds.position, ref main.bounds.size, Tint.Blue);
                                        //  Debug.Log(main.transform.rotation);
                                }
                                parent.FindProperty("bounds").Get("size").vector2Value = main.bounds.size;
                                parent.FindProperty("bounds").Get("position").vector2Value = main.bounds.position;
                                parent.FindProperty("oldPosition").vector3Value = newPosition;
                        }
                        parent.ApplyModifiedProperties();

                }


                [DrawGizmo(GizmoType.NotInSelectionHierarchy)]
                public static void DrawFollowGizmos (HighJump main, GizmoType gizmoType)
                {
                        if (main.collider2DRef != null)
                        {
                                if (main.collider2DRef is BoxCollider2D)
                                {
                                        BoxCollider2D boxCollider = main.collider2DRef as BoxCollider2D;
                                        Transform transform = boxCollider.transform;
                                        Vector2 size = boxCollider.size;
                                        Vector2 center = boxCollider.offset;
                                        SceneTools.DrawBoxCollider2D(transform, center, size, Color.green);
                                }
                        }
                        else
                        {
                                SceneTools.Square(main.bounds.position, main.bounds.size, Tint.Blue);
                        }
                }

        }
}
