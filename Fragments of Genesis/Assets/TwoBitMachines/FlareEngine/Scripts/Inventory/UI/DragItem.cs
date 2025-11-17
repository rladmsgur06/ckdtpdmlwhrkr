using UnityEngine;
using UnityEngine.UI;

namespace TwoBitMachines.FlareEngine
{
            public class DragItem : MonoBehaviour
            {
                        [System.NonSerialized] public ItemUI itemUI;
                        [System.NonSerialized] public InventorySlot oldSlot;
                        [System.NonSerialized] public Image imageIcon;
                        [System.NonSerialized] public RectTransform rectTransform;

                        public bool active => gameObject.activeInHierarchy;
                        public bool canTransfer => itemUI != null && oldSlot != null;

                        private void Awake ( )
                        {
                                    imageIcon = this.gameObject.GetComponent<Image> ( );
                                    rectTransform = this.gameObject.GetComponent<RectTransform> ( );
                        }

                        public void Enable (ItemUI item, InventorySlot oldSlot, Sprite icon)
                        {
                                    if (item == null || oldSlot == null) return;

                                    this.gameObject.SetActive (true);
                                    this.itemUI = item;
                                    this.oldSlot = oldSlot;
                                    if (imageIcon != null) imageIcon.sprite = icon;
                        }

                        public void Disable ( )
                        {
                                    this.gameObject.SetActive (false);
                        }
            }
}