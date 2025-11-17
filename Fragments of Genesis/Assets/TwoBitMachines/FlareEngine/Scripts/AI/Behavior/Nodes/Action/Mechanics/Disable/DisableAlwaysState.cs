#region 
#if UNITY_EDITOR
using TwoBitMachines.Editors;
using UnityEditor;
#endif
#endregion
using UnityEngine;

namespace TwoBitMachines.FlareEngine.AI
{
        [AddComponentMenu("")]
        public class DisableAlwaysState : Action
        {
                [SerializeField] public string alwaysState;
                [System.NonSerialized] public AIFSM aifsm;

                void Start ()
                {
                        aifsm = GetComponent<AIFSM>();
                }

                public override NodeState RunNodeLogic (Root root)
                {
                        if (aifsm == null)
                                return NodeState.Failure;

                        for (int i = 0; i < aifsm.alwaysState.Count; i++)
                        {
                                if (aifsm.alwaysState[i].stateName == alwaysState)
                                {
                                        aifsm.alwaysState[i].enabled = false;
                                        break;
                                }
                        }
                        return NodeState.Success;
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                public override bool HasNextState () { return false; }
                public override bool OnInspector (AIBase ai, SerializedObject parent, Color color, bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(45, "Disable an Always State." +
                                            "\n \nReturns Failure, Success");
                        }
                        FoldOut.Box(1, color, offsetY: -2);
                        {
                                parent.Field("Always State", "alwaysState");
                        }
                        Layout.VerticalSpacing(3);
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion
        }
}

