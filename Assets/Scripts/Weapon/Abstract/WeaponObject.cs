using System.Collections;
using UnityEngine;

public abstract class WeaponObject : MonoBehaviour
{
    [SerializeField] protected WeaponSO weaponSO;
    
    [Header("References")]
    [SerializeField] protected Transform attackOrigin;
    [SerializeField] Animator movementAnimator;
    [SerializeField] Animation attackAnimation;
    [SerializeField] CharacterController playerCC;

    float currentMovement = 0f;
    float velocity = 0f;

    float attackCounter = 0f;

    int comboIndex = 0;

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
            WeaponAttack attack = weaponSO.attacks[comboIndex];

            attackAnimation.Rewind(attack.attackAnimation.name);
            attackAnimation.Play(attack.attackAnimation.name);

            StartCoroutine(AttackWorker(attack));

            comboIndex = ++comboIndex % weaponSO.attacks.Length;
            attackCounter = 0f;
        }
    }

    IEnumerator AttackWorker(WeaponAttack attack)
    {
        yield return new WaitForSeconds(attack.attackDelay);
        Attack(attack);
    }

    protected abstract void Attack(WeaponAttack attack);
}
