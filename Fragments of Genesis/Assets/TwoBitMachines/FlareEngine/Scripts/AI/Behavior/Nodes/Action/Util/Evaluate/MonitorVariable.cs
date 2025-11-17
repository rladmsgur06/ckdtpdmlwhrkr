#region 
#if UNITY_EDITOR
using TwoBitMachines.Editors;
using UnityEditor;
#endif
#endregion
using TwoBitMachines.FlareEngine.AI.BlackboardData;
using UnityEngine;

namespace TwoBitMachines.FlareEngine.AI
{
        [AddComponentMenu("")]
        public class MonitorVariable : Action
        {
                [SerializeField] public Blackboard variable;
                [SerializeField] public MonitorType returnSuccess;
                private float previousFloat;

                public override NodeState RunNodeLogic (Root root)
                {
                        if (variable == null)
                                return NodeState.Failure;

                        if (nodeSetup == NodeSetup.NeedToInitialize)
                        {
                                previousFloat = variable.GetValue();
                        }

                        float newValue = variable.GetValue();
                        if (previousFloat != newValue)
                        {
                                float oldValue = previousFloat;
                                previousFloat = newValue;

                                if (returnSuccess == MonitorType.OnDecrease)
                                {
                                        if (newValue < oldValue)
                                        {
                                                return NodeState.Success;
                                        }
                                }
                                else if (returnSuccess == MonitorType.OnIncrease)
                                {
                                        if (newValue > oldValue)
                                        {
                                                return NodeState.Success;
                                        }
                                }
                                else
                                {
                                        return NodeState.Success;
                                }
                        }
                        return NodeState.Running;
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                public override bool OnInspector (AIBase ai, SerializedObject parent, Color color, bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(55, "Check if a blackboard variable has changed value. Typically used for float variables." +
                                        "\n \nReturns Running, Success, Failure");
                        }

                        FoldOut.Box(2, color, offsetY: -2);
                        AIBase.SetRef(ai.data, parent.Get("variable"), 0);
                        parent.Field("Return Success", "returnSuccess");
                        Layout.VerticalSpacing(3);

                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion

                public enum MonitorType
                {
                        OnDecrease,
                        OnIncrease,
                        OnAny
                }

        }
}
