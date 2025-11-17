#region 
#if UNITY_EDITOR
using TwoBitMachines.Editors;
using UnityEditor;
#endif
#endregion
using System.Collections.Generic;
using UnityEngine;

namespace TwoBitMachines.FlareEngine.AI
{
        [AddComponentMenu("")]
        public class RandomLoot : Action
        {
                [SerializeField] private InventorySO inventory;
                [SerializeField] private List<ItemSO> items = new List<ItemSO>();

                public override NodeState RunNodeLogic (Root root)
                {
                        if (inventory == null)
                                return NodeState.Failure;

                        if (nodeSetup == NodeSetup.NeedToInitialize)
                        {
                                int rand = Random.Range(0, items.Count);
                                inventory.AddToInventory(items[rand], 0);
                        }
                        return NodeState.Success;
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                public override bool HasNextState () { return false; }
                public override bool OnInspector (AIBase ai, SerializedObject parent, Color color, bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(55, "Add a random item from the list to an inventory." +
                                        "\n \nReturns Success, Failure");
                        }
                        FoldOut.Box(1, color, offsetY: -2);
                        parent.Field("Inventory SO", "inventory");
                        Layout.VerticalSpacing(3);
                        Fields.Array(parent.Get("items"), "Add Item", "Item", color, true);
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion
        }
}
