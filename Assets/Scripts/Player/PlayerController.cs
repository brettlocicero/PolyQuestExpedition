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

    [Header("Audio")]
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip[] footstepClips;
    [SerializeField] float stepInterval = 0.5f;
    [SerializeField] AudioClip jumpSound;
    [SerializeField] AudioClip landSound;

    [Header("Dash")]
    [SerializeField] Animator dashAnim;
    [SerializeField] float dashSpeed = 6f;
    [SerializeField] float dashDuration = 0.2f;
    [SerializeField] float dashCooldown = 1f;
    [SerializeField] float dashFOV = 105f;
    [SerializeField] float normalFOV = 90f;
    [SerializeField] float fovTweenTime = 0.15f;

    Tween fovTween;
    bool isDashing;
    Vector3 dashVelocity;
    float dashTimer = 0f;
    float lastDashTime;
    float stepTimer;

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
        HandleFootsteps();
        HandleDash();
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
        if (isDashing)
        {
            dashTimer -= Time.deltaTime;

            if (controller.isGrounded && yVelocity < 0)
                yVelocity = -2f;

            yVelocity += gravity * Time.deltaTime;

            float t = dashTimer / dashDuration;
            Vector3 currentVelocity = dashVelocity * t;

            currentVelocity.y = yVelocity;
            controller.Move(currentVelocity * Time.deltaTime);

            if (dashTimer <= 0f)
            {
                isDashing = false;
                ResetFOV();
            }

            return;
        }

        if (controller.isGrounded && yVelocity < -5f)
            audioSource.PlayOneShot(landSound);

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
            audioSource.PlayOneShot(jumpSound);
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

    void HandleFootsteps()
    {
        Vector2 moveInput = InputManager.Actions.Player.Move.ReadValue<Vector2>();
        bool isMoving = moveInput.magnitude > 0.1f && controller.isGrounded;

        if (!isMoving)
        {
            stepTimer = 0f;
            return;
        }

        stepTimer -= Time.deltaTime;

        if (stepTimer <= 0f)
        {
            PlayFootstep();
            stepTimer = stepInterval;
        }
    }

    void PlayFootstep()
    {
        if (footstepClips.Length == 0) return;

        AudioClip clip = footstepClips[Random.Range(0, footstepClips.Length)];
        audioSource.PlayOneShot(clip);
    }

    void HandleDash()
    {
        if (isDashing) return;

        if (InputManager.Actions.Player.Dash.triggered && Time.time >= lastDashTime + dashCooldown)
        {
            StartDash();
        }
    }

    void StartDash()
    {
        isDashing = true;
        lastDashTime = Time.time;
        PlayDashFOV();
        dashAnim.SetTrigger("Dash");

        Vector2 moveInput = InputManager.Actions.Player.Move.ReadValue<Vector2>();

        Vector3 dashDirection = transform.right * moveInput.x + transform.forward * moveInput.y;

        if (dashDirection.sqrMagnitude < 0.01f)
            dashDirection = transform.forward;

        dashDirection.Normalize();

        dashVelocity = dashDirection * dashSpeed;
        dashTimer = dashDuration;
    }

    void PlayDashFOV()
    {
        if (vcam == null) return;

        if (fovTween != null && fovTween.IsActive())
            fovTween.Kill();

        fovTween = DOTween.Sequence()
            .Append(DOTween.To(
                () => vcam.Lens.FieldOfView,
                x => vcam.Lens.FieldOfView = x,
                dashFOV,
                fovTweenTime * 0.5f
            ).SetEase(Ease.OutQuad))
            .Append(DOTween.To(
                () => vcam.Lens.FieldOfView,
                x => vcam.Lens.FieldOfView = x,
                normalFOV,
                fovTweenTime
            ).SetEase(Ease.InOutQuad));
    }

    void ResetFOV()
    {
        if (vcam == null) return;

        if (fovTween != null && fovTween.IsActive())
            fovTween.Kill();

        fovTween = DOTween.To(
            () => vcam.Lens.FieldOfView,
            x => vcam.Lens.FieldOfView = x,
            normalFOV,
            fovTweenTime
        ).SetEase(Ease.InOutQuad);
    }
}