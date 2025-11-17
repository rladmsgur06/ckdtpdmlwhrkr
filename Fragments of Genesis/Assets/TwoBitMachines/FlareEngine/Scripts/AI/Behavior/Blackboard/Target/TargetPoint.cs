using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TwoBitMachines.FlareEngine.AI.BlackboardData
{
        [AddComponentMenu("")]
        public class TargetPoint : Blackboard
        {
                [SerializeField] public Vector2 point;
                [SerializeField] public Vector2 randomOffset = Vector2.zero;

                public override Vector2 GetTarget (int index = 0)
                {
                        return RandomOffset();
                }

                public override void Set (Vector3 vector3)
                {
                        point = vector3;
                }

                public override void Set (Vector2 vector2)
                {
                        point = vector2;
                }

                private Vector2 RandomOffset ()
                {
                        Vector2 newPoint = point;
                        if (randomOffset.x != 0)
                                newPoint.x += Random.Range(-randomOffset.x, randomOffset.x);
                        if (randomOffset.y != 0)
                                newPoint.y += Random.Range(-randomOffset.y, randomOffset.y);
                        return newPoint;
                }

                #region
#if UNITY_EDITOR
                [SerializeField, HideInInspector] public Vector3 oldPosition = -Vector3.one;

                public override void OnSceneGUI (UnityEditor.Editor editor)
                {
                        if (!Application.isPlaying)
                        {
                                this.transform.position = Compute.Round(this.transform.position, 0.25f);
                        }
                        if (oldPosition == -Vector3.one)
                        {
                                oldPosition = this.transform.position;
                        }

                        Vector3 newPosition = this.transform.position;
                        bool changed = false;
                        if (this.point == Vector2.zero)
                        {
                                this.point = this.transform.position + Vector3.up * 2f;
                                oldPosition = newPosition;
                                changed = true;
                        }
                        if ((oldPosition.x != newPosition.x || oldPosition.y != newPosition.y) && !Application.isPlaying) // && TwoBitMachines.Editors.Mouse.ctrl)
                        {
                                this.point += (Vector2) (newPosition - oldPosition);
                                changed = true;
                        }
                        else
                        {
                                this.point = SceneTools.MovePositionCircleHandle(this.point, Vector2.zero, Color.green, out changed);
                        }

                        SerializedObject parent = new SerializedObject(this);
                        parent.Update();
                        {
                                parent.FindProperty("oldPosition").vector3Value = newPosition;
                                parent.FindProperty("point").vector2Value = point;
                                if (changed)
                                        editor.Repaint();
                        }
                        parent.ApplyModifiedProperties();
                }
#endif
                #endregion
        }

}
