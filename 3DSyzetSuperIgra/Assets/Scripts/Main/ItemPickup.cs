using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    [SerializeField] private LayerMask itemLayer;
    [SerializeField] private Transform playerCamera;
    [SerializeField] private KeyCode pickupKey = KeyCode.E;
    [SerializeField] private float pickupDistance = 3f;
    [SerializeField] private Inventory inventory;

    private const float HighlightCheckInterval = 0.15f;
    private float lastCheckTime;
    private GameObject currentItemInRange;
    private Entety currentPickupable;
    private IInteractable currentInteractable; // см. ниже

    private void Update()
    {
        if (Time.time - lastCheckTime > HighlightCheckInterval)
        {
            lastCheckTime = Time.time;
            FindObjectsInRange();
        }

        if (currentItemInRange != null && Input.GetKeyDown(pickupKey))
        {
            InteractWithObject();
        }
    }

    [SerializeField] private LayerMask obstacleLayer; // стены, пол и т.п.

    private LayerMask combinedMask; // кэшируем один раз

    private void Awake()
    {
        combinedMask = itemLayer | obstacleLayer;
    }

private void FindObjectsInRange()
{
    if (Physics.Raycast(playerCamera.position, playerCamera.forward,
            out RaycastHit hit, pickupDistance, combinedMask, QueryTriggerInteraction.Ignore))
    {
        bool isItem = (itemLayer.value & (1 << hit.collider.gameObject.layer)) != 0;
        UpdateHighlight(isItem ? hit.collider.gameObject : null);
    }
    else
    {
        UpdateHighlight(null);
    }
}
    private void UpdateHighlight(GameObject newObject)
    {
        if (currentItemInRange == newObject) return;

        if (currentPickupable != null && (object)currentPickupable as Object != null)
            currentPickupable.StopPointing();

        currentItemInRange = newObject;

        if (newObject != null)
        {
            currentPickupable = newObject.GetComponent<Entety>();
            currentInteractable = newObject.GetComponent<IInteractable>();
        }
        else
        {
            currentPickupable = null;
            currentInteractable = null;
        }

        currentPickupable?.Pointing();
    }

    private void InteractWithObject()
    {
        if (currentInteractable != null)
        {
            currentInteractable.Interact();
            return;
        }

        if (currentPickupable != null)
        {
            BaseItem item = currentPickupable.ReturnItem();
            if (item != null)
            {
                inventory.AddItem(item);
                GameObject toDestroy = currentItemInRange;
                currentItemInRange = null;
                currentPickupable = null;
                currentInteractable = null;
                Destroy(toDestroy);
            }
        }
    }
}