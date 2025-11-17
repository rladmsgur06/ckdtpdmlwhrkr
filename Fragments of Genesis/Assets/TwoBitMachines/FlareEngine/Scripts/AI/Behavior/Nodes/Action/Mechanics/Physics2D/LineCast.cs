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
        public class LineCast : Conditional
        {
                public LayerMask layer;
                public Blackboard origin;
                public Blackboard target;
                public CastType searchFor;

                public override NodeState RunNodeLogic (Root root)
                {
                        if (searchFor == CastType.SingleHit)
                        {
                                if (CastSingle(root))
                                        return NodeState.Success;
                        }
                        else
                        {
                                if (CastAll(root))
                                        return NodeState.Success;
                        }
                        return NodeState.Failure;

                }

                public bool CastSingle (Root root)
                {
                        RaycastHit2D hit = Physics2D.Linecast(origin.GetTarget(), target.GetTarget(), layer);
                        if (origin.hasNoTargets || target.hasNoTargets)
                        {
                                return false;
                        }
                        #region Debug
#if UNITY_EDITOR
                        if (WorldManager.viewDebugger)
                        {
                                Debug.DrawLine(origin.GetTarget(), target.GetTarget(), Color.red);
                        }
#endif
                        #endregion
                        if (hit && hit.distance > 0)
                        {
                                Root.raycastHit2D = hit;
                                return true;
                        }
                        return false;
                }

                public bool CastAll (Root root)
                {
                        root.SetLayerMask(layer);
                        int i = Physics2D.Linecast(origin.GetTarget(), target.GetTarget(), Root.filter2D, Root.rayResults);
                        if (origin.hasNoTargets || target.hasNoTargets)
                        {
                                return false;
                        }
                        #region Debug
#if UNITY_EDITOR
                        if (WorldManager.viewDebugger)
                        {
                                Debug.DrawLine(origin.GetTarget(), target.GetTarget(), Color.red);
                        }
#endif
                        #endregion
                        return i > 0;
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                public override bool OnInspector (AIBase ai, SerializedObject parent, Color color, bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(44, "Implement a LineCast using Physics2D. The results can be accessed by Nearest2DResults.");
                        }

                        FoldOut.Box(4, color, offsetY: -2);
                        parent.Field("Layer", "layer");
                        AIBase.SetRef(ai.data, parent.Get("origin"), 0);
                        AIBase.SetRef(ai.data, parent.Get("target"), 1);
                        parent.Field("Search For", "searchFor");
                        Layout.VerticalSpacing(3);
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion

        }
}
