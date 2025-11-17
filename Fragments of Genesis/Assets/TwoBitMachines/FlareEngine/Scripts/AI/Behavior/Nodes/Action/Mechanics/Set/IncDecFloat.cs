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
        public class IncDecFloat : Action
        {
                [SerializeField] public Blackboard data;
                [SerializeField] public IncDecType type;
                [SerializeField] public float by = 1f;

                public override NodeState RunNodeLogic (Root root)
                {
                        if (data == null)
                                return NodeState.Failure;

                        if (type == IncDecType.Increase)
                        {
                                data.Set(data.GetValue() + by);
                        }
                        else
                        {
                                data.Set(data.GetValue() - by);
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
                                Labels.InfoBoxTop(55, "Increase or decrease a float variable by the specified amount." +
                                        "\n \nReturns Success");
                        }
                        FoldOut.Box(2, color, offsetY: -2);
                        {
                                AIBase.SetRef(ai.data, parent.Get("data"), 0);
                                parent.FieldDouble("Type", "type", "by");
                        }
                        Layout.VerticalSpacing(3);
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion
        }

        public enum IncDecType
        {
                Increase,
                Decrease
        }

}
