#region
#if UNITY_EDITOR
using TwoBitMachines.Editors;
using UnityEditor;
#endif
#endregion
using TwoBitMachines.FlareEngine.AI.BlackboardData;
using UnityEngine;

namespace TwoBitMachines.FlareEngine.AI
{
        [AddComponentMenu("")]
        public class SetWorldFloatAtRate : Action
        {
                [SerializeField] public WorldFloat worldFloat;
                [SerializeField] public float rate = 1f;
                [SerializeField] public bool duration;
                [SerializeField] public float time;

                [System.NonSerialized] public float counter;

                public override NodeState RunNodeLogic (Root root)
                {
                        if (worldFloat == null)
                                return NodeState.Failure;

                        if (nodeSetup == NodeSetup.NeedToInitialize)
                        {
                                counter = 0;
                        }

                        worldFloat.SetValue(worldFloat.GetValue() + rate * Root.deltaTime);

                        if (duration && TwoBitMachines.Clock.Timer(ref counter, time))
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
                                Labels.InfoBoxTop(55, "Increase or decrease a World Float value at the specified rate." +
                                        "\n \nReturns Running,Success, Failure");
                        }

                        FoldOut.Box(3, color, offsetY: -2);
                        parent.Field("World Float", "worldFloat");
                        parent.Field("Rate", "rate");
                        parent.FieldAndEnable("Duration", "time", "duration");
                        Layout.VerticalSpacing(3);
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion
        }
}
