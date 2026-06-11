using UnityEngine;
using UnityEngine.EventSystems;
public class PlayerController : MonoBehaviour
{    
    public static PlayerController instatiate;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private Transform groundCheck;
    private float groundDistance = 0.4f;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private Joystick movementJoystick;

    [SerializeField] private CharacterController characterController;
    [SerializeField] float touchSensitivity = 0.48f;
    [SerializeField] float lookSmoothTime = 0.05f;
    [SerializeField] float maxLookSpeed = 8f;
    [SerializeField] float deadZone = 2f;

    private Vector2 lookVelocity;
    private Vector2 currentLookDelta;
    private int lookFingerId = -1;
    private float cameraRotationX = 0f;
    private Vector3 velocity;
    private bool isGrounded;
    private float currentSpeed;
    private bool isMobilePlatform = false;
    private bool jumpPressed = false;
    private float mouseSensitivity = 2f;
    private float lookUpLimit = 90f;
    private float lookDownLimit = -90f;
    private float walkSpeed = 5f;
    private float runSpeed = 10f;
    private float gravity = -9.81f;
    public bool RunningGame;
        public GameObject BlackPanelStopGame;
    public void SetDeviceType(string deviceType)
    {
        instatiate = this;
        isMobilePlatform = deviceType != "PC";
        
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
        public void StopGame()
    {
        RunningGame = false;
        BlackPanelStopGame.SetActive(true);
        Time.timeScale = 0f; // Останавливаем время в игре
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
    
    public void ResumeGame()
    {
        RunningGame = true;
        BlackPanelStopGame.SetActive(false);
        Time.timeScale = 1f; // Возобновляем время
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
    
    public void TogglePause()
    {
        if (RunningGame)
        {
            StopGame();
        }
        else
        {
            ResumeGame();
        }
    }
    private void Update()
    {

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
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
        
        HandleJump();
        ApplyGravity();
    }
    
    private void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        
        transform.Rotate(Vector3.up * mouseX);
        
        cameraRotationX -= mouseY;
        cameraRotationX = Mathf.Clamp(cameraRotationX, lookDownLimit, lookUpLimit);
        
        playerCamera.transform.localRotation = Quaternion.Euler(cameraRotationX, 0f, 0f);
    }
    
    private void HandleTouchLook()
    {

        foreach (Touch touch in Input.touches)
        {
            if (EventSystem.current != null &&
                EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                continue;

            // ТОЛЬКО правая часть экрана
            // if (touch.position.x < Screen.width * 0.5f)
            //     continue;

            if (touch.phase == TouchPhase.Began && lookFingerId == -1)
            {
                lookFingerId = touch.fingerId;
                currentLookDelta = Vector2.zero;
            }

            if (touch.fingerId != lookFingerId)
                continue;

            if (touch.phase == TouchPhase.Moved)
            {
                Vector2 delta = touch.deltaPosition;

                // 🛑 dead-zone как в MC
                if (delta.magnitude < deadZone)
                    return;

                delta *= touchSensitivity;

                // 🧠 накопление движения
                currentLookDelta += delta;

                // ⛔ лимит скорости
                currentLookDelta = Vector2.ClampMagnitude(currentLookDelta, maxLookSpeed);
            }

            if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                lookFingerId = -1;
            }
        }

        // 🧈 ПЛАВНОСТЬ как в Minecraft
        Vector2 smoothDelta = Vector2.SmoothDamp(
            Vector2.zero,
            currentLookDelta,
            ref lookVelocity,
            lookSmoothTime
        );

        transform.Rotate(Vector3.up * smoothDelta.x);
        cameraRotationX -= smoothDelta.y;
        cameraRotationX = Mathf.Clamp(cameraRotationX, -80f, 80f);

        playerCamera.transform.localRotation =
            Quaternion.Euler(cameraRotationX, 0f, 0f);

        // затухание
        currentLookDelta = Vector2.Lerp(currentLookDelta, Vector2.zero, Time.deltaTime * 10f);
    }

    
    private void HandleMovement()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        
        currentSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;
        
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        
        Vector3 move = transform.right * horizontal + transform.forward * vertical;
        
        if (move.magnitude > 1f)
        {
            move.Normalize();
        }
        
        characterController.Move(move * currentSpeed * Time.deltaTime);
    }
    
    private void HandleMobileMovement()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        
        float horizontal = 0f;
        float vertical = 0f;
        
        if (movementJoystick != null)
        {
            horizontal = movementJoystick.Horizontal;
            vertical = movementJoystick.Vertical;
            
            currentSpeed = walkSpeed;
        }
        
        Vector3 move = transform.right * horizontal + transform.forward * vertical;
        
        if (move.magnitude > 1f)
        {
            move.Normalize();
        }
        
        characterController.Move(move * currentSpeed * Time.deltaTime);
    }
    
    private void HandleJump()
    {
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
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }
    
    private void ApplyGravity()
    {
        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }

    public void Initialized(string userSystem)
    {
        SetDeviceType(userSystem);
    }
    
    public void OnJumpButtonPressed()
    {
        jumpPressed = true;
    }
    private void SetCursor(bool Set)
    {
        if(Set == true)
        {
            if (isMobilePlatform)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = Set;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = Set;
            }
        }
        if(Set == false)
        {
            if (isMobilePlatform)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = Set;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = Set;
            }
        }
    }

}
