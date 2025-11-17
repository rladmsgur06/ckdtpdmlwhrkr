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
        public class Repeat : Action
        {
                [SerializeField] public int repeat = 2;
                [System.NonSerialized] private int counter = 0;

                public override NodeState RunNodeLogic (Root root)
                {
                        if (nodeSetup == NodeSetup.NeedToInitialize)
                        {
                                counter = 0;
                        }
                        counter++;
                        return counter >= repeat ? NodeState.Success : NodeState.Running;
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                public override bool OnInspector (AIBase ai, SerializedObject parent, Color color, bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(65,
                                        "This will execute a specified number of times then return Success." +
                                        "\n \nReturns Running, Success");
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
