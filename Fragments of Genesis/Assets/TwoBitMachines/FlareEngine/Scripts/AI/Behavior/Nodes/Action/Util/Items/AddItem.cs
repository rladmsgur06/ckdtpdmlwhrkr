#region 
#if UNITY_EDITOR
using TwoBitMachines.Editors;
using UnityEditor;
#endif
#endregion
using UnityEngine;
using UnityEngine.Events;

namespace TwoBitMachines.FlareEngine.AI
{
        [AddComponentMenu("")]
        public class AddItem : Action
        {
                [SerializeField] public InventorySO inventorySO;
                [SerializeField] public ItemSO itemSO;
                [SerializeField] public UnityEvent onItemAdded = new UnityEvent();

                public override NodeState RunNodeLogic (Root root)
                {
                        if (inventorySO == null || itemSO == null || inventorySO.ContainsItem(itemSO.itemName))
                        {
                                return NodeState.Failure;
                        }

                        inventorySO.AddToInventory(itemSO, 0);
                        onItemAdded.Invoke();
                        return NodeState.Success;
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField, HideInInspector] public bool onAddFoldOut;

                public override bool OnInspector (AIBase ai, SerializedObject parent, Color color, bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(50, "Add the specified item to the inventory." +
                                        "\n \nReturns Success, Failure");
                        }

                        FoldOut.Box(2, color, offsetY: -2);
                        parent.Field("InventorySO", "inventorySO");
                        parent.Field("ItemSO", "itemSO");

                        Fields.EventFoldOut(parent.Get("onItemAdded"), parent.Get("onAddFoldOut"), "On Item Added", color: color);
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion
        }
}
