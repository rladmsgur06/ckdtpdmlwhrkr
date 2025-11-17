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
        public class VariableLogic : Conditional
        {
                [SerializeField] public Blackboard variable;
                [SerializeField] public VariableType variableType;
                [SerializeField] public FloatLogicType logic; // don't rename
                [SerializeField] public StringLogicType stringLogic;
                [SerializeField] public BoolLogicType boolLogic;
                [SerializeField] public Vector2LogicType vector2Logic;

                [SerializeField] public CompareTo compareTo;
                [SerializeField] public float compareFloat;
                [SerializeField] public string compareString;
                [SerializeField] public Vector2 compareVector2;
                [SerializeField] public Blackboard compareVariable;

                public override NodeState RunNodeLogic (Root root)
                {
                        if (variable == null)
                                return NodeState.Failure;

                        if (variableType == VariableType.Float)
                        {
                                float compareValue = compareTo == CompareTo.Value ? compareFloat : compareVariable != null ? compareVariable.GetValue() : 0;
                                return CompareFloat(variable.GetValue(), compareValue);
                        }
                        if (variableType == VariableType.String)
                        {
                                string compareValue = compareTo == CompareTo.Value ? compareString : compareVariable != null ? (compareVariable as StringVariable).value : "";
                                return CompareString((variable as StringVariable).value, compareValue);
                        }
                        if (variableType == VariableType.Bool)
                        {
                                return CompareBool(variable.GetValue() == 1);
                        }
                        if (variableType == VariableType.Vector2)
                        {
                                Vector2 compareValue = compareTo == CompareTo.Value ? compareVector2 : compareVariable != null ? compareVariable.GetTarget() : Vector2.zero;
                                return CompareVector2(variable.GetTarget(), compareValue);
                        }
                        return NodeState.Failure;
                }

                public NodeState CompareFloat (float a, float b)
                {
                        if (logic == FloatLogicType.Greater)
                        {
                                return a > b ? NodeState.Success : NodeState.Failure;
                        }
                        if (logic == FloatLogicType.Less)
                        {
                                return a < b ? NodeState.Success : NodeState.Failure;
                        }
                        if (logic == FloatLogicType.Equal)
                        {
                                return a == b ? NodeState.Success : NodeState.Failure;
                        }
                        if (logic == FloatLogicType.GreaterOrEqualTo)
                        {
                                return a >= b ? NodeState.Success : NodeState.Failure;
                        }
                        if (logic == FloatLogicType.LessOrEqualTo)
                        {
                                return a <= b ? NodeState.Success : NodeState.Failure;
                        }
                        if (logic == FloatLogicType.IsTrue)
                        {
                                return a > 0 ? NodeState.Success : NodeState.Failure;
                        }
                        if (logic == FloatLogicType.IsFalse)
                        {
                                return a <= 0 ? NodeState.Success : NodeState.Failure;
                        }
                        if (logic == FloatLogicType.IsEven)
                        {
                                return a % 2 == 0 ? NodeState.Success : NodeState.Failure;
                        }
                        if (logic == FloatLogicType.IsOdd)
                        {
                                return a % 2 != 0 ? NodeState.Success : NodeState.Failure;
                        }
                        if (logic == FloatLogicType.Null)
                        {
                                return variable.GetTransform() == null ? NodeState.Success : NodeState.Failure;
                        }
                        if (logic == FloatLogicType.NotNull)
                        {
                                return variable.GetTransform() != null ? NodeState.Success : NodeState.Failure;
                        }
                        return NodeState.Failure;
                }

                public NodeState CompareString (string a, string b)
                {
                        if (stringLogic == StringLogicType.Equal)
                        {
                                return a == b ? NodeState.Success : NodeState.Failure;
                        }
                        return NodeState.Failure;
                }

                public NodeState CompareBool (bool a)
                {
                        if (boolLogic == BoolLogicType.IsTrue)
                        {
                                return a ? NodeState.Success : NodeState.Failure;
                        }
                        if (boolLogic == BoolLogicType.IsFalse)
                        {
                                return !a ? NodeState.Success : NodeState.Failure;
                        }
                        return NodeState.Failure;
                }

                public NodeState CompareVector2 (Vector2 a, Vector2 b)
                {
                        if (vector2Logic == Vector2LogicType.GreaterLength)
                        {
                                return a.magnitude > b.magnitude ? NodeState.Success : NodeState.Failure;
                        }
                        if (vector2Logic == Vector2LogicType.LessLength)
                        {
                                return a.magnitude < b.magnitude ? NodeState.Success : NodeState.Failure;
                        }
                        if (vector2Logic == Vector2LogicType.EqualLength)
                        {
                                return Mathf.Abs(a.magnitude - b.magnitude) < 0.001f ? NodeState.Success : NodeState.Failure;
                        }
                        if (vector2Logic == Vector2LogicType.SameVector)
                        {
                                return a == b ? NodeState.Success : NodeState.Failure;
                        }
                        return NodeState.Failure;
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                public override bool OnInspector (AIBase ai, SerializedObject parent, Color color, bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(35, "Compare a float variable to a float value.");
                        }

                        VariableType variable = (VariableType) parent.Enum("variableType");
                        CompareTo type = (CompareTo) parent.Enum("compareTo");


                        if (variable == VariableType.Float)
                        {
                                int logic = parent.Enum("logic");

                                FoldOut.Box(5, color, offsetY: -2);
                                AIBase.SetRef(ai.data, parent.Get("variable"), 0);
                                parent.Field("Type", "variableType");
                                parent.Field("Logic", "logic");
                                parent.Field("Compare To", "compareTo");

                                if (type == CompareTo.Value)
                                {
                                        parent.Field("Compare Float", "compareFloat");
                                }
                                else
                                {
                                        AIBase.SetRef(ai.data, parent.Get("compareVariable"), 1);
                                }
                                Layout.VerticalSpacing(3);
                        }
                        if (variable == VariableType.String)
                        {
                                int logic = parent.Enum("stringLogic");

                                FoldOut.Box(5, color, offsetY: -2);
                                AIBase.SetRef(ai.data, parent.Get("variable"), 0);
                                parent.Field("Type", "variableType");
                                parent.Field("Logic", "stringLogic");
                                parent.Field("Compare To", "compareTo");

                                if (type == CompareTo.Value)
                                {
                                        parent.Field("Compare String", "compareString");
                                }
                                else
                                {
                                        AIBase.SetRef(ai.data, parent.Get("compareVariable"), 1);
                                }
                                Layout.VerticalSpacing(3);
                        }
                        if (variable == VariableType.Bool)
                        {
                                int logic = parent.Enum("boolLogic");

                                FoldOut.Box(3, color, offsetY: -2);
                                AIBase.SetRef(ai.data, parent.Get("variable"), 0);
                                parent.Field("Type", "variableType");
                                parent.Field("Logic", "boolLogic");
                                Layout.VerticalSpacing(3);
                        }
                        if (variable == VariableType.Vector2)
                        {
                                int logic = parent.Enum("vector2Logic");

                                FoldOut.Box(5, color, offsetY: -2);
                                AIBase.SetRef(ai.data, parent.Get("variable"), 0);
                                parent.Field("Type", "variableType");
                                parent.Field("Logic", "vector2Logic");
                                parent.Field("Compare To", "compareTo");

                                if (type == CompareTo.Value)
                                {
                                        parent.Field("Compare Vector2", "compareVector2");
                                }
                                else
                                {
                                        AIBase.SetRef(ai.data, parent.Get("compareVariable"), 1);
                                }
                                Layout.VerticalSpacing(3);
                        }
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion

        }

        public enum VariableType
        {
                Float,
                String,
                Bool,
                Vector2
        }

        public enum CompareTo
        {
                Value,
                OtherVariable
        }

        public enum FloatLogicType
        {
                Greater,
                Less,
                Equal,
                GreaterOrEqualTo,
                LessOrEqualTo,
                IsTrue,
                IsFalse,
                IsEven,
                IsOdd,
                Null,
                NotNull
        }

        public enum StringLogicType
        {
                Equal
        }

        public enum BoolLogicType
        {
                IsTrue,
                IsFalse
        }

        public enum Vector2LogicType
        {
                GreaterLength,
                LessLength,
                EqualLength,
                SameVector
        }
}
