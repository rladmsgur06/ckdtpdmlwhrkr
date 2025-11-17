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
        public class WorldFloatOperation : Action
        {
                [SerializeField] public WorldFloat worldFloat;
                [SerializeField] public WorldFloatOperate operation;
                [SerializeField] public OperateWith valueType;
                [SerializeField] public float floatValue;
                [SerializeField] public WorldFloat worldFloatValue;

                public override NodeState RunNodeLogic (Root root)
                {
                        if (worldFloat == null)
                                return NodeState.Failure;

                        float value = valueType == OperateWith.FloatValue ? floatValue : valueType == OperateWith.WorldFloat && worldFloatValue != null ? worldFloatValue.GetValue() : 0;
                        Operate(operation, value);
                        return NodeState.Success;
                }

                public void Operate (WorldFloatOperate logic, float value)
                {
                        if (logic == WorldFloatOperate.Add)
                        {
                                worldFloat.Increment(value);
                        }
                        if (logic == WorldFloatOperate.Subtract)
                        {
                                worldFloat.Increment(-value);
                        }
                        if (logic == WorldFloatOperate.Multiply)
                        {
                                worldFloat.SetValue(worldFloat.GetValue() * value);
                        }
                        if (logic == WorldFloatOperate.Divide && value != 0)
                        {
                                worldFloat.SetValue(worldFloat.GetValue() / value);
                        }
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                public override bool OnInspector (AIBase ai, SerializedObject parent, Color color, bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(35, "Perform a math operation on this World Float using another World Float.");
                        }


                        int type = parent.Enum("valueType");
                        FoldOut.Box(3, color, offsetY: -2);
                        {
                                parent.Field("World Float", "worldFloat");
                                parent.Field("Operation", "operation");
                                parent.FieldDouble("Operand", "valueType", "worldFloatValue", execute: type == 0);
                                parent.FieldDouble("Operand", "valueType", "floatValue", execute: type == 1);
                        }
                        Layout.VerticalSpacing(3);
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion

        }

        public enum WorldFloatOperate
        {
                Add,
                Subtract,
                Multiply,
                Divide
        }

        public enum OperateWith
        {
                WorldFloat,
                FloatValue
        }
}
