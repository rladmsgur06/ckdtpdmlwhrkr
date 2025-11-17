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
        public class SlamOnEnemy : Ability
        {
                [SerializeField] public LayerMask layer;
                [SerializeField] public float damage;
                [SerializeField] public float damageForce = 1f;
                [SerializeField] public string slamButton;
                [SerializeField] public float slamVelocity = 20f;
                [SerializeField] public float impactRadius = 2f;
                [SerializeField] public UnityEventEffect onSlam;
                [SerializeField] public UnityEventEffect onSlamBegin;
                [SerializeField] public string slamWE;
                [SerializeField] public string slamBeginWE;
                [SerializeField] public bool blockVelX = false;

                [SerializeField] public bool ground;
                [SerializeField] public float groundTime;

                [System.NonSerialized] private Health health;
                [System.NonSerialized] private bool begin;
                [System.NonSerialized] private bool holding;
                [System.NonSerialized] private bool isSlamming;
                [System.NonSerialized] private float groundCounter;

                private void Awake ()
                {
                        health = gameObject.GetComponent<Health>();
                }

                public override void Reset (AbilityManager player)
                {
                        if (isSlamming && health != null)
                        {
                                health.CanTakeDamage(true);
                        }

                        begin = false;
                        holding = false;
                        isSlamming = false;
                        groundCounter = 0;
                }

                public override bool TurnOffAbility (AbilityManager player)
                {
                        Reset(player);
                        return true;
                }

                public override bool IsAbilityRequired (AbilityManager player, ref Vector2 velocity)
                {
                        if (isSlamming)
                        {
                                return true;
                        }
                        if (pause || player.ground || velocity.y > 0)
                        {
                                return false;
                        }
                        if (player.inputs.Holding(slamButton))
                        {
                                begin = true;
                                return isSlamming = true;
                        }
                        return false;
                }

                public override void ExecuteAbility (AbilityManager player, ref Vector2 velocity, bool isRunningAsException = false) // since using EarlyExecute, this will ALWAYS execute. No need for priority.
                {
                        player.signals.Set("slamOnEnemy");
                        if (blockVelX)
                        {
                                velocity.x = 0;
                        }

                        if (begin)
                        {
                                begin = false;
                                onSlamBegin.Invoke(ImpactPacket.impact.Set(slamBeginWE, this.transform, player.world.boxCollider, player.world.position, this.transform, player.world.box.up, player.playerDirection, 0));
                        }

                        if (holding)
                        {
                                player.signals.Set("slamRecover");
                                if (Clock.TimerExpired(ref groundCounter, groundTime))
                                {
                                        Reset(player);
                                }
                                return;
                        }

                        velocity.y -= slamVelocity;
                        if (health != null)
                        {
                                health.CanTakeDamage(false);
                        }
                        if (player.world.onGround)
                        {
                                Vector2 position = player.world.position;
                                int hit = Compute.OverlapCircle(position, impactRadius, layer);
                                if (Health.HitContactResults(this.transform, Compute.contactResults, hit, -damage, damageForce, position)) // blast radius should be in all directions, derive direction from position
                                {
                                        onSlam.Invoke(ImpactPacket.impact.Set(slamWE, this.transform, player.world.boxCollider, player.world.position, this.transform, player.world.box.up, player.playerDirection, 0));
                                }
                                if (ground)
                                {
                                        holding = true;
                                        groundCounter = 0;
                                }
                                else
                                {
                                        Reset(player);
                                }
                        }
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField, HideInInspector] public bool eventsFoldOut;
                [SerializeField, HideInInspector] public bool slamBeginFoldOut;
                [SerializeField, HideInInspector] public bool slamFoldOut;

                public override bool OnInspector (SerializedObject controller, SerializedObject parent, string[] inputList, Color barColor, Color labelColor)
                {
                        if (Open(parent, "Slam On Enemy", barColor, labelColor))
                        {
                                FoldOut.Box(6, FoldOut.boxColorLight, extraHeight: 5, offsetY: -2);
                                parent.FieldDouble("Damage", "layer", "damage");
                                parent.Field("Slam Force", "damageForce");
                                parent.DropDownListAndField(inputList, "Slam Button", "slamButton", "slamVelocity");
                                Labels.FieldText("Velocity");
                                parent.Field("Impact Radius", "impactRadius");
                                parent.FieldAndEnable("Ground Time", "groundTime", "ground");
                                parent.FieldToggle("Block Vel X", "blockVelX");

                                if (FoldOut.FoldOutButton(parent.Get("eventsFoldOut")))
                                {
                                        Fields.EventFoldOutEffect(parent.Get("onSlamBegin"), parent.Get("slamBeginWE"), parent.Get("slamBeginFoldOut"), "On Slam Begin", color: FoldOut.boxColorLight);
                                        Fields.EventFoldOutEffect(parent.Get("onSlam"), parent.Get("slamWE"), parent.Get("slamFoldOut"), "On Slam End", color: FoldOut.boxColorLight);
                                }
                        }
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion

        }
}
