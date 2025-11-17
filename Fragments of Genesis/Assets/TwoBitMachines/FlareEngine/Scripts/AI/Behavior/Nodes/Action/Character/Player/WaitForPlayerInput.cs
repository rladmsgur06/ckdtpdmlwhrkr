#region 
#if UNITY_EDITOR
using TwoBitMachines.Editors;
using UnityEditor;
#endif
#endregion
using TwoBitMachines.FlareEngine.AI.BlackboardData;
using UnityEngine;
using UnityEngine.Events;

namespace TwoBitMachines.FlareEngine.AI
{
        [AddComponentMenu("")]
        public class WaitForPlayerInput : Action
        {
                [SerializeField] public Blackboard target;
                [SerializeField] public Blackboard territory;
                [SerializeField] public InputButtonSO inputButtonSO;
                [SerializeField] public bool returnSuccess;

                [SerializeField] public UnityEvent onButtonPressed;
                [SerializeField] public UnityEvent onPlayerEnter;
                [SerializeField] public UnityEvent onPlayerExit;

                [System.NonSerialized] private WaitForState state;

                public enum WaitForState
                {
                        Wait,
                        WaitForButton,
                        Open,
                        LetPlayerLeave
                }

                public override NodeState RunNodeLogic (Root root)
                {
                        if (target == null || territory == null || inputButtonSO == null)
                        {
                                return NodeState.Failure;
                        }
                        if (nodeSetup == NodeSetup.NeedToInitialize)
                        {
                                state = WaitForState.Wait;
                        }

                        switch (state)
                        {
                                case WaitForState.Wait:
                                        if (territory.Contains(target.GetTarget()) && !target.hasNoTargets)
                                        {
                                                onPlayerEnter.Invoke();
                                                state = WaitForState.WaitForButton;
                                        }
                                        break;
                                case WaitForState.WaitForButton:
                                        if (!territory.Contains(target.GetTarget()) && !target.hasNoTargets)
                                        {
                                                onPlayerExit.Invoke();
                                                state = WaitForState.Wait;
                                                break;
                                        }
                                        if (inputButtonSO.Pressed())
                                        {
                                                onButtonPressed.Invoke();
                                                state = WaitForState.LetPlayerLeave;
                                                if (returnSuccess)
                                                        return NodeState.Success;
                                        }
                                        break;
                                case WaitForState.LetPlayerLeave:
                                        if (!territory.Contains(target.GetTarget()) && !target.hasNoTargets)
                                        {
                                                onPlayerExit.Invoke();
                                                state = WaitForState.Wait;
                                                break;
                                        }
                                        break;
                        }
                        return NodeState.Running;

                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField, HideInInspector] public bool playerEnterFoldOut;
                [SerializeField, HideInInspector] public bool playerExitFoldOut;
                [SerializeField, HideInInspector] public bool buttonPressedFoldOut;
                public override bool OnInspector (AIBase ai, SerializedObject parent, Color color, bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(65, "Wait for the player to enter the territory and then for the button to be pressed. If enabled, this will return success on button pressed." +
                                        "\n \nReturns Running, Success, Failure");
                        }

                        FoldOut.Box(4, color, offsetY: -2);
                        AIBase.SetRef(ai.data, parent.Get("target"), 0);
                        AIBase.SetRef(ai.data, parent.Get("territory"), 1);
                        parent.Field("InputButtonSO", "inputButtonSO");
                        parent.FieldToggle("Return Success", "returnSuccess");
                        Layout.VerticalSpacing(3);

                        Fields.EventFoldOut(parent.Get("onPlayerEnter"), parent.Get("playerEnterFoldOut"), "On Player Enter", color: color, offsetY: -2);
                        Fields.EventFoldOut(parent.Get("onPlayerExit"), parent.Get("playerExitFoldOut"), "On Player Exit", color: color, offsetY: -2);
                        Fields.EventFoldOut(parent.Get("onButtonPressed"), parent.Get("buttonPressedFoldOut"), "On Button Pressed", color: color, offsetY: -2);
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion
        }
}
