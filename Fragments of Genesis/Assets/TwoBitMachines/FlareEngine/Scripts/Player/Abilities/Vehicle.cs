#region 
#if UNITY_EDITOR
using TwoBitMachines.Editors;
using UnityEditor;
#endif
#endregion
using UnityEngine;
using UnityEngine.Events;

namespace TwoBitMachines.FlareEngine.ThePlayer
{
        [AddComponentMenu("")]
        public class Vehicle : Ability
        {
                [SerializeField] private Vector2 passengerPosition;
                [SerializeField] public float mountTime = 0.5f;
                [SerializeField] public bool stopOnPassengerDeath;
                [SerializeField] public UnityEvent onMounted;

                [System.NonSerialized] public Passenger passengerRef;
                [System.NonSerialized] public Player passengerAbility;
                [System.NonSerialized] public Player playerRef;
                [System.NonSerialized] public float counter = 0;

                public override void Initialize (Player player)
                {
                        player.isVehicle = true;
                        player.BlockInput(true);
                        playerRef = player;
                }

                public override bool ContainsException (string typeName)
                {
                        return true; // everything is an exception
                }

                public override bool IsAbilityRequired (AbilityManager player, ref Vector2 velocity)
                {
                        if (pause)
                                return false;

                        if (passengerAbility != null)
                        {
                                playerRef.BlockInput(false);
                                return true;
                        }
                        playerRef.BlockInput(true);
                        return false;
                }

                public override void LateExecute (AbilityManager player, ref Vector2 velocity)
                {
                        if (stopOnPassengerDeath && passengerRef != null && passengerRef.isDead)
                        {
                                velocity = Vector2.zero;
                        }
                }

                public override void PostCollisionExecute (AbilityManager player, Vector2 velocity)
                {
                        if (passengerAbility == null)
                                return;

                        if (!Clock.TimerExpired(ref counter, mountTime))
                        {
                                player.signals.Set("vehicleMounted");
                        }

                        Vector3 newHoldPosition = player.signals.characterDirection > 0 ? passengerPosition : new Vector2(-passengerPosition.x, passengerPosition.y);
                        passengerAbility.transform.position = transform.position + newHoldPosition;
                        passengerAbility.signals.SetSignals(velocity, player.ground, player.world.onWallStop);
                        passengerAbility.signals.Set("onVehicle");
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField, HideInInspector] public bool eventFoldout;
                public override bool OnInspector (SerializedObject controller, SerializedObject parent, string[] inputList, Color barColor, Color labelColor)
                {
                        if (Open(parent, "Vehicle", barColor, labelColor))
                        {
                                FoldOut.Box(3, FoldOut.boxColorLight, offsetY: -2);
                                {
                                        parent.Field("Passenger Position", "passengerPosition");
                                        parent.Field("Mount Time", "mountTime");
                                        parent.FieldToggle("Stop On Passenger Death", "stopOnPassengerDeath");
                                }
                                Layout.VerticalSpacing(3);

                                Fields.EventFoldOut(parent.Get("onMounted"), parent.Get("eventFoldout"), "On Mount", color: FoldOut.boxColorLight);
                        }
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion
        }
}
