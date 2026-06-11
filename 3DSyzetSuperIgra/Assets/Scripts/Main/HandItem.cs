using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandItem : MonoBehaviour
{
    [Header("Camera Settings")]
    public Camera handCamera; // Специальная камера для предметов в руках
    
    [Header("Item Position Settings")]
    public Transform itemHolder; // Пустой объект, куда будет помещаться модель
    public Vector3 defaultPosition = new Vector3(0.5f, -0.3f, 0.8f);
    public Vector3 defaultRotation = new Vector3(10f, -20f, 5f);
    public Vector3 defaultScale = Vector3.one;
    
    private GameObject currentItemModel;
    private Item currentItem;
    
    void Start()
    {
        // Настройка камеры если нужно
        if (handCamera != null)
        {
            handCamera.enabled = true;
            handCamera.depth = 1; // Чтобы отображалась поверх основной камеры
        }
        
        // Создаем holder если его нет
        if (itemHolder == null)
        {
            itemHolder = new GameObject("ItemHolder").transform;
            itemHolder.SetParent(handCamera.transform);
            itemHolder.localPosition = defaultPosition;
            itemHolder.localRotation = Quaternion.Euler(defaultRotation);
        }
    }
    
    public void ShowItem(Item item)
    {
        // Удаляем старый предмет
        ClearCurrentItem();
        
        if (item == null || item.itemModel == null)
        {
            Debug.Log("Нет предмета или модели для отображения");
            return;
        }
        
        currentItem = item;
        
        // Создаем новую модель
        currentItemModel = Instantiate(item.itemModel, itemHolder);
        currentItemModel.transform.localPosition = Vector3.zero;
        currentItemModel.transform.localRotation = Quaternion.identity;
        currentItemModel.transform.localScale = defaultScale;
        
        // Настройка слоев для корректного отображения через камеру

        
        Debug.Log($"Показана модель предмета: {item.itemName.GetString("Russian")}");
    }
    
    public void ClearCurrentItem()
    {
        if (currentItemModel != null)
        {
            Destroy(currentItemModel);
            currentItemModel = null;
        }
        currentItem = null;
    }
    
    public Item GetCurrentItem()
    {
        return currentItem;
    }
}