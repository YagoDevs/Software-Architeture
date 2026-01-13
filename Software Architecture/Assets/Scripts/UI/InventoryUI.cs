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
    public bool debugButtonLogs = true;
    
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
        
        EnsureButtonWiring();
        
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

    void EnsureButtonWiring()
    {
        // If references were not set in Inspector, try auto-find by name in children
        if (useButton == null)
            useButton = transform.Find("UseButton")?.GetComponent<Button>() ?? GetComponentInChildren<Button>(true);

        if (dropButton == null)
            dropButton = transform.Find("DropButton")?.GetComponent<Button>();

        if (useButton != null)
        {
            useButton.onClick.RemoveListener(UseSelectedItem);
            useButton.onClick.AddListener(UseSelectedItem);
        }
        else if (debugButtonLogs)
        {
            Debug.LogWarning("InventoryUI: useButton reference is missing. Assign it in Inspector.");
        }

        if (dropButton != null)
        {
            dropButton.onClick.RemoveListener(DropSelectedItem);
            dropButton.onClick.AddListener(DropSelectedItem);
        }
        else if (debugButtonLogs)
        {
            Debug.LogWarning("InventoryUI: dropButton reference is missing. Assign it in Inspector.");
        }
    }
    
    void CreateSlots()
    {
        int maxSlots = InventoryManager.Instance.maxSlots;
        
        for (int i = 0; i < maxSlots; i++)
        {
            GameObject slotObj = Instantiate(slotPrefab, slotsParent);
            InventorySlot slot = slotObj.GetComponent<InventorySlot>();
            if (slot != null)
                slot.Initialize(this);
            slots.Add(slot);
        }
    }
    
    public void ToggleInventory()
    {
        bool isActive = !inventoryPanel.activeSelf;
        inventoryPanel.SetActive(isActive);
        
        if (isActive)
        {
            EnsureButtonWiring();
            RefreshInventory();
            // Pause the game or disable player movement
            Time.timeScale = 0f;
            SetCursorForUI(true);
        }
        else
        {
            // Resume the game
            Time.timeScale = 1f;
            itemDetailsPanel.SetActive(false);
            SetCursorForUI(false);
        }
    }

    void SetCursorForUI(bool uiOpen)
    {
        // When inventory is open, we want the cursor free.
        Cursor.lockState = uiOpen ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = uiOpen;
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
        if (debugButtonLogs)
            Debug.Log("InventoryUI: Use button clicked.");

        if (selectedSlot != null && selectedSlot.item != null)
        {
            if (debugButtonLogs)
                Debug.Log($"InventoryUI: Trying to use '{selectedSlot.item.itemData?.itemName ?? "NULL"}'");
            InventoryManager.Instance.UseItem(selectedSlot.item);
            RefreshInventory();
            itemDetailsPanel.SetActive(false);
            selectedSlot = null;
        }
        else if (debugButtonLogs)
        {
            Debug.LogWarning("InventoryUI: No selected item to use.");
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

