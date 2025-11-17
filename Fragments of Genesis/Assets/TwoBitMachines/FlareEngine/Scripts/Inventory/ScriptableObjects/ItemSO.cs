using UnityEngine;

namespace TwoBitMachines.FlareEngine
{
        [CreateAssetMenu(menuName = "FlareEngine/ItemSO")]
        public class ItemSO : JournalObject
        {
                [SerializeField] public string itemName;
                [SerializeField] public string keyName;
                [SerializeField] public string genericString;
                [SerializeField] public string extraInfo;
                [SerializeField] public string description = "Item Description.";

                [SerializeField] public bool consumable;
                [SerializeField] public float genericFloat;
                [SerializeField] public int stackLimit = 10;
                [SerializeField] public int cost = 0;

                [SerializeField] public Sprite icon;
                [SerializeField] public GameObject prefab;
                [SerializeField] public Droppable droppable;
                [SerializeField] public ForInventory forInventory;
                [SerializeField] public VendorItemRemove vendorItem;

                public bool isDroppable => droppable == Droppable.Yes;

                #region ▀▄▀▄▀▄ Editor Variables ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField, HideInInspector] public bool add;
                [SerializeField, HideInInspector] public bool foldOut;
                [SerializeField, HideInInspector] public bool remove;
                [SerializeField, HideInInspector] public bool delete;
                [SerializeField, HideInInspector] public bool deleteAsk;
                [SerializeField, HideInInspector] public bool deleteData;
                [SerializeField, HideInInspector] public bool descriptionFoldOut;
                [SerializeField, HideInInspector] public bool extraInfoFoldOut;
#endif
                #endregion

                public virtual bool UseItem (PickUpItems pickUpItem, bool toggleTool = true)
                {
                        if (pickUpItem.isVendor)
                        {
                                return pickUpItem.SellItem(this);
                        }
                        else
                        {
                                return pickUpItem.TriggerItemEvent(keyName, genericFloat, genericString, toggleTool);
                        }
                }

                public virtual void DeselectItem (PickUpItems pickUpItem)
                {
                        pickUpItem.DeselectItem(keyName);
                }

                public virtual void DropItem (Transform character, int droppedValue)
                {
                        if (!consumable)
                        {
                                Character equipment = character.GetComponent<Character>();
                                if (equipment != null)
                                {
                                        equipment.DeactivateTool(itemName);
                                }
                        }
                        if (prefab != null)
                        {
                                float randomX = Random.Range(2f, 3f);
                                float randomY = Random.Range(1f, 2f);
                                GameObject gameObject = Instantiate(prefab, character.position + Vector3.up * randomY + Vector3.right * -randomX, Quaternion.identity);
                                if (consumable)
                                {
                                        Item item = gameObject.GetComponent<Item>(); //    in case item is stacked, remember current stack value
                                        if (item != null)
                                                item.droppedValue = droppedValue;
                                }
                        }
                }

                #region Journal
                public override string Name ()
                {
                        return itemName;
                }

                public override string Description ()
                {
                        return description;
                }

                public override string ExtraInfo ()
                {
                        return "";
                }

                public override bool Complete ()
                {
                        return true;
                }

                public override Sprite Icon ()
                {
                        return icon;
                }
                #endregion

        }

        public enum Droppable
        {
                No,
                Yes
        }

        public enum ForInventory
        {
                AddToInventory,
                UseImmediately,
                NotForInventory
        }

        public enum VendorItemRemove
        {
                RemoveItemAfterSell,
                KeepItemAfterSell
        }
}
