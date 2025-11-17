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
        public class RemoveFromList : Action
        {

                [SerializeField] public Blackboard list;
                [SerializeField] public Blackboard removeItem;
                [SerializeField] public ListType newItemType;

                public override NodeState RunNodeLogic (Root root)
                {
                        if (list == null || removeItem == null)
                                return NodeState.Failure;

                        if (newItemType == ListType.GameObject)
                        {
                                if (list.RemoveFromList(removeItem.GetGameObject()))
                                        return NodeState.Success;
                        }
                        else if (newItemType == ListType.Transform)
                        {
                                if (list.RemoveFromList(removeItem.GetTransform()))
                                        return NodeState.Success;
                        }
                        else if (newItemType == ListType.Vector)
                        {
                                if (list.RemoveFromList(removeItem.GetTarget()))
                                        return NodeState.Success;
                        }

                        return NodeState.Failure;
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                public override bool OnInspector (AIBase ai, SerializedObject parent, Color color, bool onEnable)
                {
                        ;
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(60, "Remove the specified item from a list." +
                                        "\n \nReturns Success, Failure");
                        }

                        FoldOut.Box(3, color, offsetY: -2);
                        AIBase.SetRef(ai.data, parent.Get("list"), 0);
                        AIBase.SetRef(ai.data, parent.Get("removeItem"), 1);
                        parent.Field("Remove Item Type", "newItemType");
                        Layout.VerticalSpacing(3);

                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion
        }

}
