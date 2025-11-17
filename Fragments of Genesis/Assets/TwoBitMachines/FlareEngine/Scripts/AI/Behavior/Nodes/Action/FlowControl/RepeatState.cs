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
        public class RepeatState : Action
        {
                [SerializeField] public int repeat = 2;
                [System.NonSerialized] private int counter = 0;

                public override void OnReset (bool skip = false, bool enteredState = false)
                {
                        if (enteredState)
                                counter = 0;
                }

                public override NodeState RunNodeLogic (Root root)
                {
                        counter++;
                        return counter >= repeat ? NodeState.Success : NodeState.Failure;
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                public override bool OnInspector (AIBase ai, SerializedObject parent, Color color, bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(65,
                                        "This will check how many times the state has looped. Returns success when the repeat value has been reached." +
                                        "\n \nReturns Success, Failure");
                        }

                        FoldOut.Box(1, color, offsetY: -2);
                        {
                                parent.Field("Repeat", "repeat");
                        }
                        Layout.VerticalSpacing(3);
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion
        }
}
