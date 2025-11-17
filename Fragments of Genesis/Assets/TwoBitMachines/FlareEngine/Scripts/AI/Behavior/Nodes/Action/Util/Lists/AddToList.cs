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
        public class AddToList : Action
        {
                [SerializeField] public Blackboard list;
                [SerializeField] public Blackboard newItem;
                [SerializeField] public ListType newItemType;

                public override NodeState RunNodeLogic (Root root)
                {
                        if (list == null || newItem == null)
                                return NodeState.Failure;

                        if (newItemType == ListType.GameObject)
                        {
                                if (list.AddToList(newItem.GetGameObject()))
                                        return NodeState.Success;
                        }
                        else if (newItemType == ListType.Transform)
                        {
                                if (list.AddToList(newItem.GetTransform()))
                                        return NodeState.Success;
                        }
                        else if (newItemType == ListType.Vector)
                        {
                                if (list.AddToList(newItem.GetTarget()))
                                        return NodeState.Success;
                        }

                        return NodeState.Success;
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                public override bool OnInspector (AIBase ai, SerializedObject parent, Color color, bool onEnable)
                {
                        ;
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(50, "Add the specified item to a list." +
                                        "\n \nReturns Success, Failure");
                        }

                        FoldOut.Box(3, color, offsetY: -2);
                        AIBase.SetRef(ai.data, parent.Get("list"), 0);
                        AIBase.SetRef(ai.data, parent.Get("newItem"), 1);
                        parent.Field("New Item Type", "newItemType");
                        Layout.VerticalSpacing(3);

                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion
        }

        public enum ListType
        {
                Transform,
                GameObject,
                Vector
        }

}
