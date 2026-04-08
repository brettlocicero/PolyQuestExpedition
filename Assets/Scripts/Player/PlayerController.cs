using UnityEngine;
using DG.Tweening;
using Unity.Cinemachine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] float speed = 6f;
    [SerializeField] float mouseSensitivity = 2f;
    [SerializeField] float jumpHeight = 1.5f;
    [SerializeField] float gravity = -9.81f;

    [Header("Camera")]
    [SerializeField] Transform cameraHolder;
    [SerializeField] CinemachineCamera vcam;

    [SerializeField] float bobAmount = 0.05f;
    [SerializeField] float bobSpeed = 8f;
    [SerializeField] float tiltAmount = 5f;
    [SerializeField] float tiltSmooth = 6f;

    CharacterController controller;

    float yVelocity;
    float xRotation;

    Vector3 cameraStartLocalPos;
    Tween bobTween;

    float currentTilt;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        CursorManager.LockCursor();

        cameraStartLocalPos = cameraHolder.localPosition;
    }

    void Update()
    {
        HandleLook();
        HandleMovement();
        HandleJump();
        HandleCameraEffects();
    }

    void HandleLook()
    {
        Vector2 lookInput = InputManager.Actions.Player.Look.ReadValue<Vector2>();

        float mouseX = lookInput.x * mouseSensitivity;
        float mouseY = lookInput.y * mouseSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        cameraHolder.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    void HandleMovement()
    {
        Vector2 moveInput = InputManager.Actions.Player.Move.ReadValue<Vector2>();

        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;

        if (controller.isGrounded && yVelocity < 0)
            yVelocity = -2f;

        yVelocity += gravity * Time.deltaTime;

        Vector3 velocity = move * speed;
        velocity.y = yVelocity;

        controller.Move(velocity * Time.deltaTime);
    }

    void HandleJump()
    {
        if (InputManager.Actions.Player.Jump.triggered && controller.isGrounded)
        {
            yVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    void HandleCameraEffects()
    {
        Vector2 moveInput = InputManager.Actions.Player.Move.ReadValue<Vector2>();
        bool isMoving = moveInput.magnitude > 0.1f && controller.isGrounded;

        if (isMoving)
        {
            if (bobTween == null || !bobTween.IsActive())
            {
                bobTween = cameraHolder.DOLocalMoveY(cameraStartLocalPos.y + bobAmount, 1f / bobSpeed)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetEase(Ease.InOutSine);
            }
        }
        
        else
        {
            if (bobTween != null && bobTween.IsActive())
                bobTween.Kill();

            cameraHolder.DOLocalMove(cameraStartLocalPos, 0.2f);
        }

        float targetTilt = -moveInput.x * tiltAmount;
        currentTilt = Mathf.Lerp(currentTilt, targetTilt, Time.deltaTime * tiltSmooth);

        if (vcam != null)
        {
            vcam.Lens.Dutch = currentTilt;
        }
    }
}