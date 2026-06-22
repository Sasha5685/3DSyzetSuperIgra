using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using YG;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    public static PlayerController instatiate;
    
    [Header("References")]
    [SerializeField] public Camera playerCamera;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private Joystick movementJoystick;

    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 10f;
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float groundDistance = 0.4f;

    [Header("Mouse Settings")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float lookUpLimit = 90f;
    [SerializeField] private float lookDownLimit = -90f;

    [Header("Touch Settings")]
    [SerializeField] private float touchSensitivity = 0.15f;
    [SerializeField] private float lookSmoothTime = 0.08f;
    [SerializeField] private float maxLookSpeed = 6f;
    [SerializeField] private float deadZone = 2f;

    [Header("Sensitivity Settings")]
    [Range(0.1f, 3f)]
    [SerializeField] private float sensitivityMultiplier = 1f;
    
    [Header("UI Settings")]
    [SerializeField] private GameObject mobilePanel;
    [SerializeField] private Slider sensitivitySlider;
    [SerializeField] private TextMeshProUGUI sensitivityValueText;
    [SerializeField] private Button saveSensitivityButton;

    [Header("Audio Settings")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private TextMeshProUGUI musicVolumeText;
    [SerializeField] private TextMeshProUGUI sfxVolumeText;
    [SerializeField] private Button saveAudioButton;

    [Header("Music Settings")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioClip[] musicTracks;

    // Private fields
    private Vector3 velocity;
    private bool isGrounded;
    private float currentSpeed;
    private bool isMobilePlatform = false;
    private bool jumpPressed = false;
    private float cameraRotationX = 0f;

    // Touch look fields
    private Vector2 lookVelocity;
    private Vector2 currentLookDelta;
    private int lookFingerId = -1;
    private bool isLooking = false;
    private Vector2 previousTouchPosition;

    // Game state
    public bool RunningGame { get; private set; } = true;

    // Save keys
    private const string SENSITIVITY_KEY = "CameraSensitivity";
    private const string MUSIC_VOLUME_KEY = "MusicVolume";
    private const string SFX_VOLUME_KEY = "SFXVolume";

    [Header("Speed Boost Settings")]
    [SerializeField] private float speedBoostMultiplier = 3f;
    [SerializeField] private float speedBoostDuration = 120f;
    [SerializeField] private float baseWalkSpeed = 5f;
    [SerializeField] private float baseRunSpeed = 10f;
    
    private bool isSpeedBoostActive = false;
    private float speedBoostTimer = 0f;
    private Coroutine speedBoostCoroutine;

    [Header("Settings")]
    [SerializeField] private Transform HouseSpawn;

    private bool isInputLocked = false;
    private bool isAdShowing = false;
    private bool areAdButtonsEnabled = true;

    // Music
    private List<AudioClip> remainingTracks = new List<AudioClip>();
    private bool isMusicPlaying = false;

    // Audio Mixer параметры
    private const string MUSIC_VOLUME_PARAM = "MusicVolume";
    private const string SFX_VOLUME_PARAM = "SFXVolume";


    [Header("Crouch Settings")]
    [SerializeField] private float crouchHeight = 0.5f;
    [SerializeField] private float crouchSpeed = 10f;
    [SerializeField] private float crouchCameraOffset = 0.3f;
    [SerializeField] private KeyCode crouchKey = KeyCode.LeftControl;
    [SerializeField] private float crouchWalkSpeedMultiplier = 0.3f; // 50% скорости при приседе
    [SerializeField] private float crouchJumpMultiplier = 0.2f; // 20% прыжка при приседе

    private float originalHeight;
    private Vector3 originalCameraPosition;
    private bool isCrouching = false;
    private float currentCrouchProgress = 0f;
    private float originalJumpHeight;
    [Header("Run Settings")]
    [SerializeField] private KeyCode runKey = KeyCode.LeftShift;
    [SerializeField] private Button runButton; // Кнопка для мобильного бега (зажатие)
    [Header("Run UI")]
    [SerializeField] private Image runButtonImage;
    [SerializeField] private Color runActiveColor = Color.green;
    [SerializeField] private Color runInactiveColor = Color.white;
    private bool isRunning = false; // Это будет true пока кнопка зажата
    [Header("Car Controls")]
    [SerializeField] private GameObject carControlPanel; // Панель с джойстиком для машины
    [SerializeField] private Joystick  carJoystick; // Джойстик для управления машиной
    [SerializeField] private Button carBrakeButton; // Кнопка тормоза для машины
    [SerializeField] private Button carExitButton; // Кнопка выхода из машины

    private bool isInCar = false;
    private bool carBrakePressed = false;
    private Vector2 carInput = Vector2.zero;
    public void SetInCar(bool inCar)
    {
        isInCar = inCar;
        
        if (isMobilePlatform)
        {
            // Скрываем управление персонажем
            if (mobilePanel != null)
                mobilePanel.SetActive(!inCar);
            
            // Показываем управление машиной
            if (carControlPanel != null)
                carControlPanel.SetActive(inCar);
        }
        
        // Блокируем ввод персонажа
        isInputLocked = inCar;

    }

    public float GetCarHorizontalInput()
    {
        if (!isInCar || !isMobilePlatform || carJoystick == null)
            return 0f;
        return carInput.x; // Используем сохраненное значение
    }

    public float GetCarVerticalInput()
    {
        if (!isInCar || !isMobilePlatform || carJoystick == null)
            return 0f;
        return carInput.y; // Используем сохраненное значение
    }

    public bool GetCarBrakeInput()
    {
        if (!isInCar || !isMobilePlatform)
            return false;
        
        // Для ПК - пробел, для мобилки - кнопка
        if (isMobilePlatform)
            return carBrakePressed;
        
        return Input.GetKey(KeyCode.Space);
    }
    private void Awake()
    {
        instatiate = this;
        LoadSensitivity();
        LoadAudioSettings(); // Загружаем настройки аудио
        SetupUI();
        SetupMusic();
        
        baseWalkSpeed = walkSpeed;
        baseRunSpeed = runSpeed;
        originalHeight = characterController.height;
        originalCameraPosition = playerCamera.transform.localPosition;
        originalJumpHeight = jumpHeight;
    }
        // Методы для управления бегом
    public void StartRunning()
    {
        isRunning = true;
        UpdateRunButtonVisual();
    }

    public void StopRunning()
    {
        isRunning = false;
        UpdateRunButtonVisual();
    }

    private void UpdateRunButtonVisual()
    {
        if (runButtonImage != null)
        {
            runButtonImage.color = isRunning ? runActiveColor : runInactiveColor;
        }
    }
    private void Start()
    {
        // Дополнительно применяем настройки при старте
        ApplyAudioSettings();
        SetupCarControls(); 
    }
    private void SetupCarControls()
    {
        if (carBrakeButton != null)
        {
            // Добавляем EventTrigger для зажатия тормоза
            EventTrigger trigger = carBrakeButton.gameObject.GetComponent<EventTrigger>();
            if (trigger == null)
                trigger = carBrakeButton.gameObject.AddComponent<EventTrigger>();
            
            EventTrigger.Entry pointerDown = new EventTrigger.Entry();
            pointerDown.eventID = EventTriggerType.PointerDown;
            pointerDown.callback.AddListener((data) => { SetCarBrake(true); });
            trigger.triggers.Add(pointerDown);
            
            EventTrigger.Entry pointerUp = new EventTrigger.Entry();
            pointerUp.eventID = EventTriggerType.PointerUp;
            pointerUp.callback.AddListener((data) => { SetCarBrake(false); });
            trigger.triggers.Add(pointerUp);
        }
        
        if (carExitButton != null)
        {
            carExitButton.onClick.AddListener(() => {
                if (isInCar)
                {
                    // Находим машину и вызываем выход
                    CarController car = FindObjectOfType<CarController>();
                    if (car != null)
                        car.ExitCar();
                }
            });
        }
    }


    public void SetCarBrake(bool pressed)
    {
        carBrakePressed = pressed;
    }

    private void Update()
    {
        if (areAdButtonsEnabled)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                ShowAdvReward("PlayerHouse");
            }
            else if (Input.GetKeyDown(KeyCode.V))
            {
                ShowAdvReward("SpeedPlayer");
            }
        }

        // ВАЖНО: Обновляем ввод с джойстика машины ДО проверки isInputLocked
        if (isInCar && isMobilePlatform && carJoystick != null)
        {
            carInput = new Vector2(carJoystick.Horizontal, carJoystick.Vertical);
            // Для отладки - раскомментируйте чтобы проверить
            // Debug.Log($"Car Input: {carInput}");
        }

        if (isInputLocked || !RunningGame) return;

        if (!isMobilePlatform)
        {
            HandleMouseLook();
            HandleMovement();
        }
        else
        {
            HandleTouchLook();
            HandleMobileMovement();
        }
        if (!isInputLocked && RunningGame)
        {
            HandleCrouchInput();
        }
        HandleJump();
        ApplyGravity();
    }
    private void HandleCrouchInput()
    {
        bool crouchPressed = Input.GetKeyDown(crouchKey);
        
        if (crouchPressed && !isCrouching)
        {
            isCrouching = true;
        }
        else if (crouchPressed && isCrouching)
        {
            isCrouching = false;
        }
        
        // Плавное изменение высоты
        float targetProgress = isCrouching ? 1f : 0f;
        currentCrouchProgress = Mathf.MoveTowards(currentCrouchProgress, targetProgress, Time.deltaTime * crouchSpeed);
        
        // Изменяем высоту CharacterController
        float targetHeight = Mathf.Lerp(originalHeight, crouchHeight, currentCrouchProgress);
        characterController.height = targetHeight;
        
        // Изменяем положение камеры
        Vector3 targetCamPos = originalCameraPosition;
        targetCamPos.y -= currentCrouchProgress * crouchCameraOffset;
        playerCamera.transform.localPosition = Vector3.Lerp(playerCamera.transform.localPosition, targetCamPos, Time.deltaTime * crouchSpeed);
        
        // Изменяем скорость прыжка
        jumpHeight = Mathf.Lerp(originalJumpHeight, originalJumpHeight * crouchJumpMultiplier, currentCrouchProgress);
    }

    public void ToggleCrouch()
    {
        isCrouching = !isCrouching;
    }
    #region Audio Settings with AudioMixer

    private void ApplyAudioSettings()
    {
        float musicVol = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 0.5f);
        float sfxVol = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 0.7f);
        
        SetMusicVolume(musicVol);
        SetSFXVolume(sfxVol);
    }

    public void SetMusicVolume(float volume)
    {
        if (audioMixer != null)
        {
            float dB = Mathf.Log10(Mathf.Max(volume, 0.0001f)) * 20f;
            audioMixer.SetFloat(MUSIC_VOLUME_PARAM, dB);
        }
        
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.value = volume;
        }
        
        if (musicVolumeText != null)
        {
            musicVolumeText.text = $"{Mathf.Round(volume * 100)}%";
        }
    }

    public void SetSFXVolume(float volume)
    {
        if (audioMixer != null)
        {
            float dB = Mathf.Log10(Mathf.Max(volume, 0.0001f)) * 20f;
            audioMixer.SetFloat(SFX_VOLUME_PARAM, dB);
        }
        
        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.value = volume;
        }
        
        if (sfxVolumeText != null)
        {
            sfxVolumeText.text = $"{Mathf.Round(volume * 100)}%";
        }
    }

    public void LoadAudioSettings()
    {
        float musicVol = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 0.5f);
        float sfxVol = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 0.7f);
        
        SetMusicVolume(musicVol);
        SetSFXVolume(sfxVol);
        
        UpdateAudioTexts();
    }

    public void SaveAudioSettings()
    {
        float musicVol = musicVolumeSlider != null ? musicVolumeSlider.value : 0.5f;
        float sfxVol = sfxVolumeSlider != null ? sfxVolumeSlider.value : 0.7f;
        
        PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, musicVol);
        PlayerPrefs.SetFloat(SFX_VOLUME_KEY, sfxVol);
        PlayerPrefs.Save();
        
        SetMusicVolume(musicVol);
        SetSFXVolume(sfxVol);
    }

    private void UpdateAudioTexts()
    {
        if (musicVolumeText != null && musicVolumeSlider != null)
        {
            musicVolumeText.text = $"{Mathf.Round(musicVolumeSlider.value * 100)}%";
        }
        if (sfxVolumeText != null && sfxVolumeSlider != null)
        {
            sfxVolumeText.text = $"{Mathf.Round(sfxVolumeSlider.value * 100)}%";
        }
    }

    #endregion

    #region Music System

    private void SetupMusic()
    {
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = false;
            musicSource.playOnAwake = false;
        }

        if (musicTracks != null && musicTracks.Length > 0)
        {
            remainingTracks.Clear();
            remainingTracks.AddRange(musicTracks);
            PlayRandomTrack();
        }
    }

    private void PlayRandomTrack()
    {
        if (musicTracks == null || musicTracks.Length == 0) return;

        if (remainingTracks.Count == 0)
        {
            remainingTracks.Clear();
            remainingTracks.AddRange(musicTracks);
        }

        int randomIndex = Random.Range(0, remainingTracks.Count);
        AudioClip track = remainingTracks[randomIndex];
        remainingTracks.RemoveAt(randomIndex);

        StartCoroutine(PlayTrack(track));
    }

    private System.Collections.IEnumerator PlayTrack(AudioClip track)
    {
        musicSource.clip = track;
        musicSource.Play();
        isMusicPlaying = true;

        yield return new WaitForSeconds(track.length);

        if (RunningGame && !isInputLocked)
        {
            PlayRandomTrack();
        }
        else
        {
            isMusicPlaying = false;
        }
    }

    public void PauseMusic()
    {
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.Pause();
            isMusicPlaying = false;
        }
    }

    public void ResumeMusic()
    {
        if (musicSource != null && !musicSource.isPlaying && musicSource.clip != null)
        {
            if (musicSource.time >= musicSource.clip.length - 0.1f)
            {
                PlayRandomTrack();
            }
            else
            {
                musicSource.UnPause();
                isMusicPlaying = true;
            }
        }
        else if (musicSource != null && musicSource.clip == null)
        {
            PlayRandomTrack();
        }
    }

    #endregion

    #region Ad Buttons Control

    public void SetAdButtonsEnabled(bool enabled)
    {
        areAdButtonsEnabled = enabled;
    }

    #endregion

    #region Input Lock

    public void LockInput()
    {
        isInputLocked = true;
        PauseMusic();
    }

    public void UnlockInput()
    {
        isInputLocked = false;
        UpdateCursorState();
        if (RunningGame)
        {
            ResumeMusic();
        }
    }

    private void UpdateCursorState()
    {
        if (!RunningGame) return;
        
        if (isMobilePlatform)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public bool IsInputLocked()
    {
        return isInputLocked;
    }

    #endregion

    #region Speed Boost

    private void ActivateSpeedBoost()
    {
        if (speedBoostCoroutine != null)
        {
            StopCoroutine(speedBoostCoroutine);
        }
        speedBoostCoroutine = StartCoroutine(SpeedBoostCoroutine());
    }

    private System.Collections.IEnumerator SpeedBoostCoroutine()
    {
        isSpeedBoostActive = true;
        speedBoostTimer = 0f;
        
        walkSpeed = baseWalkSpeed * speedBoostMultiplier;
        runSpeed = baseRunSpeed * speedBoostMultiplier;
        
        while (speedBoostTimer < speedBoostDuration)
        {
            speedBoostTimer += Time.deltaTime;
            yield return null;
        }
        
        ResetSpeed();
    }

    private void ResetSpeed()
    {
        walkSpeed = baseWalkSpeed;
        runSpeed = baseRunSpeed;
        isSpeedBoostActive = false;
        speedBoostTimer = 0f;
        
        if (speedBoostCoroutine != null)
        {
            StopCoroutine(speedBoostCoroutine);
            speedBoostCoroutine = null;
        }
    }

    public bool IsSpeedBoostActive()
    {
        return isSpeedBoostActive;
    }

    public float GetSpeedBoostTimeRemaining()
    {
        if (!isSpeedBoostActive) return 0f;
        return speedBoostDuration - speedBoostTimer;
    }

    #endregion

    #region UI Setup

    public void ShowAdvReward(string callback)
    {
        if (!RunningGame) return;
        
        if (isAdShowing) return;
        if (!areAdButtonsEnabled) return;
        
        isAdShowing = true;
        LockInput();
        
        if (callback == "PlayerHouse")
        {
            YG2.RewardedAdvShow(callback, RewardPlayerHouse);
        }
        else if (callback == "SpeedPlayer")
        {
            YG2.RewardedAdvShow(callback, RewardSpeedPlayer);
        }
    }

    private void RewardPlayerHouse()
    {
        characterController.enabled = false;
        transform.position = HouseSpawn.position;
        characterController.enabled = true;
        OnAdComplete();
    }

    private void RewardSpeedPlayer()
    {
        ActivateSpeedBoost();
        OnAdComplete();
    }

    private void OnAdComplete()
    {
        isAdShowing = false;
        UnlockInput();
    }

    public void OnAdClosed()
    {
        isAdShowing = false;
        UnlockInput();
    }

    private void SetupUI()
    {
        if (sensitivitySlider != null)
        {
            sensitivitySlider.minValue = 0.1f;
            sensitivitySlider.maxValue = 3f;
            sensitivitySlider.value = sensitivityMultiplier;
            sensitivitySlider.onValueChanged.AddListener(OnSensitivitySliderChanged);
        }

        if (saveSensitivityButton != null)
        {
            saveSensitivityButton.onClick.AddListener(SaveSensitivity);
        }

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.minValue = 0f;
            musicVolumeSlider.maxValue = 1f;
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.minValue = 0f;
            sfxVolumeSlider.maxValue = 1f;
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        }

        if (saveAudioButton != null)
        {
            saveAudioButton.onClick.AddListener(SaveAudioSettings);
        }

        UpdateSensitivityText();
        UpdateAudioTexts();
    }

    private void OnSensitivitySliderChanged(float value)
    {
        sensitivityMultiplier = value;
        UpdateSensitivityText();
    }

    private void OnMusicVolumeChanged(float value)
    {
        SetMusicVolume(value);
        UpdateAudioTexts();
    }

    private void OnSFXVolumeChanged(float value)
    {
        SetSFXVolume(value);
        UpdateAudioTexts();
    }

    private void UpdateSensitivityText()
    {
        if (sensitivityValueText != null)
        {
            sensitivityValueText.text = $"{sensitivityMultiplier:F1}x";
        }
    }

    #endregion

    #region Initialization

    public void SetDeviceType(string deviceType)
    {
        isMobilePlatform = deviceType != "PC";
        LoadSensitivity();
        LoadAudioSettings();

        if (isMobilePlatform)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            if (mobilePanel != null) mobilePanel.SetActive(true);
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            if (mobilePanel != null) mobilePanel.SetActive(false);
        }
    }

    public void Initialized(string userSystem)
    {
        SetDeviceType(userSystem);
    }

    #endregion

    #region Settings Panel

    public void OpenSettings()
    {
        LockInput();
        Time.timeScale = 0f;
    }

    public void CloseSettings()
    {
        UnlockInput();
        Time.timeScale = 1f;
    }

    #endregion

    #region Look Handlers

    private void HandleMouseLook()
    {
        if (isInputLocked) return;
        
        float sensitivity = mouseSensitivity * sensitivityMultiplier;
        float mouseX = Input.GetAxis("Mouse X") * sensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity;

        transform.Rotate(Vector3.up * mouseX);

        cameraRotationX -= mouseY;
        cameraRotationX = Mathf.Clamp(cameraRotationX, lookDownLimit, lookUpLimit);
        playerCamera.transform.localRotation = Quaternion.Euler(cameraRotationX, 0f, 0f);
    }

    private void HandleTouchLook()
    {
        if (isInputLocked) return;
        
        ProcessTouchInput();

        if (currentLookDelta.magnitude > 0.01f)
        {
            ApplyLookDelta();
        }
    }

    private void ProcessTouchInput()
    {
        foreach (Touch touch in Input.touches)
        {
            if (IsTouchOverUI(touch)) continue;

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    OnTouchBegan(touch);
                    break;
                case TouchPhase.Moved:
                    OnTouchMoved(touch);
                    break;
                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    OnTouchEnded(touch);
                    break;
            }
        }
    }

    private bool IsTouchOverUI(Touch touch)
    {
        return EventSystem.current != null && 
               EventSystem.current.IsPointerOverGameObject(touch.fingerId);
    }

    private void OnTouchBegan(Touch touch)
    {
        lookFingerId = touch.fingerId;
        previousTouchPosition = touch.position;
        isLooking = true;
        currentLookDelta = Vector2.zero;
    }

    private void OnTouchMoved(Touch touch)
    {
        if (touch.fingerId != lookFingerId) return;

        Vector2 delta = touch.deltaPosition;
        if (delta.magnitude < 1f) return;

        float sensitivity = touchSensitivity * sensitivityMultiplier;
        delta *= sensitivity;

        currentLookDelta += delta;
        currentLookDelta = Vector2.ClampMagnitude(currentLookDelta, maxLookSpeed);
        previousTouchPosition = touch.position;
    }

    private void OnTouchEnded(Touch touch)
    {
        if (touch.fingerId != lookFingerId) return;

        lookFingerId = -1;
        isLooking = false;
        currentLookDelta = Vector2.Lerp(currentLookDelta, Vector2.zero, Time.deltaTime * 15f);
    }

    private void ApplyLookDelta()
    {
        Vector2 smoothDelta = Vector2.SmoothDamp(
            Vector2.zero,
            currentLookDelta,
            ref lookVelocity,
            lookSmoothTime
        );

        transform.Rotate(Vector3.up * smoothDelta.x);

        cameraRotationX -= smoothDelta.y;
        cameraRotationX = Mathf.Clamp(cameraRotationX, -80f, 80f);
        playerCamera.transform.localRotation = Quaternion.Euler(cameraRotationX, 0f, 0f);

        currentLookDelta = Vector2.Lerp(currentLookDelta, Vector2.zero, Time.deltaTime * 12f);
    }

    #endregion

    #region Movement Handlers

    private void HandleMovement()
    {
        if (isInputLocked) return;
        
        UpdateGroundedState();

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        
        float currentWalkSpeed = walkSpeed;
        float currentRunSpeed = runSpeed;
        
        if (isCrouching)
        {
            currentWalkSpeed *= crouchWalkSpeedMultiplier;
            currentRunSpeed *= crouchWalkSpeedMultiplier;
        }
        
        // Для ПК: зажата клавиша ИЛИ (для мобилки isRunning)
        bool runInput = Input.GetKey(runKey) || isRunning;
        currentSpeed = runInput ? currentRunSpeed : currentWalkSpeed;

        Vector3 move = (transform.right * horizontal + transform.forward * vertical);
        if (move.magnitude > 1f) move.Normalize();
        
        characterController.Move(move * currentSpeed * Time.deltaTime);
    }

    private void HandleMobileMovement()
    {
        if (isInputLocked) return;
        
        UpdateGroundedState();

        float horizontal = 0f;
        float vertical = 0f;

        if (movementJoystick != null)
        {
            horizontal = movementJoystick.Horizontal;
            vertical = movementJoystick.Vertical;
            
            float speedMultiplier = isCrouching ? crouchWalkSpeedMultiplier : 1f;
            float baseSpeed = isRunning ? runSpeed : walkSpeed; // isRunning true пока кнопка зажата
            currentSpeed = baseSpeed * speedMultiplier;
        }

        Vector3 move = (transform.right * horizontal + transform.forward * vertical);
        if (move.magnitude > 1f) move.Normalize();
        
        characterController.Move(move * currentSpeed * Time.deltaTime);
        // НЕ сбрасываем isRunning!
    }
    private void UpdateGroundedState()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
    }

    private void HandleJump()
    {
        if (isInputLocked) return;
        
        bool shouldJump = false;

        if (!isMobilePlatform)
        {
            shouldJump = Input.GetButtonDown("Jump") && isGrounded;
        }
        else
        {
            shouldJump = jumpPressed && isGrounded;
            jumpPressed = false;
        }

        if (shouldJump)
        {
            // Используем текущий jumpHeight (уже изменен в HandleCrouchInput)
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    private void ApplyGravity()
    {
        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }

    #endregion

    #region Game State

    public void StopGame()
    {
        RunningGame = false;
        Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        PauseMusic();
    }

    public void ResumeGame()
    {
        RunningGame = true;
        Time.timeScale = 1f;
        UpdateCursorState();
        ResumeMusic();
    }

    public void TogglePause()
    {
        if (RunningGame)
            StopGame();
        else
            ResumeGame();
    }

    #endregion

    #region Sensitivity Management

    public void LoadSensitivity()
    {
        sensitivityMultiplier = PlayerPrefs.GetFloat(SENSITIVITY_KEY, 1f);
        if (sensitivitySlider != null)
        {
            sensitivitySlider.value = sensitivityMultiplier;
        }
        UpdateSensitivityText();
    }

    public void SaveSensitivity()
    {
        PlayerPrefs.SetFloat(SENSITIVITY_KEY, sensitivityMultiplier);
        PlayerPrefs.Save();
        
        if (sensitivityValueText != null)
        {
            sensitivityValueText.text = $"{sensitivityMultiplier:F1}x ✓";
        }
    }

    public void ResetSensitivity()
    {
        sensitivityMultiplier = 1f;
        if (sensitivitySlider != null)
        {
            sensitivitySlider.value = 1f;
        }
        UpdateSensitivityText();
        SaveSensitivity();
    }

    public float GetSensitivity()
    {
        return sensitivityMultiplier;
    }

    public void SetSensitivity(float value)
    {
        sensitivityMultiplier = Mathf.Clamp(value, 0.1f, 3f);
        if (sensitivitySlider != null)
        {
            sensitivitySlider.value = sensitivityMultiplier;
        }
        UpdateSensitivityText();
    }

    #endregion

    #region UI Callbacks

    public void OnJumpButtonPressed()
    {
        jumpPressed = true;
    }

    #endregion

}