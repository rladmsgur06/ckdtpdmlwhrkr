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
        public class GetRandomTerritoryTarget : Action
        {
                [SerializeField] public Blackboard territory;
                [SerializeField] public Blackboard setTarget;

                public override NodeState RunNodeLogic (Root root)
                {
                        if (setTarget == null || territory == null)
                                return NodeState.Failure;
                        setTarget.Set(territory.GetRandomTarget());
                        return NodeState.Success;
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                public override bool OnInspector (AIBase ai, SerializedObject parent, Color color, bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(65, "Retrieve a random point inside a territory and set this point as a Vector2 to the specified target." +
                                        "\n \nReturns Success, Failure");
                        }

                        FoldOut.Box(2, color, offsetY: -2);
                        AIBase.SetRef(ai.data, parent.Get("territory"), 0);
                        AIBase.SetRef(ai.data, parent.Get("setTarget"), 1);
                        Layout.VerticalSpacing(3);
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion
        }
}
