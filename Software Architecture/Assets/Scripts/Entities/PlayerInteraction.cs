using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float interactionRange = 3f;
    
    [Header("UI Feedback")]
    public GameObject interactionPrompt; // Opcional: UI mostrando "Press F to collect"
    
    private ItemPickup nearbyItem;
    
    void Update()
    {
        // Detecta itens próximos
        DetectNearbyItems();
        
        // Pressione F para coletar
        if (Keyboard.current.fKey.wasPressedThisFrame)
        {
            Debug.Log("Tecla F pressionada!");
            
            if (nearbyItem != null)
            {
                CollectItem(nearbyItem);
            }
            else
            {
                Debug.Log("Nenhum item próximo!");
            }
        }
    }
    
    void DetectNearbyItems()
    {
        // Busca TODOS os colliders próximos (sem filtro de layer)
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
            Debug.LogError("Item é null!");
            return;
        }
        
        if (item.itemData == null)
        {
            Debug.LogError("ItemData do item é null!");
            return;
        }
        
        Debug.Log($"Tentando coletar: {item.itemData.itemName}");
        
        bool added = InventoryManager.Instance.AddItem(item.itemData, item.quantity);
        
        if (added)
        {
            Debug.Log($"✓ Coletado: {item.itemData.itemName} x{item.quantity}");
            Destroy(item.gameObject);
            nearbyItem = null;
        }
        else
        {
            Debug.Log("✗ Inventário cheio!");
        }
    }
    
    void ShowInteractionPrompt(bool show)
    {
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(show);
        }
    }
    
    // Visualizar o alcance no editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}

