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
        
        currentPickupable?.StopPointing();
        currentItemInRange = newObject;
        currentPickupable = newEntety;
        currentPickupable?.Pointing();
    }
    
    private void InteractWithObject()
    {
        if (!currentItemInRange) return;
        
        // Пытаемся получить компонент двери
        DoorController door = currentItemInRange.GetComponent<DoorController>();
        if (door != null)
        {
            door.Interact();
            return;
        }
        
        // Если не дверь, пробуем поднять предмет
        Entety entety = currentPickupable;
        if (entety != null)
        {
            Item item = entety.ReturnItem();
            if (item != null)
            {
                inventory.AddItem(item);
                
                ItemPickupable itemComp = currentItemInRange.GetComponent<ItemPickupable>();
                if (itemComp != null)
                {
                    Destroy(currentItemInRange);
                    currentItemInRange = null;
                    currentPickupable = null;
                }
                else
                {
                    Destroy(currentItemInRange);
                    currentItemInRange = null;
                    currentPickupable = null;
                }
            }
        }
    }
}