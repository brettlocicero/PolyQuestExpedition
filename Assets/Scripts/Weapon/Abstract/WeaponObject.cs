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
    [SerializeField] AudioSource audioSource;

    float currentMovement;
    float movementVelocity;

    float attackCounter;
    float chargeCounter;

    int comboIndex;

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

        if (!playerCC.isGrounded)
            targetMovement = 0f;

        currentMovement = Mathf.SmoothDamp(currentMovement, targetMovement, ref movementVelocity, 0.1f);
        movementAnimator.SetFloat("Movement", currentMovement);
    }

    void HandleAttack()
    {
        attackCounter += Time.deltaTime;

        bool attackPressed = InputManager.Actions.Player.Attack.IsPressed();
        bool attackReleased = InputManager.Actions.Player.Attack.WasReleasedThisFrame();

        transform.localEulerAngles = Vector3.Slerp(Vector3.zero, maxChargeRotation, chargeCounter / maxChargeTime);

        WeaponAttack attack = weaponSO.attacks[comboIndex];

        if (attackPressed)
        {
            chargeCounter += Time.deltaTime;
            chargeCounter = Mathf.Clamp(chargeCounter, 0f, maxChargeTime);
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
        }
    }

    void PerformAttack(WeaponAttack attack)
    {
        attackAnimation.Rewind(attack.attackAnimation.name);
        attackAnimation.Play(attack.attackAnimation.name);

        attackCounter = 0f;

        StartCoroutine(AttackWorker(attack));

        comboIndex = (comboIndex + 1) % weaponSO.attacks.Length;

        attackCounter -= attack.attackRatePenalty;
    }

    IEnumerator AttackWorker(WeaponAttack attack)
    {
        yield return new WaitForSeconds(attack.attackDelay);

        Attack(attack);
        PlayAttackAudio(attack);
    }

    protected abstract void Attack(WeaponAttack attack);

    void PlayAttackAudio(WeaponAttack attack)
    {
        audioSource.pitch = Random.Range(attack.pitchRange.x, attack.pitchRange.y);
        audioSource.PlayOneShot(attack.sound);
    }
}