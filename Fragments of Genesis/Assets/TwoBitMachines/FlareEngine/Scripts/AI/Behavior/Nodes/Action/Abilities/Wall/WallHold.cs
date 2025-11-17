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
        public class WallHold : Action
        {
                [SerializeField] public float holdTime = 2f;
                [SerializeField] public string signal;

                [System.NonSerialized] public float counter;
                [System.NonSerialized] public float direction;

                public override NodeState RunNodeLogic (Root root)
                {
                        if (nodeSetup == NodeSetup.NeedToInitialize)
                        {
                                counter = 0;
                                direction = root.world.rightWall ? 1f : -1f;
                        }
                        if (TwoBitMachines.Clock.Timer(ref counter, holdTime) || !root.world.onWall)
                        {
                                return NodeState.Success;
                        }
                        root.velocity.y = 0;
                        root.velocity.x = direction;
                        root.signals.Set(signal);
                        return NodeState.Running;
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                public override bool OnInspector (AIBase ai, SerializedObject parent, Color color, bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(55, "Hold a wall for the specified time." +
                                        "\n \nReturns Running, Success");
                        }
                        FoldOut.Box(1, color, offsetY: -2);
                        {
                                parent.FieldDouble("Hold Time", "holdTime", "signal");
                                Labels.FieldText("Signal");
                        }
                        Layout.VerticalSpacing(3);
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion
        }
}
