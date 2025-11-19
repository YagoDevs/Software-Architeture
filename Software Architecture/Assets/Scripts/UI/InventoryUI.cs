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
        // Create the slots
        CreateSlots();
        
        // Hide the inventory at startup
        inventoryPanel.SetActive(false);
        itemDetailsPanel.SetActive(false);
        
        // Configure action buttons
        if (useButton != null)
            useButton.onClick.AddListener(UseSelectedItem);
        
        if (dropButton != null)
            dropButton.onClick.AddListener(DropSelectedItem);
        
        // Configure category buttons
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
        // Toggle the inventory with the E key
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
            // Pause the game or disable player movement
            Time.timeScale = 0f;
        }
        else
        {
            // Resume the game
            Time.timeScale = 1f;
            itemDetailsPanel.SetActive(false);
        }
    }
    
    public void RefreshInventory()
    {
        // Clear every slot
        foreach (InventorySlot slot in slots)
        {
            slot.ClearSlot();
        }
        
        // Fetch the filtered items
        List<InventoryItem> itemsToShow = InventoryManager.Instance.GetItemsByCategory(currentCategory);
        
        // Fill the slots with the items
        for (int i = 0; i < itemsToShow.Count && i < slots.Count; i++)
        {
            slots[i].SetItem(itemsToShow[i]);
        }
        
        // Update the capacity display
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
        Debug.Log("SelectItem called!");
        
        // Deselect the previously selected slot
        if (selectedSlot != null)
        {
            selectedSlot.SetSelected(false);
        }
        
        // Highlight the newly selected slot
        selectedSlot = slot;
        selectedSlot.SetSelected(true);
        
        // Display the item details
        ShowItemDetails(slot.item);
    }
    
    void ShowItemDetails(InventoryItem item)
    {
        if (item == null || item.itemData == null)
        {
            Debug.Log("Item or ItemData is null, hiding item details panel");
            itemDetailsPanel.SetActive(false);
            
            if (itemNameText != null)
            {
                itemNameText.text = string.Empty;
                itemNameText.gameObject.SetActive(false);
            }
            
            return;
        }
        
        Debug.Log($"Showing item details: {item.itemData.itemName}");
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

