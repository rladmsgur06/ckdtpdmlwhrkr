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
        public class SetPosition : Action
        {
                [SerializeField] public Vector3 position;

                public override NodeState RunNodeLogic (Root root)
                {
                        transform.position = position;
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
                                Labels.InfoBoxTop(45, "Set the position of this transform." +
                                        "\n \nReturns Success");
                        }
                        FoldOut.Box(1, color, offsetY: -2);
                        {
                                parent.Field("Position", "position");
                        }
                        Layout.VerticalSpacing(3);
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion
        }

}
