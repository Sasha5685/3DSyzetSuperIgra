using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    [Header("Raycast Settings")]
    [SerializeField] private LayerMask itemLayer;
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private Transform playerCamera;
    [SerializeField] private float pickupDistance = 3f;
    
    [Header("Input Settings")]
    [SerializeField] private KeyCode pickupKey = KeyCode.E;
    
    [Header("References")]
    [SerializeField] private Inventory inventory;
    [SerializeField] private GameObject takeObjectPanel;
    [SerializeField] private GameObject useObjectPanel;
    
    [Header("Performance")]
    [SerializeField] private float highlightCheckInterval = 0.1f;
    
    private float lastCheckTime;
    private GameObject currentItemInRange;
    private Entety currentPickupable;
    private IInteractable currentInteractable;
    private LayerMask combinedMask;
    private Camera mainCamera;
    
    // Кэшируем результат ReturnItem()
    private BaseItem cachedItem;
    private bool isItemCached;
    
    private void Awake()
    {
        combinedMask = itemLayer | obstacleLayer;
        mainCamera = Camera.main;
        
        if (mainCamera != null)
        {
            playerCamera = mainCamera.transform;
        }
        
        if (inventory == null)
        {
            inventory = FindObjectOfType<Inventory>();
        }
    }
    
    private void Update()
    {
        if (Time.time - lastCheckTime >= highlightCheckInterval)
        {
            lastCheckTime = Time.time;
            FindObjectsInRange();
        }
        
        if (currentItemInRange != null && Input.GetKeyDown(pickupKey))
        {
            InteractWithObject();
        }
    }
    
    private void FindObjectsInRange()
    {
        if (playerCamera == null) return;
        
        RaycastHit hit;
        bool hasHit = Physics.Raycast(
            playerCamera.position,
            playerCamera.forward,
            out hit,
            pickupDistance,
            combinedMask,
            QueryTriggerInteraction.Ignore
        );
        
        if (hasHit)
        {
            GameObject hitObject = hit.collider.gameObject;
            bool isItem = (itemLayer.value & (1 << hitObject.layer)) != 0;
            UpdateHighlight(isItem ? hitObject : null);
        }
        else
        {
            UpdateHighlight(null);
        }
    }
    
    private void UpdateHighlight(GameObject newObject)
    {
        if (currentItemInRange == newObject) return;
        
        // Сбрасываем предыдущий объект
        if (currentPickupable != null)
        {
            currentPickupable.StopPointing();
            cachedItem = null;
            isItemCached = false;
            HidePanels();
        }
        
        // Обновляем ссылки
        currentItemInRange = newObject;
        currentPickupable = null;
        currentInteractable = null;
        cachedItem = null;
        isItemCached = false;
        
        // Если новый объект существует - получаем компоненты
        if (newObject != null)
        {
            currentPickupable = newObject.GetComponent<Entety>();
            currentInteractable = newObject.GetComponent<IInteractable>();
            
            // Если есть Entety - кэшируем предмет
            if (currentPickupable != null)
            {
                cachedItem = currentPickupable.ReturnItem();
                isItemCached = true;
                currentPickupable.Pointing();
            }
        }
        
        // Показываем соответствующую панель
        ShowCorrectPanel();
    }
    
    private void ShowCorrectPanel()
    {
        // Проверяем наличие IInteractable (для дверей и других интерактивных объектов)
        if (currentInteractable != null)
        {
            // Показываем панель использования
            ShowPanel(useObjectPanel);
            return;
        }
        
        // Проверяем наличие предмета для подбора
        if (currentPickupable != null && isItemCached && cachedItem != null)
        {
            if (cachedItem.IsItem)
            {
                ShowPanel(takeObjectPanel);
            }
            else
            {
                ShowPanel(useObjectPanel);
            }
        }
        else
        {
            HidePanels();
        }
    }
    
    private void ShowPanel(GameObject panel)
    {
        if (takeObjectPanel != null) takeObjectPanel.SetActive(false);
        if (useObjectPanel != null) useObjectPanel.SetActive(false);
        if (panel != null) panel.SetActive(true);
    }
    
    private void HidePanels()
    {
        if (takeObjectPanel != null) takeObjectPanel.SetActive(false);
        if (useObjectPanel != null) useObjectPanel.SetActive(false);
    }
    
    private void InteractWithObject()
    {
        // Проверяем интеракцию (для дверей и других объектов)
        if (currentInteractable != null)
        {
            currentInteractable.Interact();
            // После взаимодействия можно скрыть панель или оставить
            // HidePanels(); // Раскомментируйте если нужно скрывать после взаимодействия
            return;
        }
        
        // Проверяем подбор предмета
        if (currentPickupable != null && isItemCached && cachedItem != null)
        {
            // Проверяем что это предмет (IsItem == true)
            if (!cachedItem.IsItem)
            {
                return;
            }
            
            // Добавляем в инвентарь
            if (inventory != null)
            {
                inventory.AddItem(cachedItem);
            }
            else
            {
                Debug.LogWarning("Inventory не назначен в ItemPickup");
                return;
            }
            
            // Скрываем панели
            HidePanels();
            
            // Сохраняем объект для уничтожения
            GameObject toDestroy = currentItemInRange;
            
            // Очищаем ссылки
            currentItemInRange = null;
            currentPickupable = null;
            currentInteractable = null;
            cachedItem = null;
            isItemCached = false;
            
            // Уничтожаем объект
            if (toDestroy != null)
            {
                Destroy(toDestroy);
            }
        }
    }
    
    public void TakeObject()
    {
        if (currentItemInRange != null)
        {
            InteractWithObject();
        }
    }
    
    private void OnDisable()
    {
        if (currentPickupable != null)
        {
            currentPickupable.StopPointing();
        }
        HidePanels();
        currentItemInRange = null;
        currentPickupable = null;
        currentInteractable = null;
        cachedItem = null;
        isItemCached = false;
    }
}