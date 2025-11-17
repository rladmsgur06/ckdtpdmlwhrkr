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
        public class HighJump : Ability
        {
                [System.NonSerialized] private Vector2 force;
                [System.NonSerialized] private Vector2 forceOrigin;
                [System.NonSerialized] private bool initialized;
                [System.NonSerialized] private bool escapeY;
                [System.NonSerialized] private int type; // 1 = jump, 2 = wind
                [System.NonSerialized] private float timer;
                [System.NonSerialized] private float boostCounter = 0;

                [SerializeField] public float windRate = 0.25f;
                [SerializeField] public float slowRate = 0.25f;
                [SerializeField] public string trampolineWE;
                [SerializeField] public string windWE;
                [SerializeField] public string slowDownWE;
                [SerializeField] public string speedBoostWE;
                [SerializeField] public UnityEventEffect onTrampoline;
                [SerializeField] public UnityEventEffect onWind;
                [SerializeField] public UnityEventEffect onSpeedBoost;
                [SerializeField] public UnityEventEffect onSlowDown;


                [SerializeField] private bool pauseJump;
                [SerializeField] private bool pauseWind;
                [SerializeField] private bool pauseBoost;
                [SerializeField] private bool pauseSlow;

                [System.NonSerialized] private float windTimer = 0;
                [System.NonSerialized] private float slowTimer = 0;
                [System.NonSerialized] private float slowCounter = 0;
                [System.NonSerialized] private Transform oldBoost;


                private const int JUMP = 1;
                private const int WIND = 2;
                private const int BOOST = 3;
                private const int SLOW = 4;

                public override void Reset (AbilityManager player)
                {
                        type = 0;
                        boostCounter = 0;
                        slowCounter = 0;
                        escapeY = false;
                        initialized = false;
                        oldBoost = null;
                        force = Vector2.zero;
                }

                public override bool TurnOffAbility (AbilityManager player)
                {
                        Reset(player);
                        return true;
                }

                public override bool IsAbilityRequired (AbilityManager player, ref Vector2 velocity)
                {
                        if (pause)
                                return false;

                        if (type == JUMP && player.world.onGround)
                        {
                                Reset(player);
                        }
                        if (type == WIND && !Found(player, velocity.y))
                        {
                                Reset(player);
                        }
                        if (type == SLOW && !Found(player, velocity.y))
                        {
                                Reset(player);
                        }
                        if (type == BOOST && !Found(player, velocity.y))
                        {
                                oldBoost = null;
                        }
                        if (Found(player, velocity.y))
                        {
                                if (type == JUMP && pauseJump)
                                        type = 0;
                                if (type == WIND && pauseWind)
                                        type = 0;
                                if (type == BOOST && pauseBoost)
                                        type = 0;
                                if (type == SLOW && pauseSlow)
                                        type = 0;
                                initialized = false;
                                boostCounter = 0;
                                forceOrigin = force;
                                escapeY = false;
                        }
                        return type != 0;
                }

                public override void ExecuteAbility (AbilityManager player, ref Vector2 velocity, bool isRunningAsException = false)
                {
                        if (type == JUMP)
                                Jump(player, ref velocity);
                        if (type == WIND)
                                Wind(player, ref velocity);
                        if (type == BOOST)
                                Boost(player, ref velocity);
                        if (type == SLOW)
                                Slow(player, ref velocity);
                }
                private void Jump (AbilityManager player, ref Vector2 velocity)
                {
                        if (!initialized)
                        {
                                initialized = true;
                                velocity.y = force.y;
                                player.CheckForAirJumps();
                                onTrampoline.Invoke(ImpactPacket.impact.Set(trampolineWE, player.world.position, -force.normalized));
                        }
                        if (force.x != 0)
                        {
                                if (Compute.SameSign(velocity.x, force.x)) // dont increase force 
                                {
                                        velocity.x = force.x;
                                }
                                else
                                {
                                        force.x += velocity.x * Time.deltaTime;
                                        velocity.x = force.x;
                                }
                        }
                        player.signals.Set("highJump", velocity.y > 0);
                }
                private void Wind (AbilityManager player, ref Vector2 velocity)
                {
                        if (Clock.Timer(ref windTimer, windRate))
                        {
                                onWind.Invoke(ImpactPacket.impact.Set(windWE, player.world.position, -force.normalized));
                        }
                        if (Found(player, velocity.y))
                        {
                                velocity.y += force.y;
                                velocity.x += force.x;
                                player.signals.Set("windJump");
                                player.signals.Set("windLeft", force.x < 0);
                                player.signals.Set("windRight", force.x > 0);
                                force.y = 0;
                        }
                }
                private void Boost (AbilityManager player, ref Vector2 velocity)
                {
                        if (!initialized)
                        {
                                initialized = true;
                                onSpeedBoost.Invoke(ImpactPacket.impact.Set(speedBoostWE, player.world.position, -force.normalized));
                        }
                        boostCounter += Time.deltaTime;
                        float time = timer <= 0 ? 1f : timer;
                        force = Vector2.Lerp(forceOrigin, Vector2.zero, boostCounter / time);
                        if (player.jumpButtonActive)
                        {
                                escapeY = true;
                        }
                        float mag = escapeY ? Mathf.Abs(force.x) : force.magnitude;
                        if (boostCounter >= time || velocity.magnitude >= mag)
                        {
                                oldBoost = null;
                                type = 0;
                        }
                        if (!escapeY)
                        {
                                velocity = force;
                        }
                        else
                        {
                                velocity.x = force.x;
                        }
                        player.signals.Set("speedBoost");
                }
                private void Slow (AbilityManager player, ref Vector2 velocity)
                {
                        if (Clock.Timer(ref slowTimer, slowRate))
                        {
                                onSlowDown.Invoke(ImpactPacket.impact.Set(slowDownWE, player.world.position, -force.normalized));
                        }
                        slowCounter += Time.deltaTime;
                        float adjust = Mathf.Lerp(1f - force.x, 0, slowCounter / 0.75f);
                        if (velocity.x != 0)
                                velocity.x *= (force.x + adjust);
                        if (velocity.y != 0)
                                velocity.y *= (force.y + adjust);
                }
                private bool Found (AbilityManager player, float velocityY)
                {
                        return Interactables.HighJump.Find(player.world, velocityY, ref type, ref timer, ref force, ref oldBoost);
                }

                public void PauseJump (bool value)
                {
                        pauseJump = value;
                }
                public void PauseWind (bool value)
                {
                        pauseWind = value;
                }
                public void PauseBoost (bool value)
                {
                        pauseBoost = value;
                }
                public void PauseSlow (bool value)
                {
                        pauseWind = value;
                }
                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField, HideInInspector] private bool eventsFoldOut;
                [SerializeField, HideInInspector] private bool trampolineFoldOut;
                [SerializeField, HideInInspector] private bool windFoldOut;
                [SerializeField, HideInInspector] private bool speedBoostFoldOut;
                [SerializeField, HideInInspector] private bool slowDownFoldOut;
                public override bool OnInspector (SerializedObject controller, SerializedObject parent, string[] inputList, Color barColor, Color labelColor)
                {
                        if (Open(parent, "High Jump", barColor, labelColor))
                        {
                                FoldOut.Box(4, Tint.Box, extraHeight: 5, offsetY: -2);
                                {
                                        parent.FieldToggleAndEnable("Pause Jump", "pauseJump");
                                        parent.FieldToggleAndEnable("Pause Wind", "pauseWind");
                                        parent.FieldToggleAndEnable("Pause Boost", "pauseBoost");
                                        parent.FieldToggleAndEnable("Pause Slow", "pauseSlow");
                                }
                                if (FoldOut.FoldOutButton(parent.Get("eventsFoldOut")))
                                {
                                        Fields.EventFoldOutEffect(parent.Get("onTrampoline"), parent.Get("trampolineWE"), parent.Get("trampolineFoldOut"), "On Trampoline");
                                        Fields.EventFoldOutEffectAndRate(parent.Get("onWind"), parent.Get("windWE"), parent.Get("windRate"), parent.Get("windFoldOut"), "On Wind");
                                        Fields.EventFoldOutEffect(parent.Get("onSpeedBoost"), parent.Get("speedBoostWE"), parent.Get("speedBoostFoldOut"), "On Speed Boost");
                                        Fields.EventFoldOutEffectAndRate(parent.Get("onSlowDown"), parent.Get("slowDownWE"), parent.Get("slowRate"), parent.Get("slowDownFoldOut"), "On Slow Down");
                                }
                        }
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion

        }

}
