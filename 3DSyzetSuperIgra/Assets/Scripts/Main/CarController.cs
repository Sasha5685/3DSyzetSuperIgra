using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(RearWheelDrive))]
[RequireComponent(typeof(Outline))]
public class CarController : MusicSystem, Entety, IInteractable
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
    [SerializeField] private AudioClip engineClip;
    [SerializeField] private AudioClip enterSound;
    [SerializeField] private AudioClip exitSound;
    [SerializeField] private AudioClip NoKeys;
    [SerializeField] private float minPitch = 0.5f;
    [SerializeField] private float maxPitch = 2f;
    [SerializeField] private float maxSpeedForPitch = 30f;
    [SerializeField] private AudioMixerGroup sfxMixerGroup;

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
    
    // Дополнительный MusicSystem для SFX (звуки входа/выхода)
    private MusicSystem sfxMusicSystem;

    public GameObject TextHelp;
    public GameObject SmallCursor;
    
    private RigidbodyConstraints savedConstraints;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        drive = GetComponent<RearWheelDrive>();
        outline = GetComponent<Outline>();
        visibleObject = GetComponent<VisibleObject>();
        
        // Инициализируем MusicSystem (этот объект) для двигателя с loop = true
        InitSystem(sfxMixerGroup, true);
        SetLoop(true);
        
        // Создаем отдельный MusicSystem для SFX (звуки входа/выхода)
        sfxMusicSystem = gameObject.AddComponent<MusicSystem>();
        sfxMusicSystem.InitSystem(sfxMixerGroup, false);
        sfxMusicSystem.SetLoop(false);
        
        // Устанавливаем клип двигателя
        if (engineClip != null)
        {
            SetClip(engineClip);
        }
        
        outline.OutlineWidth = defaultWidth;
        outline.OutlineColor = defaultColor;
        
        savedConstraints = rb.constraints;
        FreezePhysics();
    }
    
    private void Start()
    {
        // Подписываемся на событие паузы
        GameManager.OnPauseStateChanged += HandlePauseState;
    }

    private void OnDestroy()
    {
        // Отписываемся от события
        GameManager.OnPauseStateChanged -= HandlePauseState;
        
        // Очищаем звуки
        ClearSound();
        if (sfxMusicSystem != null)
            sfxMusicSystem.ClearSound();
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

    #region Pause Handler
    
    private void HandlePauseState(bool isPaused) 
    { 
        if (isPaused) 
        { 
            StopSound(); 
            sfxMusicSystem?.StopSound(); 
        } 
        else 
        { 
            if (driving && GameManager.instatiate != null && GameManager.instatiate.RunningGame) 
            { 
                ResumeSound(); 
            } 
            sfxMusicSystem?.ResumeSound(); 
        } 
    }
    
    #endregion

    #region Physics Freeze / Unfreeze
    
    private void FreezePhysics()
    {
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints.FreezeAll;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
    
    private void UnfreezePhysicsForDriving()
    {
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints.None;
        }
    }
    
    #endregion

    #region Sound

    private void UpdateEngineSound()
    {
        if (!driving) return;
        
        if (GameManager.instatiate != null && !GameManager.instatiate.RunningGame)
        {
            if (IsPlaying())
                StopSound();
            return;
        }
        
        if (!IsPlaying() && driving)
        {
            PlaySound();
        }
        
        float speed = rb.velocity.magnitude;
        float t = Mathf.Clamp01(speed / maxSpeedForPitch);
        float pitch = Mathf.Lerp(minPitch, maxPitch, t);
        float volume = Mathf.Lerp(0.3f, 1f, t) * 0.25f;
        
        SetPitch(pitch);
        SetVolume(volume);
    }

    private void PlayEnterSound()
    {
        if (enterSound == null || sfxMusicSystem == null) return;
        if (GameManager.instatiate != null && !GameManager.instatiate.RunningGame) return;
        
        sfxMusicSystem.ShotSound(enterSound);
    }

    private void PlayExitSound()
    {
        if (exitSound == null || sfxMusicSystem == null) return;
        if (GameManager.instatiate != null && !GameManager.instatiate.RunningGame) return;
        
        sfxMusicSystem.ShotSound(exitSound);
    }

    private void StopEngineSound()
    {
        if (IsPlaying())
        {
            StopSound();
        }
    }

    #endregion

    #region Enter Exit

    public void EnterCar(GameObject playerObject)
    {
        if (driving)
            return;
        
        PlayEnterSound();
        
        SmallCursor.SetActive(false);
        TextHelp.SetActive(true);
        InvokeManager.instatiate.SendMessageEvent("GetInCar");
        enterTime = Time.time;
        HandItem.instatiate.itemHolder.gameObject.SetActive(false);
        
        visibleObject.SetIgnoreManager(true);
        
        rb.drag = 0.2f;
        rb.angularDrag = 5f;
        
        UnfreezePhysicsForDriving();
        

        SetOutlineState(false);

        player = playerObject.GetComponent<PlayerController>();
        playerObject.GetComponent<CharacterController>().enabled = false;
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
        
        // Запускаем звук двигателя с задержкой
        if (engineClip != null)
        {
            Invoke(nameof(StartEngineDelayed), 0.8f);
        }
    }
    
    private void StartEngineDelayed()
    {
        if (driving && !IsPlaying())
        {
            PlaySound();
        }
    }

    public void ExitCar()
    {
        if (!driving)
            return;
        
        StopEngineSound();
        PlayExitSound();
        
        SmallCursor.SetActive(true);
        TextHelp.SetActive(false);
        HandItem.instatiate.itemHolder.gameObject.SetActive(true);
        
        visibleObject.SetIgnoreManager(false);
        
        rb.drag = 10f;
        rb.angularDrag = 0.05f;
        
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        
        FreezePhysics();
        
        SetOutlineState(true);
        driving = false;

        drive.enabled = false;

        Vector3 pos = exitPoint != null
            ? exitPoint.position
            : transform.position - transform.right * 2f;

        player.transform.SetPositionAndRotation(
            pos,
            transform.rotation = Quaternion.identity
        );

        cameraTransform.SetParent(savedCameraParent);
        cameraTransform.localPosition = savedCameraLocalPosition;
        cameraTransform.localRotation = savedCameraLocalRotation;

        player.enabled = true;

        outline.enabled = true;

        ResetOutline();
        player.GetComponent<CharacterController>().enabled = true;
        player = null;
        playerCamera = null;
        cameraTransform = null;
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
        if(Inventory.instatiate.HandItem("Car key"))
            EnterCar(
                GameObject.FindGameObjectWithTag("Player")
            );
        else
        {
            sfxMusicSystem.ShotSound(NoKeys);
        }
    }

    #endregion
}