using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float interactionRange = 3f;
    
    [Header("UI Feedback")]
    public GameObject interactionPrompt; // Optional: UI showing "Press F to collect"
    
    private ItemPickup nearbyItem;
    
    void Update()
    {
        // Detect nearby items
        DetectNearbyItems();
        
        // Press F to collect
        if (Keyboard.current.fKey.wasPressedThisFrame)
        {
            Debug.Log("F key pressed!");
            
            if (nearbyItem != null)
            {
                CollectItem(nearbyItem);
            }
            else
            {
                Debug.Log("No nearby item!");
            }
        }
    }
    
    void DetectNearbyItems()
    {
        // Search all nearby colliders (no layer filter)
        Collider[] hits = Physics.OverlapSphere(transform.position, interactionRange);
        
        foreach (Collider hit in hits)
        {
            ItemPickup item = hit.GetComponent<ItemPickup>();
            if (item != null)
            {
                nearbyItem = item;
                ShowInteractionPrompt(true);
                return;
            }
        }
        
        nearbyItem = null;
        ShowInteractionPrompt(false);
    }
    
    void CollectItem(ItemPickup item)
    {
        if (item == null)
        {
            Debug.LogError("Item reference is null!");
            return;
        }
        
        if (item.itemData == null)
        {
            Debug.LogError("Item's ItemData is null!");
            return;
        }
        
        Debug.Log($"Attempting to collect: {item.itemData.itemName}");
        
        bool added = InventoryManager.Instance.AddItem(item.itemData, item.quantity);
        
        if (added)
        {
            Debug.Log($"✓ Collected: {item.itemData.itemName} x{item.quantity}");
            Destroy(item.gameObject);
            nearbyItem = null;
        }
        else
        {
            Debug.Log("✗ Inventory full!");
        }
    }
    
    void ShowInteractionPrompt(bool show)
    {
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(show);
        }
    }
    
    // Visualize the interaction range inside the editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}

