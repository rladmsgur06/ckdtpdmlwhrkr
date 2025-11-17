#if UNITY_EDITOR
using TwoBitMachines.Editors;
using UnityEditor;
#endif
using UnityEngine;

namespace TwoBitMachines.FlareEngine.AI
{
        [AddComponentMenu("")]
        public class ProjectileLinearPattern : Action
        {
                [SerializeField] public ProjectileBase projectile;

                public override NodeState RunNodeLogic (Root root)
                {
                        if (projectile == null)
                        {
                                return NodeState.Failure;
                        }
                        projectile.FireProjectileStatic(transform, transform.rotation);
                        return NodeState.Success;
                }

                #region ▀▄▀▄▀▄ Custom Inspector▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                public override bool OnInspector (AIBase ai, SerializedObject parent, Color color, bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(45, "Shoot a projectile in the direction of this transform." +
                                        "\n \nReturns Success, Failure");
                        }

                        FoldOut.Box(1, color, offsetY: -2);
                        {
                                parent.Field("Projectile", "projectile");
                        }
                        Layout.VerticalSpacing(3);
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion
        }
}
