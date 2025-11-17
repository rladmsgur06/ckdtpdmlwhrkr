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
        public class InputButtonSOGet : Conditional
        {
                [SerializeField] public InputButtonSO inputButtonSO;
                [SerializeField] public ButtonTrigger trigger;

                private void Awake ()
                {
                        WorldManager.RegisterInput(inputButtonSO);
                }

                public override NodeState RunNodeLogic (Root root)
                {
                        if (inputButtonSO == null)
                                return NodeState.Failure;

                        return inputButtonSO.Active(trigger) ? NodeState.Success : NodeState.Failure;
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                public override bool OnInspector (AIBase ai, SerializedObject parent, Color color, bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(35, "Returns success if InputButtonSO is true.");
                        }

                        FoldOut.Box(1, color, offsetY: -2);
                        parent.FieldDouble("InputButtonSO", "trigger", "inputButtonSO");
                        Layout.VerticalSpacing(3);
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion

        }

}
