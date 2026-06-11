using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public Slot[] SlotsInventory;
    public void AddItem(Item NewItem)
    {
        for (int i = 0; i < SlotsInventory.Length; i++)
        {
            if(SlotsInventory[i].IsEmpety() == true)
            {
                SlotsInventory[i].SetItem(NewItem);
                return;
            }
        }
    }
}
