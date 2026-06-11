using UnityEngine;
using System.Collections;

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
    
    [Header("Визуализация")]
    [SerializeField] private bool showDebugGizmo = true;
    [SerializeField] private Color gizmoColor = Color.green;
    
    [Header("Highlight Settings")]
    [SerializeField] private float highlightCheckInterval = 0.1f;
    
    private GameObject currentItemInRange;
    private ItemPickupable currentPickupable;
    [SerializeField] private Inventory inventory;
    
    private float lastCheckTime = 0f;
    
    void Update()
    {
        // Оптимизация - не каждый кадр ищем предметы
        if (Time.time - lastCheckTime > highlightCheckInterval)
        {
            lastCheckTime = Time.time;
            FindItemsInRadius();
        }
        
        if (Input.GetKeyDown(pickupKey) && currentItemInRange != null)
        {
            PickupItem();
        }
    }
    
    void FindItemsInRadius()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, pickupRadius, itemLayer);
        
        GameObject closestItem = null;
        ItemPickupable closestPickupable = null;
        float closestDistance = pickupDistance + 1f;
        float closestAngle = pickupAngle / 2f + 1f;
        
        foreach (var collider in hitColliders)
        {
            float distanceToItem = Vector3.Distance(playerCamera.position, collider.transform.position);
            if (distanceToItem > pickupDistance)
                continue;
            
            Vector3 directionToItem = (collider.transform.position - playerCamera.position).normalized;
            float angleToItem = Vector3.Angle(playerCamera.forward, directionToItem);
            
            if (angleToItem <= pickupAngle / 2f)
            {
                if (IsVisible(collider.transform.position, collider.gameObject))
                {
                    // Приоритет: сначала по углу (ближе к центру), потом по расстоянию
                    bool isBetter = false;
                    
                    if (angleToItem < closestAngle)
                    {
                        isBetter = true;
                    }
                    else if (Mathf.Abs(angleToItem - closestAngle) < 0.1f && distanceToItem < closestDistance)
                    {
                        isBetter = true;
                    }
                    
                    if (isBetter)
                    {
                        closestDistance = distanceToItem;
                        closestAngle = angleToItem;
                        closestItem = collider.gameObject;
                        closestPickupable = collider.GetComponent<ItemPickupable>();
                    }
                }
            }
        }
        
        // Обновляем подсветку
        if (currentItemInRange != closestItem)
        {
            // Снимаем подсветку с предыдущего предмета
            if (currentPickupable != null)
            {
                currentPickupable.Highlight(false);
            }
            
            // Устанавливаем новый предмет
            currentItemInRange = closestItem;
            currentPickupable = closestPickupable;
            
            // Включаем подсветку на новом предмете
            if (currentPickupable != null)
            {
                currentPickupable.Highlight(true);
            }
        }
    }
    
    bool IsVisible(Vector3 targetPosition, GameObject targetObject)
    {
        Vector3 direction = targetPosition - playerCamera.position;
        RaycastHit hit;
        
        if (Physics.Raycast(playerCamera.position, direction, out hit, pickupDistance))
        {
            return hit.collider.gameObject == targetObject;
        }
        return false;
    }
    
    void PickupItem()
    {
        if (currentItemInRange != null)
        {
            ItemPickupable pickupable = currentItemInRange.GetComponent<ItemPickupable>();
            
            if (pickupable != null)
            {
                pickupable.OnPickup(gameObject);
                
                if (pickupable.itemData_1 != null)
                    inventory.AddItem(pickupable.itemData_1);
                else if (pickupable.itemData_2 != null)
                    inventory.AddItem(pickupable.itemData_2);
                else if (pickupable.itemData_3 != null)
                    inventory.AddItem(pickupable.itemData_3);
                
                Destroy(currentItemInRange);
                currentItemInRange = null;
                currentPickupable = null;
            }
            else
            {
                Debug.LogError("На предмете нет компонента ItemPickupable!");
            }
        }
    }
    
    void OnDrawGizmos()
    {
        if (!showDebugGizmo) return;
        
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, pickupRadius);
        
        if (playerCamera != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(playerCamera.position, pickupDistance);
            
            Vector3 forward = playerCamera.forward;
            Vector3 rightBoundary = Quaternion.Euler(0, pickupAngle / 2f, 0) * forward;
            Vector3 leftBoundary = Quaternion.Euler(0, -pickupAngle / 2f, 0) * forward;
            
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(playerCamera.position, rightBoundary * pickupDistance);
            Gizmos.DrawRay(playerCamera.position, leftBoundary * pickupDistance);
        }
        
        if (currentItemInRange != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(currentItemInRange.transform.position, 0.3f);
        }
    }
}