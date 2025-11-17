#region 
#if UNITY_EDITOR
using TwoBitMachines.Editors;
using UnityEditor;
#endif
#endregion
using UnityEngine;
using UnityEngine.Events;

namespace TwoBitMachines.FlareEngine.AI
{
        [AddComponentMenu("")]
        public class CapturedByYoshi : Action
        {
                [SerializeField] public int weapon = 0;

                public override NodeState RunNodeLogic (Root root)
                {
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
                                Labels.InfoBoxTop(65, "This AI can be captured by Yoshi. The weapon is an index mapped to Yoshi's weapons." +
                                        "\n \nReturns Success");
                        }
                        FoldOut.Box(1, color, offsetY: -2);
                        parent.Field("Weapon", "weapon");
                        Layout.VerticalSpacing(3);
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion
        }
}
