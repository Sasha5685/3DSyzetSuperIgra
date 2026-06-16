using UnityEngine;
using System.Collections;

public class DrawerController : MonoBehaviour, Entety, IInteractable
{
    [Header("Настройки ящика")]
    [SerializeField] private Transform drawerTransform;
    [SerializeField] private Vector3 openOffset = new Vector3(0, 0, 0.5f);
    [SerializeField] private float animationSpeed = 2f;

    [Header("Тип открывания")]
    [SerializeField] private bool useRotation = false;          // true = поворот, false = движение
    [SerializeField] private Vector3 openRotation = new Vector3(0, -90f, 0); // Угол поворота
    [SerializeField] private Vector3 closedRotation = Vector3.zero;          // Начальный угол

    [Header("Outline")]
    [SerializeField] private float defaultOutlineWidth = 0.15f;
    [SerializeField] private Color defaultColor = new Color(1f, 1f, 1f, 0.5f);
    [SerializeField] private float highlightOutlineWidth = 0.6f;
    [SerializeField] private Color highlightColor = Color.yellow;
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float pulseAmplitude = 0.2f;

    [Header("Sound")]
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip closeSound;
    [SerializeField] private float soundVolume = 0.7f;

    private Outline outlineComponent;
    private Renderer[] cachedRenderers;
    private bool outlineUsable;

    private Vector3 closedPosition;
    private Vector3 openPosition;
    private Quaternion closedRotationQuat;
    private Quaternion openRotationQuat;
    private bool isHighlighted;
    private bool isAnimating;
    private bool isOpened;
    private AudioSource audioSource;
    private float pulseTimer;
    private Coroutine pulseCoroutine;
    [SerializeField] private Renderer[] outlineRenderers;

    private void CacheRenderers()
    {
        if (outlineRenderers != null && outlineRenderers.Length > 0)
        {
            cachedRenderers = outlineRenderers;
        }
        else
        {
            cachedRenderers = GetComponentsInChildren<Renderer>(true);
        }
        outlineUsable = cachedRenderers != null && cachedRenderers.Length > 0;
    }

    private void Awake()
    {
        outlineComponent = GetComponent<Outline>();
        if (outlineComponent == null)
            outlineComponent = gameObject.AddComponent<Outline>();

        outlineComponent.OutlineColor = defaultColor;
        outlineComponent.OutlineWidth = defaultOutlineWidth;
        outlineComponent.enabled = false;

        CacheRenderers();

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.volume = soundVolume;

        if (drawerTransform != null)
        {
            closedPosition = drawerTransform.localPosition;
            openPosition = closedPosition + openOffset;
            closedRotationQuat = Quaternion.Euler(closedRotation);
            openRotationQuat = Quaternion.Euler(openRotation);
        }
    }

    private bool RenderersAlive()
    {
        if (!outlineUsable) return false;
        for (int i = 0; i < cachedRenderers.Length; i++)
        {
            if (cachedRenderers[i] == null)
            {
                outlineUsable = false;
                return false;
            }
        }
        return true;
    }

    public void Pointing()
    {
        if (this == null || outlineComponent == null) return;
        if (!RenderersAlive()) return;

        isHighlighted = true;
        outlineComponent.enabled = true;
        outlineComponent.OutlineWidth = highlightOutlineWidth;
        outlineComponent.OutlineColor = highlightColor;

        if (pulseCoroutine != null)
            StopCoroutine(pulseCoroutine);
        pulseCoroutine = StartCoroutine(PulseCoroutine());
    }

    public void StopPointing()
    {
        if (this == null || outlineComponent == null) return;

        isHighlighted = false;
        pulseTimer = 0f;

        if (RenderersAlive())
        {
            outlineComponent.enabled = false;
            outlineComponent.OutlineWidth = defaultOutlineWidth;
            outlineComponent.OutlineColor = defaultColor;
        }

        if (pulseCoroutine != null)
        {
            StopCoroutine(pulseCoroutine);
            pulseCoroutine = null;
        }
    }

    private IEnumerator PulseCoroutine()
    {
        while (isHighlighted && outlineComponent != null && outlineComponent.enabled)
        {
            pulseTimer += Time.deltaTime * pulseSpeed;
            outlineComponent.OutlineWidth = highlightOutlineWidth + Mathf.Sin(pulseTimer) * pulseAmplitude;
            yield return null;
        }
        pulseCoroutine = null;
    }

    public void UseblePointing() => Pointing();

    public BaseItem ReturnItem() => null;

    public void Interact()
    {
        if (isAnimating) return;
        if (!isOpened) OpenDrawer();
        else CloseDrawer();
    }

    private void OpenDrawer()
    {
        isAnimating = true;
        isOpened = true;
        PlaySound(openSound);

        if (useRotation)
        {
            StartCoroutine(AnimateDrawerWithRotation(
                closedPosition, openPosition,
                closedRotationQuat, openRotationQuat
            ));
        }
        else
        {
            StartCoroutine(AnimateDrawer(closedPosition, openPosition));
        }
    }

    private void CloseDrawer()
    {
        isAnimating = true;
        isOpened = false;
        PlaySound(closeSound);

        if (useRotation)
        {
            StartCoroutine(AnimateDrawerWithRotation(
                openPosition, closedPosition,
                openRotationQuat, closedRotationQuat
            ));
        }
        else
        {
            StartCoroutine(AnimateDrawer(openPosition, closedPosition));
        }
    }

    private IEnumerator AnimateDrawer(Vector3 from, Vector3 to)
    {
        float duration = 1f / animationSpeed;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);

            if (drawerTransform != null)
                drawerTransform.localPosition = Vector3.LerpUnclamped(from, to, t);

            yield return null;
        }

        if (drawerTransform != null)
            drawerTransform.localPosition = to;

        isAnimating = false;
    }

    private IEnumerator AnimateDrawerWithRotation(Vector3 fromPos, Vector3 toPos, Quaternion fromRot, Quaternion toRot)
    {
        float duration = 1f / animationSpeed;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);

            if (drawerTransform != null)
            {
                drawerTransform.localPosition = Vector3.LerpUnclamped(fromPos, toPos, t);
                drawerTransform.localRotation = Quaternion.Slerp(fromRot, toRot, t);
            }

            yield return null;
        }

        if (drawerTransform != null)
        {
            drawerTransform.localPosition = toPos;
            drawerTransform.localRotation = toRot;
        }

        isAnimating = false;
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
            audioSource.PlayOneShot(clip);
    }

    public void ToggleDrawer()
    {
        if (isOpened) CloseDrawer();
        else OpenDrawer();
    }

    private void OnDestroy()
    {
        if (pulseCoroutine != null)
            StopCoroutine(pulseCoroutine);
    }
}