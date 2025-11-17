using System;
using System.Collections.Generic;
using TwoBitMachines.Editors;
using TwoBitMachines.FlareEngine.AI;
using UnityEditor;
using UnityEngine;
using System.Linq;

namespace TwoBitMachines.FlareEngine.Editors
{
        [CustomEditor(typeof(AIFSM))]
        public class AIEditor : UnityEditor.Editor
        {
                public AIFSM fsm;
                public GameObject objReference;
                public SerializedObject parent;
                public string previous;
                public bool onEnable;

                public bool changeSate;
                public string changeStateName;
                public string[] stateNames = new string[0];
                public List<string> stateNameTemp = new List<string>();
                public string[] barNames = new string[] { "States", "Reset", "Target", "Territory", "Variable" };

                public static List<string> dataList = new List<string>();
                public static List<string> actionList = new List<string>();

                private void OnEnable ()
                {
                        onEnable = true;
                        fsm = target as AIFSM;
                        parent = serializedObject;
                        objReference = fsm.gameObject;
                        if (dataList.Count == 0)
                        {
                                Util.GetFolderStructure("TwoBitMachines", "/FlareEngine/Scripts/AI/Behavior/Blackboard", "Blackboard", dataList);
                                Util.GetFolderStructure("", UserFolderPaths.Path(UserFolder.Blackboard), UserFolderPaths.FolderName(UserFolder.Blackboard), dataList, false, true);
                        }
                        if (actionList.Count == 0)
                        {
                                Util.GetFolderStructure("TwoBitMachines", "/FlareEngine/Scripts/AI/Behavior/Nodes/Action", "Action", actionList);
                                Util.GetFolderStructure("", UserFolderPaths.Path(UserFolder.AINode), UserFolderPaths.FolderName(UserFolder.AINode), actionList, false, true);
                        }
                        for (int i = 0; i < fsm.state.Count; i++)
                        {
                                fsm.state[i].HideInInspector();
                        }
                        for (int i = 0; i < fsm.alwaysState.Count; i++)
                        {
                                fsm.alwaysState[i].HideInInspector();
                        }
                        fsm.resetState.HideInInspector();
                        fsm.HideInInspectorBlackBoard();
                        SafetyCheckFailed();
                        Layout.Initialize();
                        changeSate = false;
                }

                public override void OnInspectorGUI ()
                {
                        Layout.VerticalSpacing(10);
                        Layout.Update();
                        bool createUnits = false;

                        parent.Update();
                        {
                                if (SafetyCheckFailed())
                                {
                                        return;
                                }

                                AIBase.AIType(fsm.transform, parent, ref createUnits, true);
                                SerializedProperty bar = parent.Get("barIndex");
                                FoldOut.TabBarString(Icon.Get("BackgroundLight"), Tint.Box, Tint.BoxTwo, barNames, bar, LabelType.White);
                                AIBase.BlackboardDisplay(parent, bar, fsm, dataList);

                                if (bar.intValue > 1 && FoldOut.CornerButton(Tint.BoxTwo))
                                {
                                        BlackboardMenu.Open(fsm, dataList, bar.intValue);
                                }
                                if (bar.intValue == 0)
                                {
                                        if (Application.isPlaying && fsm.currentState != null)
                                        {
                                                if (previous != fsm.root.fsmChild)
                                                {
                                                        Repaint();
                                                }
                                                previous = fsm.root.fsmChild;
                                                FoldOut.Box(2, Tint.Blue * Tint.WarmGreyB);
                                                {
                                                        Labels.Label("Current State:   " + fsm.currentState.stateName, Tint.WarmWhite, bold: true);
                                                        Labels.Label("Current Child:   " + fsm.root.fsmChild, Tint.WarmWhite, bold: true);
                                                }
                                                Layout.VerticalSpacing(5);
                                        }

                                        ListReorder.canExchangeItemsFromOtherLists = true;
                                        SerializedProperty alwaysState = parent.Get("alwaysState");
                                        GetStateNames();
                                        ShowStates(alwaysState, Tint.AlwaysState, true);
                                        DeleteState(alwaysState);

                                        SerializedProperty states = parent.Get("state");
                                        GetStateNames();
                                        ShowStates(states, Tint.Blue);
                                        DeleteState(states);
                                        ListReorder.canExchangeItemsFromOtherLists = false;

                                        if (FoldOut.CornerButton(Tint.Blue)) // add state
                                        {
                                                states.arraySize++;
                                                states.LastElement().Get("menu").boolValue = true;
                                                states.LastElement().Get("action").arraySize = 0;
                                                states.LastElement().Get("type").enumValueIndex = 0;
                                                states.LastElement().Get("stateName").stringValue = "New State";
                                                states.LastElement().Get("defaultSignal").stringValue = "Animation Signal";
                                        }
                                        if (FoldOut.CornerButton(Tint.AlwaysState, 22, 22)) // add always state
                                        {
                                                alwaysState.arraySize++;
                                                alwaysState.LastElement().Get("enabled").boolValue = true;
                                                alwaysState.LastElement().Get("action").arraySize = 0;
                                                alwaysState.LastElement().Get("stateName").stringValue = "New Always State";
                                        }
                                }
                                if (bar.intValue == 1)
                                {
                                        ResetState(parent.Get("resetState"), Tint.Delete);
                                }

                        }
                        parent.ApplyModifiedProperties();
                        Layout.VerticalSpacing(10);

                        if (createUnits)
                        {
                                AIBase.CreateUnits(fsm);
                        }
                        onEnable = false;

                        if (changeSate)
                        {
                                changeSate = false;
                                fsm.ChangeState(changeStateName);
                        }

                        ExchangeListItems();
                }

                public bool SafetyCheckFailed ()
                {
                        if (fsm != null && fsm.root == null)
                        {
                                Root tempRoot = fsm.GetComponent<Root>();
                                if (tempRoot == null)
                                {
                                        fsm.root = fsm.gameObject.AddComponent<Root>();
                                        fsm.root.canHaveChildren = true;
                                        fsm.root.nodeType = NodeType.Composite;
                                        fsm.root.hideFlags = HideFlags.HideInInspector;
                                }
                                else
                                {
                                        fsm.root = tempRoot;
                                }
                        }
                        if (fsm.root != null)
                        {
                                fsm.root.hideFlags = HideFlags.HideInInspector;
                        }
                        return fsm.root == null;
                }

                private void GetStateNames ()
                {
                        stateNameTemp.Clear();
                        for (int x = 0; x < fsm.state.Count; x++)
                        {
                                stateNameTemp.Add(fsm.state[x].stateName);
                        }
                        stateNames = stateNameTemp.ToArray();
                }

                private string StateType (int index)
                {
                        return index == 0 ? "[P]" : index == 1 ? "[S]" : "[SS]";
                }

                public void ShowStates (SerializedProperty states, Color color, bool isAlways = false)
                {
                        bool isPlaying = Application.isPlaying;
                        for (int i = 0; i < states.arraySize; i++)
                        {

                                ListReorder.listIndexRef = i;
                                SerializedProperty state = states.Element(i);

                                FoldOut.Bar(state, color, space: 25)
                                        .LabelAndEdit("stateName", "editName")
                                        .RightButton(toolTip: "Add Node")
                                        .RightButton("deleteAsk", "Delete", toolTip: "Delete State")
                                        .RightButton("delete", "Close", execute: state.Bool("deleteAsk"))
                                        .RightButton("signal", "Signal", toolTip: "Settings", execute: !isAlways && state.Bool("foldOut"))
                                        .RightButton("changeState", "Lightning", toolTip: "Change To this State", execute: isPlaying)
                                        .RF("type", 16, -4, 3)
                                        .BlockRight(20, color, 21)
                                        .LabelRight(StateType(state.Enum("type")), offsetX: 22);
                                Rect rectStart = Bar.barStart;
                                rectStart.x = Bar.startXOrigin;
                                rectStart.y += 2;

                                if (ListReorder.Grip(parent, states, Bar.barStart.CenterRectHeight(), i, Tint.White))
                                {
                                        return;
                                }

                                bool activeState = false;
                                if (!isAlways && isPlaying && fsm.root.stateName == state.String("stateName"))
                                {
                                        activeState = true;
                                        Rect rect = new Rect(Bar.barStart) { x = Bar.startXOrigin - 8f, y = Bar.barStart.y - 2, width = 3f, height = Bar.barStart.height + 5f };
                                        rect.DrawRect(Tint.Green);
                                }

                                if (Bar.FoldOpen(state.Get("foldOut")))
                                {
                                        if (!isAlways && state.Bool("signal"))
                                        {
                                                FoldOut.BoxSingle(2, Tint.Orange * Tint.WarmWhiteB);
                                                {
                                                        state.FieldAndEnable("Default Signal", "defaultSignal", "useSignal", execute: !isAlways);
                                                        // Labels.Label("Default Signal", Layout.GetLastRect(100, 18, 14, 2), Tint.White);
                                                        state.FieldToggleAndEnable("Cant Interrupt", "cantInterrupt");
                                                }
                                                Layout.VerticalSpacing(2);
                                        }
                                        DisplayNodes(state, state.Get("action"), Tint.Orange, activeState, !isAlways);
                                }
                                if (state.ReadBool("add"))
                                {
                                        NodeContextMenu(new Vector2(isAlways ? 1 : 0, i), rectStart);
                                }
                                if (isPlaying && state.ReadBool("changeState"))
                                {
                                        changeSate = true;
                                        changeStateName = state.String("stateName");
                                }
                        }
                }

                public void ResetState (SerializedProperty state, Color color)
                {
                        bool open = FoldOut.Bar(state, color)
                                .Label("Reset State")
                                .RightButton()
                                .RF("type", 16, -4, 3)
                                .BlockRight(20, color, 21)
                                .LabelRight(StateType(state.Enum("type")), offsetX: 22)
                                .FoldOut();

                        Rect rectStart = Bar.barStart;
                        rectStart.x = Bar.startXOrigin;
                        rectStart.y += 2;

                        if (open)
                        {
                                DisplayNodes(state, state.Get("action"), Tint.Orange, false, false);
                        }
                        if (state.ReadBool("add"))
                        {
                                NodeContextMenu(new Vector2(2, 0), rectStart);
                        }
                }

                private void DisplayNodes (SerializedProperty state, SerializedProperty nodes, Color color, bool isActive, bool showNextState = true)
                {
                        if (nodes.arraySize == 0)
                        {
                                Layout.VerticalSpacing(5);
                        }


                        for (int i = 0; i < nodes.arraySize; i++)
                        {
                                SerializedProperty node = nodes.Element(i).Get("node");
                                if (node.objectReferenceValue == null)
                                {
                                        nodes.DeleteArrayElement(i);
                                        return;
                                }
                                Color childColor = color;
                                if (isActive && i == fsm.root.fsmIndex)
                                {
                                        childColor = Tint.Green;
                                }
                                SerializedObject element = new SerializedObject(node.objectReferenceValue);
                                element.Update();
                                {
                                        var hideFlagsProp = element.FindProperty("m_ObjectHideFlags");
                                        hideFlagsProp.intValue = (int) HideFlags.HideInInspector;

                                        string name = node.objectReferenceValue.GetType().ToString().Split('.')[3];
                                        Node inspectNode = node.objectReferenceValue as Node;
                                        SerializedProperty nodeElement = nodes.Element(i);
                                        bool isOpen = element.Bool("foldOut");

                                        FoldOut.Bar(element, childColor * Tint.WarmWhiteB, 25)
                                                .Label(Util.ToProperCase(name))
                                                .RightButton("deleteAsk", "Delete")
                                                .RightButton("delete", "Close", execute: element.Bool("deleteAsk"));

                                        if (inspectNode != null && inspectNode.HasNextState())
                                        {
                                                GUI.enabled = isOpen;
                                                bool nextSateSet = nodeElement.Bool("goToSuccess") || nodeElement.Bool("goToFailure");
                                                Color nextColor = nextSateSet ? Tint.Blue : Tint.WarmWhite;
                                                if (nextSateSet || isOpen)
                                                {
                                                        FoldOut.bar.RightButton("openNext", "RightArrow", "Next State", nextColor, nextColor);
                                                }
                                                GUI.enabled = true;
                                        }

                                        FoldOut.bar.BR("showInfo", "Info", execute: isOpen);

                                        if (ListReorder.Grip(state, nodes, Bar.barStart.CenterRectHeight(), i, Tint.White))
                                        {
                                                return; // when reorder list changes, it should exit out
                                        }
                                        if (Bar.FoldOpen(element.Get("foldOut")))
                                        {
                                                if (inspectNode == null || !inspectNode.OnInspector(fsm, element, childColor, onEnable)) // if not custom inspector, display it raw
                                                {
                                                        AIBase.IterateObject(element, fsm.data, setReference: true);
                                                }
                                                if (showNextState && (inspectNode == null || inspectNode.HasNextState()) && element.Bool("openNext"))
                                                {
                                                        FoldOut.Box(2, childColor, offsetY: -2);
                                                        {
                                                                nodeElement.DropDownListAndEnable(stateNames, "On Success", "onSuccess", "goToSuccess");
                                                                nodeElement.DropDownListAndEnable(stateNames, "On Failure", "onFailure", "goToFailure");
                                                        }
                                                        Layout.VerticalSpacing(3);
                                                }
                                        }
                                        if (element.Bool("delete"))
                                        {
                                                DestroyImmediate(node.objectReferenceValue);
                                                nodes.DeleteArrayElement(i);
                                                return;
                                        }
                                }
                                element.ApplyModifiedProperties();
                        }
                }

                private void NodeContextMenu (Vector2 stateIndex, Rect rect)
                {
                        if (Event.current.shift)
                        {
                                parent.Get("stateActionIndex").vector2Value = stateIndex;
                                parent.ApplyModifiedProperties();
                                PopupWindow.Show(rect, new SearchableContextMenu(actionList, selected =>
                                {
                                        SearchForState(selected);
                                }));
                                return;
                        }

                        GenericMenu menu = new GenericMenu();
                        parent.Get("stateActionIndex").vector2Value = stateIndex;
                        for (int i = 0; i < actionList.Count; i++)
                        {
                                menu.AddItem(new GUIContent(actionList[i]), false, SearchForState, actionList[i]);
                        }
                        menu.ShowAsContext();
                }

                private void SearchForState (object obj)
                {
                        string path = (string) obj;
                        string[] splitPath = path.Split('/');
                        parent.Update();
                        {
                                Vector2 key = parent.Get("stateActionIndex").vector2Value;
                                if (key.x == 2)
                                {
                                        CreateNode(parent.Get("resetState"), splitPath[splitPath.Length - 1]);
                                }
                                else
                                {
                                        SerializedProperty states = key.x == 1 ? parent.Get("alwaysState") : parent.Get("state");
                                        for (int i = 0; i < states.arraySize; i++)
                                                if (key.y == i)
                                                {
                                                        CreateNode(states.Element(i), splitPath[splitPath.Length - 1]);
                                                        break;
                                                }
                                }
                        }
                        parent.ApplyModifiedProperties();
                }

                private void CreateNode (SerializedProperty state, string nameType)
                {
                        parent.Get("stateActionIndex").vector2Value = Vector2.one * 1f;
                        string actionType = "TwoBitMachines.FlareEngine.AI." + nameType;
                        if (EditorTools.RetrieveType(actionType, out Type type))
                        {
                                Node node = fsm.gameObject.gameObject.AddComponent(type) as Node;
                                node.nameType = nameType;
                                node.nodeType = node is Conditional ? NodeType.Conditional : NodeType.Action;
                                node.hideFlags = HideFlags.HideInInspector;
                                SerializedProperty children = state.Get("action");
                                state.SetTrue("foldOut");
                                children.arraySize++;
                                children.LastElement().Get("node").objectReferenceValue = node;
                                children.LastElement().Get("goToSuccess").boolValue = false;
                                children.LastElement().Get("goToFailure").boolValue = false;
                        }
                }

                private void DeleteState (SerializedProperty array)
                {
                        for (int i = 0; i < array.arraySize; i++)
                        {
                                SerializedProperty state = array.Element(i);
                                if (state.Bool("delete"))
                                {
                                        SerializedProperty actions = state.Get("action");
                                        for (int j = 0; j < actions.arraySize; j++)
                                        {
                                                SerializedProperty node = actions.Element(j).Get("node");
                                                if (node.objectReferenceValue != null)
                                                {
                                                        DestroyImmediate(node.objectReferenceValue);
                                                }
                                        }
                                        array.DeleteArrayElement(i);
                                        return;
                                }
                        }
                }

                private void OnSceneGUI ()
                {
                        if (fsm == null)
                        {
                                return;
                        }
                        Mouse.Update();
                        for (int i = 0; i < fsm.data.Count; i++)
                        {
                                fsm.data[i].OnSceneGUI(this);
                                PrefabUtility.RecordPrefabInstancePropertyModifications(fsm.data[i]);
                        }
                        for (int i = 0; i < fsm.state.Count; i++)
                        {
                                for (int j = 0; j < fsm.state[i].action.Count; j++)
                                {
                                        fsm.state[i].action[j].node.OnSceneGUI(this);
                                        PrefabUtility.RecordPrefabInstancePropertyModifications(fsm.state[i].action[j].node);
                                }
                        }
                        for (int i = 0; i < fsm.alwaysState.Count; i++)
                        {
                                for (int j = 0; j < fsm.alwaysState[i].action.Count; j++)
                                {
                                        fsm.alwaysState[i].action[j].node.OnSceneGUI(this);
                                        PrefabUtility.RecordPrefabInstancePropertyModifications(fsm.alwaysState[i].action[j].node);
                                }
                        }
                        for (int i = 0; i < fsm.resetState.action.Count; i++)
                        {
                                fsm.resetState.action[i].node.OnSceneGUI(this);
                                PrefabUtility.RecordPrefabInstancePropertyModifications(fsm.resetState.action[i].node);
                        }
                        // if (Event.current.type == EventType.Layout) UnityEditor.HandleUtility.AddDefaultControl (GUIUtility.GetControlID (FocusType.Passive));
                }

                private void ExchangeListItems ()
                {
                        if (ListReorder.canSwap && ListReorder.listSwapObj == fsm) // this could affect other aIFSM
                        {
                                Undo.RecordObject(fsm, "Swap List Items");
                                ListReorder.canListSwap = false;
                                ListReorder.canListSwapDifferentList = false;

                                if (ListReorder.srcListIndex >= 0 && ListReorder.dstListIndex >= 0 && ListReorder.srcListIndex < fsm.state.Count && ListReorder.dstListIndex < fsm.state.Count)
                                {
                                        AIState stateSource = fsm.state[ListReorder.srcListIndex];
                                        AIState stateDestination = fsm.state[ListReorder.dstListIndex];

                                        if (stateSource == stateDestination)
                                        {
                                                ListReorder.isActive = true;
                                        }
                                        else if (ListReorder.srcItemIndex >= 0 && ListReorder.srcItemIndex < stateSource.action.Count)
                                        {
                                                stateSource.SetActiveFalse();
                                                stateDestination.action.Add(stateSource.action[ListReorder.srcItemIndex]);
                                                stateSource.action.Remove(stateSource.action[ListReorder.srcItemIndex]);
                                        }
                                }
                        }
                }

                private void OnDisable ()
                {
                        if (fsm == null && objReference != null && !EditorApplication.isPlayingOrWillChangePlaymode)
                        {
                                objReference.AddComponent<AIClean>();
                        }
                }

                [DrawGizmo(GizmoType.NotInSelectionHierarchy)]
                public static void DrawWhenObjectIsNotSelected (AIFSM fsm, GizmoType gizmoType)
                {
                        for (int i = 0; i < fsm.data.Count; i++)
                        {
                                if (fsm.data[i] != null)
                                {
                                        fsm.data[i].DrawWhenNotSelected();
                                }
                        }
                }
        }


        public class SearchableContextMenu : PopupWindowContent
        {
                private List<string> Items;
                private Action<string> OnSelect;
                private string Search = "";
                private Vector2 Scroll;

                public SearchableContextMenu (List<string> items, Action<string> onSelect)
                {
                        Items = items;
                        OnSelect = onSelect;
                }

                public override Vector2 GetWindowSize ()
                {
                        return new Vector2(255, 180);
                }

                public override void OnGUI (Rect rect)
                {
                        Search = EditorGUILayout.TextField("Search", Search);

                        IEnumerable<string> visibleItems = Items;

                        if (!string.IsNullOrWhiteSpace(Search))
                        {
                                visibleItems = Items.Where(i => i.IndexOf(Search, StringComparison.OrdinalIgnoreCase) >= 0);
                        }

                        Scroll = EditorGUILayout.BeginScrollView(Scroll);
                        foreach (var item in visibleItems)
                        {
                                string displayName = item.Contains("/") ? item.Split('/').Last() : item;

                                if (GUILayout.Button(displayName))
                                {
                                        OnSelect?.Invoke(item);
                                        editorWindow.Close();
                                }
                        }
                        EditorGUILayout.EndScrollView();

                        // Search = EditorGUILayout.TextField("Search", Search);

                        // if (!string.IsNullOrWhiteSpace(Search))
                        // {
                        //         Scroll = EditorGUILayout.BeginScrollView(Scroll);
                        //         foreach (var item in Items.Where(i => i.IndexOf(Search, StringComparison.OrdinalIgnoreCase) >= 0))
                        //         {
                        //                 string displayName = item.Contains("/") ? item.Split('/').Last() : item;

                        //                 if (GUILayout.Button(displayName))
                        //                 {
                        //                         OnSelect?.Invoke(item);
                        //                         editorWindow.Close();
                        //                 }
                        //         }
                        //         EditorGUILayout.EndScrollView();
                        // }
                }
        }
}
