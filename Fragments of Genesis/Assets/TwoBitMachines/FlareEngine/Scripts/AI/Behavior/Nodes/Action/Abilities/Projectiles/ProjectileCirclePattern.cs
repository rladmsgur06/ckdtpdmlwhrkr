#if UNITY_EDITOR
using TwoBitMachines.Editors;
using UnityEditor;
#endif
using UnityEngine;

namespace TwoBitMachines.FlareEngine.AI
{
        [AddComponentMenu("")]
        public class ProjectileCirclePattern : Action
        {
                [SerializeField] public ProjectileBase projectile;
                [SerializeField] public CirclePattern type;
                [SerializeField] public float startAngle = 0f;
                [SerializeField] public float endAngle = 360f;
                [SerializeField] public float radius;
                [SerializeField] public float duration = 0.5f;
                [SerializeField] public int projectiles;

                [System.NonSerialized] private float counter;
                [System.NonSerialized] private int fired;

                public override NodeState RunNodeLogic (Root root)
                {
                        if (projectile == null || projectiles <= 0)
                        {
                                return NodeState.Failure;
                        }

                        if (nodeSetup == NodeSetup.NeedToInitialize)
                        {
                                counter = 0f;
                                fired = 0;
                        }

                        Vector2 direction = transform.right;

                        if (type == CirclePattern.Instant)
                        {
                                for (int i = 0; i < projectiles; i++)
                                {
                                        float rotate = Mathf.Lerp(startAngle, endAngle, (float) i / projectiles);
                                        Vector3 currentDirection = direction.Rotate(rotate);
                                        projectile.FireProjectileStatic(transform.position + currentDirection * radius, currentDirection);
                                }
                                return NodeState.Success;
                        }
                        else
                        {
                                float rate = duration / projectiles;
                                if (TwoBitMachines.Clock.Timer(ref counter, rate))
                                {
                                        float shoot = rate > Time.deltaTime ? 1f : Mathf.CeilToInt(Time.deltaTime / rate);
                                        for (int i = 0; i < shoot && fired < projectiles; i++)
                                        {
                                                float rotate = Random.Range(startAngle, endAngle);
                                                Vector3 currentDirection = direction.Rotate(rotate);
                                                projectile.FireProjectileStatic(transform.position + currentDirection * radius, currentDirection);
                                                fired++;
                                        }
                                }
                                if (fired >= projectiles)
                                {
                                        return NodeState.Success;
                                }
                                return NodeState.Running;
                        }
                }

                #region ▀▄▀▄▀▄ Custom Inspector▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                public override bool OnInspector (AIBase ai, SerializedObject parent, Color color, bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(35, "Rotate and shoot a projectile." +
                                        "\n \nReturns Success, Failure");
                        }

                        int type = parent.Enum("type");

                        FoldOut.Box(5, color, offsetY: -2);
                        {
                                parent.Field("Projectile", "projectile");
                                parent.Field("Type", "type", execute: type == 0);
                                parent.FieldDouble("Type", "type", "duration", execute: type == 1);
                                parent.FieldDouble("Angle", "startAngle", "endAngle");
                                Labels.FieldDoubleText("Start", "End");
                                parent.Field("Radius", "radius");
                                parent.Field("Projectiles", "projectiles");
                        }
                        Layout.VerticalSpacing(3);
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion
        }

        public enum CirclePattern
        {
                Instant,
                RandomTimeRate
        }
}
