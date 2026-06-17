using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class WeaponObject : MonoBehaviour
{
    [SerializeField] protected WeaponSO weaponSO;
    
    [Header("Runtime")]
    [SerializeField] List<ShardSO> upgrades = new();
    [SerializeField] WeaponRuntimeStats runtimeStats = new();

    [Header("Charge Settings")]
    [SerializeField] float maxChargeTime = 1f;
    [SerializeField] float rotationSmoothSpeed = 10f;
    [SerializeField] Vector3 maxChargeRotation;
    
    [Header("Object References")]
    [SerializeField] protected Transform attackOrigin;
    [SerializeField] Animator movementAnimator;
    [SerializeField] Animation attackAnimation;
    [SerializeField] CharacterController playerCC;
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioSource hitAudioSource;

    protected Transform mainCamTform;

    float currentMovement;
    float movementVelocity;

    float attackCounter;
    float chargeCounter;

    int comboIndex;

    bool inAttack = false;

    Vector3 currentPosition;
    Vector3 currentRotation;
    Vector3 targetRotation;
    Vector3 targetPosition;

    Coroutine attackRoutine;
    bool passivesInitialized;

    void Start()
    {
        attackCounter = GetAttackRate();
        currentRotation = transform.localEulerAngles;
        currentPosition = transform.localPosition;
        mainCamTform = Camera.main.transform;
        passivesInitialized = true;
        TriggerUpgrades(WeaponUpgradeType.Passive, null);
    }

    void Update()
    {
        HandleAttack();

        HandleRotation();
        HandleMovementAnimation();
    }

    void HandleRotation()
    {
        currentRotation = Vector3.Lerp(currentRotation, targetRotation, Time.deltaTime * rotationSmoothSpeed);
        transform.localEulerAngles = currentRotation;
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

        if (inAttack || InventoryManager.instance.isOpen) return;

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
            if (attackCounter >= GetAttackRate())
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

        attack = runtimeStats.ApplyTo(attack);
        TriggerUpgrades(WeaponUpgradeType.OnAttack, attack);

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

        WeaponHit[] hits = Attack(attack);
        if (hits.Length > 0)
        {
            StartCoroutine(TriggerHitstop(0.1f, attack.attackAnimation));
            PlayContactAudio();

            TriggerUpgrades(WeaponUpgradeType.OnHit, attack, hits);

            if (HasKilledEnemy(hits))
                TriggerUpgrades(WeaponUpgradeType.OnKill, attack, GetKilledHits(hits));
        }
        
        PlayAttackAudio(attack);

        yield return new WaitForSeconds(attack.attackDelay);

        inAttack = false;
        targetRotation = Vector3.zero;
    }

    protected abstract WeaponHit[] Attack(WeaponAttack attack);

    void PlayAttackAudio(WeaponAttack attack)
    {
        audioSource.pitch = Random.Range(attack.pitchRange.x, attack.pitchRange.y);
        audioSource.PlayOneShot(attack.sound);
    }

    IEnumerator TriggerHitstop(float duration, AnimationClip anim)
    {
        AnimationState state = attackAnimation[anim.name];

        float originalSpeed = state.speed;
        state.speed = 0f;

        yield return new WaitForSeconds(duration);

        state.speed = originalSpeed;
    }

    void PlayContactAudio()
    {
        // pitch already randomized from PlayAttackAudio
        hitAudioSource.pitch = Random.Range(0.9f, 1.1f);
        hitAudioSource.PlayOneShot(weaponSO.hitSound);
    }
    
    public void AddUpgrade(ShardSO upgradeSO)
    {
        if (!upgradeSO)
            return;

        int stacks = 0;
        foreach (ShardSO upgrade in upgrades)
        {
            if (upgrade == upgradeSO)
                stacks++;
        }

        if (stacks >= upgradeSO.MaxStacks)
            return;

        upgrades.Add(upgradeSO);

        if (passivesInitialized)
        {
            WeaponContext context = CreateContext(WeaponUpgradeType.Passive, null, null);
            upgradeSO.Apply(context);
        }
    }

    public void AddAttackSpeedPercent(float percentBonus)
    {
        runtimeStats.AddAttackSpeedPercent(percentBonus);
    }

    public void AddStunTime(float flatBonus, float percentBonus)
    {
        runtimeStats.AddStunTime(flatBonus, percentBonus);
    }

    public void AddKnockback(float flatBonus, float percentBonus)
    {
        runtimeStats.AddKnockback(flatBonus, percentBonus);
    }

    public float GetAttackRate()
    {
        return runtimeStats.GetAttackRate(weaponSO.attackRate);
    }
    
    void TriggerUpgrades(WeaponUpgradeType trigger, WeaponAttack attack, WeaponHit[] hits = null)
    {
        WeaponContext context = CreateContext(trigger, attack, hits);

        foreach (ShardSO upgradeSO in upgrades) 
        {
            if (!upgradeSO)
                continue;

            upgradeSO.Apply(context);
        }
    }

    WeaponContext CreateContext(WeaponUpgradeType trigger, WeaponAttack attack, WeaponHit[] hits)
    {
        hits ??= new WeaponHit[0];

        EnemyAI[] targets = new EnemyAI[hits.Length];
        for (int i = 0; i < hits.Length; i++)
        {
            targets[i] = hits[i].enemy;
        }

        return new WeaponContext
        {
            weapon = this,
            weaponSO = weaponSO,
            runtimeStats = runtimeStats,
            attacker = gameObject,
            attack = attack,
            trigger = trigger,
            targets = targets,
            hits = hits,
            hitPoint = hits.Length > 0 ? hits[0].point : Vector3.zero
        };
    }

    bool HasKilledEnemy(WeaponHit[] hits)
    {
        foreach (WeaponHit hit in hits)
        {
            if (hit.killed)
                return true;
        }

        return false;
    }

    WeaponHit[] GetKilledHits(WeaponHit[] hits)
    {
        List<WeaponHit> killedHits = new();

        foreach (WeaponHit hit in hits)
        {
            if (hit.killed)
                killedHits.Add(hit);
        }

        return killedHits.ToArray();
    }
}
