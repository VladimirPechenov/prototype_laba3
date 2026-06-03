using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 3.2f;
    [SerializeField] private float runSpeed = 5.8f;
    [SerializeField] private float crouchSpeed = 1.8f;
    [SerializeField] private float airControl = 0.55f;
    [SerializeField] private float jumpHeight = 1.1f;
    [SerializeField] private float gravity = -22f;

    [Header("Survival")]
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private float maxStamina = 5f;
    [SerializeField] private float staminaRecovery = 1.25f;
    [SerializeField] private float maxBattery = 45f;
    [SerializeField] private Light flashlight;

    [Header("Jump Assist")]
    [SerializeField] private float coyoteTime = 0.14f;
    [SerializeField] private float jumpBufferTime = 0.14f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundRadius = 0.24f;
    [SerializeField] private LayerMask groundMask = ~0;

    [Header("Look")]
    [SerializeField] private float mouseSensitivity = 0.12f;
    [SerializeField] private bool lockCursor = true;

    private CharacterController controller;
    private Vector3 verticalVelocity;
    private float coyoteTimer;
    private float jumpBufferTimer;
    private float stamina;
    private float battery;
    private float standingHeight;
    private int health;
    private PlayerHealth playerHealth;

    public bool IsGrounded { get; private set; }
    public bool IsCrouching { get; private set; }
    public bool FlashlightOn => flashlight != null && flashlight.enabled;
    public float Battery01 => maxBattery <= 0f ? 0f : battery / maxBattery;
    public float Stamina01 => maxStamina <= 0f ? 0f : stamina / maxStamina;
    public int Health => health;
    public float NoiseRadius { get; private set; }

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        standingHeight = controller.height;
        stamina = maxStamina;
        battery = maxBattery;
        health = maxHealth;
        playerHealth = GetComponent<PlayerHealth>();
    }

    private void Start()
    {
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void Update()
    {
        UpdateLook();
        UpdateGroundedState();
        UpdateJumpBuffer();
        UpdateFlashlight();
        Move();
        ApplyGravityAndJump();
        int displayHealth = playerHealth != null ? playerHealth.CurrentHealth : health;
        int displayMaxHealth = playerHealth != null ? playerHealth.MaxHealth : maxHealth;
        GameManager.Instance?.UpdatePlayerStatus(displayHealth, displayMaxHealth, Stamina01, Battery01, FlashlightOn, IsCrouching);
    }

    private void UpdateLook()
    {
        if (Mouse.current == null)
            return;

        Vector2 mouseDelta = Mouse.current.delta.ReadValue();
        transform.Rotate(Vector3.up * mouseDelta.x * mouseSensitivity);
    }

    private void UpdateGroundedState()
    {
        Vector3 checkPosition = groundCheck != null ? groundCheck.position : transform.position + Vector3.down * 0.95f;
        IsGrounded = Physics.CheckSphere(checkPosition, groundRadius, groundMask, QueryTriggerInteraction.Ignore);

        if (IsGrounded)
        {
            coyoteTimer = coyoteTime;
            if (verticalVelocity.y < 0f)
                verticalVelocity.y = -2f;
        }
        else
        {
            coyoteTimer -= Time.deltaTime;
        }
    }

    private void UpdateJumpBuffer()
    {
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            jumpBufferTimer = jumpBufferTime;
        else
            jumpBufferTimer -= Time.deltaTime;
    }

    private void Move()
    {
        Vector2 input = ReadMoveInput();
        Vector3 desiredMove = transform.right * input.x + transform.forward * input.y;
        desiredMove = Vector3.ClampMagnitude(desiredMove, 1f);

        bool hasMoveInput = input.sqrMagnitude > 0.01f;
        IsCrouching = Keyboard.current != null && Keyboard.current.leftCtrlKey.isPressed;
        bool wantsRun = Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed && !IsCrouching;
        bool isRunning = wantsRun && stamina > 0f && hasMoveInput;
        float speed = IsCrouching ? crouchSpeed : isRunning ? runSpeed : walkSpeed;
        float control = IsGrounded ? 1f : airControl;

        controller.height = Mathf.Lerp(controller.height, IsCrouching ? standingHeight * 0.55f : standingHeight, 12f * Time.deltaTime);
        controller.center = Vector3.down * ((standingHeight - controller.height) * 0.5f);

        if (isRunning)
            stamina = Mathf.Max(0f, stamina - Time.deltaTime);
        else
            stamina = Mathf.Min(maxStamina, stamina + staminaRecovery * Time.deltaTime);

        NoiseRadius = hasMoveInput ? IsCrouching ? 2f : isRunning ? 15f : 5f : 0f;
        controller.Move(desiredMove * speed * control * Time.deltaTime);
    }

    private void UpdateFlashlight()
    {
        if (flashlight == null || Keyboard.current == null)
            return;

        if (Keyboard.current.fKey.wasPressedThisFrame && battery > 0f)
            flashlight.enabled = !flashlight.enabled;

        if (!flashlight.enabled)
            return;

        battery = Mathf.Max(0f, battery - Time.deltaTime);
        flashlight.intensity = Mathf.Lerp(0.4f, 4f, Battery01);
        if (battery <= 0f)
            flashlight.enabled = false;
    }

    public void TakeDamage(int amount)
    {
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(amount);
            return;
        }

        health = Mathf.Max(0, health - amount);
        GameManager.Instance?.UpdatePlayerStatus(health, maxHealth, Stamina01, Battery01, FlashlightOn, IsCrouching);

        if (health <= 0)
        {
            health = maxHealth;
            stamina = maxStamina;
            transform.position = new Vector3(0f, 1.05f, -24f);
        }
    }

    private void ApplyGravityAndJump()
    {
        if (jumpBufferTimer > 0f && coyoteTimer > 0f)
        {
            verticalVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            jumpBufferTimer = 0f;
            coyoteTimer = 0f;
        }

        verticalVelocity.y += gravity * Time.deltaTime;
        controller.Move(verticalVelocity * Time.deltaTime);
    }

    private static Vector2 ReadMoveInput()
    {
        if (Keyboard.current == null)
            return Vector2.zero;

        Vector2 input = Vector2.zero;
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
            input.x -= 1f;
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
            input.x += 1f;
        if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
            input.y -= 1f;
        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
            input.y += 1f;

        return input;
    }
}
