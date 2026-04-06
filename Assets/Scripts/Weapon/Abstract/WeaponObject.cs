using System.Collections;
using UnityEngine;

public abstract class WeaponObject : MonoBehaviour
{
    [SerializeField] protected WeaponSO weaponSO;

    [Header("Charge Settings")]
    [SerializeField] float maxChargeTime = 1f;
    [SerializeField] float rotationSmoothSpeed = 10f;
    [SerializeField] Vector3 maxChargeRotation;
    
    [Header("Block Settings")]
    [SerializeField] float blockSpeed = 2f;
    [SerializeField] Vector3 maxBlockRotation;
    [SerializeField] Vector3 maxBlockPosition;

    [Header("Object References")]
    [SerializeField] protected Transform attackOrigin;
    [SerializeField] Animator movementAnimator;
    [SerializeField] Animation attackAnimation;
    [SerializeField] CharacterController playerCC;
    [SerializeField] AudioSource audioSource;

    protected Transform mainCamTform;

    float currentMovement;
    float movementVelocity;

    float attackCounter;
    float chargeCounter;

    int comboIndex;

    bool inAttack = false;
    bool inBlock = false;

    Vector3 currentPosition;
    Vector3 currentRotation;
    Vector3 targetRotation;
    Vector3 targetPosition;

    Coroutine attackRoutine;

    void Start()
    {
        attackCounter = weaponSO.attackRate;
        currentRotation = transform.localEulerAngles;
        currentPosition = transform.localPosition;
        mainCamTform = Camera.main.transform;
    }

    void Update()
    {
        HandleBlock();
        HandleAttack();

        HandleRotation();
        HandlePosition();
        HandleMovementAnimation();
    }

    void HandleRotation()
    {
        currentRotation = Vector3.Lerp(currentRotation, targetRotation, Time.deltaTime * rotationSmoothSpeed);
        transform.localEulerAngles = currentRotation;
    }
    
    void HandlePosition() 
    {
        currentPosition = Vector3.Lerp(currentPosition, targetPosition, Time.deltaTime * blockSpeed);
        transform.localPosition = currentPosition;
    }

    void HandleMovementAnimation()
    {
        float targetMovement = InputManager.Actions.Player.Move.ReadValue<Vector2>().magnitude;

        if (!playerCC.isGrounded)
            targetMovement = 0f;

        currentMovement = Mathf.SmoothDamp(currentMovement, targetMovement, ref movementVelocity, 0.1f);
        movementAnimator.SetFloat("Movement", currentMovement);
    }

    void HandleAttack()
    {
        attackCounter += Time.deltaTime;

        if (inAttack || inBlock) return;

        bool attackPressed = InputManager.Actions.Player.Attack.IsPressed();
        bool attackReleased = InputManager.Actions.Player.Attack.WasReleasedThisFrame();

        WeaponAttack attack = weaponSO.attacks[comboIndex];

        if (attackPressed)
        {
            chargeCounter += Time.deltaTime;
            chargeCounter = Mathf.Clamp(chargeCounter, 0f, maxChargeTime);

            float chargePercent = chargeCounter / maxChargeTime;
            targetRotation = Vector3.Lerp(Vector3.zero, maxChargeRotation, chargePercent);
        }

        if (attackReleased)
        {
            if (attackCounter >= weaponSO.attackRate)
            {
                if (chargeCounter >= maxChargeTime * 0.5f)
                    attack = weaponSO.heavyAttack;

                PerformAttack(attack);
            }

            chargeCounter = 0f;
            targetRotation = Vector3.zero;
        }
    }

    void PerformAttack(WeaponAttack attack)
    {
        if (attackRoutine != null)
            StopCoroutine(attackRoutine);

        attackAnimation.Rewind(attack.attackAnimation.name);
        attackAnimation.Play(attack.attackAnimation.name);

        attackCounter = 0f;
        attackRoutine = StartCoroutine(AttackWorker(attack));

        comboIndex = (comboIndex + 1) % weaponSO.attacks.Length;
        attackCounter -= attack.attackRatePenalty;
    }

    IEnumerator AttackWorker(WeaponAttack attack)
    {
        inAttack = true;

        yield return new WaitForSeconds(attack.attackDelay);

        bool hitEnemy = Attack(attack);
        if (hitEnemy) 
        {
            StartCoroutine(TriggerHitstop(0.05f, attack.attackAnimation));
        }
        
        PlayAttackAudio(attack);

        yield return new WaitForSeconds(attack.attackDelay);

        inAttack = false;
        targetRotation = Vector3.zero;
    }

    protected abstract bool Attack(WeaponAttack attack);

    void PlayAttackAudio(WeaponAttack attack)
    {
        audioSource.pitch = Random.Range(attack.pitchRange.x, attack.pitchRange.y);
        audioSource.PlayOneShot(attack.sound);
    }

    void HandleBlock()
    {
        if (inAttack) return;

        inBlock = InputManager.Actions.Player.SecondaryAttack.IsPressed();

        if (inBlock)
        {
            targetRotation = maxBlockRotation;
            targetPosition = maxBlockPosition;
        }
        
        else if (!InputManager.Actions.Player.Attack.IsPressed())
        {
            targetRotation = Vector3.zero;
            targetPosition = Vector3.zero;
        }
    }

    IEnumerator TriggerHitstop(float duration, AnimationClip anim)
    {
        AnimationState state = attackAnimation[anim.name];

        float originalSpeed = state.speed;
        state.speed = 0f;

        yield return new WaitForSeconds(duration);

        state.speed = originalSpeed;
    }
}