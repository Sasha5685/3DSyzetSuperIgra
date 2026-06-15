using System.Runtime.Serialization.Formatters;
using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    [Header("Настройки рейкаста")]
    [SerializeField] private float pickupRadius = 2f;
    [SerializeField] private float pickupAngle = 360f;
    [SerializeField] private LayerMask itemLayer;
    [SerializeField] private Transform playerCamera;
    
    [Header("Настройки поднятия")]
    [SerializeField] private KeyCode pickupKey = KeyCode.E;
    [SerializeField] private float pickupDistance = 3f;
    
    [Header("Highlight Settings")]
    private float highlightCheckInterval = 0.15f;
    
    private GameObject currentItemInRange;
    private Entety currentPickupable;
    [SerializeField] private Inventory inventory;
    
    private float lastCheckTime;
    
    private void Update()
    {
        float currentTime = Time.time;
        if (currentTime - lastCheckTime > highlightCheckInterval)
        {
            lastCheckTime = currentTime;
            FindObjectsInRange();
        }
        
        if (Input.GetKeyDown(pickupKey) && currentItemInRange)
        {
            InteractWithObject();
        }
    }
    
    private void FindObjectsInRange()
    {
        Ray ray = new Ray(playerCamera.position, playerCamera.forward);
        
        GameObject targetObject = null;
        Entety targetEntety = null;
        
        if (Physics.Raycast(ray, out RaycastHit hit, pickupDistance, itemLayer))
        {
            targetObject = hit.collider.gameObject;
            targetEntety = hit.collider.GetComponent<Entety>();
        }
        
        UpdateHighlight(targetObject, targetEntety);
    }
    
    private void UpdateHighlight(GameObject newObject, Entety newEntety)
    {
        if (currentItemInRange == newObject) return;
        
        if (currentPickupable != null && currentPickupable != null)
        {
            currentPickupable.StopPointing();
        }
        currentItemInRange = newObject;
        currentPickupable = newEntety;
        currentPickupable?.Pointing();
    }
    
    // Добавьте этот метод в класс ItemPickup
    private void InteractWithObject()
    {
        if (!currentItemInRange) return;

        // Проверяем, является ли объект машиной
        CarController car = currentItemInRange.GetComponent<CarController>();
        if (car != null)
        {
            car.Interact();
            return;
        }

        WoodPlanks woodPlanks = currentItemInRange.GetComponent<WoodPlanks>();
        if (woodPlanks != null)
        {
            woodPlanks.Interact();
            return;
        }
        
        DoorController door = currentItemInRange.GetComponent<DoorController>();
        if (door != null)
        {
            door.Interact();
            return;
        }
        
        Person Person = currentItemInRange.GetComponent<Person>();
        if (Person != null)
        {
            Person.Interact();
            return;
        }


        // Поднятие предмета
        Entety entety = currentPickupable;
        if (entety != null)
        {
            BaseItem item = entety.ReturnItem();
            if (item != null)
            {
                inventory.AddItem(item);
                Destroy(currentItemInRange);
                currentItemInRange = null;
                currentPickupable = null;
            }
        }
    }
}