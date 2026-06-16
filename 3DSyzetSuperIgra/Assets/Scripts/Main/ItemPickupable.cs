using UnityEngine;
using System;
using System.Collections;

public class ItemPickupable : MonoBehaviour, Entety
{   
    [SerializeField] private BaseItem itemData;
    
    [Header("Default Outline")]
    [SerializeField] private float defaultOutlineWidth = 0.15f;
    [SerializeField] private Color defaultColor = new Color(1f, 1f, 1f, 0.5f);
    
    [Header("Highlight Outline")]
    [SerializeField] private float highlightOutlineWidth = 0.6f;
    [SerializeField] private Color highlightColor = Color.yellow;
    [SerializeField] private float highlightAnimationSpeed = 8f;
    
    [Header("Pulse Effect")]
    [SerializeField] private bool enablePulseEffect = true;
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float pulseAmplitude = 0.2f;
    
    private Outline outlineComponent;
    private float targetWidth;
    private Color targetColor;
    private float currentWidth;
    private Color currentColor;
    private bool isHighlighted = false;
    private float pulseTimer = 0f;
    private Coroutine animationCoroutine;
    
    void Awake()
    {
        SetupOutline();
    }
    
    void SetupOutline()
    {
        outlineComponent = GetComponent<Outline>();
        
        if (outlineComponent == null)
        {
            outlineComponent = gameObject.AddComponent<Outline>();
        }
        
        outlineComponent.OutlineColor = defaultColor;
        outlineComponent.OutlineWidth = defaultOutlineWidth;
        outlineComponent.enabled = false; // ← ИЗМЕНЕНО: ВЫКЛЮЧЕН ПО УМОЛЧАНИЮ
        
        currentWidth = defaultOutlineWidth;
        currentColor = defaultColor;
        targetWidth = defaultOutlineWidth;
        targetColor = defaultColor;
    }
    
    public void Pointing()
    {
        isHighlighted = true;
        targetWidth = highlightOutlineWidth;
        targetColor = highlightColor;
        outlineComponent.enabled = true; // ← ДОБАВЛЕНО: ВКЛЮЧАЕМ ПРИ НАВЕДЕНИИ
        
        if (animationCoroutine != null)
            StopCoroutine(animationCoroutine);
        animationCoroutine = StartCoroutine(AnimateOutline());
    }
    
    public void StopPointing()
    {
        isHighlighted = false;
        targetWidth = defaultOutlineWidth;
        targetColor = defaultColor;
        pulseTimer = 0f;
        outlineComponent.enabled = false; // ← ДОБАВЛЕНО: ВЫКЛЮЧАЕМ КОГДА ОТВЕЛИ ВЗГЛЯД
        
        if (animationCoroutine != null)
            StopCoroutine(animationCoroutine);
        animationCoroutine = StartCoroutine(AnimateOutline());
    }
    
    private IEnumerator AnimateOutline()
    {
        float animationDuration = 1f / highlightAnimationSpeed;
        float elapsedTime = 0f;
        
        float startWidth = currentWidth;
        Color startColor = currentColor;
        
        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / animationDuration;
            
            currentWidth = Mathf.Lerp(startWidth, targetWidth, t);
            currentColor = Color.Lerp(startColor, targetColor, t);
            
            float finalWidth = currentWidth;
            if (isHighlighted && enablePulseEffect && outlineComponent.enabled) // ← ДОБАВЛЕНА ПРОВЕРКА
            {
                pulseTimer += Time.deltaTime * pulseSpeed;
                float pulse = Mathf.Sin(pulseTimer) * pulseAmplitude;
                finalWidth += pulse;
            }
            
            if (outlineComponent != null && outlineComponent.enabled) // ← ДОБАВЛЕНА ПРОВЕРКА
            {
                outlineComponent.OutlineWidth = finalWidth;
                outlineComponent.OutlineColor = currentColor;
            }
            
            yield return null;
        }
        
        currentWidth = targetWidth;
        currentColor = targetColor;
        
        if (outlineComponent != null && outlineComponent.enabled) // ← ДОБАВЛЕНА ПРОВЕРКА
        {
            outlineComponent.OutlineWidth = targetWidth;
            outlineComponent.OutlineColor = targetColor;
        }
        
        if (isHighlighted && enablePulseEffect)
        {
            animationCoroutine = StartCoroutine(PulseCoroutine());
        }
        else
        {
            animationCoroutine = null;
        }
    }
    
    private IEnumerator PulseCoroutine()
    {
        while (isHighlighted && enablePulseEffect && outlineComponent != null && outlineComponent.enabled) // ← ДОБАВЛЕНА ПРОВЕРКА
        {
            pulseTimer += Time.deltaTime * pulseSpeed;
            float pulse = Mathf.Sin(pulseTimer) * pulseAmplitude;
            outlineComponent.OutlineWidth = targetWidth + pulse;
            yield return null;
        }
        
        if (outlineComponent != null && !isHighlighted)
            outlineComponent.OutlineWidth = targetWidth;
            
        animationCoroutine = null;
    }
    
    public void UseblePointing()
    {
        Pointing();
    }
    
    public BaseItem ReturnItem()
    {
        return itemData;
    }
    
    void OnDestroy()
    {
        if (animationCoroutine != null && gameObject.activeInHierarchy)
            StopCoroutine(animationCoroutine);
    }
}