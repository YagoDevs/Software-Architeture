/*
This script is used to detect nearby pickups and collect items with a key press, showing an optional interaction prompt.
*/

using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

// this is for the interaction with the items, so we can collect them
public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float interactionRange = 3f;
    
    [Header("UI Feedback")]
    public GameObject interactionPrompt; // Optional: UI showing "Press F to collect"
    public TextMeshProUGUI interactionPromptText; // Optional: set text like "Press F - Potion"
    
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

        ItemPickup best = null;
        float bestDist = float.MaxValue;

        foreach (Collider hit in hits)
        {
            if (hit == null) continue;
            ItemPickup item = hit.GetComponentInParent<ItemPickup>();
            if (item == null) continue;
            if (item.itemData == null) continue;

            float d = Vector3.Distance(transform.position, item.transform.position);
            if (d < bestDist)
            {
                bestDist = d;
                best = item;
            }
        }

        nearbyItem = best;
        ShowInteractionPrompt(nearbyItem != null);
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

        if (interactionPromptText != null)
        {
            // If user only assigns the TMP text, toggle it directly too.
            if (interactionPromptText.gameObject != null)
                interactionPromptText.gameObject.SetActive(show);

            if (show && nearbyItem != null && nearbyItem.itemData != null)
                interactionPromptText.text = $"Press F - {nearbyItem.itemData.itemName}";
            else
                interactionPromptText.text = "Press F";
        }
    }
    
    // Visualize the interaction range inside the editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}

