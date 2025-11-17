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
        public class BalanceBeam : Ability // beam should be an edge collider 2d!
        {
                [SerializeField] public string button;
                [SerializeField] public string beamTag = "Untagged";
                [SerializeField] public float difficultySpeed = 1f;
                [SerializeField] public float moveTime = 0.5f;
                [SerializeField] public bool hasTimeLit;
                [SerializeField] public float timeLit = 10f;
                [SerializeField] public UnityEvent onBegin;
                [SerializeField] public UnityEvent onEnd;
                [SerializeField] public UnityEventFloat sliderValue;

                // all these should add up to 1f
                [SerializeField] public float safeZone = 0.4f;
                [SerializeField] public float dangerZone = 0.3f;

                [System.NonSerialized] private bool isBalancing;
                [System.NonSerialized] private bool moveForward;
                [System.NonSerialized] private bool moveBackward;
                [System.NonSerialized] private float beamDirection;
                [System.NonSerialized] private float counter;
                [System.NonSerialized] private float timer;
                [System.NonSerialized] private float sign = 1f;
                [System.NonSerialized] private ContactFilter2D filter2D;
                [System.NonSerialized] private RaycastHit2D[] results = new RaycastHit2D[20];

                private float fallZone => 1f - Mathf.Clamp01(safeZone + dangerZone);
                private float fallZoneLeft => fallZone * 0.5f;
                private float fallZoneRight => 1f - fallZoneLeft;
                private float dangerZoneLeft => fallZoneLeft + dangerZone * 0.5f;
                private float dangerZoneRight => 1f - dangerZoneLeft;
                private bool notMoving => !moveBackward && !moveForward;

                private void Start ()
                {
                        filter2D = new ContactFilter2D();
                        filter2D.useLayerMask = true;
                        filter2D.useTriggers = true;
                        filter2D.layerMask = WorldManager.collisionMask;
                }

                public override void Reset (AbilityManager player)
                {
                        if (isBalancing)
                        {
                                onEnd.Invoke();
                        }
                        isBalancing = false;
                        moveForward = false;
                        moveBackward = false;
                        counter = 0;
                        timer = 0;
                        sign = 1f;
                }

                public override bool TurnOffAbility (AbilityManager player)
                {
                        FallThrough(player);
                        Reset(player);
                        return true;
                }

                public override bool IsAbilityRequired (AbilityManager player, ref Vector2 velocity)
                {
                        if (pause)
                                return false;

                        // What happens when ability is cancelled?, fall through
                        if (isBalancing && notMoving && (!player.world.onGround || player.world.verticalTransform == null || beamTag == "" || !player.world.verticalTransform.gameObject.CompareTag(beamTag)))
                        {
                                Reset(player);
                                return false;
                        }
                        if (isBalancing)
                        {
                                return true;
                        }
                        if (player.world.onGround && player.world.verticalTransform != null && beamTag != "" && player.world.verticalTransform.gameObject.CompareTag(beamTag))
                        {
                                Reset(player);
                                onBegin.Invoke();
                                beamDirection = player.playerDirection == 0 ? 1f : player.playerDirection;
                                return isBalancing = true;
                        }
                        return false;
                }

                public override void ExecuteAbility (AbilityManager player, ref Vector2 velocity, bool isRunningAsException = false)
                {
                        velocity.x = 0;
                        float value = PingPong();
                        sliderValue.Invoke(value);
                        player.signals.Set("balanceBeam");

                        if (notMoving && player.inputs.Pressed(button))
                        {
                                if (value < fallZoneLeft || value > fallZoneRight)
                                {
                                        if (FallThrough(player))
                                        {
                                                return;
                                        }
                                }
                                else if (value < dangerZoneLeft || value > dangerZoneRight)
                                {
                                        counter = 0;
                                        moveBackward = true; // move back
                                }
                                else
                                {
                                        counter = 0;
                                        moveForward = true; // move forward
                                }
                        }

                        if (moveForward)
                        {
                                player.signals.Set("balanceBeamForward");
                                velocity.x = Mathf.Abs(player.walk.speed) * beamDirection;
                                if (Clock.Timer(ref counter, moveTime))
                                {
                                        moveForward = moveBackward = false;
                                }
                        }
                        else if (moveBackward)
                        {
                                player.signals.Set("balanceBeamBackward");
                                player.signals.ForceDirection((int) beamDirection);
                                velocity.x = Mathf.Abs(player.walk.speed) * -beamDirection;
                                if (Clock.Timer(ref counter, moveTime))
                                {
                                        moveForward = moveBackward = false;
                                }
                        }
                }

                private bool FallThrough (AbilityManager player)
                {
                        if (isBalancing && !IsRealGround(player))
                        {
                                transform.position += (Vector3) player.world.box.down * 0.1f;
                                player.world.onGround = false;
                                player.world.box.Update();
                                Reset(player);
                                return true;
                        }
                        return false;
                }

                private bool IsRealGround (AbilityManager player)
                {
                        BoxInfo box = player.world.box;
                        Vector2 corner = box.bottomLeft;
                        float magnitude = box.skin.y + 0.1f;

                        for (int i = 0; i < box.rays.y; i++)
                        {
                                Vector2 origin = corner + box.right * (box.spacing.x * i);
                                int length = Physics2D.Raycast(origin, -box.up, filter2D, results, magnitude);

                                for (int j = 0; j < length; j++)
                                {
                                        RaycastHit2D hit = results[j];
                                        if (hit && !(hit.collider is EdgeCollider2D))
                                        {
                                                return true;
                                        }
                                }
                        }
                        return false;
                }

                private float PingPong ()
                {
                        timer += Time.deltaTime * difficultySpeed * sign;
                        if (timer <= 0)
                        {
                                timer = 0;
                                sign = 1f;
                        }
                        if (timer >= 1f)
                        {
                                timer = 1f;
                                sign = -1f;
                        }
                        return timer;
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField] public bool beginFoldOut;
                [SerializeField] public bool endFoldOut;
                [SerializeField] public bool sliderFoldOut;
                [SerializeField] public bool eventFoldOut;
                [SerializeField] public Transform dangerZoneUI;
                [SerializeField] public Transform safeZoneUI;
                public override bool OnInspector (SerializedObject controller, SerializedObject parent, string[] inputList, Color barColor, Color labelColor)
                {
                        if (Open(parent, "Balance Beam", barColor, labelColor))
                        {
                                FoldOut.Box(4, FoldOut.boxColorLight, extraHeight: 5, offsetY: -2);
                                {
                                        parent.DropDownList(inputList, "button", "button");
                                        parent.Field("Beam Tag", "beamTag");
                                        parent.Field("Difficulty Speed", "difficultySpeed");
                                        parent.Field("Move Time", "moveTime");
                                }

                                if (FoldOut.FoldOutButton(parent.Get("eventFoldOut")))
                                {
                                        Fields.EventFoldOut(parent.Get("onBegin"), parent.Get("beginFoldOut"), "On Begin", color: FoldOut.boxColorLight);
                                        Fields.EventFoldOut(parent.Get("onEnd"), parent.Get("endFoldOut"), "On End", color: FoldOut.boxColorLight);
                                        Fields.EventFoldOut(parent.Get("sliderValue"), parent.Get("sliderFoldOut"), "Slider Value", color: FoldOut.boxColorLight);
                                }

                                FoldOut.Box(2, FoldOut.boxColorLight);
                                {
                                        parent.FieldDouble("Safe Zone", "safeZone", "safeZoneUI");
                                        parent.FieldDouble("Danger Zone", "dangerZone", "dangerZoneUI");
                                        parent.Clamp("safeZone", 0, 0.5f);
                                        parent.Clamp("dangerZone", 0, 0.5f);
                                }
                                Layout.VerticalSpacing(5);
                        }
                        return true;
                }

                private void OnDrawGizmosSelected ()
                {
                        if (safeZoneUI != null)
                        {
                                Vector3 scale = safeZoneUI.transform.localScale;
                                safeZoneUI.transform.localScale = new Vector3(safeZone, scale.y, scale.z);
                                if (scale.x != safeZone)
                                {
                                        EditorUtility.SetDirty(safeZoneUI);
                                }
                        }
                        if (dangerZoneUI != null)
                        {
                                Vector3 scale = dangerZoneUI.transform.localScale;
                                dangerZoneUI.transform.localScale = new Vector3(safeZone + dangerZone, scale.y, scale.z);
                                if (scale.x != (safeZone + dangerZone))
                                {
                                        EditorUtility.SetDirty(dangerZoneUI);
                                }
                        }
                }
#pragma warning restore 0414
#endif
                #endregion

        }

}
