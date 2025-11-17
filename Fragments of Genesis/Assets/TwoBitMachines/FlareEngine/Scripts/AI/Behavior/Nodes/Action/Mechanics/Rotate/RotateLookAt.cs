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
        public class RotateLookAt : Action
        {
                [SerializeField] public Blackboard lookAt;
                [SerializeField] public float speed;
                [SerializeField] public float lowerLimit = 0;
                [SerializeField] public float upperLimit = 360f;
                [SerializeField] public float previousAngle = 0;
                [SerializeField] public bool range = false;

                void Start ()
                {
                        if (lookAt == null)
                                return;
                        float angle = Mathf.Clamp(Mathf.Repeat(Angle(), 360), lowerLimit, upperLimit);
                        transform.eulerAngles = new Vector3(0, 0, angle);
                        previousAngle = angle;
                }

                public override NodeState RunNodeLogic (Root root)
                {
                        if (lookAt == null)
                        {
                                return NodeState.Failure;
                        }

                        if (Root.deltaTime != 0)
                        {
                                if (!range)
                                {
                                        transform.eulerAngles = new Vector3(0, 0, Mathf.Repeat(Angle(), 360f));
                                }
                                else
                                {
                                        float angle = Mathf.Repeat(Angle(), 360f);
                                        if ((Mathf.Abs(angle - previousAngle) > 180) && (angle > upperLimit || angle < lowerLimit)) //angle changed from 360 to 0 or vice versa and escaped limits
                                        {
                                                if (angle < previousAngle) // angle moved right, escaped upper limit
                                                        angle = upperLimit;
                                                else if (angle > previousAngle) // angle moved left, escaped lower limit
                                                        angle = lowerLimit;
                                        }
                                        float speed = Mathf.Abs(angle - previousAngle) > 45 ? 8f : 25f;
                                        angle = Mathf.Clamp(angle, lowerLimit, upperLimit);
                                        float lerpedAngle = Mathf.Lerp(previousAngle, angle, Root.deltaTime * speed);
                                        transform.eulerAngles = new Vector3(0, 0, lerpedAngle);
                                        previousAngle = lerpedAngle;
                                }

                        }
                        return NodeState.Running;
                }

                private float Angle ()
                {
                        Vector3 dir = (Vector3) lookAt.GetTarget() - transform.position;
                        return Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                public override bool HasNextState () { return false; }
                public override bool OnInspector (AIBase ai, SerializedObject parent, Color color, bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(45, "Rotate to look at the specified target." +
                                        "\n \nReturns Running, Failure");
                        }

                        FoldOut.Box(2, color, offsetY: -2);
                        {
                                AIBase.SetRef(ai.data, parent.Get("lookAt"), 0);
                                parent.FieldDoubleAndEnable("Range", "lowerLimit", "upperLimit", "range");
                                Labels.FieldDoubleText("Lower", "Upper", rightSpacing: 18);
                                parent.Clamp("lowerLimit", 0, upperLimit);
                                parent.Clamp("upperLimit", lowerLimit, 360f);
                        }
                        Layout.VerticalSpacing(3);
                        return true;
                }

                public override void OnSceneGUI (UnityEditor.Editor editor)
                {
                        if (!range)
                                return;
                        Draw.GLStart();
                        Draw.GLPartialCircle(transform.position, 2f, Color.green, lowerLimit, upperLimit, 8);
                        Draw.GLEnd();
                }
#pragma warning restore 0414
#endif
                #endregion
        }
}
