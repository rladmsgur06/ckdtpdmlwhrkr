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
        public class RepeatTimer : Decorator
        {
                [SerializeField] public ClockRandom type;
                [SerializeField] public float timer = 1f;
                [System.NonSerialized] public float counter;

                public override NodeState RunNodeLogic (Root root)
                {
                        if (children.Count == 0)
                                return NodeState.Failure;

                        if (nodeSetup == NodeSetup.NeedToInitialize)
                        {
                                counter = timer;
                        }
                        if (TwoBitMachines.Clock.Timer(ref counter, timer))
                        {
                                children[0].RunChild(root);
                        }
                        return NodeState.Running;
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                public override bool OnInspector (AIBase ai, SerializedObject parent, Color color, bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(35, "This will execute a child node each time the timer completes.");
                        }
                        FoldOut.Box(1, color, offsetY: -2);
                        parent.Field("Timer", "timer");
                        Layout.VerticalSpacing(3);
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion
        }
}
