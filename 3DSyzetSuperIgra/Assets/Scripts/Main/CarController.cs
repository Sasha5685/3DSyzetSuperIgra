using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(RearWheelDrive))]
[RequireComponent(typeof(Outline))]
public class CarController : MonoBehaviour, Entety, IInteractable
{
    [Header("Player")]
    [SerializeField] private Transform exitPoint;

    [Header("Camera")]
    [SerializeField] private Vector3 cameraOffset = new(0, 1.5f, -5f);
    [SerializeField] private float cameraSmooth = 8f;
    [SerializeField] private float cameraPitch = 10f;

    [Header("Brake")]
    [SerializeField] private float handBrakeForce = 2000f;

    [Header("Sound")]
    [SerializeField] private AudioSource engineSound; // Компонент звука двигателя
    [SerializeField] private AudioClip enterSound;    // Звук входа в машину
    [SerializeField] private AudioClip exitSound;     // Звук выхода из машины
    [SerializeField] private float minPitch = 0.5f;   // Минимальная высота тона (на холостых)
    [SerializeField] private float maxPitch = 2f;     // Максимальная высота тона (на полной скорости)
    [SerializeField] private float maxSpeedForPitch = 30f; // Максимальная скорость для изменения тона

    [Header("Outline")]
    [SerializeField] private float defaultWidth = 0.15f;
    [SerializeField] private Color defaultColor = new(1,1,1,0.5f);

    [SerializeField] private float highlightWidth = 0.6f;
    [SerializeField] private Color highlightColor = Color.yellow;

    [SerializeField] private bool pulse = true;
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float pulseAmplitude = 0.2f;

    private Rigidbody rb;
    private RearWheelDrive drive;
    private Outline outline;

    private PlayerController player;
    private Camera playerCamera;

    private bool driving;

    private float pulseTimer;
    private Vector3 cameraVelocity;

    private Transform cameraTransform;

    private Transform savedCameraParent;
    private Vector3 savedCameraLocalPosition;
    private Quaternion savedCameraLocalRotation;
    [SerializeField] private float exitDelay = 1f;

    private float enterTime;
    private VisibleObject visibleObject;
    private AudioSource audioSource; // Для звуков входа/выхода


    public GameObject TextHelp;

    public GameObject SmallCursor;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        drive = GetComponent<RearWheelDrive>();
        outline = GetComponent<Outline>();
        visibleObject = GetComponent<VisibleObject>();
        
        // Создаем отдельный AudioSource для звуков входа/выхода
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        
        // Настраиваем двигатель
        if (engineSound == null)
            engineSound = gameObject.AddComponent<AudioSource>();
        
        outline.OutlineWidth = defaultWidth;
        outline.OutlineColor = defaultColor;
    }
    
    private void Start()
    {
        // Изначально звук двигателя выключен
        if (engineSound != null)
        {
            engineSound.loop = true;
            engineSound.playOnAwake = false;
        }
    }

    private void Update()
    {
        if (!driving)
            return;

        if (Input.GetKeyDown(KeyCode.E) &&
            Time.time - enterTime >= exitDelay)
        {
            ExitCar();
        }

        if (Input.GetKey(KeyCode.Space))
            HandBrake();
        
        // Обновляем звук двигателя в зависимости от скорости
        UpdateEngineSound();
    }

    private void LateUpdate()
    {
        if (!driving)
            return;

        UpdateCamera();

        if (pulse)
        {
            pulseTimer += Time.deltaTime * pulseSpeed;
            outline.OutlineWidth =
                highlightWidth +
                Mathf.Sin(pulseTimer) * pulseAmplitude;
        }
    }

    #region Sound

    private void UpdateEngineSound()
    {
        if (engineSound == null || !driving) return;
        
        // Если двигатель не играет - запускаем
        if (!engineSound.isPlaying && driving)
        {
            engineSound.Play();
        }
        
        // Изменяем тон в зависимости от скорости
        float speed = rb.velocity.magnitude;
        float t = Mathf.Clamp01(speed / maxSpeedForPitch);
        engineSound.pitch = Mathf.Lerp(minPitch, maxPitch, t);
        
        // Опционально: меняем громкость в зависимости от оборотов
        engineSound.volume = Mathf.Lerp(0.3f, 1f, t);
    }
    
    private void PlayEnterSound()
    {
        if (enterSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(enterSound, 0.7f);
        }
    }
    
    private void PlayExitSound()
    {
        if (exitSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(exitSound, 0.7f);
        }
    }
    
    private void StopEngineSound()
    {
        if (engineSound != null && engineSound.isPlaying)
        {
            engineSound.Stop();
        }
    }

    #endregion

    #region Enter Exit

    public void EnterCar(GameObject playerObject)
    {
        if (driving)
            return;
            SmallCursor.SetActive(false);
        TextHelp.SetActive(true);
                InvokeManager.instatiate.SendMessageEvent("GetInCar");
        enterTime = Time.time;
        HandItem.instatiate.itemHolder.gameObject.SetActive(false);
        // Проигрываем звук входа
        PlayEnterSound();
        
        // Временно отключаем VisibleManager для этой машины
        visibleObject.SetIgnoreManager(true);
        
        // Устанавливаем drag когда игрок в машине
        rb.drag = 0.2f;
        rb.angularDrag = 5f;
        
        SetOutlineState(false);

        player = playerObject.GetComponent<PlayerController>();
        playerCamera = player.playerCamera;

        cameraTransform = playerCamera.transform;

        savedCameraParent = cameraTransform.parent;
        savedCameraLocalPosition = cameraTransform.localPosition;
        savedCameraLocalRotation = cameraTransform.localRotation;

        cameraTransform.SetParent(null);

        player.enabled = false;

        driving = true;

        outline.enabled = false;

        drive.enabled = true;

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        
        // Запускаем звук двигателя с небольшой задержкой
        if (engineSound != null)
        {
            Invoke(nameof(StartEngineDelayed), 0.2f);
        }
    }
    
    private void StartEngineDelayed()
    {
        if (driving && engineSound != null && !engineSound.isPlaying)
        {
            engineSound.Play();
        }
    }

    public void ExitCar()
    {
        if (!driving)
            return;
                        SmallCursor.SetActive(true);
                    TextHelp.SetActive(false);
        HandItem.instatiate.itemHolder.gameObject.SetActive(true);
        // Останавливаем звук двигателя
        StopEngineSound();
        
        // Проигрываем звук выхода
        PlayExitSound();
        
        // Возвращаем управление VisibleManager
        visibleObject.SetIgnoreManager(false);
        
        // Возвращаем drag когда игрок вышел
        rb.drag = 10f;
        rb.angularDrag = 0.05f;
        
        // Мгновенная остановка перед выходом
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        
        SetOutlineState(true);
        driving = false;

        drive.enabled = false;

        Vector3 pos = exitPoint != null
            ? exitPoint.position
            : transform.position - transform.right * 2f;

        player.transform.SetPositionAndRotation(
            pos,
            transform.rotation
        );

        cameraTransform.SetParent(savedCameraParent);
        cameraTransform.localPosition = savedCameraLocalPosition;
        cameraTransform.localRotation = savedCameraLocalRotation;

        player.enabled = true;

        outline.enabled = true;

        ResetOutline();

        player = null;
        playerCamera = null;
        cameraTransform = null;
    }

    private void ResetDrag()
    {
        rb.drag = 0.1f;
        rb.angularDrag = 0.05f;
    }
    #endregion

    #region Camera
    private void UpdateCamera()
    {
        Vector3 targetPosition =
            transform.TransformPoint(cameraOffset);

        cameraTransform.position =
            Vector3.SmoothDamp(
                cameraTransform.position,
                targetPosition,
                ref cameraVelocity,
                1f / cameraSmooth
            );

        Vector3 lookPoint =
            transform.position + Vector3.up * 1.2f;

        Quaternion targetRotation =
            Quaternion.LookRotation(
                lookPoint - cameraTransform.position
            );

        cameraTransform.rotation =
            Quaternion.Slerp(
                cameraTransform.rotation,
                targetRotation,
                Time.deltaTime * cameraSmooth
            );
    }
    #endregion

    #region Brake

    private void HandBrake()
    {
        rb.AddForce(
            -rb.velocity.normalized * handBrakeForce,
            ForceMode.Force
        );

        if (rb.velocity.magnitude < 0.5f)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    #endregion

    #region Outline

    private void ResetOutline()
    {
        outline.OutlineWidth = defaultWidth;
        outline.OutlineColor = defaultColor;
    }

    public void Pointing()
    {
        if (driving)
            return;

        outline.OutlineWidth = highlightWidth;
        outline.OutlineColor = highlightColor;
    }

    public void StopPointing()
    {
        if (driving)
            return;

        ResetOutline();
    }

    public void UseblePointing()
    {
        Pointing();
    }

    #endregion

    #region Entety

    public BaseItem ReturnItem()
    {
        return null;
    }
    private void SetOutlineState(bool state)
    {
        outline.enabled = state;
    }
    public void Interact()
    {
        if (driving)
            return;

        Debug.Log(111);
        if(Inventory.instatiate.HandItem("Car key"))
            EnterCar(
                GameObject.FindGameObjectWithTag("Player")
            );
    }

    #endregion
}