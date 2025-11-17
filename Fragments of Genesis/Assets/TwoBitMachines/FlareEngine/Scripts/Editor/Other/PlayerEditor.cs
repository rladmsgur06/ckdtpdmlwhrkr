using System;
using System.Collections.Generic;
using TwoBitMachines.Editors;
using TwoBitMachines.FlareEngine.ThePlayer;
using UnityEditor;
using UnityEngine;

//using UnityEngine.UIElements;
//using UnityEditor.UIElements;
//using TwoBitMachines.UIElement.Editor;


namespace TwoBitMachines.FlareEngine.Editors
{
        public delegate void Display (SerializedProperty parent, Player main, int ID);

        [CustomEditor(typeof(Player))]
        public class PlayerEditor : UnityEditor.Editor
        {
                private Player main;
                private SerializedObject so;
                private GameObject mainGameObject;
                private List<string> availableList = new List<string>();
                private Dictionary<string, SerializedObject> foundList = new Dictionary<string, SerializedObject>();
                // public VisualElement root;

                public static int abilitiesListSize = 0;
                public static string[] abilityNamesList;
                public static string[] inputList = new string[] { "None" };

                private void OnEnable ()
                {
                        main = target as ThePlayer.Player;
                        so = serializedObject;
                        mainGameObject = main.gameObject;
                        Layout.Initialize();
                        if (abilityNamesList == null || abilityNamesList.Length == 0 || abilitiesListSize != abilityNamesList.Length)
                        {
                                List<string> paths = new List<string>();
                                Util.GetFileNames("TwoBitMachines", "/FlareEngine/Scripts/Player/Abilities", paths, false);
                                Util.GetFileNames("", UserFolderPaths.Path(UserFolder.Player), paths, false);
                                abilityNamesList = paths.ToArray();
                                abilitiesListSize = abilityNamesList.Length;
                        }
                        if (inputList == null || inputList.Length <= 1 || inputList.Length != (main.inputs.inputSO.Count + 1))
                        {
                                InputList(main);
                        }
                        for (int i = 0; i < main.ability.Count; i++)
                        {
                                if (main.ability[i] != null)
                                        main.ability[i].hideFlags = HideFlags.HideInInspector;
                        }
                        Rigidbody2D rb = main.GetComponent<Rigidbody2D>();
                        if (rb != null)
                                rb.sleepMode = RigidbodySleepMode2D.NeverSleep;
                }

                public void InputList (ThePlayer.Player main)
                {
                        int size = main.inputs.inputSO.Count;
                        inputList = new string[size + 1];
                        inputList[0] = "None";

                        for (int i = 0; i < main.inputs.inputSO.Count; i++)
                        {
                                inputList[i + 1] = main.inputs.inputSO[i].buttonName;
                        }
                }

                public override void OnInspectorGUI ()
                {
                        Layout.Update();
                        if (inputList.Length <= 1 || inputList.Length != (main.inputs.inputSO.Count + 1))
                        {
                                InputList(main);
                        }

                        so.Update();
                        {
                                InputInspectorEditor.Display(so, so.Get("inputs").Get("inputSO"), main, this);
                                SerializedProperty abilities = so.Get("ability");
                                List<SerializedObject> serialObjList = GetAbilities(abilities);
                                GetActiveAbilitiesList(serialObjList, availableList, foundList);
                                PriorityInspectorEditor.Display(so, abilities, abilityNamesList);
                                DisplayAbilities(abilities, foundList, Tint.SoftDark, Tint.WarmWhite);

                                CreateAbility(availableList);
                                DeleteAbility(abilities, serialObjList);
                        }
                        so.ApplyModifiedProperties();

                }

                private void GetActiveAbilitiesList (List<SerializedObject> array, List<string> availableList, Dictionary<string, SerializedObject> foundList)
                {
                        availableList.Clear();
                        for (int i = 0; i < abilityNamesList.Length; i++)
                                if (abilityNamesList[i] != "None" && abilityNamesList[i] != "All") //                set available list
                                        availableList.Add(abilityNamesList[i]);

                        foundList.Clear();
                        for (int i = 0; i < array.Count; i++)
                        {
                                SerializedObject element = array[i]; //.Element (i);
                                availableList.Remove(element.String("abilityName"));
                                foundList.Add(element.String("abilityName"), element);
                        }
                }

                private void DisplayAbilities (SerializedProperty abilities, Dictionary<string, SerializedObject> foundList, Color barColor, Color labelColor)
                {
                        bool open = so.Bool("abilityFoldOut");
                        if (!FoldOut.Bar(so, Tint.SoftDark * Tint.LightGrey)
                                .Label("Abilities", labelColor)
                                .RightButton("settings", "Gear", "Settings", execute: open)
                                .RightButton("view", "EyeOpen", "View Mode", execute: open)
                                .FoldOut("abilityFoldOut")
                        )
                                return;

                        if (so.ReadBool("view"))
                        {
                                AbilityView();
                        }

                        Settings(so.Get("world"), so.Get("abilities").Get("gravity"), so.Get("signals"), barColor, labelColor);

                        int viewIndex = so.Int("viewIndex");

                        if (viewIndex <= 1)
                        {
                                Walk.OnInspector(so.Get("abilities").Get("walk"), inputList, barColor, labelColor); //  walk ability always displayed
                        }

                        for (int i = abilities.arraySize - 1; i >= 0; i--)
                        {
                                bool canView = (viewIndex == 0 || viewIndex == (i + 2));
                                if (!canView)
                                        continue;

                                UnityEngine.Object obj = abilities.Element(i).objectReferenceValue;
                                if (obj == null)
                                        continue;
                                Ability ability = (Ability) obj;
                                SerializedObject abilityObj = new SerializedObject(abilities.Element(i).objectReferenceValue);

                                if (ability != null)
                                        ability.hideFlags = HideFlags.HideInInspector;
                                abilityObj.Update();
                                if (!ability.OnInspector(so, abilityObj, inputList, barColor, labelColor))
                                {
                                        string name = abilityObj.String("abilityName");
                                        if (Ability.Open(abilityObj, Util.ToProperCase(name), barColor, labelColor))
                                        {
                                                int fields = EditorTools.CountObjectFields(abilityObj);
                                                if (fields != 0)
                                                        FoldOut.Box(fields, FoldOut.boxColorLight, offsetY: -2);
                                                EditorTools.IterateObject(abilityObj, fields);
                                                if (fields == 0)
                                                        Layout.VerticalSpacing(3);
                                        }
                                }
                                abilityObj.ApplyModifiedProperties();
                        }
                }

                private void Settings (SerializedProperty world, SerializedProperty gravity, SerializedProperty signals, Color barColor, Color labelColor)
                {
                        if (so.Bool("settings"))
                        {
                                FoldOut.Box(4, FoldOut.boxColor, offsetY: -2);
                                {
                                        gravity.FieldDouble("Gravity, Jump", "jumpTime", "jumpHeight");
                                        Labels.FieldDoubleText("Time", "Height");
                                        gravity.Field("Gravity Multiplier", "multiplier");
                                        gravity.Field("Terminal Velocity", "terminalVelocity");
                                        signals.Field("Sprite Engine", "spriteEngine");
                                }
                                Layout.VerticalSpacing(3);

                                FoldOut.Box(1, FoldOut.boxColor);
                                {
                                        world.Get("box").Field("Collision Rays", "rays");
                                        world.Get("box").ClampV2Int("rays", min: 2, max: 100);
                                }
                                Layout.VerticalSpacing(5);

                                int extraHeight = world.Bool("climbSlopes") ? 3 : 0;
                                FoldOut.Box(1 + extraHeight, FoldOut.boxColor);
                                {
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
                                Layout.VerticalSpacing(5);

                                FoldOut.Box(7, FoldOut.boxColor, extraHeight: 3);
                                {
                                        world.Field("Skip Edge 2D", "skipDownEdge");
                                        world.FieldToggleAndEnable("Auto Jump Edge 2D", "jumpThroughEdge");
                                        world.FieldToggleAndEnable("Check Corners", "horizontalCorners");
                                        world.FieldToggleAndEnable("Use Bridges", "useBridges");
                                        world.FieldToggleAndEnable("Use Moving Platforms", "useMovingPlatform");
                                        world.FieldToggleAndEnable("Use Moving Platform Momentum", "useMovingPlatformMomentum");
                                        world.FieldToggleAndEnable("Collide With World Only", "collideWorldOnly");
                                        bool eventOpen = FoldOut.FoldOutButton(world.Get("eventsFoldOut"));
                                        Fields.EventFoldOutEffect(world.Get("onCrushed"), world.Get("crushedWE"), world.Get("crushedFoldOut"), "Crushed By Platform", execute: eventOpen);
                                }
                        }
                }

                private void AbilityView ()
                {
                        GenericMenu menu = new GenericMenu();
                        int viewIndex = so.Int("viewIndex");
                        menu.AddItem(new GUIContent("View All"), viewIndex == 0, AbilityView, 0);
                        menu.AddItem(new GUIContent("Walk"), viewIndex == 1, AbilityView, 1);
                        for (int i = 0; i < main.ability.Count; i++)
                        {
                                if (main.ability[i].pause)
                                {
                                        menu.AddItem(new GUIContent(main.ability[i].abilityName + "  -- Paused"), viewIndex == (i + 2), AbilityView, i + 2);
                                }
                                else
                                {
                                        menu.AddItem(new GUIContent(main.ability[i].abilityName), viewIndex == (i + 2), AbilityView, i + 2);
                                }
                        }
                        menu.ShowAsContext();
                }

                public void AbilityView (object obj)
                {
                        so.Update();
                        so.Get("viewIndex").intValue = (int) obj;
                        so.ApplyModifiedProperties();
                }



                private void CreateAbility (List<string> availableList)
                {
                        if (FoldOut.CornerButton(Tint.Delete))
                        {
                                GenericMenu menu = new GenericMenu();
                                for (int i = 0; i < availableList.Count; i++)
                                {
                                        menu.AddItem(new GUIContent(availableList[i]), false, CreateAbilityInstance, availableList[i]);
                                }
                                menu.ShowAsContext();
                        }
                }

                private void CreateAbilityInstance (object obj)
                {
                        string typeName = (string) obj;
                        string fullTypeName = "TwoBitMachines.FlareEngine.ThePlayer." + typeName;
                        Type type = EditorTools.RetrieveType(fullTypeName);
                        if (type == null)
                        {
                                Debug.LogWarning("FlareEngine: Ability type '" + typeName + "' not found.");
                                return;
                        }
                        Component comp = main.transform.gameObject.AddComponent(type);
                        comp.hideFlags = HideFlags.HideInInspector;
                        Ability newAbility = (Ability) comp;
                        newAbility.abilityName = typeName;

                        if (typeName == "Firearms")
                                newAbility.exception.Add("Jump");
                        if (typeName == "PushBack")
                                newAbility.exception.Add("Crouch");

                        so.Update();
                        SerializedProperty abilities = so.Get("ability");
                        abilities.arraySize++;
                        abilities.LastElement().objectReferenceValue = newAbility;
                        so.ApplyModifiedProperties();
                }

                private void DeleteAbility (SerializedProperty abilities, List<SerializedObject> array)
                {
                        for (int i = 0; i < array.Count; i++)
                                if (array[i].ReadBool("delete"))
                                {
                                        for (int j = abilities.arraySize - 1; j >= 0; j--)
                                        {
                                                if (abilities.Element(j).objectReferenceValue == array[i].targetObject)
                                                {
                                                        DestroyImmediate(array[i].targetObject);
                                                        abilities.DeleteArrayElement(j);
                                                        return;
                                                }
                                        }
                                }
                }

                private List<SerializedObject> GetAbilities (SerializedProperty abilities)
                {
                        List<SerializedObject> serialObjList = new List<SerializedObject>();
                        for (int i = abilities.arraySize - 1; i >= 0; i--)
                        {
                                if (abilities.Element(i).objectReferenceValue == null)
                                {
                                        abilities.DeleteArrayElement(i);
                                        continue;
                                }
                                serialObjList.Add(new SerializedObject(abilities.Element(i).objectReferenceValue));

                        }
                        return serialObjList;
                }

                private void OnDisable ()
                {
                        if (main == null && mainGameObject != null && !EditorApplication.isPlayingOrWillChangePlaymode)
                        {
                                mainGameObject.AddComponent<AbilityClean>();
                        }
                }

        }
}

// public override VisualElement CreateInspectorGUI ()
// {
//         if (root != null && root.parent != null) // dont recreate tree accidentally, locking inspector and dragging sprites will cause this
//                 return root;
//         root = ElementTools.InitializeRoot();


//         InputsSection(so, root);
//         return root;
// }

// // NEW CODE

// public void InputsSection (SerializedObject so, VisualElement container)
// {
//         BarPlus unit = new BarPlus(container, so, so, Tint.Blue);
//         unit.FoldOut("Inputs", Color.white, 1, 1, "inputFoldOut", bold: true);
//         unit.BottomSpace();

//         unit.LazyContentLoad(() =>
//         {
//                 // SerializedProperty inputs = so.Get("inputs").Get("inputSO");

//                 // for (int i = 0; i < inputs.arraySize; i++)
//                 // {
//                 //         if (inputs.Element(i) == null || inputs.Element(i).objectReferenceValue == null)
//                 //         {
//                 //                 continue;
//                 //         }

//                 UIBlockList block = new UIBlockList(unit.content, (block) =>
//                 {
//                         block.RebindList(so, so.Get("inputs").Get("inputSO"), true, true, (index) =>
//                         {
//                                 SerializedProperty element = so.Get("inputs").Get("inputSO").Element(index);
//                                 if (element == null || element.objectReferenceValue == null)
//                                 {
//                                         return;
//                                 }
//                                 SerializedObject input = new SerializedObject(element.objectReferenceValue);
//                                 //input.Update();
//                                 {

//                                         BarPlus unit = new BarPlus(container, input, input, Tint.Blue, so.Get("inputs").Get("inputSO"), bind: false);
//                                         unit.Grip();
//                                         unit.FoldOut(input.String("buttonName"), Color.white, 1, 1, "foldOut", bold: true);
//                                         unit.Field("type");
//                                         unit.Field("mouse");
//                                         unit.BottomSpace();

//                                         unit.Bind(input);
//                                         // extra step
//                                         //     PropertyField field = new PropertyField(input);



//                                         // HardBindPlus b = new HardBindPlus(block, input, Tint.BoxGrey);
//                                         // Template.Label(b.header, input.String("buttonName"));
//                                         // Template.Field(b.header, "type");
//                                         // Template.Field(b.header, "mouse");
//                                         //b.Bind(input);

//                                         // VisualElement field = block.FieldDouble(so.Get("inputs").Get("inputSO").Element(index), input.String("buttonName"), "type", "mouse");
//                                         // b.header.Button("xsMinus", () =>
//                                         //  {
//                                         //          so.Get("inputs").Get("inputSO").Delete(index, true);
//                                         //          so.ApplyProperties();
//                                         //          block.CallBack();
//                                         //  });
//                                 }
//                                 //input.ApplyModifiedProperties();
//                         });
//                 });

//         });
// }
