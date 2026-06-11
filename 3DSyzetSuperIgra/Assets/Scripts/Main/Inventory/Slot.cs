using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Slot : MonoBehaviour
{
    public int IdSlot;
    public Image UISlot;
    public TextMeshProUGUI UIName;
    public Item Item;
    public void SetItem(Item newItem)
    {
        Item = newItem;
        UISlot.sprite = Item.itemIcon;
        UIName.text = Item.itemName.GetString("Russian");
    }
    public bool IsEmpety()
    {
        if(Item == null){return true;}
        else{return false;}
    }
}
