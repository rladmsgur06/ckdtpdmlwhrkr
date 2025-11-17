#if UNITY_EDITOR
using TwoBitMachines.Editors;
using UnityEditor;
#endif
using UnityEngine;

namespace TwoBitMachines.FlareEngine.AI
{
        [AddComponentMenu("")]
        public class Probability : Action
        {
                [SerializeField] public float probability = 0.5f;

                public override NodeState RunNodeLogic (Root root)
                {
                        return Random.value <= probability ? NodeState.Success : NodeState.Failure;
                }

                #region ▀▄▀▄▀▄ Custom Inspector▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                public override bool OnInspector (AIBase ai, SerializedObject parent, Color color, bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(45, "Returns Success if a random value is below the given probability. \n \nReturns Failure, Success");
                        }

                        FoldOut.Box(1, color, offsetY: -2);
                        {
                                parent.Field("Probability", "probability");
                                parent.Clamp("probability");
                        }
                        Layout.VerticalSpacing(3);
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion
        }

}
