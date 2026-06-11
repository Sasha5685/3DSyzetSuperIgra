using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    [Header("Настройки рейкаста")]
    [SerializeField] private float pickupRadius = 2f;
    [SerializeField] private float pickupAngle = 360f; // Полный круг
    [SerializeField] private LayerMask itemLayer;
    [SerializeField] private Transform playerCamera;
    
    [Header("Настройки поднятия")]
    [SerializeField] private KeyCode pickupKey = KeyCode.E;
    [SerializeField] private float pickupDistance = 3f;
    
    [Header("Визуализация")]
    [SerializeField] private bool showDebugGizmo = true;
    [SerializeField] private Color gizmoColor = Color.green;
    
    private GameObject currentItemInRange;
    [SerializeField] private Inventory inventory;
    
    
    void Update()
    {
        // Круговой рейкаст для поиска предметов
        FindItemsInRadius();
        
        // Поднятие предмета по кнопке
        if (Input.GetKeyDown(pickupKey) && currentItemInRange != null)
        {
            PickupItem();
        }
    }
    
    void FindItemsInRadius()
    {
        // Получаем все коллайдеры в радиусе
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, pickupRadius, itemLayer);
        
        GameObject closestItem = null;
        float closestDistance = pickupDistance + 1f;
        
        foreach (var collider in hitColliders)
        {
            // Проверяем расстояние
            float distanceToItem = Vector3.Distance(playerCamera.position, collider.transform.position);
            if (distanceToItem > pickupDistance)
                continue;
            
            // Проверяем угол между направлением взгляда и предметом
            Vector3 directionToItem = (collider.transform.position - playerCamera.position).normalized;
            float angleToItem = Vector3.Angle(playerCamera.forward, directionToItem);
            
            // Проверяем, находится ли предмет в угле обзора
            if (angleToItem <= pickupAngle / 2f)
            {
                // Дополнительная проверка прямым лучом (нет ли препятствий)
                if (IsVisible(collider.transform.position, collider.gameObject))
                {
                    if (distanceToItem < closestDistance)
                    {
                        closestDistance = distanceToItem;
                        closestItem = collider.gameObject;
                    }
                }
            }
        }
        currentItemInRange = closestItem;
    }
    
    // ИСПРАВЛЕНО: передаем targetObject для проверки
    bool IsVisible(Vector3 targetPosition, GameObject targetObject)
    {
        Vector3 direction = targetPosition - playerCamera.position;
        RaycastHit hit;
        
        if (Physics.Raycast(playerCamera.position, direction, out hit, pickupDistance))
        {
            // Проверяем, попали ли мы в целевой предмет
            return hit.collider.gameObject == targetObject;
        }
        return false;
    }
    
    void PickupItem()
    {
        if (currentItemInRange != null)
        {
            ItemPickupable pickupable = currentItemInRange.GetComponent<ItemPickupable>();
            
            if (pickupable != null && pickupable.itemData != null)
            {
                if (inventory != null)
                {
                    inventory.AddItem(pickupable.itemData);
                    Destroy(currentItemInRange);
                    currentItemInRange = null;
                    Debug.Log($"Поднят предмет: {pickupable.itemData.name}");
                }
                else
                {
                    Debug.LogError("Inventory не найден!");
                }
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
        
        // Радиус поиска
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, pickupRadius);
        
        if (playerCamera != null)
        {
            // Радиус поднятия
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(playerCamera.position, pickupDistance);
            
            // Визуализация угла обзора
            Vector3 forward = playerCamera.forward;
            Vector3 rightBoundary = Quaternion.Euler(0, pickupAngle / 2f, 0) * forward;
            Vector3 leftBoundary = Quaternion.Euler(0, -pickupAngle / 2f, 0) * forward;
            
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(playerCamera.position, rightBoundary * pickupDistance);
            Gizmos.DrawRay(playerCamera.position, leftBoundary * pickupDistance);
        }
        
        // Визуализация текущего предмета
        if (currentItemInRange != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(currentItemInRange.transform.position, 0.3f);
        }
    }
}