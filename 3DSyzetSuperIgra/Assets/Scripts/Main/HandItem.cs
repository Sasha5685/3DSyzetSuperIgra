using UnityEngine;

public class HandItem : MonoBehaviour
{
    [Header("Camera Settings")]
    public Camera handCamera; 
    
    [Header("Item Position Settings")]
    public Transform itemHolder; 
    public Vector3 defaultPosition = new Vector3(0.5f, -0.3f, 0.8f);
    public Vector3 defaultRotation = new Vector3(10f, -20f, 5f);
    public Vector3 defaultScale = Vector3.one;
    
    private GameObject currentItemModel;
    private Item currentItem;
    
    private void Start()
    {
        handCamera.enabled = true;
        handCamera.depth = 1;
    }
    
    public void ShowItem(Item item)
    {
        ClearCurrentItem();
        if (item == null || item.itemModel == null){return;}
        currentItem = item;
        
        // Создаем новую модель
        currentItemModel = Instantiate(item.itemModel, itemHolder);
        currentItemModel.transform.localPosition = Vector3.zero;
        currentItemModel.transform.localRotation = Quaternion.identity;
        currentItemModel.transform.localScale = defaultScale;
    }
    
    public void ClearCurrentItem()
    {
        if (currentItemModel != null){Destroy(currentItemModel);currentItemModel = null;}
        currentItem = null;
    }
    
    public Item GetCurrentItem()
    {
        return currentItem;
    }
}