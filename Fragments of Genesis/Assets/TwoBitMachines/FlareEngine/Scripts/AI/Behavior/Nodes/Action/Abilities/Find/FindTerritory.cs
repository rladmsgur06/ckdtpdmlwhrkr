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
        public class FindTerritory : Conditional
        {
                [SerializeField] public Blackboard target;
                [SerializeField] public Blackboard territory;
                [SerializeField] public UnityEvent onFound;
                [SerializeField] public UnityEvent onExit;

                [System.NonSerialized] private State state;
                private enum State { Waiting, Found }

                public override NodeState RunNodeLogic (Root root)
                {
                        if (target == null || territory == null)
                        {
                                return NodeState.Failure;
                        }
                        if (nodeSetup == NodeSetup.NeedToInitialize)
                        {
                                state = State.Waiting;
                        }

                        if (state == State.Waiting)
                        {
                                if (territory.Contains(target.GetTarget()))
                                {
                                        if (this.target.hasNoTargets)
                                        {
                                                return NodeState.Running;
                                        }

                                        state = State.Found;
                                        onFound.Invoke();
                                }
                        }
                        if (state == State.Found)
                        {
                                if (!territory.Contains(target.GetTarget()))
                                {
                                        if (this.target.hasNoTargets)
                                        {
                                                return NodeState.Running;
                                        }
                                        state = State.Waiting;
                                        onExit.Invoke();
                                        return NodeState.Success;
                                }
                        }
                        return NodeState.Running;
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField, HideInInspector] public bool eventsFoldOut;
                [SerializeField, HideInInspector] public bool foundFoldOut;
                [SerializeField, HideInInspector] public bool exitFoldOut;
                public override bool OnInspector (AIBase ai, SerializedObject parent, Color color, bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(40, "Find the specified Territory. Use the found and exit events. Returns Success on exit.");
                        }
                        FoldOut.Box(2, color, extraHeight: 5, offsetY: -2);
                        {
                                AIBase.SetRef(ai.data, parent.Get("target"), 0);
                                AIBase.SetRef(ai.data, parent.Get("territory"), 1);
                        }
                        if (FoldOut.FoldOutButton(parent.Get("eventsFoldOut")))
                        {
                                Fields.EventFoldOut(parent.Get("onFound"), parent.Get("foundFoldOut"), "On Found", color: color);
                                Fields.EventFoldOut(parent.Get("onExit"), parent.Get("exitFoldOut"), "On Exit", color: color);
                        }
                        return true;
                }

#pragma warning restore 0414
#endif
                #endregion

        }

}
