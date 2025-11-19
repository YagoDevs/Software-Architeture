using UnityEngine;

namespace InventorySystem.Core
{
    [CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
    public class ItemData : ScriptableObject
    {
        [Header("Item Info")]
        public string itemName;
        public Sprite icon;
        [TextArea(3, 6)]
        public string description;
        
        [Header("Item Properties")]
        public ItemCategory category;
        public bool isStackable = true;
        public int maxStack = 99;
        public int weight = 1;
        
        [Header("Item Effects")]
        public bool isConsumable = false;
        public int healthRestore = 0;
    }

    public enum ItemCategory
    {
        All,
        Potions,
        Materials,
        Food,
        Weapons,
        Keys
    }
}
