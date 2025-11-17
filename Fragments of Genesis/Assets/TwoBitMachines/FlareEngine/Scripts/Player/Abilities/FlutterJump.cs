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
        public class FlutterJump : Ability
        {
                [SerializeField] public string flutterButton;
                [SerializeField] public float duration = 1.5f;
                [SerializeField] public float amplitude = 1f;
                [SerializeField] public float frequency = 10f;

                [System.NonSerialized] private Jump jumpAbility;
                [System.NonSerialized] private bool isFluttering;
                [System.NonSerialized] private bool mustReset;
                [System.NonSerialized] private float counter;

                public override void Initialize (Player player)
                {
                        jumpAbility = GetComponent<Jump>();
                }

                public override void Reset (AbilityManager player)
                {
                        counter = 0;
                        isFluttering = false;
                }

                public override bool TurnOffAbility (AbilityManager player)
                {
                        Reset(player);
                        return true;
                }

                public override bool IsAbilityRequired (AbilityManager player, ref Vector2 velocity)
                {
                        if (pause)
                        {
                                return false;
                        }
                        if (player.world.onGround)
                        {
                                mustReset = false;
                        }
                        if (isFluttering)
                        {
                                if (!player.inputs.Holding(flutterButton))
                                {
                                        Reset(player);
                                        return false;
                                }
                                return true;
                        }
                        if (!mustReset && !player.world.onGround && player.inputs.Holding(flutterButton))
                        {
                                if (jumpAbility == null || jumpAbility.minJumpHeight == 0 || velocity.y > jumpAbility.minJumpVel || velocity.y <= 0)
                                {
                                        counter = 0;
                                        isFluttering = true;
                                        return true;
                                }
                        }
                        return isFluttering;
                }

                public override void ExecuteAbility (AbilityManager player, ref Vector2 velocity, bool isRunningAsException = false)
                {
                        velocity.y = Compute.SineWave(0, frequency, amplitude);
                        player.signals.Set("flutter");

                        if (Clock.Timer(ref counter, duration))
                        {
                                mustReset = true;
                                Reset(player);
                        }
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                public override bool OnInspector (SerializedObject controller, SerializedObject parent, string[] inputList, Color barColor, Color labelColor)
                {
                        if (Open(parent, "Flutter", barColor, labelColor))
                        {
                                FoldOut.Box(4, FoldOut.boxColorLight);
                                {
                                        parent.DropDownList(inputList, "Button", "flutterButton");
                                        parent.Field("Duration", "duration");
                                        parent.Field("Strength", "amplitude");
                                        parent.Field("Frequency", "frequency");
                                }
                                Layout.VerticalSpacing(5);
                        }
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion
        }
}
