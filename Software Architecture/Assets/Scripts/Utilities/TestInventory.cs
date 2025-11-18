using UnityEngine;

public class TestInventory : MonoBehaviour
{
    [Header("Items de Teste")]
    public ItemData[] testItems;
    
    void Start()
    {
        // Adiciona alguns itens de teste ao iniciar
        if (testItems != null && testItems.Length > 0)
        {
            foreach (ItemData item in testItems)
            {
                if (item != null)
                {
                    InventoryManager.Instance.AddItem(item, Random.Range(1, 5));
                }
            }
        }
    }
    
    void Update()
    {
        // Tecla T para adicionar item aleatório
        if (UnityEngine.InputSystem.Keyboard.current.tKey.wasPressedThisFrame)
        {
            if (testItems != null && testItems.Length > 0)
            {
                ItemData randomItem = testItems[Random.Range(0, testItems.Length)];
                if (randomItem != null)
                {
                    InventoryManager.Instance.AddItem(randomItem, 1);
                    Debug.Log($"Adicionado: {randomItem.itemName}");
                }
            }
        }
    }
}

