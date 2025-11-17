using UnityEngine;

namespace TwoBitMachines.FlareEngine
{
        [AddComponentMenu ("Flare Engine/一Inventory/Item")]
        public class Item : MonoBehaviour
        {
                [SerializeField] public ItemPickUpType type;
                [SerializeField] public ItemSO itemSO;
                [SerializeField] public bool foldOut;
                [SerializeField] public string itemWE;
                [SerializeField] public string journal = "journal";
                [SerializeField] public UnityEventEffect onFound;

                [System.NonSerialized] public int droppedValue = 0; // in case item is stacked and is dropped

                public void InvokeEvent ( )
                {
                        if (itemSO != null)
                        {
                                onFound.Invoke (ImpactPacket.impact.Set (itemWE, transform.position, Vector2.zero));
                                Journal.AddToJournal (itemSO, journal);
                        }
                }
        }

        public enum ItemPickUpType
        {
                Item,
                Tool,
                ToolSwap
        }
}