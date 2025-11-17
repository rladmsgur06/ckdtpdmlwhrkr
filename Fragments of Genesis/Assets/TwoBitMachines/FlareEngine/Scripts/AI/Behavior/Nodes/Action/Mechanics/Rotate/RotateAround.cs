#region 
#if UNITY_EDITOR
using System.Runtime;
using TwoBitMachines.Editors;
using UnityEditor;
#endif
#endregion
using TwoBitMachines.FlareEngine.AI.BlackboardData;
using UnityEngine;

namespace TwoBitMachines.FlareEngine.AI
{
        [AddComponentMenu("")]
        public class RotateAround : Action
        {
                [SerializeField] public Blackboard target;
                [SerializeField] public float speed;

                public override NodeState RunNodeLogic (Root root)
                {
                        if (target == null)
                        {
                                return NodeState.Failure;
                        }
                        if (Root.deltaTime != 0)
                        {
                                transform.RotateAround(target.GetTarget(), Vector3.forward, speed * Root.deltaTime);
                                if (target.hasNoTargets)
                                {
                                        return NodeState.Failure;
                                }
                        }
                        return NodeState.Running;
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                public override bool HasNextState () { return false; }
                public override bool OnInspector (AIBase ai, SerializedObject parent, Color color, bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(55, "Rotate the transform around the specified target" +
                                        "\n \nReturns Running, Failure");
                        }

                        FoldOut.Box(2, color, offsetY: -2);
                        AIBase.SetRef(ai.data, parent.Get("target"), 0);
                        parent.Field("Speed", "speed");
                        Layout.VerticalSpacing(3);
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion
        }
}
