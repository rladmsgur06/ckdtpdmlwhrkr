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
        public class UseItem : Action
        {
                [SerializeField] public InventorySO inventorySO;
                [SerializeField] public ItemSO itemSO;
                [SerializeField] public bool removeItemOnUse;
                [SerializeField] public UnityEvent onItemUsed = new UnityEvent();

                public override NodeState RunNodeLogic (Root root)
                {
                        if (inventorySO == null || itemSO == null || !inventorySO.ContainsItem(itemSO.itemName))
                        {
                                return NodeState.Failure;
                        }

                        if (removeItemOnUse)
                        {
                                inventorySO.RemoveItem(itemSO.itemName);
                        }
                        onItemUsed.Invoke();
                        return NodeState.Success;
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField, HideInInspector] public bool onUseFoldOut;

                public override bool OnInspector (AIBase ai, SerializedObject parent, Color color, bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(55, "Check if the specified item is in the inventory, use it, and remove it." +
                                        "\n \nReturns Success, Failure");
                        }

                        FoldOut.Box(3, color, offsetY: -2);
                        parent.Field("InventorySO", "inventorySO");
                        parent.Field("ItemSO", "itemSO");
                        parent.FieldToggle("Remove Item On Use", "removeItemOnUse");

                        Fields.EventFoldOut(parent.Get("onItemUsed"), parent.Get("onUseFoldOut"), "On Item Used", color: color);
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion
        }
}
