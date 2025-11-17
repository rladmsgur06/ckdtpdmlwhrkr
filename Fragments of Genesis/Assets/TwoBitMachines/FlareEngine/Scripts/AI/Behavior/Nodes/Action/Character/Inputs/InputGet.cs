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
        public class InputGet : Conditional
        {
                [SerializeField] public InputGetType type;
                [SerializeField] public KeyCode key;
                [SerializeField] public InputGetMouse mouse;

                public override NodeState RunNodeLogic (Root root)
                {
                        if (type == InputGetType.Key)
                        {
                                return Input.GetKeyDown(key) ? NodeState.Success : NodeState.Failure;
                        }
                        else
                        {
                                return Input.GetMouseButtonDown((int) mouse) ? NodeState.Success : NodeState.Failure;
                        }
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                public override bool OnInspector (AIBase ai, SerializedObject parent, Color color, bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(35, "Get Input KeyDown or MouseDown.");
                        }

                        int key = parent.Enum("type");
                        FoldOut.Box(2, color, offsetY: -2);
                        parent.Field("Type", "type");
                        parent.Field("KeyCode", "key", execute: key == 0);
                        parent.Field("Mouse", "mouse", execute: key == 1);
                        Layout.VerticalSpacing(3);
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion

        }

        public enum InputGetType
        {
                Key,
                Mouse
        }

        public enum InputGetMouse
        {
                Left,
                Right,
                Middle
        }
}
