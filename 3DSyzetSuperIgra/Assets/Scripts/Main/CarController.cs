using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CarController : MonoBehaviour, Entety
{
    [Header("Настройки посадки/выхода")]
    [SerializeField] private Transform exitPoint;
    [SerializeField] private float exitDistance = 2f;
    
    [Header("Настройки обводки (Outline)")]
    [SerializeField] private float defaultOutlineWidth = 0.15f;
    [SerializeField] private Color defaultColor = new Color(1f, 1f, 1f, 0.5f);
    [SerializeField] private float highlightOutlineWidth = 0.6f;
    [SerializeField] private Color highlightColor = Color.yellow;
    [SerializeField] private float highlightAnimationSpeed = 8f;
    
    [Header("Настройки пульсации")]
    [SerializeField] private bool enablePulseEffect = true;
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float pulseAmplitude = 0.2f;
    
    [Header("Настройки камеры")]
    [SerializeField] private Vector3 cameraOffset = new Vector3(0, 1.5f, -5f);
    [SerializeField] private float cameraRotationX = 10f;
    [SerializeField] private float cameraSmoothSpeed = 5f;
    
    [Header("Настройки торможения")]
    [SerializeField] private float handbrakeForce = 2000f;
    
    [Header("Настройки звуков")]
    [SerializeField] private AudioClip enterCarSound;
    [SerializeField] private AudioClip exitCarSound;
    [SerializeField] private AudioClip engineSound;
    [SerializeField] private float enginePitchMin = 0.5f;
    [SerializeField] private float enginePitchMax = 1.5f;
    [SerializeField] private float maxSpeedForPitch = 120f;
    [SerializeField] private float soundVolume = 0.7f;
    
    // Компоненты машины
    private Rigidbody carRigidbody;
    private RearWheelDrive carController;
    private Outline outlineComponent;
    private AudioSource engineAudioSource;
    
    // Компоненты игрока
    private GameObject currentDriver;
    private PlayerController playerController;
    private Camera playerCamera;
    private Transform originalCameraParent;
    private Vector3 originalCameraPosition;
    private Quaternion originalCameraRotation;
    private CursorLockMode originalCursorLockMode;
    private bool originalCursorVisible;
    
    // Состояния
    private bool isPlayerDriving = false;
    private bool isEngineRunning = false;
    
    // Для обводки
    private float targetWidth;
    private Color targetColor;
    private float currentWidth;
    private Color currentColor;
    private bool isHighlighted = false;
    private float pulseTimer = 0f;
    private Coroutine animationCoroutine;
    
    // Для плавного следования камеры
    private Vector3 cameraVelocity;
    
    // Флаги для предотвращения двойного вызова
    private bool isExiting = false;
    private bool isEntering = false;
    
    // Для задержки запуска звука двигателя
    private Coroutine engineStartCoroutine;
    
    void Start()
    {
        SetupCar();
        SetupOutline();
        SetupEngineAudioSource();
    }
    
    private void SetupCar()
    {
        carRigidbody = GetComponent<Rigidbody>();
        carController = GetComponent<RearWheelDrive>();
        
        if (carController != null)
        {
            carController.enabled = false;
        }
    }
    
    private void SetupEngineAudioSource()
    {
        engineAudioSource = gameObject.AddComponent<AudioSource>();
        engineAudioSource.loop = true;
        engineAudioSource.playOnAwake = false;
        engineAudioSource.volume = soundVolume;
        engineAudioSource.spatialBlend = 1f;
        engineAudioSource.rolloffMode = AudioRolloffMode.Logarithmic;
        engineAudioSource.maxDistance = 30f;
        engineAudioSource.clip = engineSound;
    }
    
    private void PlaySoundAtCamera(AudioClip clip, float volume)
    {
        if (clip != null && Camera.main != null)
        {
            AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position, volume);
        }
    }
    
    void Update()
    {
        if (isPlayerDriving)
        {
            if (Input.GetKeyDown(KeyCode.E) && !isExiting)
            {
                ExitCar();
            }
            
            if (Input.GetKey(KeyCode.Space) && carRigidbody != null)
            {
                ApplyHandbrake();
            }
            
            UpdateEngineSound();
        }
    }
    
    private void ApplyHandbrake()
    {
        Vector3 brakeForce = -carRigidbody.velocity.normalized * handbrakeForce;
        carRigidbody.AddForce(brakeForce, ForceMode.Force);
        
        if (carRigidbody.velocity.magnitude < 0.5f)
        {
            carRigidbody.velocity = Vector3.zero;
            carRigidbody.angularVelocity = Vector3.zero;
        }
        
        if (carController != null && carController.enabled)
        {
            WheelCollider[] wheels = GetComponentsInChildren<WheelCollider>();
            foreach (WheelCollider wheel in wheels)
            {
                wheel.brakeTorque = handbrakeForce * 0.5f;
                StartCoroutine(ResetBrakeTorque(wheel));
            }
        }
    }
    
    private IEnumerator ResetBrakeTorque(WheelCollider wheel)
    {
        yield return new WaitForSeconds(0.1f);
        if (wheel != null)
        {
            wheel.brakeTorque = 0;
        }
    }
    
    private void UpdateEngineSound()
    {
        if (engineAudioSource == null || engineSound == null) return;
        
        if (isEngineRunning && carRigidbody != null)
        {
            if (!engineAudioSource.isPlaying)
            {
                engineAudioSource.Play();
            }
            
            float speedKmh = carRigidbody.velocity.magnitude * 3.6f;
            float pitch = Mathf.Lerp(enginePitchMin, enginePitchMax, speedKmh / maxSpeedForPitch);
            engineAudioSource.pitch = Mathf.Clamp(pitch, enginePitchMin, enginePitchMax);
        }
        else
        {
            if (engineAudioSource.isPlaying)
            {
                engineAudioSource.Stop();
            }
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
    
    void LateUpdate()
    {
        if (isPlayerDriving && playerCamera != null)
        {
            FollowCarWithCamera();
        }
    }
    
    private void FollowCarWithCamera()
    {
        if (playerCamera == null) return;
        
        Vector3 targetPosition = transform.TransformPoint(cameraOffset);
        Quaternion targetRotation = Quaternion.LookRotation(transform.position - targetPosition);
        targetRotation *= Quaternion.Euler(cameraRotationX, 0, 0);
        
        playerCamera.transform.position = Vector3.SmoothDamp(
            playerCamera.transform.position, 
            targetPosition, 
            ref cameraVelocity, 
            1f / cameraSmoothSpeed
        );
        playerCamera.transform.rotation = Quaternion.Slerp(
            playerCamera.transform.rotation, 
            targetRotation, 
            Time.deltaTime * cameraSmoothSpeed
        );
    }
    
    public void EnterCar(GameObject player)
    {
        if (isEntering) return;
        isEntering = true;
        isExiting = false;
        
        currentDriver = player;
        playerController = player.GetComponent<PlayerController>();
        playerCamera = player.GetComponentInChildren<Camera>();
        
        if (playerController != null && playerCamera != null)
        {
            isPlayerDriving = true;
            
            // Сначала проигрываем звук посадки
            PlaySoundAtCamera(enterCarSound, soundVolume);
            
            player.SetActive(false);
            
            if (outlineComponent != null)
                outlineComponent.enabled = false;
            
            if (animationCoroutine != null)
                StopCoroutine(animationCoroutine);
            
            originalCameraParent = playerCamera.transform.parent;
            originalCameraPosition = playerCamera.transform.localPosition;
            originalCameraRotation = playerCamera.transform.localRotation;
            originalCursorLockMode = Cursor.lockState;
            originalCursorVisible = Cursor.visible;
            
            playerCamera.transform.SetParent(null);
            
            if (carController != null)
            {
                carController.enabled = false;
                StartCoroutine(EnableCarController());
            }
            
            playerController.enabled = false;
            
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            
            if (carRigidbody != null)
            {
                carRigidbody.velocity = Vector3.zero;
                carRigidbody.angularVelocity = Vector3.zero;
            }
            
            // Запускаем звук двигателя с задержкой (после звука посадки)
            if (engineStartCoroutine != null)
                StopCoroutine(engineStartCoroutine);
            engineStartCoroutine = StartCoroutine(StartEngineWithDelay(enterCarSound != null ? enterCarSound.length : 0.5f));
        }
        
        StartCoroutine(ResetEnterFlag());
    }
    
    private IEnumerator StartEngineWithDelay(float delay)
    {
        // Ждём пока закончится звук посадки
        yield return new WaitForSeconds(delay);
        isEngineRunning = true;
    }
    
    private IEnumerator EnableCarController()
    {
        yield return new WaitForSeconds(0.1f);
        if (carController != null)
        {
            carController.enabled = true;
        }
    }
    
    private IEnumerator ResetEnterFlag()
    {
        yield return new WaitForSeconds(0.5f);
        isEntering = false;
    }
    
    public void ExitCar()
    {
        if (isExiting) return;
        isExiting = true;
        
        // Сначала выключаем звук двигателя
        isEngineRunning = false;
        if (engineAudioSource != null && engineAudioSource.isPlaying)
        {
            engineAudioSource.Stop();
        }
        
        // Потом проигрываем звук выхода
        PlaySoundAtCamera(exitCarSound, soundVolume);
        
        if (playerController != null && playerCamera != null)
        {
            isPlayerDriving = false;
            
            if (carController != null)
                carController.enabled = false;
            
            if (carRigidbody != null)
            {
                carRigidbody.velocity = Vector3.zero;
                carRigidbody.angularVelocity = Vector3.zero;
            }
            
            if (outlineComponent != null)
            {
                outlineComponent.enabled = true;
                ResetOutline();
            }
            
            if (playerCamera != null)
            {
                if (originalCameraParent != null)
                    playerCamera.transform.SetParent(originalCameraParent);
                
                playerCamera.transform.localPosition = originalCameraPosition;
                playerCamera.transform.localRotation = originalCameraRotation;
            }
            
            if (currentDriver != null)
            {
                currentDriver.SetActive(true);
                
                Vector3 exitPosition = exitPoint != null ? exitPoint.position : transform.position - transform.right * exitDistance;
                exitPosition.y += 1f;
                currentDriver.transform.position = exitPosition;
                currentDriver.transform.rotation = transform.rotation;
            }
            
            if (playerController != null)
                playerController.enabled = true;
            
            Cursor.lockState = originalCursorLockMode;
            Cursor.visible = originalCursorVisible;
            
            if (playerController != null)
            {
                playerController.SetDeviceType(Application.isMobilePlatform ? "Mobile" : "PC");
            }
            
            currentDriver = null;
            playerController = null;
        }
        
        StartCoroutine(ResetExitFlag());
    }
    
    private IEnumerator ResetExitFlag()
    {
        yield return new WaitForSeconds(0.5f);
        isExiting = false;
    }
    
    private void ResetOutline()
    {
        if (outlineComponent != null && outlineComponent.enabled)
        {
            outlineComponent.OutlineWidth = defaultOutlineWidth;
            outlineComponent.OutlineColor = defaultColor;
        }
        
        currentWidth = defaultOutlineWidth;
        currentColor = defaultColor;
        targetWidth = defaultOutlineWidth;
        targetColor = defaultColor;
        isHighlighted = false;
        pulseTimer = 0f;
    }
    
    // ==================== ИНТЕРФЕЙС ENTETY ====================
    
    public void Pointing()
    {
        if (isPlayerDriving || outlineComponent == null) return;
        
        isHighlighted = true;
        targetWidth = highlightOutlineWidth;
        targetColor = highlightColor;
        
        if (animationCoroutine != null)
            StopCoroutine(animationCoroutine);
        animationCoroutine = StartCoroutine(AnimateOutline());
    }
    
    public void StopPointing()
    {
        if (isPlayerDriving || outlineComponent == null) return;
        
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
            
            if (outlineComponent != null && outlineComponent.enabled)
            {
                outlineComponent.OutlineWidth = finalWidth;
                outlineComponent.OutlineColor = currentColor;
            }
            
            yield return null;
        }
        
        currentWidth = targetWidth;
        currentColor = targetColor;
        
        if (outlineComponent != null && outlineComponent.enabled)
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
        while (isHighlighted && enablePulseEffect && outlineComponent != null && outlineComponent.enabled)
        {
            pulseTimer += Time.deltaTime * pulseSpeed;
            float pulse = Mathf.Sin(pulseTimer) * pulseAmplitude;
            outlineComponent.OutlineWidth = targetWidth + pulse;
            yield return null;
        }
        
        if (outlineComponent != null && !isHighlighted && outlineComponent.enabled)
            outlineComponent.OutlineWidth = targetWidth;
        
        animationCoroutine = null;
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
        if (!isPlayerDriving && !isEntering)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                EnterCar(player);
            }
        }
        else if (isPlayerDriving && !isExiting)
        {
            ExitCar();
        }
    }
    
    void OnDestroy()
    {
        if (animationCoroutine != null && gameObject.activeInHierarchy)
            StopCoroutine(animationCoroutine);
        
        if (engineAudioSource != null)
            engineAudioSource.Stop();
    }
}