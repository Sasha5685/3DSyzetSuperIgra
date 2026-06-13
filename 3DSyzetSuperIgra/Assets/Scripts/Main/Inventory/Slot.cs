using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class Slot : MonoBehaviour, IPointerClickHandler
{
    public int IdSlot;
    public Image UISlot;
    public TextMeshProUGUI UIName;
    public BaseItem  Item;
    public GameObject Selected;
    
    void Start()
    {
        Color color = UISlot.color;
        color.a = 0f;
        UISlot.color = color;
    }
    
    public void ClearItem()
    {
        Item = null;
        
        if (UISlot != null)
        {
            UISlot.sprite = null;
            Color color = UISlot.color;
            color.a = 0f;
            UISlot.color = color;
        }
        
        if (UIName != null)
        {
            UIName.text = "";
        }
    }
    
    public void SetItem(BaseItem  newItem) // Обновите существующий метод
    {
        Item = newItem;
        
        if (newItem == null)
        {
            ClearItem();
            return;
        }
        
        if (UISlot != null && newItem.itemIcon != null)
        {
            UISlot.sprite = newItem.itemIcon;
            Color color = UISlot.color;
            color.a = 1f;
            UISlot.color = color;
        }
        
        if (UIName != null && newItem.itemName != null)
        {
            UIName.text = newItem.itemName.GetString("Russian");
        }
    }
    
    public bool IsEmpety()
    {
        if(Item == null){return true;}
        else{return false;}
    }
    
    public void SetSelected(bool isSelected)
    {
        if(Selected != null)
            Selected.SetActive(isSelected);
    }
    
    // Обработчик клика по слоту
    public void OnPointerClick(PointerEventData eventData)
    {
        // Находим Inventory и сообщаем о выборе слота
        Inventory inventory = GetComponentInParent<Inventory>();
        if (inventory != null)
        {
            inventory.SelectSlot(this);
        }
    }
}