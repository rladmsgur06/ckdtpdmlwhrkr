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
        public class PauseCollisions : Action
        {
                [SerializeField] public bool enable;
                [SerializeField] public bool translate;
                [SerializeField] public bool revertOnReset;

                public override NodeState RunNodeLogic (Root root)
                {
                        root.pauseCollision = enable;
                        root.moveWithTranslate = translate;
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
                                Labels.InfoBoxTop(55, "Pause world collisions. If enabled, choose to move transform using Translate or not." +
                                        "\n \nReturns Success");
                        }

                        FoldOut.Box(2, color, offsetY: -2);
                        parent.Field("Enable", "enable");
                        parent.Field("Translate", "translate");
                        Layout.VerticalSpacing(3);
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion
        }
}
