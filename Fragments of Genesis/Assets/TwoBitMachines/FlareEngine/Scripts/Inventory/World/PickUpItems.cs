using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace TwoBitMachines.FlareEngine
{
        [AddComponentMenu("Flare Engine/一Inventory/PickUpItems")]
        public class PickUpItems : MonoBehaviour
        {
                [SerializeField] public List<Inventory> inventories = new List<Inventory>();
                [SerializeField] public List<ItemEventMapper> events = new List<ItemEventMapper>();
                [SerializeField] public LayerMask layer;
                [SerializeField] public bool enableMouseClick;
                [SerializeField] private ItemEventData itemEventData = new ItemEventData();
                [SerializeField] private Character character;

                [SerializeField] public WorldFloatSO playerMoney;
                [SerializeField] public Transform player;
                [SerializeField] public UnityEvent onSell;
                [SerializeField] public UnityEvent onReject;
                [SerializeField] public bool isVendor;

                [System.NonSerialized] private float toolSwapCounter;

                #region ▀▄▀▄▀▄ Editor Variables ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField] private bool addEvent;
                [SerializeField] private bool addInventory;
                [SerializeField] private bool eventFoldOut;
                [SerializeField] private bool vendorFoldOut;
                [SerializeField] private bool inventoryFoldOut;
                [SerializeField] private bool onSellFoldOut;
                [SerializeField] private bool onRejectFoldOut;
                [SerializeField] private bool vendorEventsFoldOut;
#pragma warning restore 0414
#endif
                #endregion

                private void Start ()
                {
                        for (int i = 0; i < inventories.Count; i++)
                        {
                                if (inventories[i] != null)
                                {
                                        inventories[i].Initialize(this);
                                }
                        }
                        character = this.transform.GetComponent<Character>();
                        toolSwapCounter = 0.75f;
                }


                private void OnDisable ()
                {
                        for (int i = 0; i < inventories.Count; i++)
                        {
                                if (inventories[i] != null)
                                {
                                        inventories[i].ClearInventories();
                                }
                        }
                }

                private void OnTriggerEnter2D (Collider2D collider)
                {
                        FoundItem(collider);
                }

                private void FoundItem (Collider2D collider)
                {
                        if (collider.gameObject.layer == WorldManager.hideLayer && gameObject.layer != WorldManager.hideLayer)
                        {
                                return;
                        }
                        if (gameObject.layer == WorldManager.hideLayer && collider.gameObject.layer != WorldManager.hideLayer)
                        {
                                return;
                        }

                        Quest quest = collider.GetComponent<Quest>();
                        if (quest != null)
                        {
                                quest.AcceptQuest();
                                return;
                        }

                        Item item = collider.GetComponent<Item>();
                        if (item != null && item.type != ItemPickUpType.Item)
                        {
                                ToolSwap(item);
                        }
                        else
                        {
                                AddItem(item, item != null ? item.droppedValue : 0);
                        }
                }

                public bool AddItem (Item item, int droppedValue)
                {
                        if (item == null || item.itemSO == null)
                        {
                                return false;
                        }
                        if (item.itemSO.forInventory == ForInventory.NotForInventory)
                        {
                                item.InvokeEvent();
                                return true;
                        }
                        if (item.itemSO.forInventory == ForInventory.UseImmediately)
                        {
                                if (item.itemSO.UseItem(this, true))
                                {
                                        item.InvokeEvent(); // on found event
                                        item.gameObject.SetActive(false);
                                        return true;
                                }
                                else if (!isVendor)
                                {
                                        item.InvokeEvent();
                                        item.gameObject.SetActive(false);
                                        return true;
                                }
                        }
                        for (int i = 0; i < inventories.Count; i++)
                        {
                                if (inventories[i].inventorySO != null && inventories[i].inventorySO.AddToInventory(item.itemSO, droppedValue))
                                {
                                        item.InvokeEvent();
                                        item.gameObject.SetActive(false);
                                        return true;
                                }
                        }
                        return false;
                }

                public bool SellItem (ItemSO itemSO)
                {
                        if (playerMoney == null || player == null)
                        {
                                return false;
                        }

                        PickUpItems pickUpItem = player.GetComponent<PickUpItems>();

                        if (pickUpItem == null)
                        {
                                return false;
                        }

                        if (playerMoney.value < itemSO.cost)
                        {
                                onReject.Invoke();
                                return false;
                        }
                        if (itemSO.forInventory == ForInventory.UseImmediately)
                        {
                                if (itemSO.UseItem(pickUpItem, true))
                                {
                                        playerMoney.IncrementValue(-itemSO.cost);
                                        onSell.Invoke();
                                        return true;
                                }
                        }
                        for (int i = 0; i < pickUpItem.inventories.Count; i++)
                        {
                                if (pickUpItem.inventories[i].inventorySO != null && pickUpItem.inventories[i].inventorySO.AddToInventory(itemSO, 0))
                                {
                                        playerMoney.IncrementValue(-itemSO.cost);
                                        onSell.Invoke();
                                        return true;
                                }
                        }
                        onReject.Invoke();
                        return false;
                }

                public bool TriggerItemEvent (string name, float genericFloat, string genericString, bool toggle)
                {
                        itemEventData.Reset(genericFloat, genericString, toggle);
                        for (int i = 0; i < events.Count; i++)
                        {
                                if (events[i].keyName == name)
                                {
                                        ItemEventMapper current = events[i];
                                        if (character != null && !current.ConditionSuccess(character))
                                        {
                                                current.conditionFailed.Invoke();
                                                return false;
                                        }
                                        current.itemEvent.Invoke(itemEventData);
                                        bool success = !itemEventData.success.HasValue ? true : itemEventData.success.Value; // if it was not set, read as true
                                        return success; // itemEventData.success == null ? true : itemEventData == true ? true : false;
                                }
                        }
                        return false;
                }

                public void DeselectItem (string name)
                {
                        for (int i = 0; i < events.Count; i++)
                        {
                                if (events[i].keyName == name)
                                {
                                        events[i].deselect.Invoke();
                                        return;
                                }
                        }
                }

                public void LateUpdate ()
                {
                        if (enableMouseClick && Input.GetMouseButtonDown(0))
                        {
                                Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                                Collider2D collider = Physics2D.OverlapCircle(mousePosition, 0.5f, layer);
                                if (collider != null)
                                {
                                        FoundItem(collider);
                                }
                        }

                        for (int i = 0; i < inventories.Count; i++)
                        {
                                if (inventories[i].toggle != null && inventories[i].toggle.Pressed() && !Inventory.blockInventories)
                                {
                                        if (!BlockInventory(inventories[i]))
                                        {
                                                GameObject inventory = inventories[i].gameObject;
                                                inventory.SetActive(!inventory.activeInHierarchy);
                                        }
                                }
                        }
                        Clock.TimerExpired(ref toolSwapCounter, 0.75f);
                }

                private bool BlockInventory (Inventory current)
                {
                        for (int i = 0; i < current.block.Count; i++)
                        {
                                if (current.block[i] == null)
                                        continue;
                                if (current.block[i].gameObject.activeInHierarchy)
                                        return true;
                        }
                        return false;
                }

                private void ToolSwap (Item item)
                {
                        if (item.transform != this.transform)
                        {
                                if (item.type == ItemPickUpType.ToolSwap)
                                {
                                        if (toolSwapCounter < 0.70f)
                                                return;
                                        toolSwapCounter = 0f; // prevent rapid swaps (or will look glitchy)
                                        if (character != null)
                                                character.DeactivateAllTools();
                                        Transform child = transform.GetChild(0);
                                        if (child != null && child.GetComponent<Tool>() != null)
                                        {
                                                Item itemTool = child.GetComponent<Item>();
                                                if (itemTool != null && itemTool.type == ItemPickUpType.ToolSwap)
                                                {
                                                        child.transform.parent = item.transform.parent;
                                                        child.transform.eulerAngles = Vector3.zero;
                                                        child.transform.position = item.transform.position;
                                                        child.gameObject.SetActive(true);
                                                }
                                        }

                                        item.InvokeEvent();
                                        item.transform.parent = this.transform;
                                        item.transform.SetSiblingIndex(0);
                                }
                                else
                                {

                                        if (character != null)
                                                character.DeactivateAllTools();
                                        item.InvokeEvent();
                                        item.transform.parent = this.transform;
                                        item.transform.SetSiblingIndex(0);
                                }
                        }
                }
        }

        public enum PickUpItemCondition
        {
                None,
                PlayerOnGround,
                PlayerOnWall
        }

        [System.Serializable]
        public class ItemEventMapper
        {
                [SerializeField] public string keyName;
                [SerializeField] public PickUpItemCondition condition;
                [SerializeField] public UnityEventItem itemEvent;
                [SerializeField] public UnityEvent deselect;
                [SerializeField] public UnityEvent conditionFailed;

                public bool ConditionSuccess (Character character)
                {
                        if (condition == PickUpItemCondition.None)
                        {
                                return true;
                        }
                        if (condition == PickUpItemCondition.PlayerOnGround && character.world.onGround)
                        {
                                return true;
                        }
                        if (condition == PickUpItemCondition.PlayerOnWall && character.world.onWall)
                        {
                                return true;
                        }
                        return false;
                }

#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField] private bool foldOut;
                [SerializeField] private bool eventFoldOut;
                [SerializeField] private bool deselectFoldOut;
                [SerializeField] private bool conditionFoldOut;
#pragma warning restore 0414
#endif

        }

}
