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
        public class PlayAnimation : Decorator
        {
                [SerializeField] public string animationSignal = "";

                public override NodeState RunNodeLogic (Root root)
                {
                        if (children.Count == 0)
                                return NodeState.Failure;

                        root.signals.Set(animationSignal);
                        return children[0].RunChild(root);
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                public override bool OnInspector (AIBase ai, SerializedObject parent, Color color, bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(43,
                                        "This will set the specified animation signal and execute its child node concurrently. "
                                );
                        }

                        FoldOut.Box(1, color, offsetY: -2);
                        parent.Field("Animation Signal", "animationSignal");
                        Layout.VerticalSpacing(3);
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion
        }
}
