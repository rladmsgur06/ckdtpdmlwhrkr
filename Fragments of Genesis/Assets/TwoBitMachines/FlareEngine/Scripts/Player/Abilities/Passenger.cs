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
        public class Passenger : Ability
        {
                [SerializeField] public LayerMask vehicleLayer;
                [SerializeField] public string exit;
                [SerializeField] public bool exitOnDeath;
                [SerializeField] public UnityEvent onExit;

                [System.NonSerialized] private Player player;
                [System.NonSerialized] private Health health;
                [System.NonSerialized] private Vehicle vehicle;
                [System.NonSerialized] private Collider2D vehicleCollider;

                private bool isExiting = false;
                public bool isDead => health != null && health.GetValue() <= 0;
                private RaycastHit2D[] results = new RaycastHit2D[10];
                private ContactFilter2D filter2D = new ContactFilter2D();

                public override void Initialize (Player playerRef)
                {
                        player = playerRef;
                        health = gameObject.GetComponent<Health>();
                        filter2D = new ContactFilter2D();
                        filter2D.useTriggers = true;
                        filter2D.useLayerMask = true;
                        filter2D.layerMask = vehicleLayer;
                }

                public void ExitVehicle ()
                {
                        if (vehicle != null)
                        {
                                vehicle.passengerRef = null;
                                vehicle.passengerAbility = null;
                                vehicle = null;
                        }
                        player.BlockInput(false);
                }

                public bool ExitCollider (AbilityManager player)
                {
                        BoxInfo box = player.world.box;
                        int length = Physics2D.Raycast(box.bottomCenter, -box.up, filter2D, results, 0.1f);
                        for (int i = 0; i < length; i++)
                        {
                                if (results[i].collider == vehicleCollider)
                                        return false;
                        }
                        return true;
                }

                public override void Reset (AbilityManager player)
                {
                        ExitVehicle();
                        isExiting = false;
                        player.onVehicle = false;
                }

                public override bool TurnOffAbility (AbilityManager player)
                {
                        Reset(player);
                        return true;
                }

                public override bool IsAbilityRequired (AbilityManager player, ref Vector2 velocity)
                {
                        if (isExiting && (player.ground || ExitCollider(player)))
                        {
                                isExiting = false;
                        }

                        if (pause)
                                return false;

                        if ((vehicle != null && player.inputs.PressedUnblocked(exit)) || (exitOnDeath && isDead))
                        {
                                Reset(player);
                                onExit.Invoke();
                                isExiting = true;
                                return false;
                        }

                        if (vehicle != null)
                        {
                                return true;
                        }

                        if (Search(player, velocity))
                        {
                                return true;
                        }
                        return false;
                }

                public override void ExecuteAbility (AbilityManager player, ref Vector2 velocity, bool isRunningAsException = false)
                {
                        player.onVehicle = true;
                        this.player.BlockInput(true);
                        velocity = Vector2.zero;
                }

                public bool Search (AbilityManager player, Vector2 velocity)
                {
                        if (vehicle != null || player.ground || velocity.y > 0)
                                return false;

                        Vector2 v = velocity * Time.deltaTime;
                        BoxInfo box = player.world.box;
                        float magnitude = Mathf.Abs(v.y) + box.skin.y;
                        Vector2 origin = box.bottomCenter + box.right * v.x;
                        RaycastHit2D hit = Physics2D.Raycast(origin, -box.up, magnitude, vehicleLayer);

                        #region Debug
#if UNITY_EDITOR
                        if (WorldManager.viewDebugger)
                        {
                                Debug.DrawRay(origin, -box.up * magnitude, Color.blue);
                        }
#endif
                        #endregion

                        if (hit)
                        {
                                Vehicle vehicle = hit.transform.GetComponent<Vehicle>();
                                if (vehicle != null && (!isExiting || hit.collider != vehicleCollider))
                                {
                                        this.vehicle = vehicle;
                                        this.vehicle.passengerRef = this;
                                        this.vehicle.passengerAbility = this.player;
                                        this.vehicle.onMounted.Invoke();
                                        this.vehicle.counter = 0;

                                        isExiting = false;
                                        vehicleCollider = hit.collider;
                                        player.signals.characterDirection = this.vehicle.playerRef.signals.characterDirection;
                                        return true;
                                }
                        }
                        return false;
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                public override bool OnInspector (SerializedObject controller, SerializedObject parent, string[] inputList, Color barColor, Color labelColor)
                {
                        if (Open(parent, "Passenger", barColor, labelColor))
                        {
                                FoldOut.Box(3, FoldOut.boxColorLight, offsetY: -2);
                                {
                                        parent.Field("Layer", "vehicleLayer");
                                        parent.DropDownList(inputList, "Exit", "exit");
                                        parent.FieldToggleAndEnable("Exit On Death", "exitOnDeath");
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
