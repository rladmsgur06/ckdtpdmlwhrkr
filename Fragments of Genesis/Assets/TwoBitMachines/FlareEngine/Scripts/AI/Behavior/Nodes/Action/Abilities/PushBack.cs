#region 
#if UNITY_EDITOR
using TwoBitMachines.Editors;
using UnityEditor;
#endif
#endregion
using TwoBitMachines.TwoBitSprite;
using UnityEngine;

namespace TwoBitMachines.FlareEngine.AI
{
        [AddComponentMenu("")]
        public class PushBack : Action
        {
                [SerializeField] public float distanceX = 2;
                [SerializeField] public float jumpForce = 5;
                [SerializeField] public float pushTime = 0.25f;
                [SerializeField] public float holdTime = 0.25f;
                [SerializeField] public int flash = 1;
                [SerializeField] public string stateName;
                [SerializeField] public bool selectInterrupt;
                [SerializeField] public Color color = Color.white;
                [SerializeField] public Material material;
                [SerializeField] public SpriteRenderer spriteRenderer;
                [SerializeField] public SpriteEngineBase spriteEngine;

                [SerializeField] public bool useSignal;
                [SerializeField] public string signalName;
                [SerializeField] public string failedSignalName;
                [SerializeField] public float compareFloat;
                [SerializeField] public FloatLogicType logic;
                [SerializeField] public WorldFloat variable;

                [System.NonSerialized] private Material originMaterial;
                [System.NonSerialized] private Color originColor;
                [System.NonSerialized] private Vector2 appliedForce;
                [System.NonSerialized] private Vector2 forceDirection;
                [System.NonSerialized] private AIFSM parent;
                [System.NonSerialized] private Root rootRef;

                [System.NonSerialized] private bool pushBack;
                [System.NonSerialized] private bool flashToggle;
                [System.NonSerialized] private bool beginPushBack;
                [System.NonSerialized] private int rootIndex;
                [System.NonSerialized] private int pushBackCalls;
                [System.NonSerialized] private int characterDirection;
                [System.NonSerialized] private float appliedForceXRef;
                [System.NonSerialized] private float acceleration;
                [System.NonSerialized] private float flashCounter;
                [System.NonSerialized] private float flashTime;
                [System.NonSerialized] private float pushCounter;
                [System.NonSerialized] private float holdCounter;

                public void Start ()
                {
                        if (material != null)
                        {
                                material = new Material(material);
                        }
                        if (spriteRenderer != null)
                        {
                                originMaterial = spriteRenderer.material;
                        }
                        if (spriteRenderer != null)
                        {
                                originColor = spriteRenderer.color;
                        }
                        parent = GetComponent<AIFSM>();
                        rootRef = GetComponent<Root>();
                }

                public override void OnReset (bool skip = false, bool enteredState = false)
                {
                        if (skip)
                                return;
                        if (spriteRenderer != null && spriteRenderer.material != originMaterial)
                                spriteRenderer.material = originMaterial;
                        if (spriteRenderer != null)
                                spriteRenderer.color = originColor;
                        if (spriteEngine != null)
                                spriteEngine.Pause(false);
                        rootRef.direction = characterDirection;
                        rootRef.signals.characterDirection = characterDirection;
                        beginPushBack = false;
                        pushBack = false;
                        flashToggle = false;
                        pushBackCalls = 0;
                        flashCounter = 0;
                        flashTime = 0;
                        pushCounter = 0;
                        holdCounter = 0;
                }

                public void ActivatePushBack (ImpactPacket packet)
                {

                        if (packet.damageValue >= 0)
                        {
                                return; // Only apply push back on negative values
                        }
                        if (selectInterrupt && !rootRef.selectInterrupt)
                        {
                                return;
                        }
                        if (rootRef.aiState != null && rootRef.aiState.cantInterrupt)
                        {
                                return;
                        }


                        pushCounter = 0;
                        holdCounter = 0;
                        pushBack = true;
                        beginPushBack = true;
                        flashToggle = false;
                        rootRef.selectInterrupt = false;
                        flashCounter = 1000f; //  Go into flash immediately
                        float totalTime = pushTime + holdTime;
                        flashTime = flash <= 1 ? totalTime / 2f : totalTime / ((float) flash * 2f);
                        forceDirection = packet.direction;
                        acceleration = (-2f * distanceX) / Mathf.Pow(totalTime, 2f);
                        appliedForce = acceleration * forceDirection * totalTime;
                        parent.ChangeStateSkipReset(stateName);
                        rootRef.foundInterrupt = rootIndex;
                        appliedForceXRef = appliedForce.x;
                        pushBackCalls++;
                }

                public override NodeState RunNodeLogic (Root root)
                {
                        rootIndex = root.interruptIndex; // weakness: this has the potential of failing on the first frame since it's not set, but that will be extremely rare
                        if (!pushBack)
                                return NodeState.Failure;

                        if (beginPushBack)
                        {
                                if (pushBackCalls <= 1f)
                                        characterDirection = root.signals.characterDirection; // pushbackcalls prevents resetting direction if pushback is already active
                                beginPushBack = false;
                                if (spriteEngine != null)
                                        spriteEngine.Pause(true);
                        }

                        if (pushCounter == 0 && Root.deltaTime != 0)
                        {
                                root.velocity.y = forceDirection.y > 0 ? jumpForce * (forceDirection.y) : jumpForce * (forceDirection.y * 3f);
                                root.hasJumped = forceDirection.y > 0 ? true : root.hasJumped;
                        }

                        if (TwoBitMachines.Clock.Timer(ref flashCounter, flashTime) && spriteRenderer != null)
                        {
                                flashToggle = !flashToggle;
                                if (material != null)
                                {
                                        material.color = color;
                                        spriteRenderer.color = color;
                                        spriteRenderer.material = flashToggle ? material : originMaterial;
                                }
                                else
                                {
                                        spriteRenderer.color = flashToggle ? color : originColor;
                                }
                        }

                        root.signals.Set("pushBack", true);
                        root.signals.Set("pushBackLeft", characterDirection < 0);
                        root.signals.Set("pushBackRight", characterDirection > 0);

                        if (TwoBitMachines.Clock.TimerExpired(ref pushCounter, pushTime))
                        {
                                if (TwoBitMachines.Clock.TimerExpired(ref holdCounter, holdTime))
                                {
                                        root.velocity.x = 0;
                                        OnReset();
                                        return NodeState.Success;
                                }
                                return NodeState.Running;
                        }
                        if (useSignal)
                        {
                                NodeState state = WorldFloatLogic.Compare(logic, variable.GetValue(), compareFloat);
                                root.signals.Set(state == NodeState.Success ? signalName : failedSignalName);
                        }
                        appliedForce -= acceleration * forceDirection * Root.deltaTime;
                        root.velocity.x = Compute.SameSign(appliedForceXRef, appliedForce.x) ? -appliedForce.x : 0;
                        return NodeState.Running;
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                public override bool OnInspector (AIBase ai, SerializedObject parent, Color color, bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(152, "If damaged, the AI will be pushed back. To trigger, use the Health On Value Changed to call ActivatePushBack on this class. If the material exists, it will be applied to the Sprite for a flash. Otherwise, the Sprite Render color will be used. If SpriteEngine reference exists, it will pause the current sprite. If working with a FSM, specify the state that will execute the push back. Signals: pushBack, pushBackLeft, pushBackRight" +
                                        "\n \nReturns Running, Success, Failure");
                        }

                        FoldOut.Box(6, color, offsetY: -2);
                        {
                                parent.Field("State Name", "stateName");
                                parent.Field("Jump Force", "jumpForce");
                                parent.Field("Distance X", "distanceX");
                                parent.FieldDouble("Push, Hold Times", "pushTime", "holdTime");
                                parent.FieldDouble("Flash", "flash", "color");
                                parent.FieldToggleAndEnable("Select Interrupt", "selectInterrupt");
                        }
                        Layout.VerticalSpacing(3);

                        FoldOut.Box(3, color, offsetY: -2);
                        {
                                parent.Field("Material", "material");
                                parent.Field("Sprite Renderer", "spriteRenderer");
                                parent.Field("Sprite Engine", "spriteEngine");
                        }
                        Layout.VerticalSpacing(3);

                        bool useSignal = parent.Bool("useSignal");
                        FoldOut.Box(3, color, offsetY: -2);
                        {
                                parent.FieldAndEnable("Use Signal", "variable", "useSignal");
                                GUI.enabled = useSignal;
                                parent.FieldDouble("Compare Float", "logic", "compareFloat");
                                parent.FieldDouble("Success, Fail Signal", "signalName", "failedSignalName");
                                GUI.enabled = true;
                        }
                        Layout.VerticalSpacing(3);
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion
        }
}
