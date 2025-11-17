#if UNITY_EDITOR
using TwoBitMachines.Editors;
using UnityEditor;
#endif
using UnityEngine;

namespace TwoBitMachines.FlareEngine.AI
{
        [AddComponentMenu("")]
        public class Rotate : Action
        {
                [SerializeField] public RotatePattern pattern;
                [SerializeField] public GameObject rotateObject;
                [SerializeField] public Tween tween = Tween.Linear;
                [SerializeField] public float rotateBy = 180f;
                [SerializeField] public float rotateSpeed = 10f;
                [SerializeField] public float duration = 1f;
                [SerializeField] public bool timeLimit = false;

                [System.NonSerialized] private float completeCounter;
                [System.NonSerialized] private float lerpTime;
                [System.NonSerialized] private float startAngle;
                [System.NonSerialized] private float endAngle;

                public override NodeState RunNodeLogic (Root root)
                {
                        Transform transform = rotateObject != null ? rotateObject.transform : this.transform;

                        if (nodeSetup == NodeSetup.NeedToInitialize)
                        {
                                completeCounter = 0;
                                lerpTime = 0;
                                if (pattern == RotatePattern.RotateByAngle)
                                {
                                        startAngle = transform.eulerAngles.z;
                                        endAngle = startAngle + rotateBy;
                                }
                        }

                        if (pattern == RotatePattern.RotateByAngle)
                        {
                                lerpTime += Time.deltaTime;
                                float percent = lerpTime / duration;
                                float currentAngle = Mathf.Lerp(startAngle, endAngle, EasingFunction.Run(tween, percent));
                                transform.localEulerAngles = new Vector3(0, 0, currentAngle);
                                if (percent >= 1f)
                                {
                                        return NodeState.Success;
                                }
                        }
                        else
                        {
                                transform.Rotate(Vector3.forward * rotateSpeed * 10f * Time.deltaTime, Space.Self);
                                if (timeLimit && TwoBitMachines.Clock.Timer(ref completeCounter, duration))
                                {
                                        return NodeState.Success;
                                }
                        }
                        return NodeState.Running;
                }

                #region ▀▄▀▄▀▄ Custom Inspector▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                public override bool OnInspector (AIBase ai, SerializedObject parent, Color color, bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(65, "Rotate. If rotate object is set, this object will be rotated instead of the AI object." +
                                        "\n \nReturns Success, Failure");
                        }

                        int type = parent.Enum("pattern");
                        FoldOut.Box(4, color, offsetY: -2);
                        {
                                parent.Field("Type", "pattern", execute: type == 0);
                                parent.FieldDouble("Type", "pattern", "rotateBy", execute: type == 1);
                                parent.Field("Rotate Speed", "rotateSpeed", execute: type == 0);
                                parent.Field("Easing", "tween", execute: type == 1);
                                parent.Field("Duration", "duration", execute: type == 1);
                                parent.FieldAndEnable("Duration", "duration", "timeLimit", execute: type == 0);
                                parent.Field("Rotate Object", "rotateObject");
                        }
                        Layout.VerticalSpacing(3);
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion
        }
}
