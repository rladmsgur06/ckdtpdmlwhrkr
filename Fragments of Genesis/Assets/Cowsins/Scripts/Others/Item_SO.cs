using UnityEngine;

namespace cowsins2D
{
    [CreateAssetMenu(menuName = "Cowsins/New Inventory item", fileName = "New Inventory Item", order = 2)]
    public class Item_SO : ScriptableObject
    {
        [Header("Basic")] public string itemName;
        public string description;
        public Sprite itemIcon;
        [Header("[Only assign for 2.5D games]")] public GameObject item3DObject;
        [Range(1, 99), Header("Others")] public int maxStack = 1;

        /// <summary>
        /// You can override this on your own Items_SO. Notice that this is not a function. It returns a boolean. If you can use it, it will return true, if not it will return false.
        /// You can adjust this behaviour as much as you want.
        /// </summary>
        /// <param name="controller">WeaponController necessary to pass. You can use this to perform custom operations and to access the player in case you need it.</param>
        /// <returns></returns>
        public virtual bool Use(WeaponController controller)
        {
            Debug.LogErrorFormat("You are trying to call the base Item_SO method");
            return true;
        }
    }
}