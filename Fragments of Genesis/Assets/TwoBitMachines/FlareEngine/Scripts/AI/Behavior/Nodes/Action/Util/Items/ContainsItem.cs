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
        public class ContainsItem : Action
        {
                [SerializeField] public InventorySO inventorySO;
                [SerializeField] public ItemSO itemSO;

                public override NodeState RunNodeLogic (Root root)
                {
                        if (inventorySO == null || itemSO == null || !inventorySO.ContainsItem(itemSO.itemName))
                        {
                                return NodeState.Failure;
                        }
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
                                Labels.InfoBoxTop(50, "Does the inventory contain the specified item?" +
                                        "\n \nReturns Success, Failure");
                        }

                        FoldOut.Box(2, color, offsetY: -2);
                        parent.Field("InventorySO", "inventorySO");
                        parent.Field("ItemSO", "itemSO");
                        Layout.VerticalSpacing(5);
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion
        }
}
