#region
#if UNITY_EDITOR
using TwoBitMachines.Editors;
using UnityEditor;
#endif
#endregion
using UnityEngine;

namespace TwoBitMachines.FlareEngine.ThePlayer
{
        [AddComponentMenu("")]
        public class Magnet : Ability
        {
                [SerializeField] public Transform origin;
                [SerializeField] public float radius = 5f;
                [SerializeField] public float force = 25f;
                [SerializeField] public LayerMask targetLayer;

                [SerializeField] public InputButtonSO pushButton;
                [SerializeField] public InputButtonSO pullButton;

                public override bool IsAbilityRequired (AbilityManager player, ref Vector2 velocity)
                {
                        if (pause)
                                return false;

                        return (pushButton != null && pushButton.Holding()) || (pullButton != null && pullButton.Holding());
                }

                public override void ExecuteAbility (AbilityManager player, ref Vector2 velocity, bool isRunningAsException = false)
                {
                        if (pushButton && pushButton.Holding())
                        {
                                Move(1);
                        }
                        if (pullButton && pullButton.Holding())
                        {
                                Move(-1);
                        }
                }

                private void Move (int direction)
                {
                        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius, targetLayer);

                        foreach (Collider2D hit in hits)
                        {
                                Character character = hit.GetComponent<Character>();
                                if (character != null)
                                {
                                        Vector2 dir = (hit.transform.position - transform.position).normalized * direction;
                                        character.externalVelocity += (Vector2) (dir * force);
                                        character.Execute();
                                        Physics2D.SyncTransforms();
                                }
                        }
                }

                private void OnDrawGizmosSelected ()
                {
                        Gizmos.color = Color.cyan;
                        Gizmos.DrawWireSphere(transform.position, radius);
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                public override bool OnInspector (SerializedObject controller, SerializedObject parent, string[] inputList, Color barColor, Color labelColor)
                {
                        if (Open(parent, "Magnet", barColor, labelColor))
                        {
                                FoldOut.Box(5, FoldOut.boxColorLight, offsetY: -2);
                                {
                                        parent.Field("Target Layer", "targetLayer");
                                        parent.Slider("Radius", "radius", 1f, 20f);
                                        parent.Slider("Force", "force", 1f, 25f);
                                        parent.Field("Push Button", "pushButton");
                                        parent.Field("Pull Button", "pullButton");
                                }
                                Layout.VerticalSpacing(3);
                        }
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion
        }
}
