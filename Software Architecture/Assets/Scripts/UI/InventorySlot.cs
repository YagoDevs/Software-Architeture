using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IPointerClickHandler
{
    [Header("UI References")]
    public Image iconImage;
    public TextMeshProUGUI quantityText;
    public GameObject selectedBorder;
    
    [HideInInspector]
    public InventoryItem item;
    
    private InventoryUI inventoryUI;
    
    void Start()
    {
        inventoryUI = GetComponentInParent<InventoryUI>();
        if (selectedBorder != null)
            selectedBorder.SetActive(false);
    }
    
    public void SetItem(InventoryItem newItem)
    {
        item = newItem;
        
        if (item != null && item.itemData != null)
        {
            iconImage.sprite = item.itemData.icon;
            iconImage.enabled = true;
            
            if (item.quantity > 1)
            {
                quantityText.text = "x" + item.quantity.ToString();
                quantityText.enabled = true;
            }
            else
            {
                quantityText.enabled = false;
            }
        }
        else
        {
            ClearSlot();
        }
    }
    
    public void ClearSlot()
    {
        item = null;
        iconImage.sprite = null;
        iconImage.enabled = false;
        quantityText.enabled = false;
        
        if (selectedBorder != null)
            selectedBorder.SetActive(false);
    }
    
    public void SetSelected(bool isSelected)
    {
        if (selectedBorder != null)
            selectedBorder.SetActive(isSelected);
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Slot clicked!");
        
        if (item == null)
        {
            Debug.Log("Slot is empty!");
            return;
        }
        
        if (inventoryUI == null)
        {
            Debug.LogError("InventoryUI not found!");
            return;
        }
        
        Debug.Log($"Selecting item: {item.itemData?.itemName ?? "no name"}");
        inventoryUI.SelectItem(this);
    }
}

