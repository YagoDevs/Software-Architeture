using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    [Header("Item Configuration")]
    public ItemData itemData;
    public int quantity = 1;
    
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
        // Rotação suave
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        
        // Movimento para cima e para baixo (bobbing)
        float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
    
    void OnTriggerEnter(Collider other)
    {
        // Verifica se é o player
        if (other.CompareTag("Player"))
        {
            // Tenta adicionar ao inventário
            bool added = InventoryManager.Instance.AddItem(itemData, quantity);
            
            if (added)
            {
                Debug.Log($"Coletado: {itemData.itemName} x{quantity}");
                Destroy(gameObject);
            }
            else
            {
                Debug.Log("Inventário cheio!");
            }
        }
    }
}

