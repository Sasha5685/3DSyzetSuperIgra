using UnityEngine;
using System.Collections;

public class DoorController : MonoBehaviour, Entety, IInteractable
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

    [Header("Outline")]
    [SerializeField] private float defaultOutlineWidth = 0.15f;
    [SerializeField] private Color defaultColor = new Color(1f, 1f, 1f, 0.5f);
    [SerializeField] private float highlightOutlineWidth = 0.6f;
    [SerializeField] private Color highlightColor = Color.yellow;
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float pulseAmplitude = 0.2f;
    
    private Outline outlineComponent;
    private bool isHighlighted = false;
    private float pulseTimer = 0f;
    private Coroutine pulseCoroutine;
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
    
    private void SetupOutline()
    {
        outlineComponent = GetComponent<Outline>();
        if (outlineComponent == null)
        {
            outlineComponent = gameObject.AddComponent<Outline>();
        }
        outlineComponent.OutlineColor = defaultColor;
        outlineComponent.OutlineWidth = defaultOutlineWidth;
        outlineComponent.enabled = false;
    }
    
    void Start()
    {
        if (doorAnimator != null)
        {
            doorAnimator.enabled = true;
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
            doorAnimator.enabled = false;
        }
    }
    
    private void SetupAnimator()
    {
        if (doorAnimator == null)
        {
            doorAnimator = GetComponent<Animator>();
        }
        
        if (doorAnimator != null)
        {
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
            if (!playerLayerWasExcluded)
            {
                doorCollider.excludeLayers |= playerLayer;
                playerLayerWasExcluded = true;
            }
        }
        else
        {
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
        if (this == null || outlineComponent == null) return;
        
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
        outlineComponent.enabled = false;
        outlineComponent.OutlineWidth = defaultOutlineWidth;
        outlineComponent.OutlineColor = defaultColor;
        
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
    
    public void UseblePointing()
    {
        Pointing();
    }
    
    public BaseItem ReturnItem()
    {
        return null;
    }
    
    public void Interact()
    {
        if (isAnimating) return;
        if (LockDoor)
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
            if (isAnimating) return;
            
            isAnimating = true;
            doorAnimator.enabled = true;
            doorAnimator.SetTrigger("RepitOpen");
            
            StartCoroutine(WaitForAnimation(() => {
                isAnimating = false;
                
                if (doorAnimator != null)
                {
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
        ExcludePlayerLayer(true);
        PlaySound(openSound);
        
        if (doorAnimator != null)
        {
            isAnimating = true;
            doorAnimator.enabled = true;
            doorAnimator.ResetTrigger(closeTrigger);
            doorAnimator.SetTrigger(openTrigger);
            
            StartCoroutine(WaitForAnimation(() => {
                isOpen = true;
                isAnimating = false;
                
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
        ExcludePlayerLayer(false);
        PlaySound(closeSound);
        
        if (doorAnimator != null)
        {
            isAnimating = true;
            doorAnimator.enabled = true;
            doorAnimator.ResetTrigger(openTrigger);
            doorAnimator.SetTrigger(closeTrigger);
            
            StartCoroutine(WaitForAnimation(() => {
                isOpen = false;
                isAnimating = false;
                
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
    
    private void OnDestroy()
    {
        if (pulseCoroutine != null)
            StopCoroutine(pulseCoroutine);
        
        if (outlineComponent != null)
        {
            outlineComponent.enabled = false;
        }
    }
}