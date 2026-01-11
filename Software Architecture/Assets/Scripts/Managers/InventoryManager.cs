using System.Collections.Generic;
using System;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    // UI/Quests can subscribe to refresh when inventory changes.
    public event Action OnInventoryChanged;
    
    [Header("Inventory Settings")]
    public int maxCapacity = 250;
    public int currentCapacity = 0;
    
    [Header("Inventory Data")]
    public List<InventoryItem> items = new List<InventoryItem>();
    public int maxSlots = 25; // 5x5 grid
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public bool AddItem(ItemData itemData, int quantity = 1)
    {
        if (itemData == null) return false;
        if (quantity <= 0) return false;

        // Check if the capacity limit would be exceeded
        int totalWeight = itemData.weight * quantity;
        if (currentCapacity + totalWeight > maxCapacity)
        {
            Debug.Log("Inventory full!");
            return false;
        }
        
        // Check whether the item can be stacked
        if (itemData.isStackable)
        {
            InventoryItem existingItem = items.Find(x => x.itemData == itemData);
            if (existingItem != null)
            {
                existingItem.quantity += quantity;
                currentCapacity += totalWeight;
                OnInventoryChanged?.Invoke();
                return true;
            }
        }
        
        // Ensure there is room for a new slot
        if (items.Count >= maxSlots)
        {
            Debug.Log("No slots available!");
            return false;
        }
        
        // Add a new item entry
        InventoryItem newItem = new InventoryItem
        {
            itemData = itemData,
            quantity = quantity
        };
        items.Add(newItem);
        currentCapacity += totalWeight;

        OnInventoryChanged?.Invoke();
        
        return true;
    }
    
    public void RemoveItem(ItemData itemData, int quantity = 1)
    {
        if (itemData == null) return;
        if (quantity <= 0) return;

        InventoryItem item = items.Find(x => x.itemData == itemData);
        if (item != null)
        {
            item.quantity -= quantity;
            currentCapacity -= itemData.weight * quantity;
            
            if (item.quantity <= 0)
            {
                items.Remove(item);
            }

            if (currentCapacity < 0) currentCapacity = 0;
            OnInventoryChanged?.Invoke();
        }
    }
    
    public void UseItem(InventoryItem item)
    {
        if (item == null || item.itemData == null) return;
        if (!item.itemData.isConsumable) return;

        // Gameplay logic for consuming items (simple + extensible)
        // Example: HP potion
        if (item.itemData.healthRestore > 0)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                Debug.LogWarning("No GameObject with tag 'Player' found. Cannot apply consumable effects.");
                return;
            }

            var playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth == null)
            {
                Debug.LogWarning("Player does not have PlayerHealth. Cannot apply consumable effects.");
                return;
            }

            bool healed = playerHealth.TryHeal(item.itemData.healthRestore);
            if (!healed)
            {
                // Don't waste potion when already full HP (simple UX)
                Debug.Log("HP already full. Consumable not used.");
                return;
            }
        }

        Debug.Log($"Using {item.itemData.itemName}");
        RemoveItem(item.itemData, 1);
        // RemoveItem already triggers OnInventoryChanged.
    }
    
    public List<InventoryItem> GetItemsByCategory(ItemCategory category)
    {
        if (category == ItemCategory.All)
            return items;
        
        return items.FindAll(x => x.itemData.category == category);
    }
}

[System.Serializable]
public class InventoryItem
{
    public ItemData itemData;
    public int quantity;
}

