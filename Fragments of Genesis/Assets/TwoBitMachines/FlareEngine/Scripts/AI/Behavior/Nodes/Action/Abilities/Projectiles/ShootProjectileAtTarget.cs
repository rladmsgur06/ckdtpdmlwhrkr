#if UNITY_EDITOR
using TwoBitMachines.Editors;
using UnityEditor;
#endif
using TwoBitMachines.FlareEngine.AI.BlackboardData;
using UnityEngine;

namespace TwoBitMachines.FlareEngine.AI
{
        [AddComponentMenu("")]
        public class ShootProjectileAtTarget : Action
        {
                [SerializeField] public ProjectileBase projectile;
                [SerializeField] public UseAIRotation rotateTo;
                [SerializeField] public Blackboard target;
                [SerializeField] public Transform firePoint;
                [SerializeField] public float offset = 0;
                [SerializeField] public bool mirrorFirePoint;

                public override NodeState RunNodeLogic (Root root)
                {
                        if (projectile == null || target == null || firePoint == null)
                        {
                                return NodeState.Failure;
                        }

                        if (rotateTo == UseAIRotation.Yes)
                        {
                                Vector2 direction = transform.right;
                                projectile.FireProjectile(firePoint, direction.Rotate(offset), Vector2.zero);
                        }
                        else
                        {
                                Quaternion rotation = GetDirectionToTarget(target.GetTarget());
                                if (this.target.hasNoTargets)
                                {
                                        return NodeState.Failure;
                                }
                                projectile.FireProjectile(firePoint, rotation, Vector2.zero);
                        }
                        return NodeState.Success;
                }

                private Quaternion GetDirectionToTarget (Vector2 target)
                {
                        float direction = target.x < transform.position.x ? -1f : 1f;
                        Vector2 targetNormal = (target - (Vector2) transform.position).normalized;
                        float angle = Compute.AngleDirection(transform.right * direction, targetNormal);
                        float rotateAngle = direction < 0 ? -angle : angle;
                        if (mirrorFirePoint)
                        {
                                Compute.FlipLocalPositionX(firePoint, direction);
                        }
                        return Quaternion.Euler(0, direction < 0 ? 180f : 0f, rotateAngle);
                }

                #region ▀▄▀▄▀▄ Custom Inspector▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                public override bool OnInspector (AIBase ai, SerializedObject parent, Color color, bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(45, "Shoot a projectile at a target." +
                                        "\n \nReturns Success, Failure");
                        }

                        int rotate = parent.Enum("rotateTo");
                        FoldOut.Box(5, color, offsetY: -2);
                        {
                                AIBase.SetRef(ai.data, parent.Get("target"), 0);
                                parent.Field("Use AI Rotation", "rotateTo", execute: rotate == 0);
                                parent.FieldDouble("Use AI Rotation", "rotateTo", "offset", execute: rotate == 1);
                                Labels.FieldText("offset", execute: rotate == 1);
                                parent.Field("Projectile", "projectile");
                                parent.Field("Fire Point", "firePoint");
                                parent.FieldToggleAndEnable("Mirror Fire Point", "mirrorFirePoint");
                        }
                        Layout.VerticalSpacing(3);
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion
        }

        public enum UseAIRotation
        {
                No,
                Yes
        }

}
