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
        public class SetValue : Action
        {
                [SerializeField] public Blackboard data;
                [SerializeField] public SetValueType type;
                [SerializeField] public bool boolVal;
                [SerializeField] public float floatVal;
                [SerializeField] public string stringVal;
                [SerializeField] public Vector3 vectorVal;
                [SerializeField] public Transform transformVal;

                public override NodeState RunNodeLogic (Root root)
                {
                        if (data == null)
                                return NodeState.Failure;

                        if (type == SetValueType.Float)
                        {
                                data.Set(floatVal);
                        }
                        else if (type == SetValueType.Int)
                        {
                                data.Set(floatVal);
                        }
                        else if (type == SetValueType.Bool)
                        {
                                data.Set(boolVal);
                        }
                        else if (type == SetValueType.Vector)
                        {
                                data.Set(vectorVal);
                        }
                        else if (type == SetValueType.String)
                        {
                                data.Set(stringVal);
                        }
                        else if (type == SetValueType.Transform)
                        {
                                data.Set(transformVal);
                        }
                        else if (type == SetValueType.GameObject)
                        {
                                if (transformVal != null)
                                        data.Set(transformVal.gameObject);
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
                                Labels.InfoBoxTop(55, "Set the value of the blackboard variable." +
                                        "\n \nReturns Success, Failure");
                        }
                        int index = parent.Enum("type");
                        FoldOut.Box(3, color, offsetY: -2);
                        AIBase.SetRef(ai.data, parent.Get("data"), 0);
                        parent.Field("Type", "type");
                        parent.Field("Float", "floatVal", execute: index == 0);
                        parent.Field("Int", "floatVal", execute: index == 1);
                        parent.Field("Bool", "boolVal", execute: index == 2);
                        parent.Field("Vector", "vectorVal", execute: index == 3);
                        parent.Field("String", "stringVal", execute: index == 4);
                        parent.Field("Transform", "transformVal", execute: index == 5);
                        parent.Field("GameObject", "transformVal", execute: index == 6);
                        Layout.VerticalSpacing(3);
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion
        }

        public enum SetValueType
        {
                Float,
                Int,
                Bool,
                Vector,
                String,
                Transform,
                GameObject
        }
}
