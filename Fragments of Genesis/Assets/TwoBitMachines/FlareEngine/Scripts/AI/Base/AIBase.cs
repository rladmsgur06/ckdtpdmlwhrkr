#region
#if UNITY_EDITOR
using TwoBitMachines.Editors;
using UnityEditor;
#endif
#endregion
using System.Collections.Generic;
using TwoBitMachines.FlareEngine.AI.BlackboardData;
using UnityEngine;

namespace TwoBitMachines.FlareEngine.AI
{
        [AddComponentMenu("")]
        public class AIBase : Character
        {
                [SerializeField] public Root root;
                [SerializeField] public bool thisIsPlayer;
                [SerializeField] public bool resetToActive;
                [SerializeField] public bool movingPlatformGravity;
                [SerializeField] public Gravity gravity = new Gravity();
                [SerializeField] public AIDamagePack damage = new AIDamagePack();
                [SerializeField] public List<Blackboard> data = new List<Blackboard>();

                [System.NonSerialized] public Vector2 velocity;
                [System.NonSerialized] public bool initialized;
                [System.NonSerialized] public bool slowDownActive;
                [System.NonSerialized] public float timeScaleIntensity;
                public bool MovingPlatformHasGravity => movingPlatformGravity && type == CharacterType.MovingPlatform;

                public void Awake ()
                {
                        if (!turnOffSignals)
                        {
                                if (thisIsPlayer)
                                {
                                        signals.InitializeToPlayer(transform);
                                }
                                else
                                {
                                        signals.InitializeToSpriteEngine(transform);
                                }
                        }
                        if (type == CharacterType.MovingPlatform)
                        {
                                movingPlatform.Initialize(transform);
                        }
                        if (type == CharacterType.Regular)
                        {
                                world.Initialize(transform);
                        }
                        if (MovingPlatformHasGravity)
                        {
                                // world.collideWorldOnly = true;
                                world.Initialize(transform);
                        }

                        gravity.Initialize();
                        root.type = type;
                        root.world = world;
                        root.gravity = gravity;
                        root.signals = signals;
                        root.movingPlatform = movingPlatform;
                        Initialize();
                }

                public override void OnStart ()
                {
                        if (damage.canDealDamage)
                        {
                                AIDamage aIDamage = gameObject.AddComponent<AIDamage>();
#if UNITY_EDITOR
                                aIDamage.hideFlags = HideFlags.HideInInspector;
#endif
                        }
                }

                public virtual void Initialize ()
                {

                }

                public override void OnEnabled (bool onEnable)
                {
                        root.pause = !onEnable;
                        if (initialized && (type == CharacterType.Regular || MovingPlatformHasGravity))
                        {
                                world.Reset();
                                world.box.Update();
                        }
                        initialized = true;
                }

                public void Pause (bool value)
                {
                        root.pause = value;
                }

                public bool IsPaused ()
                {
                        return root.pause;
                }

                public void PauseDamage (bool value)
                {
                        damage.pauseDamage = value;
                }

                public void PauseDamageTimer (float time)
                {
                        damage.pauseDamageTimer = true;
                        damage.pauseTimer = time;
                }

                public void IncreaseDamage_Add (float value)
                {
                        damage.damage += value;
                }

                public void IncreaseDamage_Multiply (float value)
                {
                        damage.damage *= value;
                }

                public void ApplyGravity ()
                {
                        if (type == CharacterType.Regular || MovingPlatformHasGravity)
                        {
                                gravity.Execute(root.onSurface || world.onCeiling, ref velocity);
                        }
                }

                public void Collision (Vector2 velocity, ref float gravity)
                {
                        if (type == CharacterType.Regular)
                        {
                                if (!root.pauseCollision)
                                {
                                        initialVelocity = velocity;
                                        world.Move(ref velocity, ref gravity, root.hasJumped, Root.deltaTime, ref root.onSurface);
                                        world.box.Update();
                                }
                                else
                                {
                                        world.ResetCollisionInfo();
                                        if (root.moveWithTranslate)
                                        {
                                                transform.Translate(velocity * Root.deltaTime);
                                        }
                                        else
                                        {
                                                transform.position += (Vector3) velocity * Root.deltaTime;
                                        }
                                }
                        }
                        else if (type == CharacterType.MovingPlatform)
                        {
                                if (MovingPlatformHasGravity && !root.pauseCollision)
                                {
                                        initialVelocity = velocity;
                                        if (MovingPlatformHasGravity)
                                                world.boxCollider.enabled = false;
                                        world.Move(ref velocity, ref gravity, root.hasJumped, Root.deltaTime, ref root.onSurface);
                                        if (MovingPlatformHasGravity)
                                                world.boxCollider.enabled = true;
                                        world.box.Update();

                                        if (Root.deltaTime != 0)
                                        {
                                                movingPlatform.UpdatePlatform(velocity / Root.deltaTime, ref root.isShaking, false);
                                        }
                                }
                                else
                                {
                                        if (MovingPlatformHasGravity)
                                        {
                                                world.ResetCollisionInfo();
                                        }
                                        else
                                        {
                                                gravity = 0;
                                        }
                                        movingPlatform.UpdatePlatform(velocity, ref root.isShaking);
                                }
                        }
                        else
                        {
                                transform.position += (Vector3) velocity * Root.deltaTime;
                                gravity = 0; // dont remember gravity
                        }

                        if (!turnOffSignals && !thisIsPlayer)
                        {
                                signals.SetSignals(velocity, root.onSurface, world.onWallStop);
                        }

                        setVelocity = velocity; // equipment velocity
                        externalVelocity = Vector2.zero;
                }

                public void SlowDown ()
                {
                        if (slowDownActive)
                        {
                                Root.deltaTime = Time.deltaTime * timeScaleIntensity;
                        }
                }

                public void SlowDownReset ()
                {
                        if (slowDownActive)
                        {
                                Root.deltaTime = Time.deltaTime;
                        }
                }

                public virtual void ChangeState (string stateName)
                {

                }

                public virtual void ChangeStateCheckInterrupt (string stateName)
                {

                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀ 
#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField, HideInInspector] public List<Node> tempChildren = new List<Node>();
                [SerializeField, HideInInspector] public List<GameObject> units = new List<GameObject>();

                [SerializeField, HideInInspector] public Node inspectNode;
                [SerializeField, HideInInspector] public NodeMessage nodeMessage;
                [SerializeField, HideInInspector] public Matrix4x4 oldGUIMatrix;

                [SerializeField, HideInInspector] public string tempNameType = "";
                [SerializeField, HideInInspector] public string inspectNameType = "";

                [SerializeField, HideInInspector] public int barIndex;
                [SerializeField, HideInInspector] public int tempUnits;
                [SerializeField, HideInInspector] public int menuIndex;
                [SerializeField, HideInInspector] public int deleteIndex;
                [SerializeField, HideInInspector] public int signalIndex = -1;

                [SerializeField, HideInInspector] public float panX;
                [SerializeField, HideInInspector] public float panY;
                [SerializeField, HideInInspector] public float snapToGrid = 10f;
                [SerializeField, HideInInspector] public float scaling = 1f; // for zooming

                [SerializeField, HideInInspector] public bool add;
                [SerializeField, HideInInspector] public bool active;
                [SerializeField, HideInInspector] public bool foldOut;
                [SerializeField, HideInInspector] public bool foundNodes;
                [SerializeField, HideInInspector] public bool settingsFoldOut;
                [SerializeField, HideInInspector] public bool showNodeMessage;
                [SerializeField, HideInInspector] public bool isSelectingNodes;
                [SerializeField, HideInInspector] public bool blackboardFoldOut;
                [SerializeField, HideInInspector] public bool collisionSettings;
                [SerializeField, HideInInspector] public bool aNodeIsBeingDragged;
                [SerializeField, HideInInspector] public EditorWindow window;
                public static EditorWindow windowStatic;

                [SerializeField, HideInInspector] public Vector2 selectSize;
                [SerializeField, HideInInspector] public Vector2 selectPosition;
                [SerializeField, HideInInspector] public Vector2 ScrollPosition;
                [SerializeField, HideInInspector] public Vector2 startHoldPoint;
                [SerializeField, HideInInspector] public Vector2 stateActionIndex = Vector2.one * -1f;
                [System.NonSerialized] private static List<string> target = new List<string>();

                public static void AIType (Transform transform, SerializedObject parent, ref bool createUnits, bool isFSM = false)
                {
                        FoldOut.Box(1, Tint.BoxTwo, extraHeight: 3);
                        {
                                parent.Field("AI Type ", "type");
                        }

                        if (!FoldOut.FoldOutButton(parent.Get("mainFoldOut")))
                        {
                                return;
                        }

                        CharacterType type = (CharacterType) parent.Get("type").enumValueIndex;

                        if (type == CharacterType.MovingPlatform)
                        {
                                FoldOut.BoxSingle(1, Tint.BoxTwo);
                                {
                                        parent.FieldToggle("Has Gravity", "movingPlatformGravity");
                                }
                                Layout.VerticalSpacing(2);
                        }

                        if (type == CharacterType.Regular || parent.Bool("movingPlatformGravity"))
                        {
                                if (transform.gameObject.GetComponent<BoxCollider2D>() == null)
                                {
                                        transform.gameObject.AddComponent<BoxCollider2D>();
                                }
                                CollisionSettings(parent, parent.Get("world"), parent.Get("gravity"), parent.Get("signals"));
                        }

                        if (FoldOut.Bar(parent, Tint.BoxTwo).Label("Settings", FoldOut.titleColor, false).FoldOut("settingsFoldOut"))
                        {
                                if (isFSM)
                                {
                                        FoldOut.Box(6, FoldOut.boxColor, offsetY: -2);
                                        {
                                                parent.FieldToggle("Turn Off Signals", "turnOffSignals");
                                                parent.FieldToggle("This Is The Player", "thisIsPlayer");
                                                parent.FieldToggle("Activate On Reset", "resetToActive");
                                                parent.FieldToggle("Reset To First State", "resetToFirst");
                                                parent.FieldToggleAndEnable("Use Late Update", "executeInLateUpdate");
                                                createUnits = parent.FieldAndButton("Create Units", "tempUnits", "Add");
                                        }
                                        Layout.VerticalSpacing(3);
                                }
                                else
                                {
                                        FoldOut.Box(3, FoldOut.boxColor, offsetY: -2);
                                        {
                                                parent.Field("Turn Off Signals", "turnOffSignals");
                                                parent.Field("Signals for Player", "thisIsPlayer");
                                                createUnits = parent.FieldAndButton("Create Units", "tempUnits", "Add");
                                        }
                                        Layout.VerticalSpacing(3);
                                }
                        }

                        if (FoldOut.Bar(parent.Get("damage"), Tint.BoxTwo).Label("Damage", FoldOut.titleColor, false).BRE("canDealDamage").FoldOut("damageFoldOut"))
                        {
                                SerializedProperty damage = parent.Get("damage");
                                GUI.enabled = damage.Bool("canDealDamage");

                                FoldOut.Box(4, FoldOut.boxColor, offsetY: -2);
                                {
                                        damage.Field("Layer", "layer");
                                        damage.Field("Direction", "direction");
                                        damage.FieldDouble("Amount", "damage", "force");
                                        Labels.FieldDoubleText("Damage", "Force");
                                        damage.FieldToggleAndEnable("Pause", "pauseDamage");
                                }
                                Layout.VerticalSpacing(3);
                                GUI.enabled = true;
                        }

                }

                public static void CollisionSettings (SerializedObject parent, SerializedProperty world, SerializedProperty gravity, SerializedProperty signals)
                {
                        if (FoldOut.Bar(parent, Tint.BoxTwo).Label("Collision", FoldOut.titleColor, false).FoldOut("collisionSettings"))
                        {
                                int extraHeightS = world.Bool("climbSlopes") ? 3 : 0;
                                FoldOut.Box(6 + extraHeightS, FoldOut.boxColor, offsetY: -2);
                                {
                                        gravity.FieldDouble("Gravity, Jump", "jumpTime", "jumpHeight");
                                        Labels.FieldDoubleText("Time", "Height");
                                        gravity.Field("Gravity Multiplier", "multiplier");
                                        gravity.Field("Terminal Velocity", "terminalVelocity");
                                        signals.Field("Sprite Engine", "spriteEngine");
                                        world.Get("box").Field("Collision Rays", "rays");
                                        world.Get("box").ClampV2Int("rays", min: 2, max: 100);
                                        world.FieldAndEnableHalf("Climb Slopes", "maxSlopeAngle", "climbSlopes");
                                        world.Clamp("maxSlopeAngle", 0, 88f);
                                        Labels.FieldText("Max Slope", rightSpacing: Layout.boolWidth + 4);

                                        if (world.Bool("climbSlopes"))
                                        {
                                                world.FieldAndEnable("Rotate To Slope", "rotateRate", "rotateToSlope");
                                                Labels.FieldText("Rotate Rate", rightSpacing: Layout.boolWidth + 4);
                                                world.Clamp("rotateRate", max: 2f);
                                                GUI.enabled = world.Bool("rotateToSlope");
                                                {
                                                        world.FieldAndEnable("Rectify In Air", "rotateTo", "rectifyInAir");
                                                        world.FieldToggleAndEnable("Rotate To Wall", "rotateToWall");
                                                }
                                                GUI.enabled = true;
                                        }
                                }
                                Layout.VerticalSpacing(3);

                                FoldOut.Box(6, FoldOut.boxColor, extraHeight: 3);
                                {
                                        world.FieldToggle("Auto Jump 2D Edge", "jumpThroughEdge");
                                        world.FieldToggle("Check Corners", "horizontalCorners");
                                        world.FieldToggle("Use Bridges", "useBridges");
                                        world.FieldToggle("Use Moving Platforms", "useMovingPlatform");
                                        world.FieldToggle("Collide With World Only", "collideWorldOnly");
                                        parent.FieldToggleAndEnable("Use Late Update", "executeInLateUpdate");

                                        if (FoldOut.FoldOutButton(world.Get("eventsFoldOut")))
                                        {
                                                Fields.EventFoldOutEffect(world.Get("onCrushed"), world.Get("crushedWE"), world.Get("crushedFoldOut"), "Crushed By Platform");
                                        }
                                }

                        }
                }

                public static void BlackboardDisplay (SerializedObject parent, SerializedProperty bar, AIBase ai, List<string> dataList, int size = 1)
                {
                        if (bar.intValue <= size)
                        {
                                return;
                        }

                        int type = bar.intValue - (size + 1);
                        SerializedProperty dataArray = parent.Get("data");
                        for (int i = 0; i < dataArray.arraySize; i++)
                        {
                                // check if property is null
                                SerializedProperty element = dataArray.Element(i);
                                if (element.objectReferenceValue == null)
                                {
                                        dataArray.DeleteArrayElement(i);
                                        break;
                                }

                                // display blackboard data fields in inspector
                                SerializedObject data = new SerializedObject(element.objectReferenceValue);

                                int index = (int) data.Enum("blackboardType");

                                if (index != type)
                                {
                                        continue;
                                }

                                data.Update();
                                {
                                        var hideFlagsProp = data.FindProperty("m_ObjectHideFlags");
                                        hideFlagsProp.intValue = (int) HideFlags.HideInInspector;
                                        string name = element.objectReferenceValue.GetType().Name;
                                        FoldOut.Bar(data).Label(Util.ToProperCase(name));
                                        FoldOut.bar.BR("delete", "Delete");
                                        bool focus = name == "Territory" ? Bar.ButtonRight("Target", Color.white) : false;

                                        if (FoldOut.bar.FoldOut())
                                        {
                                                if (focus)
                                                {
                                                        Territory territory = element.objectReferenceValue as Territory;
                                                        Debug.Log(element.objectReferenceValue.GetType());
                                                        SceneTools.MoveSceneCamera(territory.bounds.position);
                                                }
                                                int fields = EditorTools.CountObjectFields(data);
                                                if (fields != 0)
                                                        FoldOut.Box(fields, FoldOut.boxColor, offsetY: -2);
                                                EditorTools.IterateObject(data, fields);
                                                if (index == 1)
                                                {
                                                        SerializedProperty create = data.Get("createPaths");
                                                        if (create != null && FoldOut.LargeButton("Create Paths +", Tint.Orange, Tint.White, Icon.Get("BackgroundLight")))
                                                        {
                                                                create.boolValue = true;
                                                        }
                                                }
                                                if (fields == 0)
                                                {
                                                        Layout.VerticalSpacing(3);
                                                }
                                        }
                                }
                                data.ApplyModifiedProperties();

                                if (data.Bool("delete"))
                                {
                                        DestroyImmediate(element.objectReferenceValue);
                                        dataArray.DeleteArrayElement(i);
                                        break;
                                }
                        }

                }

                public static void CreateUnits (AIBase ai)
                {
                        // Keep units individual with box collider, transform, etc., the only thing that should stay the same is the ai code and sprite order in layer
                        List<GameObject> newUnits = new List<GameObject>();
                        for (int i = 0; i < ai.tempUnits; i++)
                        {
                                newUnits.Add(Instantiate(ai.gameObject, ai.transform.position, Quaternion.identity));
                        }
                        for (int i = 0; i < newUnits.Count; i++)
                        {
                                if (i < ai.units.Count && ai.units[i] != null)
                                {
                                        // set transform
                                        Transform oldT = ai.units[i].GetComponent<Transform>();
                                        Transform newT = newUnits[i].GetComponent<Transform>();
                                        if (oldT != null && newT != null)
                                        {
                                                newT.position = oldT.position;
                                                newT.rotation = oldT.rotation;
                                                newT.localScale = oldT.localScale;
                                                newT.parent = oldT.parent;
                                        }
                                        // set color
                                        SpriteRenderer oldS = ai.units[i].GetComponent<SpriteRenderer>();
                                        SpriteRenderer newS = newUnits[i].GetComponent<SpriteRenderer>();
                                        SpriteRenderer templateS = ai.GetComponent<SpriteRenderer>();
                                        if (oldS != null && newS != null && templateS != null)
                                        {
                                                newS.color = oldS.color;
                                                newS.sortingLayerID = templateS.sortingLayerID;
                                                newS.sortingLayerName = templateS.sortingLayerName;
                                                newS.sortingOrder = templateS.sortingOrder;
                                                newS.renderingLayerMask = templateS.renderingLayerMask;
                                                newS.drawMode = templateS.drawMode;
                                        }
                                        //box sizes
                                        BoxCollider2D oldB = ai.units[i].GetComponent<BoxCollider2D>();
                                        BoxCollider2D newB = newUnits[i].GetComponent<BoxCollider2D>();
                                        if (oldB != null && newB != null)
                                        {
                                                newB.size = oldB.size;
                                                newB.offset = oldB.offset;
                                                newB.isTrigger = oldB.isTrigger;
                                        }
                                }

                                newUnits[i].name = ai.name + "_" + (i + 2).ToString();

                                AITree newTree = newUnits[i].GetComponent<AITree>();
                                if (newTree != null)
                                {
                                        newTree.units.Clear(); //
                                        newTree.tempUnits = 0;
                                }

                                Node[] nodes = newUnits[i].GetComponents<Node>();
                                for (int k = 0; k < nodes.Length; k++)
                                {
                                        nodes[k].hideFlags = HideFlags.HideInInspector;
                                }

                                Blackboard[] data = newUnits[i].GetComponents<Blackboard>();
                                for (int k = 0; k < data.Length; k++)
                                {
                                        data[k].hideFlags = HideFlags.HideInInspector;
                                }

                                WorldFloat[] health = newUnits[i].GetComponents<WorldFloat>();
                                for (int k = 0; k < health.Length; k++)
                                {
                                        if (health[k].isHealth)
                                        {
                                                health[k].variableName = health[k].variableName + "_" + i.ToString();
                                        }
                                }
                        }
                        for (int i = 0; i < ai.units.Count; i++)
                        {
                                if (ai.units[i] != null)
                                        DestroyImmediate(ai.units[i]);
                        }
                        ai.units.Clear();
                        for (int i = 0; i < newUnits.Count; i++)
                        {
                                ai.units.Add(newUnits[i]);
                        }
                }

                public static void IterateObject (SerializedObject element, List<Blackboard> data, Node node = null, bool setReference = false)
                {
                        int i = 0;
                        SerializedProperty iterator = element.GetIterator();
                        while (iterator.NextVisible(true) && i++ < 1000) // prevent an infinite loop
                        {
                                string name = iterator.name;
                                if (name == "m_Script")
                                {
                                        continue;
                                }
                                SerializedProperty prop = element.FindProperty(iterator.name);

                                if (prop != null)
                                {
                                        if (node != null && (node is Decorator) && !(node is Inverter))
                                        {
                                                if (name == "canInterrupt" || name == "onInterrupt")
                                                {
                                                        prop.enumValueIndex = 0;
                                                        continue;
                                                }
                                        }

                                        if (setReference && SetRef(data, prop, i))
                                        {
                                                continue;
                                        }

                                        if (prop.type == "UnityEvent")
                                                EditorGUILayout.PropertyField(prop, includeChildren: true);
                                        else
                                                prop.FieldProperty(name);
                                }
                        }
                }

                public static bool SetRef<T> (List<Blackboard> data, SerializedProperty field, int index)
                {
                        string typeName = field.type.Replace("PPtr<$", "").Replace(">", "");
                        string typePath = "TwoBitMachines.FlareEngine.AI.BlackboardData." + typeName;
                        if (typeName != "Pathfinding" && typeName != "PathfindingBasic" && EditorTools.ValidateType(typePath)) // detect if field is blackboard data type
                        {
                                // Get all blackboard data types
                                target.Clear();
                                for (int j = 0; j < data.Count; j++)
                                {
                                        if (data[j] == null || data[j] is TreeVariable || data[j] is AIFSMVariable || !(data[j] is T))
                                        {
                                                continue;
                                        }
                                        target.Add(data[j].dataName);
                                }
                                for (int j = 0; j < data.Count; j++) // set ai tree data after main tree so that indexes don't get mixed up if removed
                                {
                                        if (data[j] != null && (data[j] is TreeVariable || data[j] is AIFSMVariable))
                                        {
                                                AITree sharedTree = data[j].GetTree();
                                                if (sharedTree != null)
                                                {
                                                        for (int k = 0; k < sharedTree.data.Count; k++)
                                                        {
                                                                if (sharedTree.data[k] is T)
                                                                        target.Add(sharedTree.data[k].dataName);
                                                        }
                                                }

                                                AIFSM sharedAIFSM = data[j].GetAIFSM();
                                                if (sharedAIFSM != null)
                                                {
                                                        for (int k = 0; k < sharedAIFSM.data.Count; k++)
                                                        {
                                                                if (sharedAIFSM.data[k] is T)
                                                                        target.Add(sharedAIFSM.data[k].dataName);
                                                        }
                                                }
                                        }
                                }

                                if (target.Count == 0)
                                {
                                        Labels.Label("Blackboard Data Required. Must create it. ");
                                        return false;
                                }

                                // Set blackboard data type
                                SerializedProperty refName = field.serializedObject.Get("refName");
                                field.serializedObject.DropDownList(target.ToArray(), Util.ToProperCase(field.name), GetRefName(refName, index));
                                string chosenTarget = GetRefName(refName, index).stringValue;
                                for (int j = 0; j < data.Count; j++) // set reference
                                {
                                        if (data[j] == null || data[j] is T)
                                                continue;

                                        if (data[j] is TreeVariable)
                                        {
                                                AITree sharedTree = data[j].GetTree();
                                                if (sharedTree == null)
                                                        continue;
                                                for (int k = 0; k < sharedTree.data.Count; k++)
                                                {
                                                        if (sharedTree.data[k].dataName == chosenTarget)
                                                        {
                                                                field.objectReferenceValue = sharedTree.data[k];
                                                                break;
                                                        }
                                                }
                                        }
                                        else if (data[j] is AIFSMVariable)
                                        {
                                                AIFSM sharedAIFSM = data[j].GetAIFSM();
                                                if (sharedAIFSM == null)
                                                        continue;
                                                for (int k = 0; k < sharedAIFSM.data.Count; k++)
                                                {
                                                        if (sharedAIFSM.data[k].dataName == chosenTarget)
                                                        {
                                                                field.objectReferenceValue = sharedAIFSM.data[k];
                                                                break;
                                                        }
                                                }
                                        }
                                        else if (data[j].dataName == chosenTarget)
                                        {
                                                field.objectReferenceValue = data[j];
                                                break;
                                        }
                                }
                                return true;
                        }
                        return false;
                }

                public static bool SetRef (List<Blackboard> data, SerializedProperty field, int index)
                {
                        string typeName = field.type.Replace("PPtr<$", "").Replace(">", "");
                        string typePath = "TwoBitMachines.FlareEngine.AI.BlackboardData." + typeName;
                        if (typeName != "Pathfinding" && typeName != "PathfindingBasic" && EditorTools.ValidateType(typePath)) // detect if field is blackboard data type
                        {
                                // Get all blackboard data types
                                target.Clear();
                                for (int j = 0; j < data.Count; j++)
                                {
                                        if (data[j] == null || data[j] is TreeVariable || data[j] is AIFSMVariable)
                                        {
                                                continue;
                                        }
                                        target.Add(data[j].dataName);
                                }
                                for (int j = 0; j < data.Count; j++) // set ai tree data after main tree so that indexes don't get mixed up if removed
                                {
                                        if (data[j] != null && (data[j] is TreeVariable || data[j] is AIFSMVariable))
                                        {
                                                AITree sharedTree = data[j].GetTree();
                                                if (sharedTree != null)
                                                {
                                                        for (int k = 0; k < sharedTree.data.Count; k++)
                                                        {
                                                                target.Add(sharedTree.data[k].dataName);
                                                        }
                                                }

                                                AIFSM sharedAIFSM = data[j].GetAIFSM();
                                                if (sharedAIFSM != null)
                                                {
                                                        for (int k = 0; k < sharedAIFSM.data.Count; k++)
                                                        {
                                                                target.Add(sharedAIFSM.data[k].dataName);
                                                        }
                                                }
                                        }
                                }

                                if (target.Count == 0)
                                {
                                        Labels.Label("Blackboard Data Required. Must create it. ");
                                        return false;
                                }

                                // Set blackboard data type
                                SerializedProperty refName = field.serializedObject.Get("refName");
                                field.serializedObject.DropDownList(target.ToArray(), Util.ToProperCase(field.name), GetRefName(refName, index));
                                string chosenTarget = GetRefName(refName, index).stringValue;
                                for (int j = 0; j < data.Count; j++) // set reference
                                {
                                        if (data[j] == null)
                                                continue;

                                        if (data[j] is TreeVariable)
                                        {
                                                AITree sharedTree = data[j].GetTree();
                                                if (sharedTree == null)
                                                        continue;
                                                for (int k = 0; k < sharedTree.data.Count; k++)
                                                {
                                                        if (sharedTree.data[k].dataName == chosenTarget)
                                                        {
                                                                field.objectReferenceValue = sharedTree.data[k];
                                                                break;
                                                        }
                                                }
                                        }
                                        else if (data[j] is AIFSMVariable)
                                        {
                                                AIFSM sharedAIFSM = data[j].GetAIFSM();
                                                if (sharedAIFSM == null)
                                                        continue;
                                                for (int k = 0; k < sharedAIFSM.data.Count; k++)
                                                {
                                                        if (sharedAIFSM.data[k].dataName == chosenTarget)
                                                        {
                                                                field.objectReferenceValue = sharedAIFSM.data[k];
                                                                break;
                                                        }
                                                }
                                        }
                                        else if (data[j].dataName == chosenTarget)
                                        {
                                                field.objectReferenceValue = data[j];
                                                break;
                                        }
                                }
                                return true;
                        }
                        return false;
                }

                public void HideInInspectorBlackBoard ()
                {
                        for (int i = 0; i < data.Count; i++)
                        {
                                data[i].hideFlags = HideFlags.HideInInspector;
                        }
                }

                private static SerializedProperty GetRefName (SerializedProperty array, int id)
                {
                        for (int i = 0; i < array.arraySize; i++)
                        {
                                if (i == id)
                                        return array.Element(i);
                        }
                        array.arraySize++;
                        return array.LastElement();
                }

#pragma warning restore 0414
#endif
                #endregion
        }

}
