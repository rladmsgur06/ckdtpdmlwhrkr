#region 
#if UNITY_EDITOR
using TwoBitMachines.Editors;
using UnityEditor;
#endif
#endregion
using UnityEngine;

namespace TwoBitMachines.FlareEngine.AI
{
        [AddComponentMenu("")]
        public class RotatePosition : Action
        {
                [SerializeField] public Vector2 center;
                [SerializeField] public float radius = 1f;
                [SerializeField] public float offset = 0f;
                [SerializeField] public float speed = 10f;

                public void Start ()
                {
                        Vector2 newDirection = Compute.RotateVector(Vector2.right, offset);
                        transform.position = center + newDirection * radius;
                }

                public override NodeState RunNodeLogic (Root root)
                {
                        if (nodeSetup == NodeSetup.NeedToInitialize)
                        {
                                if (radius <= 0)
                                        radius = 1f;
                        }
                        if (Time.deltaTime != 0)
                        {
                                Vector2 startPoint = transform.position;
                                Vector2 direction = (startPoint - center) / radius;
                                Vector2 newDirection = Compute.RotateVector(direction, speed * Time.deltaTime);
                                Vector2 endPoint = center + newDirection * radius;
                                root.velocity = (endPoint - startPoint) / Time.deltaTime;
                        }
                        return NodeState.Running;
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                public override bool HasNextState () { return false; }
                public override bool OnInspector (AIBase ai, SerializedObject parent, Color color, bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(55, "Rotate the transform's position around the center point at the specified radius." +
                                        "\n \nReturns Running");
                        }

                        FoldOut.Box(4, color, offsetY: -2);
                        {
                                parent.Field("Center", "center");
                                parent.Field("Radius", "radius");
                                parent.Field("Speed", "speed");
                                parent.Field("Offset Angle", "offset");
                        }
                        Layout.VerticalSpacing(3);
                        return true;
                }

                public override void OnSceneGUI (UnityEditor.Editor editor)
                {
                        hideFlags = HideFlags.HideInInspector;
                        if (center == Vector2.zero)
                                center = transform.position;
                        center = SceneTools.MovePositionCircleHandle(center, Vector2.zero, Tint.White, out bool changed);
                        if (!Application.isPlaying)
                        {
                                Vector2 newDirection = Compute.RotateVector(Vector2.right, offset);
                                transform.position = center + newDirection * radius;
                        }
                }

                public override void OnDrawGizmos ()
                {
                        Draw.GLStart();
                        Draw.GLCircle(center, 0.3f, Tint.Green);
                        Draw.GLCircle(center, radius, Tint.Green, (int) radius * 2);
                        Draw.GLEnd();

                        Draw.GLStart(GL.QUADS);
                        Draw.GLArrow(center, 0.3f, speed > 0 ? 180f : 0f, Tint.Green);
                        Draw.GLEnd();
                }
#pragma warning restore 0414
#endif
                #endregion
        }
}
