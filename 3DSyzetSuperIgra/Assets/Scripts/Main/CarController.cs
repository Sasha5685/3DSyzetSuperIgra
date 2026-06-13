using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CarController : MonoBehaviour, Entety
{
    [Header("Движение")]
    [SerializeField] private float motorForce = 1500f;
    [SerializeField] private float maxSpeed = 25f;
    [SerializeField] private float brakeForce = 2000f;
    [SerializeField] private float reverseForce = 1000f;
    
    [Header("Поворот")]
    [SerializeField] private float turnSpeed = 2f;
    [SerializeField] private float turnSensitivity = 1.5f;
    
    [Header("Физика")]
    [SerializeField] private float downforce = 100f;
    [SerializeField] private float grip = 2f;
    
    [Header("Выход из машины")]
    [SerializeField] private Transform exitPoint;
    [SerializeField] private float exitDistance = 2f;
    
    [Header("Обводка")]
    [SerializeField] private float defaultOutlineWidth = 0.15f;
    [SerializeField] private Color defaultColor = new Color(1f, 1f, 1f, 0.5f);
    [SerializeField] private float highlightOutlineWidth = 0.6f;
    [SerializeField] private Color highlightColor = Color.yellow;
    [SerializeField] private float highlightAnimationSpeed = 8f;
    
    [Header("Пульсация")]
    [SerializeField] private bool enablePulseEffect = true;
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float pulseAmplitude = 0.2f;
    
    [Header("Камера")]
    [SerializeField] private Vector3 cameraOffset = new Vector3(0, 1.8f, -4f);
    [SerializeField] private float cameraRotationX = 10f;
    [SerializeField] private float cameraRotationY = 0f;
    
    // Компоненты
    private Rigidbody rb;
    private Outline outlineComponent;
    private Camera playerCamera;
    private PlayerController playerController;
    
    // Состояния игрока
    private bool isPlayerDriving = false;
    private GameObject currentDriver;
    private Vector3 originalCameraPosition;
    private Quaternion originalCameraRotation;
    private Transform originalCameraParent;
    private CursorLockMode originalCursorLockMode;
    private bool originalCursorVisible;
    
    // Управление
    private float gasInput;
    private float steeringInput;
    private bool isBraking;
    private float currentSpeed;
    
    // Обводка
    private float targetWidth;
    private Color targetColor;
    private float currentWidth;
    private Color currentColor;
    private bool isHighlighted = false;
    private float pulseTimer = 0f;
    private Coroutine animationCoroutine;
    
    void Start()
    {
        SetupCar();
        SetupOutline();
    }
    
    private void SetupCar()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
        
        rb.mass = 1200f;
        rb.drag = 0.3f;
        rb.angularDrag = 0.5f;
        rb.centerOfMass = new Vector3(0, -0.3f, 0);
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }
    
    private void SetupOutline()
    {
        outlineComponent = GetComponent<Outline>();
        if (outlineComponent == null) outlineComponent = gameObject.AddComponent<Outline>();
        
        outlineComponent.OutlineColor = defaultColor;
        outlineComponent.OutlineWidth = defaultOutlineWidth;
        outlineComponent.enabled = true;
        
        currentWidth = defaultOutlineWidth;
        currentColor = defaultColor;
        targetWidth = defaultOutlineWidth;
        targetColor = defaultColor;
    }
    
    void Update()
    {
        if (isPlayerDriving)
        {
            gasInput = Input.GetAxis("Vertical");
            steeringInput = Input.GetAxis("Horizontal");
            isBraking = Input.GetKey(KeyCode.Space);
            
            if (Input.GetKeyDown(KeyCode.E)) ExitCar();
            
            currentSpeed = rb.velocity.magnitude;
        }
    }
    
    void FixedUpdate()
    {
        if (isPlayerDriving)
        {
            HandleMovement();
            HandleSteering();
            ApplyGrip();
            ApplyDownforce();
        }
    }
    
    private void HandleMovement()
    {
        currentSpeed = rb.velocity.magnitude;
        
        // Тормоз
        if (isBraking)
        {
            rb.AddForce(-rb.velocity.normalized * brakeForce);
            return;
        }
        
        // Вперёд
        if (gasInput > 0.1f && currentSpeed < maxSpeed)
        {
            rb.AddForce(transform.forward * gasInput * motorForce);
        }
        // Назад
        else if (gasInput < -0.1f)
        {
            rb.AddForce(transform.forward * gasInput * reverseForce);
        }
    }
    
    private void HandleSteering()
    {
        if (currentSpeed < 0.5f) return;
        
        float turn = steeringInput * turnSensitivity * turnSpeed * (currentSpeed / maxSpeed);
        Quaternion turnRotation = Quaternion.Euler(0f, turn, 0f);
        rb.MoveRotation(rb.rotation * turnRotation);
    }
    
    private void ApplyGrip()
    {
        Vector3 localVelocity = transform.InverseTransformDirection(rb.velocity);
        float sidewaysSpeed = localVelocity.x;
        rb.AddForce(-transform.right * sidewaysSpeed * grip);
    }
    
    private void ApplyDownforce()
    {
        float speedFactor = currentSpeed / maxSpeed;
        rb.AddForce(-transform.up * downforce * speedFactor);
    }
    
    public void EnterCar(GameObject player)
    {
        currentDriver = player;
        playerController = player.GetComponent<PlayerController>();
        playerCamera = player.GetComponentInChildren<Camera>();
        
        if (playerController != null && playerCamera != null)
        {
            isPlayerDriving = true;
            player.SetActive(false);
            
            if (outlineComponent != null) outlineComponent.enabled = false;
            if (animationCoroutine != null) StopCoroutine(animationCoroutine);
            
            originalCameraParent = playerCamera.transform.parent;
            originalCameraPosition = playerCamera.transform.localPosition;
            originalCameraRotation = playerCamera.transform.localRotation;
            originalCursorLockMode = Cursor.lockState;
            originalCursorVisible = Cursor.visible;
            
            playerCamera.transform.SetParent(transform);
            playerCamera.transform.localPosition = cameraOffset;
            playerCamera.transform.localRotation = Quaternion.Euler(cameraRotationX, cameraRotationY, 0f);
            
            playerController.enabled = false;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
    
    public void ExitCar()
    {
        if (playerController != null && playerCamera != null)
        {
            isPlayerDriving = false;
            
            if (outlineComponent != null)
            {
                outlineComponent.enabled = true;
                EnableOutline();
            }
            
            playerCamera.transform.SetParent(originalCameraParent);
            playerCamera.transform.localPosition = originalCameraPosition;
            playerCamera.transform.localRotation = originalCameraRotation;
            
            currentDriver.SetActive(true);
            
            Vector3 exitPosition = exitPoint != null ? exitPoint.position : transform.position - transform.right * exitDistance;
            exitPosition.y += 1f;
            currentDriver.transform.position = exitPosition;
            currentDriver.transform.rotation = transform.rotation;
            
            playerController.enabled = true;
            Cursor.lockState = originalCursorLockMode;
            Cursor.visible = originalCursorVisible;
            
            if (playerController != null) playerController.SetDeviceType(Application.isMobilePlatform ? "Mobile" : "PC");
            
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            
            currentDriver = null;
            playerController = null;
        }
    }
    
    private void EnableOutline()
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
    
    public void Pointing()
    {
        if (isPlayerDriving || outlineComponent == null) return;
        
        isHighlighted = true;
        targetWidth = highlightOutlineWidth;
        targetColor = highlightColor;
        
        if (animationCoroutine != null) StopCoroutine(animationCoroutine);
        animationCoroutine = StartCoroutine(AnimateOutline());
    }
    
    public void StopPointing()
    {
        if (isPlayerDriving || outlineComponent == null) return;
        
        isHighlighted = false;
        targetWidth = defaultOutlineWidth;
        targetColor = defaultColor;
        pulseTimer = 0f;
        
        if (animationCoroutine != null) StopCoroutine(animationCoroutine);
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
                finalWidth += Mathf.Sin(pulseTimer) * pulseAmplitude;
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
        
        if (isHighlighted && enablePulseEffect) animationCoroutine = StartCoroutine(PulseCoroutine());
        else animationCoroutine = null;
    }
    
    private IEnumerator PulseCoroutine()
    {
        while (isHighlighted && enablePulseEffect && outlineComponent != null && outlineComponent.enabled)
        {
            pulseTimer += Time.deltaTime * pulseSpeed;
            outlineComponent.OutlineWidth = targetWidth + Mathf.Sin(pulseTimer) * pulseAmplitude;
            yield return null;
        }
        if (outlineComponent != null && !isHighlighted && outlineComponent.enabled) outlineComponent.OutlineWidth = targetWidth;
        animationCoroutine = null;
    }
    
    public void UseblePointing() => Pointing();
    public BaseItem ReturnItem() => null;
    
    public void Interact()
    {
        if (!isPlayerDriving)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) EnterCar(player);
        }
    }
    
    void OnDestroy()
    {
        if (animationCoroutine != null && gameObject.activeInHierarchy) StopCoroutine(animationCoroutine);
    }
}