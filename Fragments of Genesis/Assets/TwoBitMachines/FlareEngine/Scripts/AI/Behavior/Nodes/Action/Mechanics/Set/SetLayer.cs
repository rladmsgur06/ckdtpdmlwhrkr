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
        public class SetLayer : Action
        {
                [SerializeField] public Blackboard target;
                [SerializeField] public int layer;

                public override NodeState RunNodeLogic (Root root)
                {
                        if (target == null)
                        {
                                return NodeState.Failure;
                        }

                        Transform transform = target.GetTransform();
                        if (transform == null)
                        {
                                return NodeState.Failure;
                        }

                        transform.gameObject.layer = layer;
                        return NodeState.Success;
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                public override bool OnInspector (AIBase ai, SerializedObject parent, Color color, bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(45, "Set the layer of the gameObject." +
                                        "\n \nReturns  Success, Failure");
                        }

                        FoldOut.Box(2, color, offsetY: -2);
                        AIBase.SetRef(ai.data, parent.Get("target"), 0);
                        parent.Get("layer").FieldLayer("Layer");
                        Layout.VerticalSpacing(3);
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion
        }

}
