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
        public class Repeater : Decorator
        {
                [SerializeField] public int repeat = 2;

                public override NodeState RunNodeLogic (Root root)
                {
                        if (children.Count == 0)
                                return NodeState.Failure;

                        NodeState nodeState = NodeState.Success;
                        for (int i = 0; i < repeat; i++)
                        {
                                nodeState = children[0].RunChild(root);
                        }
                        return nodeState;
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                public override bool OnInspector (AIBase ai, SerializedObject parent, Color color, bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(43,
                                        "This will execute the child node by the amount of times specified in the repeat value."
                                );
                        }

                        FoldOut.Box(1, color, offsetY: -2);
                        parent.Field("Repeat", "repeat");
                        Layout.VerticalSpacing(3);
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion
        }
}
