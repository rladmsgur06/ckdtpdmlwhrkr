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
        public class TouchingLayers : Conditional
        {
                public LayerMask layer;
                public Collider2D collider2DRef;

                public override NodeState RunNodeLogic (Root root)
                {
                        if (collider2DRef == null)
                                return NodeState.Failure;

                        if (Physics2D.IsTouchingLayers(collider2DRef, layer))
                        {
                                return NodeState.Success;
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
                                Labels.InfoBoxTop(35, "Check if the specified collider is touching any other collider in the specified layer.");
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
