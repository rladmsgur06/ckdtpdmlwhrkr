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
        public class SetValueFromBlackboard : Action
        {
                [SerializeField] public Blackboard data;
                [SerializeField] public Blackboard from;
                [SerializeField] public SetValueType type;

                public override NodeState RunNodeLogic (Root root)
                {
                        if (data == null || from == null)
                                return NodeState.Failure;

                        if (type == SetValueType.Float)
                        {
                                data.Set(from.GetValue());
                        }
                        else if (type == SetValueType.Int)
                        {
                                data.Set(from.GetValue());
                        }
                        else if (type == SetValueType.Bool)
                        {
                                data.Set(from.GetValue());
                        }
                        else if (type == SetValueType.Vector)
                        {
                                data.Set(from.GetTarget());
                        }
                        else if (type == SetValueType.Transform)
                        {
                                data.Set(from.GetTransform());
                        }
                        else if (type == SetValueType.GameObject)
                        {
                                if (from.GetTransform() != null)
                                        data.Set(from.GetTransform().gameObject);
                        }
                        return NodeState.Success;
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                public override bool OnInspector (AIBase ai, SerializedObject parent, Color color, bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(55, "Set the value of the blackboard data variable with that of the blackboard from variable." +
                                        "\n \nReturns Success, Failure");
                        }

                        FoldOut.Box(3, color, offsetY: -2);
                        AIBase.SetRef(ai.data, parent.Get("data"), 0);
                        AIBase.SetRef(ai.data, parent.Get("from"), 1);
                        parent.Field("Type", "type");
                        Layout.VerticalSpacing(3);
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion
        }
}
