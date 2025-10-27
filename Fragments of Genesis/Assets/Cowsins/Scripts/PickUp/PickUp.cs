using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace cowsins2D
{
    public class PickUp : Interactable
    {
        protected Rigidbody2D rb;
        [SerializeField] protected bool includeNamePickUpText = true;
        [Tooltip("SpriteRenderer to display the item icon."), SerializeField] protected Transform graphics;

        protected virtual void Start()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        protected virtual void Update()
        {
            if (rb == null || rb.velocity.magnitude <= .1f) return;

            // Extra Gravity
            rb.AddForce(Vector2.down * 100f, ForceMode2D.Force);
        }

        protected void SetPickeableGraphics(Item_SO item)
        {
            // Verify that there are no missing references before setting the image to the item icon.
            if (item == null || graphics == null)
            {
                Debug.LogWarning(this.name + " ´s variable is null.");
                return;
            }
            SpriteRenderer spriteRenderer = graphics.GetComponent<SpriteRenderer>();

            if (graphics != null && spriteRenderer != null)
                spriteRenderer.sprite = item.itemIcon;
            else
            {
                GameObject obj = Instantiate(item.item3DObject, transform.position, Quaternion.identity);
                obj.transform.SetParent(graphics.transform);
            }
        }
    }
}