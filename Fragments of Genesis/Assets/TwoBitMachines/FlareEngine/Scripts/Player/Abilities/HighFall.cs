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
        public class HighFall : Ability
        {
                [SerializeField] public float height = 10f;
                [SerializeField] public float damage = 1f;
                [SerializeField] public float holdTime = 1f;
                // [SerializeField] public string damageSignal = "highFallDamage";
                // [SerializeField] public string nearGroundSignal = "highFallDanger";

                private Health health;

                private bool active = false;
                private bool inAir = false;
                private float jumpPoint = 0;
                private float counter = 0;

                public override void Initialize (Player player)
                {
                        health = gameObject.GetComponent<Health>();
                }

                public override void Reset (AbilityManager player)
                {
                        if (active && health != null && health.cantTakeDamage)
                        {
                                health.CanTakeDamage(true);
                        }
                        active = false;
                        inAir = false;
                        jumpPoint = 0;
                        counter = 0;
                }

                public override bool IsAbilityRequired (AbilityManager player, ref Vector2 velocity)
                {
                        if (pause)
                        {
                                return false;
                        }
                        if (active)
                        {
                                return true;
                        }

                        if (!player.world.onGround && player.world.wasOnGround)
                        {
                                jumpPoint = player.world.oldPosition.y;
                                inAir = true;
                        }
                        if (inAir && player.world.onGround && player.world.position.y < jumpPoint && Mathf.Abs(jumpPoint - player.world.oldPosition.y) < height)
                        {
                                inAir = false;
                        }
                        if (inAir && player.world.onGround && player.world.position.y < jumpPoint && Mathf.Abs(jumpPoint - player.world.oldPosition.y) >= height)
                        {
                                counter = 0;
                                inAir = false;
                                if (health != null)
                                {
                                        Health.IncrementHealth(transform, transform, -damage, Vector2.up);
                                        health.Recover(true); // reset damage timer, you need to take care of damage time
                                        health.CanTakeDamage(false);
                                }
                                return holdTime == 0 ? false : active = true;
                        }
                        return false;
                }

                public override void ExecuteAbility (AbilityManager player, ref Vector2 velocity, bool isRunningAsException = false)
                {
                        player.velocity.x = 0;
                        player.signals.Set("highFallDamage");
                        if (health != null)
                        {
                                health.CanTakeDamage(false);
                        }

                        if (Clock.Timer(ref counter, holdTime))
                        {
                                Reset(player);
                        }

                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                public override bool OnInspector (SerializedObject controller, SerializedObject parent, string[] inputList, Color barColor, Color labelColor)
                {
                        if (Open(parent, "High Fall", barColor, labelColor))
                        {
                                FoldOut.Box(3, FoldOut.boxColorLight, offsetY: -2);
                                {
                                        parent.Field("Height", "height");
                                        parent.Field("Damage", "damage");
                                        parent.Field("Hold Time", "holdTime");
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
