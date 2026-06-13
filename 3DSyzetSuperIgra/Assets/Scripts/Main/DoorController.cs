using UnityEngine;
using System.Collections;

public class DoorController : MonoBehaviour, Entety
{
    [Header("Door Settings")]
    [SerializeField] private Animator doorAnimator;
    [SerializeField] private string openTrigger = "Open";
    [SerializeField] private string closeTrigger = "Close";
    [SerializeField] private bool isOpen = false;
    [SerializeField] private float animationDuration = 1f;
    
    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip openSound;
        [SerializeField] private AudioClip RepitOpenDoorSound;
    [SerializeField] private AudioClip closeSound;
    [SerializeField] private float soundVolume = 1f;
    
    [Header("Collider Settings")]
    [SerializeField] private Collider doorCollider;
    [SerializeField] private LayerMask playerLayer;
    
    [Header("Outline Settings")]
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
    private bool isAnimating = false;
    private bool playerLayerWasExcluded = false;
    public bool LockDoor;
    
    void Awake()
    {
        SetupOutline();
        SetupAnimator();
        SetupAudio();
        SetupCollider();
    }
    
    void Start()
    {
        // Останавливаем автоматическое проигрывание анимации
        if (doorAnimator != null)
        {
            doorAnimator.enabled = true;
            // Принудительно устанавливаем состояние без проигрывания анимации
            if (isOpen)
            {
                doorAnimator.Play("DoorOpen", 0, 1f);
                ExcludePlayerLayer(true);
            }
            else
            {
                doorAnimator.Play("DoorClosed", 0, 0f);
                ExcludePlayerLayer(false);
            }
            doorAnimator.Update(0f);
            // Отключаем animator чтобы он не перезаписал состояние
            doorAnimator.enabled = false;
        }
    }
    
    private void SetupOutline()
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
    
    private void SetupAnimator()
    {
        if (doorAnimator == null)
        {
            doorAnimator = GetComponent<Animator>();
        }
        
        if (doorAnimator == null)
        {
            Debug.LogWarning($"DoorController on {gameObject.name} has no Animator component!");
        }
        else
        {
            // Включаем animator только когда нужно
            doorAnimator.enabled = false;
        }
    }
    
    private void SetupAudio()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null && (openSound != null || closeSound != null))
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        
        if (audioSource != null)
        {
            audioSource.volume = soundVolume;
            audioSource.playOnAwake = false;
        }
    }
    
    private void SetupCollider()
    {
        if (doorCollider == null)
        {
            doorCollider = GetComponent<Collider>();
            if (doorCollider == null)
            {
                doorCollider = GetComponentInChildren<Collider>();
            }
        }
    }
    
    private void ExcludePlayerLayer(bool exclude)
    {
        if (doorCollider == null) return;
        
        if (exclude)
        {
            // Добавляем слой игрока в исключения
            if (!playerLayerWasExcluded)
            {
                doorCollider.excludeLayers |= playerLayer;
                playerLayerWasExcluded = true;
            }
        }
        else
        {
            // Убираем слой игрока из исключений
            if (playerLayerWasExcluded)
            {
                doorCollider.excludeLayers &= ~playerLayer;
                playerLayerWasExcluded = false;
            }
        }
    }
    
    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip, soundVolume);
        }
    }
    
    public void Pointing()
    {
        if (isHighlighted) return;
        
        isHighlighted = true;
        targetWidth = highlightOutlineWidth;
        targetColor = highlightColor;
        
        if (animationCoroutine != null)
            StopCoroutine(animationCoroutine);
        animationCoroutine = StartCoroutine(AnimateOutline());
    }
    
    public void StopPointing()
    {
        if (!isHighlighted) return;
        
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
        
        currentWidth = targetWidth;
        currentColor = targetColor;
        
        if (outlineComponent != null)
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
    
    public BaseItem  ReturnItem()
    {
        Debug.LogWarning("DoorController: ReturnItem called but doors don't return items!");
        return null;
    }
    
    public void Interact()
    {
        
        if (isAnimating) return;
        if(LockDoor == true)
        {
            RepitOpenDoor();
            return;
        }
        if (!isOpen)
        {
            OpenDoor();
        }
        else
        {
            CloseDoor();
        }
    }
    
    private void RepitOpenDoor()
    {
        PlaySound(RepitOpenDoorSound);
        
        if (doorAnimator != null)
        {
            // Останавливаем текущую анимацию если есть
            if (isAnimating) return;
            
            isAnimating = true;
            
            // Включаем animator
            doorAnimator.enabled = true;
            
            // Запускаем анимацию неудачной попытки открытия
            doorAnimator.SetTrigger("RepitOpen");
            
            StartCoroutine(WaitForAnimation(() => {
                isAnimating = false;
                
                // Отключаем animator после анимации
                if (doorAnimator != null)
                {
                    // Возвращаем дверь в исходное состояние
                    if (isOpen)
                        doorAnimator.Play("DoorOpen", 0, 1f);
                    else
                        doorAnimator.Play("DoorClosed", 0, 0f);
                    
                    doorAnimator.Update(0f);
                    doorAnimator.enabled = false;
                }
            }));
        }
    }
    private void OpenDoor()
    {
        
        // СРАЗУ добавляем слой игрока в исключения - игрок может проходить сразу
        ExcludePlayerLayer(true);
        
        // Проигрываем звук открытия
        PlaySound(openSound);
        
        if (doorAnimator != null)
        {
            isAnimating = true;
            
            // Включаем animator и проигрываем анимацию
            doorAnimator.enabled = true;
            doorAnimator.ResetTrigger(closeTrigger);
            doorAnimator.SetTrigger(openTrigger);
            
            StartCoroutine(WaitForAnimation(() => {
                isOpen = true;
                isAnimating = false;
                
                // Отключаем animator и фиксируем финальное состояние
                if (doorAnimator != null)
                {
                    doorAnimator.Play("DoorOpen", 0, 1f);
                    doorAnimator.Update(0f);
                    doorAnimator.enabled = false;
                }
            }));
        }
        else
        {
            isOpen = true;
        }
    }
    
    private void CloseDoor()
    {
        Debug.Log($"Closing door {gameObject.name}");
        
        // СРАЗУ убираем слой игрока из исключений - игрок не может проходить
        ExcludePlayerLayer(false);
        
        // Проигрываем звук закрытия
        PlaySound(closeSound);
        
        if (doorAnimator != null)
        {
            isAnimating = true;
            
            // Включаем animator и проигрываем анимацию
            doorAnimator.enabled = true;
            doorAnimator.ResetTrigger(openTrigger);
            doorAnimator.SetTrigger(closeTrigger);
            
            StartCoroutine(WaitForAnimation(() => {
                isOpen = false;
                isAnimating = false;
                
                // Отключаем animator и фиксируем финальное состояние
                if (doorAnimator != null)
                {
                    doorAnimator.Play("DoorClosed", 0, 0f);
                    doorAnimator.Update(0f);
                    doorAnimator.enabled = false;
                }
            }));
        }
        else
        {
            isOpen = false;
        }
    }
    
    private IEnumerator WaitForAnimation(System.Action onComplete)
    {
        float elapsedTime = 0f;
        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        onComplete?.Invoke();
    }
    
    void OnDestroy()
    {
        if (animationCoroutine != null && gameObject.activeInHierarchy)
            StopCoroutine(animationCoroutine);
    }
}