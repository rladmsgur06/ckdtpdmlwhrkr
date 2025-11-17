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
        public class LayerResult : Conditional
        {
                public LayerMask layer;

                public override NodeState RunNodeLogic (Root root)
                {
                        if (Root.raycastHit2D.transform == null)
                        {
                                return NodeState.Failure;
                        }
                        if ((layer.value & (1 << Root.raycastHit2D.transform.gameObject.layer)) > 0)
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
                                Labels.InfoBoxTop(44, "Rays casted using Single Hit can compare if the resulting object belongs to the specified layer.");
                        }

                        FoldOut.Box(1, color, offsetY: -2);
                        parent.Field("Layer", "layer");
                        Layout.VerticalSpacing(3);
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion
        }
}
