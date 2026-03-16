using System.Collections;
using UnityEngine;

public abstract class WeaponObject : MonoBehaviour
{
    [SerializeField] protected WeaponSO weaponSO;

    [Header("Settings")]
    [SerializeField] float maxChargeTime = 1f;
    [SerializeField] Vector3 maxChargeRotation;
    
    [Header("References")]
    [SerializeField] protected Transform attackOrigin;
    [SerializeField] Animator movementAnimator;
    [SerializeField] Animation attackAnimation;
    [SerializeField] CharacterController playerCC;

    float currentMovement = 0f;
    float velocity = 0f;

    float attackCounter = 0f;
    float chargeCounter = 0f;

    int comboIndex = 0;

    bool isAttacking = false;

    Vector3 currentRotation;

    void Start()
    {
        attackCounter = weaponSO.attackRate;
    }

    void Update()
    {
        HandleMovementAnimation();
        HandleAttack();
    }

    void HandleMovementAnimation()
    {
        float targetMovement = InputManager.Actions.Player.Move.ReadValue<Vector2>().magnitude;
        if (!playerCC.isGrounded) targetMovement = 0;

        currentMovement = Mathf.SmoothDamp(currentMovement, targetMovement, ref velocity, 0.1f);
        movementAnimator.SetFloat("Movement", currentMovement);
    }

    void HandleAttack()
    {
        if (isAttacking) return;

        attackCounter += Time.deltaTime;
        bool attackPressed = InputManager.Actions.Player.Attack.IsPressed();
        bool attackReleased = InputManager.Actions.Player.Attack.WasReleasedThisFrame();
        transform.localEulerAngles = Vector3.Lerp(Vector3.zero, maxChargeRotation, chargeCounter / maxChargeTime);

        WeaponAttack attack = weaponSO.attacks[comboIndex];
        if (attackPressed)
        {
            chargeCounter += Time.deltaTime;
            chargeCounter = Mathf.Clamp(chargeCounter, 0f, maxChargeTime);
        }

        if (attackReleased && attackCounter >= weaponSO.attackRate)
        {
            if (chargeCounter >= maxChargeTime / 2f)
                attack = weaponSO.heavyAttack;

            attackAnimation.Rewind(attack.attackAnimation.name);
            attackAnimation.Play(attack.attackAnimation.name);

            StartCoroutine(AttackWorker(attack));

            comboIndex = ++comboIndex % weaponSO.attacks.Length;
            attackCounter = 0f;
            chargeCounter = 0f;
        }

        else if (attackReleased)
        {
            chargeCounter = 0f;
        }
    }

    IEnumerator AttackWorker(WeaponAttack attack)
    {
        isAttacking = true;

        yield return new WaitForSeconds(attack.attackDelay);
        Attack(attack);

        isAttacking = false;
    }

    protected abstract void Attack(WeaponAttack attack);
}
