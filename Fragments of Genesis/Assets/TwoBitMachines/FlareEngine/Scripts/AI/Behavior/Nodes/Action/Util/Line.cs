#region 
#if UNITY_EDITOR
using System.Collections.Generic;
using TwoBitMachines.Editors;
using UnityEditor;
#endif
#endregion
using UnityEngine;
using UnityEngine.Events;
using TwoBitMachines.FlareEngine.AI.BlackboardData;

namespace TwoBitMachines.FlareEngine.AI
{
        [AddComponentMenu("")]
        public class Line : Action
        {
                [SerializeField] public LineRenderer lineRenderer;
                [SerializeField] public Blackboard targetA;
                [SerializeField] public Blackboard targetB;

                public override NodeState RunNodeLogic (Root root)
                {
                        if (lineRenderer == null || targetA == null || targetB == null)
                        {
                                return NodeState.Failure;
                        }
                        lineRenderer.SetPosition(0, targetA.GetTarget());
                        lineRenderer.SetPosition(1, targetB.GetTarget());
                        return NodeState.Running;
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                public override bool OnInspector (AIBase ai, SerializedObject parent, Color color, bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(55, "Use a Line Renderer to draw a line between two points. " +
                                        "\n \nReturns Running, Failure");
                        }

                        FoldOut.Box(3, color, offsetY: -2);
                        {
                                parent.Field("Line Renderer", "lineRenderer");
                                AIBase.SetRef(ai.data, parent.Get("targetA"), 0);
                                AIBase.SetRef(ai.data, parent.Get("targetB"), 1);
                        }
                        Layout.VerticalSpacing(3);
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion
        }
}
