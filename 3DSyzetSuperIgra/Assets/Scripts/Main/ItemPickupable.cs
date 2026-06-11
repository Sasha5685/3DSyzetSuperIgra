using UnityEngine;
using System.Collections;

public class ItemPickupable : MonoBehaviour
{   
    public KeyItem itemData_1;
    public HealItem itemData_2;
    public CarAttributeItem itemData_3;
    
    [Header("Default Outline (Always Active)")]
    [SerializeField] private float defaultOutlineWidth = 0.15f;
    [SerializeField] private Color defaultColor = new Color(1f, 1f, 1f, 0.5f); // Полупрозрачный белый
    
    [Header("Highlight Outline (On Hover)")]
    [SerializeField] private float highlightOutlineWidth = 0.6f;
    [SerializeField] private Color highlightColor = Color.yellow;
    [SerializeField] private float highlightAnimationSpeed = 8f;
    
    [Header("Pulse Effect")]
    [SerializeField] private bool enablePulseEffect = true;
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float pulseAmplitude = 0.2f;
    
    private Outline outlineComponent;
    private float currentOutlineWidth;
    private Color currentOutlineColor;
    private bool isHighlighted = false;
    private bool isInitialized = false;
    private float pulseTimer = 0f;
    
    void Start()
    {
        SetupOutline();
        StartCoroutine(InitializeWithDelay());
    }
    
    void SetupOutline()
    {
        outlineComponent = GetComponent<Outline>();
        
        if (outlineComponent == null)
        {
            outlineComponent = gameObject.AddComponent<Outline>();
        }
        
        // Настройка Outline
        outlineComponent.OutlineColor = defaultColor;
        outlineComponent.OutlineWidth = defaultOutlineWidth;
        outlineComponent.enabled = true; // Всегда включен
        
        currentOutlineWidth = defaultOutlineWidth;
        currentOutlineColor = defaultColor;
    }
    
    IEnumerator InitializeWithDelay()
    {
        yield return null; // Ждем один кадр
        isInitialized = true;
    }
    
    void Update()
    {
        if (!isInitialized) return;
        
        // Определяем целевые значения
        float targetWidth = isHighlighted ? highlightOutlineWidth : defaultOutlineWidth;
        Color targetColor = isHighlighted ? highlightColor : defaultColor;
        
        // Добавляем пульсацию если выделен
        if (isHighlighted && enablePulseEffect)
        {
            pulseTimer += Time.deltaTime * pulseSpeed;
            float pulse = Mathf.Sin(pulseTimer) * pulseAmplitude;
            targetWidth += pulse;
        }
        else
        {
            pulseTimer = 0f;
        }
        
        // Плавное изменение
        currentOutlineWidth = Mathf.Lerp(currentOutlineWidth, targetWidth, Time.deltaTime * highlightAnimationSpeed);
        currentOutlineColor = Color.Lerp(currentOutlineColor, targetColor, Time.deltaTime * highlightAnimationSpeed);
        
        outlineComponent.OutlineWidth = currentOutlineWidth;
        outlineComponent.OutlineColor = currentOutlineColor;
    }
    
    public void Highlight(bool highlight)
    {
        isHighlighted = highlight;
    }
    
    public void OnPickup(GameObject picker)
    {
        StartCoroutine(PickupEffect());
    }
    
    IEnumerator PickupEffect()
    {
        // Вспышка при поднятии
        float originalWidth = currentOutlineWidth;
        Color originalColor = currentOutlineColor;
        
        outlineComponent.OutlineColor = Color.white;
        outlineComponent.OutlineWidth = 1f;
        
        yield return new WaitForSeconds(0.15f);
        
        outlineComponent.OutlineColor = originalColor;
        outlineComponent.OutlineWidth = originalWidth;
    }
}