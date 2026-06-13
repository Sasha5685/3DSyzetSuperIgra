using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class WoodPlanks : MonoBehaviour, Entety
{
    
    [Header("Required Item to Destroy")]
    [SerializeField] private BaseItem  requiredItem; // Какой предмет нужен в руках
    
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
    private Inventory playerInventory;
    private bool isDestroyed = false; // Флаг для отслеживания уничтожения
    public Rigidbody rigidbodys;
        [Header("Sound Settings")]
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] [Range(0f, 1f)] private float pickupVolume = 0.7f;
    void Start()
    {
        FindPlayerInventory();
        outlineComponent = GetComponent<Outline>();
        rigidbodys = GetComponent<Rigidbody>();
    }
    
    private void FindPlayerInventory()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerInventory = player.GetComponent<Inventory>();
        }
        
        if (playerInventory == null)
        {
            playerInventory = FindObjectOfType<Inventory>();
        }
    }
    
    public void Pointing()
    {
        if (isDestroyed) return; // Проверка перед выполнением
        
        isHighlighted = true;
        targetWidth = highlightOutlineWidth;
        targetColor = highlightColor;
        
        if (animationCoroutine != null)
            StopCoroutine(animationCoroutine);
        animationCoroutine = StartCoroutine(AnimateOutline());
    }
    
    public void StopPointing()
    {
        if (isDestroyed) return; // Проверка перед выполнением
        
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
            // Проверка на уничтожение объекта
            if (isDestroyed || this == null || gameObject == null)
                yield break;
            
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / animationDuration;
            
            currentWidth = Mathf.Lerp(startWidth, targetWidth, t);
            currentColor = Color.Lerp(startColor, targetColor, t);
            
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
        
        if (isDestroyed || this == null || gameObject == null)
            yield break;
        
        currentWidth = targetWidth;
        currentColor = targetColor;
        
        if (outlineComponent != null)
        {
            outlineComponent.OutlineWidth = targetWidth;
            outlineComponent.OutlineColor = targetColor;
        }
        
        if (isHighlighted && enablePulseEffect && !isDestroyed)
        {
            animationCoroutine = StartCoroutine(PulseCoroutine());
        }
        else
        {
            animationCoroutine = null;
        }
    }
        private void PlayPickupSound()
    {
            AudioSource.PlayClipAtPoint(pickupSound, Camera.main.transform.position, pickupVolume);
    }
    private IEnumerator PulseCoroutine()
    {
        while (isHighlighted && enablePulseEffect && outlineComponent != null && !isDestroyed)
        {
            if (isDestroyed || this == null || gameObject == null)
                yield break;
                
            pulseTimer += Time.deltaTime * pulseSpeed;
            float pulse = Mathf.Sin(pulseTimer) * pulseAmplitude;
            outlineComponent.OutlineWidth = targetWidth + pulse;
            yield return null;
        }
        
        if (outlineComponent != null && !isHighlighted && !isDestroyed)
            outlineComponent.OutlineWidth = targetWidth;
            
        animationCoroutine = null;
    }
    
    public void UseblePointing()
    {
        Pointing();
    }
    
    public BaseItem  ReturnItem()
    {
        return null;
    }
    

    public void Interact()
    {
        // Проверка на уничтожение
        if (isDestroyed) return;
        
        if (HasRequiredItemInHand())
        {
            isDestroyed = true; // Устанавливаем флаг до уничтожения
            
            // Останавливаем все корутины
            if (animationCoroutine != null)
            {
                StopCoroutine(animationCoroutine);
                animationCoroutine = null;
            }
            Destroy(outlineComponent);
            gameObject.layer = 0;
            isDestroyed = true;
        
            if (animationCoroutine != null)
            {
                StopCoroutine(animationCoroutine);
                animationCoroutine = null;
            }
            rigidbodys.useGravity = true;
            rigidbodys.isKinematic = false;
            PlayPickupSound();
            Debug.Log($"WoodPlanks уничтожен с помощью {requiredItem?.itemName?.GetString("Russian")}");
            Destroy(this);
        }
        else
        {
            Debug.Log($"Нужен предмет: {requiredItem?.itemName?.GetString("Russian")} чтобы уничтожить это");
        }
    }
    
    private bool HasRequiredItemInHand()
    {
        if (isDestroyed) return false;
        
        if (playerInventory == null)
        {
            FindPlayerInventory();
            if (playerInventory == null)
            {
                Debug.LogWarning("Inventory не найден!");
                return false;
            }
        }
        
        Slot currentSlot = playerInventory.GetCurrentSelectedSlot();
        
        if (currentSlot == null || currentSlot.IsEmpety())
        {
            return false;
        }
        
        BaseItem  currentItem = currentSlot.Item;
        
        if (requiredItem != null && currentItem != null)
        {
            return currentItem == requiredItem;
        }
        
        return false;
    }
    
    private void OnDestroy()
    {
        isDestroyed = true;
        
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }
    }
}