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
        public class SetDirection : Action
        {
                [SerializeField] public Character character;
                [SerializeField] public int direction = 1;

                public override NodeState RunNodeLogic (Root root)
                {
                        if (character == null)
                                return NodeState.Failure;
                        root.direction = (int) Mathf.Sign(direction);
                        character.signals.SetDirection(root.direction);
                        return NodeState.Success;
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                public override bool OnInspector (AIBase ai, SerializedObject parent, Color color, bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(55, "Set the direction of a character." +
                                        "\n \nReturns Success, Failure");
                        }

                        FoldOut.Box(2, color, offsetY: -2);
                        parent.Field("Character", "character");
                        parent.Field("Direction", "direction");
                        Layout.VerticalSpacing(3);
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion
        }
}
