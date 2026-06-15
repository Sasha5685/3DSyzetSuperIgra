using UnityEngine;

public class HandItem : MonoBehaviour
{
    public static HandItem instatiate;
    [Header("Camera Settings")]
    public Camera handCamera; 
    
    [Header("Item Position Settings")]
    public Transform itemHolder; 
    public Vector3 defaultPosition = new Vector3(0.5f, -0.3f, 0.8f);
    public Vector3 defaultRotation = new Vector3(10f, -20f, 5f);
    public Vector3 defaultScale = Vector3.one;
    
    private GameObject currentItemModel;
    private BaseItem  currentItem;
    
    private void Start()
    {
        instatiate = this;
        handCamera.enabled = true;
        handCamera.depth = 1;
    }
    
    public void ShowItem(BaseItem item)
    {
        ClearCurrentItem();
        if (item == null || item.itemModel == null){return;}
        currentItem = item;
        
        currentItemModel = Instantiate(item.itemModel, itemHolder);
        currentItemModel.transform.localPosition = defaultPosition;  // ИЗМЕНИТЬ: было Vector3.zero
        currentItemModel.transform.localRotation = Quaternion.Euler(defaultRotation);  // ИЗМЕНИТЬ: было Quaternion.identity
        currentItemModel.transform.localScale = defaultScale;
    }
    
    public void ClearCurrentItem()
    {
        if (currentItemModel != null){Destroy(currentItemModel);currentItemModel = null;}
        currentItem = null;
    }
    
    public BaseItem  GetCurrentItem()
    {
        return currentItem;
    }
}