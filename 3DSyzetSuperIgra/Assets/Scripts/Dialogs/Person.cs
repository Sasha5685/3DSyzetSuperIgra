using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Person : MonoBehaviour, Entety, IInteractable
{
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
    
    public string ClickPerson;
    private bool isAlive = true;
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
        outlineComponent.enabled = true;
        
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
        
        if (animationCoroutine != null)
            StopCoroutine(animationCoroutine);
        animationCoroutine = StartCoroutine(AnimateOutline());
    }
    
    public void StopPointing()
    {
        if (!isAlive) return; // Самая быстрая проверка
        isHighlighted = false;
        targetWidth = defaultOutlineWidth;
        targetColor = defaultColor;
        pulseTimer = 0f;
        
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
            
            // Плавный lerp к целевым значениям
            currentWidth = Mathf.Lerp(startWidth, targetWidth, t);
            currentColor = Color.Lerp(startColor, targetColor, t);
            
            // Добавляем пульсацию если выделен
            float finalWidth = currentWidth;
            if (isHighlighted && enablePulseEffect)
            {
                pulseTimer += Time.deltaTime * pulseSpeed;
                float pulse = Mathf.Sin(pulseTimer) * pulseAmplitude;
                finalWidth += pulse;
            }
            
            if (outlineComponent != null)
            {
                outlineComponent.OutlineWidth = finalWidth;
                outlineComponent.OutlineColor = currentColor;
            }
            
            yield return null;
        }
        
        // Финальные значения
        currentWidth = targetWidth;
        currentColor = targetColor;
        
        if (outlineComponent != null)
        {
            outlineComponent.OutlineWidth = targetWidth;
            outlineComponent.OutlineColor = targetColor;
        }
        
        // Если нужна постоянная пульсация, запускаем её отдельно
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
        while (isHighlighted && enablePulseEffect && outlineComponent != null)
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
    public void Interact()
    {
        InvokeManager.instatiate.SendMessageEvent(ClickPerson);
    }
    public BaseItem  ReturnItem()
    {
        return null;
    
    }
        private void OnDestroy()
    {
        isAlive = false;
        if(animationCoroutine != null)
        StopCoroutine(animationCoroutine);
    }
}
