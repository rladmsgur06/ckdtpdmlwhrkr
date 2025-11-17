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
        public class DamageEffect : Action
        {
                [SerializeField] public float duration = 2f;
                [SerializeField] public float slowSpeed = 1f;
                [SerializeField] public float damageRate = 0f;
                [SerializeField] public string animationSignal;
                [SerializeField] public string tagName;
                [SerializeField] public bool startOnEnter;

                [System.NonSerialized] public float counter;
                [System.NonSerialized] public bool isActive = false;
                [System.NonSerialized] public Health health;

                public override NodeState RunNodeLogic (Root root)
                {
                        if (nodeSetup == NodeSetup.NeedToInitialize)
                        {
                                counter = 0;
                                isActive = startOnEnter;
                                if (damageRate != 0 && health == null)
                                {
                                        health = this.gameObject.GetComponent<Health>();
                                }
                        }
                        if (isActive)
                        {
                                if (TwoBitMachines.Clock.Timer(ref counter, duration))
                                {
                                        ;
                                        isActive = false;
                                        return NodeState.Success;
                                }
                                root.signals.Set(animationSignal);
                                root.velocity.x *= slowSpeed;
                                if (damageRate != 0 && health != null)
                                {
                                        health.IncrementValue(null, -damageRate * Root.deltaTime, Vector2.zero);
                                }
                        }
                        return NodeState.Running;
                }

                public void ActivateDamageEffect (GameObject gameObject)
                {
                        if (gameObject == null)
                                return;

                        if (tagName != "" && gameObject.CompareTag(tagName))
                        {
                                isActive = true;
                                counter = 0;
                        }
                }

                public void ActivateDamageEffect (ImpactPacket impact)
                {
                        if (impact.attacker.gameObject == null || impact.damageValue >= 0)
                                return;

                        if (tagName != "" && impact.attacker.gameObject.CompareTag(tagName))
                        {
                                isActive = true;
                                counter = 0;
                        }
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                public override bool OnInspector (AIBase ai, SerializedObject parent, Color color, bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(105, "Slow down the enemy, apply damage, and play the specified animation signal for the duration amount. Enable Start On Enter to activate this on state change, or call ActivateDamageEffect and use a gameObject's Tag." +
                                        "\n \nReturns Running, Success");
                        }

                        FoldOut.Box(6, color, offsetY: -2);
                        parent.Field("Duration", "duration");
                        parent.Slider("Slow Speed", "slowSpeed");
                        parent.Field("Damage Rate", "damageRate");
                        parent.Field("Animation Signal", "animationSignal");
                        parent.Field("Has Tag", "tagName");
                        parent.FieldToggle("Start On Enter", "startOnEnter");
                        Layout.VerticalSpacing(3);
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion
        }
}
