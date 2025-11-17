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
        public class JumpOnEnemy : Ability
        {
                [SerializeField] public LayerMask layer;
                [SerializeField] public float damage = 1f;
                [SerializeField] public float damageForce = 1f;
                [SerializeField] public float bounceForce = 10f;
                [SerializeField] public float bounceForceBoost = 1f;
                [SerializeField] public string bounceWE;
                [SerializeField] public string boostButton;
                [SerializeField] public string avoidTag;
                [SerializeField] public UnityEventEffect onBounce;

                [System.NonSerialized] private Health health;
                [System.NonSerialized] private bool jumpHitActive;

                private void Awake ()
                {
                        health = this.gameObject.GetComponent<Health>();
                }
                public override void Reset (AbilityManager player)
                {
                        jumpHitActive = false;
                }

                public override void EarlyExecute (AbilityManager player, ref Vector2 velocity) // since using EarlyExecute, this will ALWAYS execute. No need for priority.
                {
                        if (jumpHitActive)
                        {
                                player.signals.Set("jumpOnEnemy");
                                if (player.ground)
                                {
                                        jumpHitActive = false;
                                }
                        }
                        if (pause || player.onVehicle || player.ground || velocity.y > 0 || (health != null && health.GetValue() == 0))
                        {
                                return;
                        }

                        Vector2 v = velocity * Time.deltaTime;
                        BoxInfo box = player.world.box;
                        float magnitude = Mathf.Abs(v.y) + box.skin.y + 0.1f;
                        Vector2 corner = box.bottomLeft;

                        for (int i = 0; i < box.rays.y; i++)
                        {
                                Vector2 origin = corner + box.right * (box.spacing.x * i + v.x);
                                RaycastHit2D hit = Physics2D.Raycast(origin, -box.up, magnitude, layer);

                                #region Debug
#if UNITY_EDITOR
                                if (WorldManager.viewDebugger)
                                {
                                        Debug.DrawRay(origin, -box.up * magnitude, Color.blue);
                                }
#endif
                                #endregion

                                if (hit && avoidTag != "" && avoidTag != "Untagged" && hit.transform.gameObject.CompareTag(avoidTag))
                                {
                                        continue;
                                }

                                if (hit && damage != 0 && Health.IncrementHealth(transform, hit.transform, -damage, box.down * damageForce))
                                {
                                        float boost = player.inputs.Holding(boostButton) ? bounceForceBoost : 1f;
                                        velocity.y = bounceForce * boost;
                                        onBounce.Invoke(ImpactPacket.impact.Set(bounceWE, hit.transform, hit.collider, hit.transform.position, transform, player.world.box.down, player.playerDirection, -damage));
                                        jumpHitActive = true;
                                        return;
                                }
                        }
                }

                public void SetDamage (float newDamage)
                {
                        damage = newDamage;
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField, HideInInspector] private bool bounceFoldOut;
                public override bool OnInspector (SerializedObject controller, SerializedObject parent, string[] inputList, Color barColor, Color labelColor)
                {
                        if (Open(parent, "Jump On Enemy", barColor, labelColor))
                        {
                                FoldOut.Box(4, FoldOut.boxColorLight, offsetY: -2);
                                {
                                        string[] tags = UnityEditorInternal.InternalEditorUtility.tags;
                                        parent.Field("Layer", "layer");
                                        parent.Field("Bounce Force", "bounceForce");
                                        parent.FieldDouble("Damage", "damage", "damageForce");
                                        Labels.FieldText("Force", rightSpacing: 3);
                                        //parent.Field("Avoid Tag", "avoidTag");
                                        parent.DropDownList(tags, "Avoid Tag", "avoidTag");
                                }
                                Layout.VerticalSpacing(3);

                                FoldOut.Box(1, FoldOut.boxColorLight);
                                {
                                        parent.DropDownListAndField(inputList, "Boost Button", "boostButton", "bounceForceBoost");
                                        Labels.FieldText("Boost", rightSpacing: 2);
                                }
                                Layout.VerticalSpacing(5);

                                Fields.EventFoldOutEffect(parent.Get("onBounce"), parent.Get("bounceWE"), parent.Get("bounceFoldOut"), "On Bounce", color: FoldOut.boxColorLight);
                        }
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion

        }
}
