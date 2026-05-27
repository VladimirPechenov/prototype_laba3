using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 8f;
    [SerializeField] private float airControl = 0.55f;
    [SerializeField] private float jumpHeight = 1.6f;
    [SerializeField] private float gravity = -22f;

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

    public bool IsGrounded { get; private set; }

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
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
        Move();
        ApplyGravityAndJump();
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

        bool isRunning = Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed;
        float speed = isRunning ? runSpeed : walkSpeed;
        float control = IsGrounded ? 1f : airControl;

        controller.Move(desiredMove * speed * control * Time.deltaTime);
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
