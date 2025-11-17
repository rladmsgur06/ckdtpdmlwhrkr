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
        public class Move : Action
        {
                [SerializeField] public Vector2 velocity;
                [SerializeField] public bool hasTimer;
                [SerializeField] public float time = 1f;
                [SerializeField] public bool flipVelX;
                [System.NonSerialized] private float counter;

                public override NodeState RunNodeLogic (Root root)
                {
                        if (nodeSetup == NodeSetup.NeedToInitialize)
                        {
                                counter = 0;
                        }
                        if (velocity.x != 0)
                        {
                                root.velocity.x = flipVelX ? Mathf.Abs(velocity.x) * root.direction : velocity.x;
                        }
                        if (velocity.y != 0)
                        {
                                root.velocity.y = velocity.y;
                        }
                        if (hasTimer && TwoBitMachines.Clock.Timer(ref counter, time))
                        {
                                return NodeState.Success;
                        }
                        return NodeState.Running;
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                public override bool HasNextState () { return false; }
                public override bool OnInspector (AIBase ai, SerializedObject parent, Color color, bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(65, "This will move the AI with the specified velocity. This can have a time limit. If FlipVelX true, velocity x will flip to AI direction." +
                                        "\n \nReturns Success, Running");
                        }

                        FoldOut.Box(3, color, offsetY: -2);
                        {
                                parent.Field("Velocity", "velocity");
                                parent.FieldAndEnable("Time Limit", "time", "hasTimer");
                                parent.FieldToggleAndEnable("Flip Vel X", "flipVelX");
                        }
                        Layout.VerticalSpacing(3);
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion
        }
}
