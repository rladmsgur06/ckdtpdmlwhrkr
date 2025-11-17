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
        public class WorldBoolLogic : Conditional
        {
                [SerializeField] public WorldBool variable;

                public override NodeState RunNodeLogic (Root root)
                {
                        return variable != null && variable.GetValue() ? NodeState.Success : NodeState.Failure;
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                public override bool OnInspector (AIBase ai, SerializedObject parent, Color color, bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(35, "Check if the World Bool is True or False.");
                        }

                        FoldOut.Box(1, color, offsetY: -2);
                        parent.Field("World Bool", "variable");
                        Layout.VerticalSpacing(3);
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion

        }
}
