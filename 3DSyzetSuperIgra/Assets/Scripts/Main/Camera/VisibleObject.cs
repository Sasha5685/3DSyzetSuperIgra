using UnityEngine;

public class VisibleObject : MonoBehaviour
{
    [SerializeField] private bool includeChildren = true;
    [SerializeField] private bool extendedOutlineRange = false; // ← НОВАЯ ГАЛОЧКА
    [SerializeField] private float extendedRangeMultiplier = 2f; // ← НА СКОЛЬКО ДАЛЬШЕ

    private Renderer[] renderers;
    private bool hidden;
    private Outline outline;
    private bool ignoreManager = false;
    
    public void SetIgnoreManager(bool ignore)
    {
        ignoreManager = ignore;
    }
    
    private void Awake()
    {
        outline = GetComponent<Outline>();
        renderers = includeChildren
            ? GetComponentsInChildren<Renderer>(true)
            : GetComponents<Renderer>();
    }
    
    private void Start()
    {
        VisibleManager manager = FindObjectOfType<VisibleManager>();

        if (manager != null)
            manager.RegisterObject(this);
    }

    private void OnDestroy()
    {
        VisibleManager manager = FindObjectOfType<VisibleManager>();

        if (manager != null)
            manager.UnregisterObject(this);
    }
    
    public void UpdateVisibility(float sqrDistance,
                                 float outlineDistanceSqr,
                                 float hideDistanceSqr)
    {
        if (ignoreManager) return;
        
        // ↓↓↓ ДОБАВЬТЕ ЭТОТ БЛОК ↓↓↓
        // Если у объекта увеличенная дальность — умножаем дистанции
        float currentOutlineDistance = outlineDistanceSqr;
        float currentHideDistance = hideDistanceSqr;
        
        if (extendedOutlineRange)
        {
            currentOutlineDistance *= extendedRangeMultiplier;
            currentHideDistance *= extendedRangeMultiplier;
        }
        // ↑↑↑ КОНЕЦ БЛОКА ↑↑↑
        
        if (sqrDistance > currentHideDistance)
        {
            SetVisible(false);

            if (outline != null)
                outline.enabled = false;
        }
        else if (sqrDistance > currentOutlineDistance)
        {
            SetVisible(true);

            if (outline != null)
                outline.enabled = false;
        }
        else
        {
            SetVisible(true);

            if (outline != null)
                outline.enabled = true;
        }
    }

    private void SetVisible(bool visible)
    {
        hidden = !visible;

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
                renderers[i].enabled = visible;
        }
    }
}