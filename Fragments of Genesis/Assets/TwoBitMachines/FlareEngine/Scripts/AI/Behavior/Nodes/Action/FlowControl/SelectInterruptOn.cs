
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
        public class SelectInterruptOn : Action
        {
                public override NodeState RunNodeLogic (Root root)
                {
                        root.selectInterrupt = true;
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
                                Labels.InfoBoxTop(45, "Select interrupt turned on." +
                                        "\n \nReturns Success");
                        }
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion
        }
}
