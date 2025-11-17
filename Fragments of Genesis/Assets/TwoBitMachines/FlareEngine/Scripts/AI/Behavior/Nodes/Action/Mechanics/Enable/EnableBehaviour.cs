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
        public class EnableBehaviour : Action
        {
                [SerializeField] public Behaviour behaviourRef;
                [SerializeField] public Renderer renderRef;
                [SerializeField] public bool enable;

                public override NodeState RunNodeLogic (Root root)
                {
                        if (behaviourRef != null)
                                behaviourRef.enabled = enable;
                        if (renderRef != null)
                                renderRef.enabled = enable;
                        return NodeState.Success;
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                public override bool HasNextState () { return false; }
                public override bool OnInspector (AIBase ai, SerializedObject parent, Color color, bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(55, "This will enable or disable the specified behaviour or renderer." +
                                        "\n \nReturns Success");
                        }

                        FoldOut.Box(3, color, offsetY: -2);
                        parent.Field("Behaviour", "behaviourRef");
                        parent.Field("Renderer", "renderRef");
                        parent.Field("Enable", "enable");
                        Layout.VerticalSpacing(3);
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion
        }
}
