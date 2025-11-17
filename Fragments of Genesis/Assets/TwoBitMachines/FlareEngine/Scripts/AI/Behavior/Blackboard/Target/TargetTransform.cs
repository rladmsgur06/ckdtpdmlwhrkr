using UnityEngine;

namespace TwoBitMachines.FlareEngine.AI.BlackboardData
{
        [AddComponentMenu("")]
        public class TargetTransform : Blackboard
        {
                [SerializeField] public Transform target;
                [SerializeField] public Vector2 offset;

                public override Vector2 GetTarget (int index = 0)
                {
                        return target != null ? target.position + (Vector3) offset : this.transform.position;
                }

                public override void Set (Transform transform)
                {
                        target = transform;
                }

                public override void Set (Vector3 vector3)
                {
                        if (target != null)
                                target.position = vector3 + (Vector3) offset;
                }

                public override void Set (Vector2 vector2)
                {
                        if (target != null)
                                target.position = vector2 + offset;
                }

                public override Transform GetTransform ()
                {
                        return target;
                }

                public override Vector2 GetOffset ()
                {
                        return offset;
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
                        if (oldPosition == Vector3.zero)
                        {
                                oldPosition = newPosition;
                        }
                        if ((oldPosition.x != newPosition.x || oldPosition.y != newPosition.y) && target != null && target != transform && !Application.isPlaying) // && TwoBitMachines.Editors.Mouse.ctrl)
                        {
                                target.position += (Vector3) (newPosition - oldPosition);
                        }
                        oldPosition = newPosition;
                }
#endif
                #endregion
        }

}
