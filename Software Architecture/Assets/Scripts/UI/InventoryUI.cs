using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class InventoryUI : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject inventoryPanel;
    
    [Header("Inventory Grid")]
    public Transform slotsParent;
    public GameObject slotPrefab;
    private List<InventorySlot> slots = new List<InventorySlot>();
    
    [Header("Item Details Panel")]
    public GameObject itemDetailsPanel;
    public Image itemDetailIcon;
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI itemDescriptionText;
    
    [Header("Capacity Display")]
    public TextMeshProUGUI capacityText;
    
    [Header("Buttons")]
    public Button useButton;
    public Button dropButton;
    
    [Header("Category Buttons")]
    public Button allButton;
    public Button potionsButton;
    public Button materialsButton;
    public Button foodButton;
    public Button weaponsButton;
    public Button keysButton;
    
    private InventorySlot selectedSlot;
    private ItemCategory currentCategory = ItemCategory.All;
    
    void Start()
    {
        // Cria os slots
        CreateSlots();
        
        // Oculta o inventário no início
        inventoryPanel.SetActive(false);
        itemDetailsPanel.SetActive(false);
        
        // Configura os botões
        if (useButton != null)
            useButton.onClick.AddListener(UseSelectedItem);
        
        if (dropButton != null)
            dropButton.onClick.AddListener(DropSelectedItem);
        
        // Configura botões de categoria
        if (allButton != null)
            allButton.onClick.AddListener(() => FilterByCategory(ItemCategory.All));
        if (potionsButton != null)
            potionsButton.onClick.AddListener(() => FilterByCategory(ItemCategory.Potions));
        if (materialsButton != null)
            materialsButton.onClick.AddListener(() => FilterByCategory(ItemCategory.Materials));
        if (foodButton != null)
            foodButton.onClick.AddListener(() => FilterByCategory(ItemCategory.Food));
        if (weaponsButton != null)
            weaponsButton.onClick.AddListener(() => FilterByCategory(ItemCategory.Weapons));
        if (keysButton != null)
            keysButton.onClick.AddListener(() => FilterByCategory(ItemCategory.Keys));
    }
    
    void Update()
    {
        // Abre/fecha o inventário com a tecla E
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            ToggleInventory();
        }
    }
    
    void CreateSlots()
    {
        int maxSlots = InventoryManager.Instance.maxSlots;
        
        for (int i = 0; i < maxSlots; i++)
        {
            GameObject slotObj = Instantiate(slotPrefab, slotsParent);
            InventorySlot slot = slotObj.GetComponent<InventorySlot>();
            slots.Add(slot);
        }
    }
    
    public void ToggleInventory()
    {
        bool isActive = !inventoryPanel.activeSelf;
        inventoryPanel.SetActive(isActive);
        
        if (isActive)
        {
            RefreshInventory();
            // Pausa o jogo ou desabilita o movimento do jogador
            Time.timeScale = 0f;
        }
        else
        {
            // Despausa o jogo
            Time.timeScale = 1f;
            itemDetailsPanel.SetActive(false);
        }
    }
    
    public void RefreshInventory()
    {
        // Limpa todos os slots
        foreach (InventorySlot slot in slots)
        {
            slot.ClearSlot();
        }
        
        // Pega os itens filtrados
        List<InventoryItem> itemsToShow = InventoryManager.Instance.GetItemsByCategory(currentCategory);
        
        // Preenche os slots com os itens
        for (int i = 0; i < itemsToShow.Count && i < slots.Count; i++)
        {
            slots[i].SetItem(itemsToShow[i]);
        }
        
        // Atualiza a capacidade
        UpdateCapacityDisplay();
    }
    
    void UpdateCapacityDisplay()
    {
        if (capacityText != null)
        {
            capacityText.text = $"Capacity {InventoryManager.Instance.currentCapacity}/{InventoryManager.Instance.maxCapacity}";
        }
    }
    
    public void SelectItem(InventorySlot slot)
    {
        Debug.Log("SelectItem chamado!");
        
        // Desmarca o slot anterior
        if (selectedSlot != null)
        {
            selectedSlot.SetSelected(false);
        }
        
        // Marca o novo slot
        selectedSlot = slot;
        selectedSlot.SetSelected(true);
        
        // Mostra os detalhes do item
        ShowItemDetails(slot.item);
    }
    
    void ShowItemDetails(InventoryItem item)
    {
        if (item == null || item.itemData == null)
        {
            Debug.Log("Item ou ItemData é null, ocultando painel de detalhes");
            itemDetailsPanel.SetActive(false);
            
            if (itemNameText != null)
            {
                itemNameText.text = string.Empty;
                itemNameText.gameObject.SetActive(false);
            }
            
            return;
        }
        
        Debug.Log($"Mostrando detalhes do item: {item.itemData.itemName}");
        itemDetailsPanel.SetActive(true);
        
        if (itemDetailIcon != null)
            itemDetailIcon.sprite = item.itemData.icon;
        
        if (itemNameText != null)
        {
            if (!itemNameText.gameObject.activeSelf)
                itemNameText.gameObject.SetActive(true);
            
            itemNameText.text = item.itemData.itemName;
        }
        
        if (itemDescriptionText != null)
            itemDescriptionText.text = item.itemData.description;
    }
    
    void UseSelectedItem()
    {
        if (selectedSlot != null && selectedSlot.item != null)
        {
            InventoryManager.Instance.UseItem(selectedSlot.item);
            RefreshInventory();
            itemDetailsPanel.SetActive(false);
            selectedSlot = null;
        }
    }
    
    void DropSelectedItem()
    {
        if (selectedSlot != null && selectedSlot.item != null)
        {
            InventoryManager.Instance.RemoveItem(selectedSlot.item.itemData, 1);
            RefreshInventory();
            itemDetailsPanel.SetActive(false);
            selectedSlot = null;
        }
    }
    
    void FilterByCategory(ItemCategory category)
    {
        currentCategory = category;
        RefreshInventory();
    }
}

