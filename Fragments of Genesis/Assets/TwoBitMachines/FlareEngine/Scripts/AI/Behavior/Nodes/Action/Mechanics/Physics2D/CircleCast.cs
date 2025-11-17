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
        public class CircleCast : Conditional
        {
                public LayerMask layer;
                public Blackboard origin;
                public CastType searchFor;
                public RayDirection directionType;
                public Vector2 direction;
                public float radius = 1f;
                public float length = 1f;

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
                        RaycastHit2D hit = Physics2D.CircleCast(origin.GetTarget(), radius, Direction(root.direction), length, layer);

                        if (this.origin.hasNoTargets)
                        {
                                return false;
                        }
                        #region Debug
#if UNITY_EDITOR
                        if (WorldManager.viewDebugger)
                        {
                                Debug.DrawRay(origin.GetTarget(), Direction(root.direction) * length, Color.red);
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
                        int i = Physics2D.CircleCast(origin.GetTarget(), radius, Direction(root.direction), Root.filter2D, Root.rayResults, length);
                        if (this.origin.hasNoTargets)
                        {
                                return false;
                        }
                        #region Debug
#if UNITY_EDITOR
                        if (WorldManager.viewDebugger)
                        {
                                Debug.DrawRay(origin.GetTarget(), Direction(root.direction) * length, Color.red);
                        }
#endif
                        #endregion

                        return i > 0;
                }

                public Vector2 Direction (float characterDirection)
                {
                        if (directionType == RayDirection.CharacterDirection)
                                return Vector2.right * characterDirection;
                        return direction;
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                public override bool OnInspector (AIBase ai, SerializedObject parent, Color color, bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(44, "Implement a CircleCast using Physics2D. The results can be accessed by Nearest2DResults.");
                        }

                        int type = parent.Enum("directionType");
                        int height = type == 0 ? 1 : 0;
                        FoldOut.Box(6 + height, color, offsetY: -2);
                        parent.Field("Layer", "layer");
                        AIBase.SetRef(ai.data, parent.Get("origin"), 0);
                        parent.Field("Radius", "radius");
                        parent.Field("Length", "length");
                        parent.Field("Search For", "searchFor");
                        parent.Field("Direction Type", "directionType");
                        parent.Field("Direction", "direction", type == 0);
                        Layout.VerticalSpacing(3);
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion
        }
}
