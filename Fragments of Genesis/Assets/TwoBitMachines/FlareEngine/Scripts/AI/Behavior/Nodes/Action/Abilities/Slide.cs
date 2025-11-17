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
        public class Slide : Action
        {
                [SerializeField] public float speed = 5f;
                [SerializeField] public float time = 2f;
                [System.NonSerialized] private float counter = 0;
                [System.NonSerialized] private float direction = 0;

                public override NodeState RunNodeLogic (Root root)
                {
                        if (nodeSetup == NodeSetup.NeedToInitialize)
                        {
                                counter = 0;
                                direction = root.direction;
                                if (time <= 0)
                                        return NodeState.Failure;
                        }
                        counter += Root.deltaTime;
                        root.velocity.x = direction * speed * (1f - EasingFunction.Run(Tween.EaseOutCubic, counter / time));
                        return counter >= time ? NodeState.Success : NodeState.Running;
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                public override bool OnInspector (AIBase ai, SerializedObject parent, Color color, bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(55, "The AI will slide on the ground and come to a stop. Slide direction is determined by AI direction." +
                                        "\n \nReturns Running, Success, Failure");
                        }

                        FoldOut.Box(2, color, offsetY: -2);
                        parent.Field("Speed", "speed");
                        parent.Field("Time", "time");
                        Layout.VerticalSpacing(3);
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion
        }
}
