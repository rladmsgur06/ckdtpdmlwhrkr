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
        public class SetTransform : Action
        {
                [SerializeField] public Blackboard targetTransform;
                [SerializeField] public TransformType type;
                [SerializeField] public Vector3 value;
                [SerializeField] public float time = 0.1f;
                [SerializeField] public bool lerp;

                [SerializeField] public TransformPositionType positionType;
                [SerializeField] public Transform positionTransform;
                [SerializeField] public Blackboard positionTarget;

                [System.NonSerialized] private float counter = 0;
                [System.NonSerialized] private Vector3 startValue = Vector3.zero;
                [System.NonSerialized] private Vector3 currentValue = Vector3.zero;

                public override NodeState RunNodeLogic (Root root)
                {
                        if (targetTransform == null)
                                return NodeState.Failure;

                        Transform t = targetTransform.GetTransform();
                        if (t == null)
                                return NodeState.Failure;

                        bool complete = false;
                        if (nodeSetup == NodeSetup.NeedToInitialize)
                        {
                                counter = 0;
                                currentValue = Vector3.zero;
                                if (type == TransformType.Position)
                                {
                                        startValue = t.localPosition;
                                }
                                else if (type == TransformType.Rotate)
                                {
                                        startValue = t.localEulerAngles;
                                }
                                else
                                {
                                        startValue = t.localScale;
                                }
                        }

                        if (type == TransformType.Position)
                        {
                                if (positionType == TransformPositionType.Transform)
                                {
                                        if (positionTransform != null)
                                                value = positionTransform.position;
                                }
                                if (positionType == TransformPositionType.Target)
                                {
                                        if (positionTarget != null)
                                                value = positionTarget.GetTarget();
                                }
                        }

                        if (lerp)
                        {
                                counter += Root.deltaTime;
                                float percent = time == 0 ? 1f : counter / time;
                                currentValue.x = Mathf.Lerp(startValue.x, value.x, percent);
                                currentValue.y = Mathf.Lerp(startValue.y, value.y, percent);
                                currentValue.z = Mathf.Lerp(startValue.z, value.z, percent);
                                complete = percent >= 1f;
                        }
                        else
                        {
                                complete = true;
                                currentValue = value;
                        }

                        if (type == TransformType.Position)
                                t.localPosition = currentValue;
                        else if (type == TransformType.Rotate)
                                t.localEulerAngles = currentValue;
                        else
                                t.localScale = currentValue;
                        return complete ? NodeState.Success : NodeState.Running;
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                public override bool OnInspector (AIBase ai, SerializedObject parent, Color color, bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(75, "Set the position, rotation, or scale of a transform instantly or by lerping. This should not be used by a moving platform unless it's for the reset state." +
                                        "\n \nReturns Running, Success, Failure");
                        }

                        int type = parent.Enum("type");
                        if (type == 0)
                        {
                                int positionType = parent.Enum("positionType");

                                FoldOut.Box(5, color, offsetY: -2);
                                AIBase.SetRef(ai.data, parent.Get("targetTransform"), 0);
                                parent.Field("Type", "type");
                                parent.Field("Position Type", "positionType");
                                parent.Field("Vector", "value", execute: positionType == 0);
                                parent.Field("Transform", "positionTransform", execute: positionType == 1);
                                if (positionType == 2)
                                        AIBase.SetRef(ai.data, parent.Get("positionTarget"), 1);
                                parent.FieldAndEnable("Lerp Duration", "time", "lerp");
                                Layout.VerticalSpacing(3);
                        }
                        else
                        {

                                FoldOut.Box(4, color, offsetY: -2);
                                AIBase.SetRef(ai.data, parent.Get("targetTransform"), 0);
                                parent.Field("Type", "type");
                                parent.Field("New Value", "value");
                                parent.FieldAndEnable("Lerp Duration", "time", "lerp");
                                Layout.VerticalSpacing(3);
                        }

                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion
        }

        public enum TransformType
        {
                Position,
                Rotate,
                Scale
        }

        public enum TransformPositionType
        {
                Vector,
                Transform,
                Target
        }
}
