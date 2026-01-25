/*
This script is used to represent a pickup in the world (bob + rotate) and optionally auto-collect it when the player enters a trigger.
*/

using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    [Header("Item Configuration")]
    public ItemData itemData;
    public int quantity = 1;
    public bool autoPickupOnTrigger = false;
    
    [Header("Visual")]
    public float rotationSpeed = 50f;
    public float bobSpeed = 2f;
    public float bobHeight = 0.3f;
    
    private Vector3 startPosition;
    
    void Start()
    {
        startPosition = transform.position;
    }
    
    void Update()
    {
        // Smooth rotation
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        
        // Vertical bobbing motion
        float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (!autoPickupOnTrigger) return;
        // Check if the collider belongs to the player
        if (other.CompareTag("Player"))
        {
            // Attempt to add the item to the inventory
            bool added = InventoryManager.Instance.AddItem(itemData, quantity);
            
            if (added)
            {
                Debug.Log($"Collected: {itemData.itemName} x{quantity}");
                Destroy(gameObject);
            }
            else
            {
                Debug.Log("Inventory full!");
            }
        }
    }

    public void Configure(ItemData data, int qty)
    {
        itemData = data;
        quantity = Mathf.Max(1, qty);
    }
}

