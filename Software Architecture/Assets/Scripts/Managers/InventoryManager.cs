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

    [Header("Audio (optional)")]
    public bool playPickupSfx = true;
    public AudioSource audioSource; // if null, uses PlayClipAtPoint
    public AudioClip pickupItemSfx;
    [Range(0f, 1f)] public float sfxVolume = 0.9f;
    
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

        if (audioSource == null)
            audioSource = GetComponentInChildren<AudioSource>();
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
                PlayPickupSfx();
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
        PlayPickupSfx();
        
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

            // Be robust: PlayerHealth might be on parent/child depending on prefab setup.
            var playerHealth = player.GetComponentInParent<PlayerHealth>();
            if (playerHealth == null)
                playerHealth = player.GetComponentInChildren<PlayerHealth>();
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

    public int GetItemQuantity(ItemData itemData)
    {
        if (itemData == null) return 0;

        // First: strict reference match (ideal).
        InventoryItem item = items.Find(x => x != null && x.itemData == itemData);
        if (item != null) return item.quantity;

        // Fallback: match by itemName (helps if you accidentally created duplicated ItemData assets).
        // Keep it simple for the project requirements.
        int total = 0;
        for (int i = 0; i < items.Count; i++)
        {
            var it = items[i];
            if (it == null || it.itemData == null) continue;
            if (it.itemData.itemName == itemData.itemName)
                total += it.quantity;
        }
        return total;
    }

    void PlayPickupSfx()
    {
        if (!playPickupSfx) return;
        if (pickupItemSfx == null) return;
        if (sfxVolume <= 0f) return;

        // Prefer playing at player position so it's audible even if InventoryManager is off-screen.
        var player = GameObject.FindGameObjectWithTag("Player");
        Vector3 pos = player != null ? player.transform.position : Vector3.zero;

        if (audioSource != null)
        {
            audioSource.PlayOneShot(pickupItemSfx, sfxVolume);
            return;
        }

        AudioSource.PlayClipAtPoint(pickupItemSfx, pos, sfxVolume);
    }
}

[System.Serializable]
public class InventoryItem
{
    public ItemData itemData;
    public int quantity;
}

