using UnityEngine;

public class VisibleObject : MonoBehaviour
{
    [SerializeField] private bool includeChildren = true;

    private Renderer[] renderers;
    private bool hidden;
    private Outline outline;

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
    if (sqrDistance > hideDistanceSqr)
    {
        SetVisible(false);

        if (outline != null)
            outline.enabled = false;
    }
    else if (sqrDistance > outlineDistanceSqr)
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