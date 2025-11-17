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
        public class OverlapCollider : Conditional
        {
                public LayerMask layer;
                public Collider2D collider2DRef;

                public override NodeState RunNodeLogic (Root root)
                {
                        if (collider2DRef == null)
                                return NodeState.Failure;

                        root.SetLayerMask(layer);
                        int i = Physics2D.OverlapCollider(collider2DRef, Root.filter2D, Root.colliderResults);
                        return i > 0 ? NodeState.Success : NodeState.Failure;
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                public override bool OnInspector (AIBase ai, SerializedObject parent, Color color, bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(44, "Implement an OverlapCollider using Physics2D. The results can be accessed by Nearest2DResults.");
                        }

                        FoldOut.Box(2, color, offsetY: -2);
                        parent.Field("Layer", "layer");
                        parent.Field("Collider 2D", "collider2DRef");
                        Layout.VerticalSpacing(3);
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion
        }
}
