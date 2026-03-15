using System.Collections;
using UnityEngine;

public abstract class WeaponObject : MonoBehaviour
{
    [SerializeField] WeaponSO weaponSO;
    
    [Header("Hitbox Settings")]
    [SerializeField] WeaponHitbox hitbox;

    [Header("References")]
    [SerializeField] Animator movementAnimator;
    [SerializeField] Animation attackAnimation;
    [SerializeField] CharacterController playerCC;

    float currentMovement = 0f;
    float velocity = 0f;

    float attackCounter = 0f;

    int comboIndex = 0;

    Coroutine hitboxCoroutine;

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
        attackCounter += Time.deltaTime;
        bool isAttacking = InputManager.Actions.Player.Attack.ReadValue<float>() > 0;
        if (isAttacking && attackCounter >= weaponSO.attackRate)
        {
            Attack();
        }
    }

    protected virtual void Attack()
    {
        WeaponAttack attack = weaponSO.attacks[comboIndex];

        if (hitboxCoroutine != null)
        {
            StopCoroutine(hitboxCoroutine);
        }

        hitboxCoroutine = StartCoroutine(HitboxWorker(attack));

        attackAnimation.Play(attack.attackAnimation.name);
        attackCounter = 0f;
        comboIndex = ++comboIndex % weaponSO.attacks.Length;

    }

    IEnumerator HitboxWorker(WeaponAttack attack)
    {
        yield return new WaitForSeconds(attack.hitboxTimeRange.x);
        hitbox.active = true;

        yield return new WaitForSeconds(attack.hitboxTimeRange.y);
        hitbox.active = false;
    }
}
