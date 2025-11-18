using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }
    
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
        // Verifica se excede a capacidade
        int totalWeight = itemData.weight * quantity;
        if (currentCapacity + totalWeight > maxCapacity)
        {
            Debug.Log("Inventário cheio!");
            return false;
        }
        
        // Verifica se o item é empilhável
        if (itemData.isStackable)
        {
            InventoryItem existingItem = items.Find(x => x.itemData == itemData);
            if (existingItem != null)
            {
                existingItem.quantity += quantity;
                currentCapacity += totalWeight;
                return true;
            }
        }
        
        // Verifica se tem espaço para novo slot
        if (items.Count >= maxSlots)
        {
            Debug.Log("Sem slots disponíveis!");
            return false;
        }
        
        // Adiciona novo item
        InventoryItem newItem = new InventoryItem
        {
            itemData = itemData,
            quantity = quantity
        };
        items.Add(newItem);
        currentCapacity += totalWeight;
        
        return true;
    }
    
    public void RemoveItem(ItemData itemData, int quantity = 1)
    {
        InventoryItem item = items.Find(x => x.itemData == itemData);
        if (item != null)
        {
            item.quantity -= quantity;
            currentCapacity -= itemData.weight * quantity;
            
            if (item.quantity <= 0)
            {
                items.Remove(item);
            }
        }
    }
    
    public void UseItem(InventoryItem item)
    {
        if (item.itemData.isConsumable)
        {
            // Aqui você pode adicionar lógica para usar o item
            Debug.Log($"Usando {item.itemData.itemName}");
            RemoveItem(item.itemData, 1);
        }
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

