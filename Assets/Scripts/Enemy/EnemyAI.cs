using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
using Type = System.Type;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(AudioSource))]
public class EnemyAI : MonoBehaviour
{
    // --- State Machine ---
    [Header("State Machine")]
    [Tooltip("Assign only the behavior modules this enemy should be allowed to use.")]
    [SerializeField] EnemyStateSO[] stateAssets;
    [Tooltip("Optional. If blank, the enemy starts with Chase when available, otherwise the first assigned state.")]
    [SerializeField] EnemyStateSO startingState;

    private EnemyBaseState currentState;
    private readonly Dictionary<Type, EnemyBaseState> statesByType = new Dictionary<Type, EnemyBaseState>();
    private readonly List<EnemyBaseState> selectableStates = new List<EnemyBaseState>();

    [Header("Identity & Target")]
    [SerializeField] string enemyName;
    public Transform target;

    [Header("Stats")]
    [SerializeField] int maxHealth = 30;
    private int health;

    [Header("Attack Settings")]
    public float attackRange = 2f;
    public float combatThresholdRange = 7f; 
    public float attackCooldown = 1f;
    [Tooltip("How long the enemy stands still to recover after an attack finishes.")]
    public float attackRecoveryDuration = 0.5f; // 0.5 seconds of post-attack freeze
    [HideInInspector] public float lastAttackTime;

    [Header("Movement")]
    public float engageDistance = 30f;
    public bool alwaysLookAtPlayer = false;
    public float rotationSpeed = 10f;

    [Header("Behavior Weights")]
    [Range(0f, 1f)] 
    [SerializeField] float chargeChance = 0.5f;


    [Header("FX & Polish")]
    [SerializeField] AudioClip hitSFX;
    [SerializeField] GameObject deathFX;
    public Animator anim;
    [SerializeField] ParticleSystem damagedParticles;
    public ContactParticles contactParticles;
    [SerializeField] GameObject[] randomizedObjects;
    [SerializeField] AudioClip[] passiveSounds;
    [SerializeField] Vector2 passiveAudioPitchRange;

    [Header("Drops")]
    [SerializeField] ItemDropObject[] itemDropObjects;

    // --- Component References ---
    [HideInInspector] public NavMeshAgent Agent;
    [HideInInspector] public AudioSource AudioSource;
    [HideInInspector] public Rigidbody Rb;

    // --- Runtime Trackers ---
    [HideInInspector] public bool tookDamage = false;
    [HideInInspector] public float stunTimer = 0f;
    [HideInInspector] public float sqrDistToTarget = Mathf.Infinity;

    EnemySpawnerObject enemySpawnerObject;

    void Awake()
    {
        Agent = GetComponent<NavMeshAgent>();
        AudioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        health = maxHealth;
        target = PlayerInstance.instance.transform;
        InitStates();

        Agent.updateRotation = false; // Stops NavMesh from forcing the enemy to face its walking direction
        Agent.updateUpAxis = false;   // Keeps the enemy upright smoothly

        if (anim)
            anim.speed = UnityEngine.Random.Range(0.95f, 1.05f);

        RandomizeAppearance();
        PlayPassiveSound();

        SwitchToStartingState();
    }

    void InitStates()
    {
        statesByType.Clear();
        selectableStates.Clear();

        foreach (EnemyStateSO stateAsset in stateAssets)
        {
            if (!stateAsset)
                continue;

            if (statesByType.ContainsKey(stateAsset.StateType))
            {
                Debug.LogWarning($"{name} has more than one {stateAsset.StateType.Name} assigned. Only the first one will be used.", this);
                continue;
            }

            EnemyBaseState state = stateAsset.CreateState(this);
            statesByType.Add(stateAsset.StateType, state);
            selectableStates.Add(state);
        }

        if (selectableStates.Count == 0)
            Debug.LogError($"{name} has no enemy states assigned. Add state assets to the EnemyAI component.", this);
    }

    void Update()
    {
        if (target == null) return;

        sqrDistToTarget = (target.position - transform.position).sqrMagnitude;

        // Manage our stun clock here globally
        if (stunTimer > 0f)
        {
            stunTimer -= Time.deltaTime;
            if (stunTimer <= 0f)
            {
                stunTimer = 0f;
                // If stun ended while we were in StunnedState, evaluate where to go next
                if (currentState == GetState<StunnedState>())
                    DetermineNextState();
            }
        }

        // Delegate logic execution to active state
        currentState?.UpdateState();
    }

    void SwitchToStartingState()
    {
        if (startingState && TrySwitchState(startingState.StateType))
            return;

        if (TrySwitchState<ChaseState>())
            return;

        if (selectableStates.Count > 0)
            SwitchState(selectableStates[0]);
    }

    public bool SwitchState(EnemyBaseState newState)
    {
        if (newState == null)
            return false;

        if (currentState == newState)
            return true;

        currentState?.ExitState();
        currentState = newState;
        currentState.EnterState();
        return true;
    }

    public bool TrySwitchState<T>() where T : EnemyBaseState
    {
        return TrySwitchState(typeof(T));
    }

    public bool TrySwitchSelectableState<T>() where T : EnemyBaseState
    {
        if (statesByType.TryGetValue(typeof(T), out EnemyBaseState state) &&
            state.Definition.CanBeSelected(this))
        {
            return SwitchState(state);
        }

        return false;
    }

    public bool TrySwitchState(Type stateType)
    {
        if (statesByType.TryGetValue(stateType, out EnemyBaseState state))
            return SwitchState(state);

        return false;
    }

    public T GetState<T>() where T : EnemyBaseState
    {
        if (statesByType.TryGetValue(typeof(T), out EnemyBaseState state))
            return state as T;

        return null;
    }

    public bool HasState<T>() where T : EnemyBaseState
    {
        return statesByType.ContainsKey(typeof(T));
    }

    public void DetermineNextState()
    {
        if (stunTimer > 0f)
        {
            TrySwitchState<StunnedState>();
            return;
        }

        bool isEngaged = sqrDistToTarget <= engageDistance * engageDistance || tookDamage;
        if (!isEngaged)
        {
            TrySwitchState<ChaseState>();
            return;
        }

        // --- MODULAR COMBAT SELECTION ---
        if (sqrDistToTarget <= combatThresholdRange * combatThresholdRange)
        {
            // If we are already right next to them and ready to attack, prioritize striking
            if (sqrDistToTarget <= attackRange * attackRange && CanAttack() && TrySwitchState<AttackState>())
            {
                return;
            }

            if (TrySwitchWeightedCombatState(true))
                return;
        }

        if (TrySwitchState<ChaseState>())
            return;

        TrySwitchWeightedCombatState(true);
    }

    public bool TrySwitchBestCombatState()
    {
        return TrySwitchWeightedCombatState(false);
    }

    bool TrySwitchWeightedCombatState(bool allowCurrentState)
    {
        float totalWeight = 0f;

        foreach (EnemyBaseState state in selectableStates)
        {
            if (!CanSelectCombatState(state, allowCurrentState))
                continue;

            totalWeight += GetStateWeight(state);
        }

        if (totalWeight <= 0f)
            return false;

        float roll = UnityEngine.Random.value * totalWeight;
        foreach (EnemyBaseState state in selectableStates)
        {
            if (!CanSelectCombatState(state, allowCurrentState))
                continue;

            roll -= GetStateWeight(state);
            if (roll <= 0f)
                return SwitchState(state);
        }

        return false;
    }

    bool CanSelectCombatState(EnemyBaseState state, bool allowCurrentState)
    {
        return state != null &&
               (allowCurrentState || state != currentState) &&
               !(state is ChaseState) &&
               !(state is StunnedState) &&
               state.Definition.CanBeSelected(this);
    }

    float GetStateWeight(EnemyBaseState state)
    {
        if (state is ChargeState)
            return Mathf.Max(0f, state.Definition.SelectionWeight * chargeChance);

        return state.Definition.SelectionWeight;
    }

    public bool CanAttack()
    {
        return Time.time >= lastAttackTime + attackCooldown;
    }

    // --- Combat / Damage Integrations ---

    public bool TakeDamage(WeaponAttack attack)
    {
        PlayHitDirectionAnimation(attack.attackDirection);
        return ApplyDamageCalculation(attack.damage, attack.stunTime, attack.attackDirection);
    }

    public bool TakeDamage(int damage, float stunTime)
    {
        return ApplyDamageCalculation(damage, stunTime);
    }

    private bool ApplyDamageCalculation(int damage, float stunDuration, AttackDirection direction = AttackDirection.Neutral)
    {
        tookDamage = true;
        health -= damage;

        if (health <= 0)
        {
            Die(direction);
            return true;
        }

        PlayDamageAudio();
        
        if (damagedParticles)
            damagedParticles.Play();

        // Trigger stun transition seamlessly
        StunEnemy(stunDuration);
        return false;
    }

    public void StunEnemy(float duration)
    {
        stunTimer = Mathf.Max(stunTimer, duration);
        TrySwitchState<StunnedState>();
    }

    public void ApplyKnockback(Vector3 force)
    {
        // Stub for now, need to figure out how to add knockback in?
    }

    // --- Contextual Helper Functions ---

    void PlayHitDirectionAnimation(AttackDirection direction)
    {
        if (!anim) return;

        switch (direction)
        {
            case AttackDirection.Left:
                anim.SetTrigger("HitLeft");
                break;
            case AttackDirection.Right:
                anim.SetTrigger("HitRight");
                break;
            default:
                anim.SetTrigger("HitLeft");
                break;
        }
    }

    void PlayDamageAudio()
    {
        if (!hitSFX) return;
        AudioSource.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
        AudioSource.PlayOneShot(hitSFX);
    }

    void Die(AttackDirection direction)
    {
        SpawnDrops();

        if (deathFX)
        {
            GameObject deathFXObj = Instantiate(deathFX, transform.position, transform.rotation);
            ApplyForcesToBody(deathFXObj, direction);
            Destroy(deathFXObj, 10f);
        }

        if (enemySpawnerObject)
        {
            enemySpawnerObject.RegisterEnemyDeath(this);
        }

        Destroy(gameObject);
    }

    void SpawnDrops()
    {
        foreach (ItemDropObject drop in itemDropObjects)
        {
            if (UnityEngine.Random.value <= drop.dropChance)
                Instantiate(drop, transform.position, transform.rotation);
        }
    }

    void ApplyForcesToBody(GameObject deathFXObj, AttackDirection direction)
    {
        Vector3 dir = -transform.forward * 10f;
        if (direction == AttackDirection.Left)
            dir = -transform.forward * 5f + transform.right * 10f;
        else if (direction == AttackDirection.Right)
            dir = -transform.forward * 5f - transform.right * 10f;

        Rigidbody[] rigidbodies = deathFXObj.GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody body in rigidbodies)
        {
            body.AddForce(dir, ForceMode.Impulse);
        }
    }

    void RandomizeAppearance()
    {
        foreach (GameObject obj in randomizedObjects)
        {
            if (UnityEngine.Random.value <= 0.4f)
                obj.SetActive(false);
        }
    }

    void PlayPassiveSound()
    {
        if (passiveSounds.Length > 0)
            StartCoroutine(Worker());
    }

    IEnumerator Worker()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.75f);
            AudioSource.pitch = UnityEngine.Random.Range(passiveAudioPitchRange.x, passiveAudioPitchRange.y);
            AudioSource.PlayOneShot(passiveSounds[UnityEngine.Random.Range(0, passiveSounds.Length)]);

            yield return new WaitForSeconds(UnityEngine.Random.Range(3f, 6f));

            AudioSource.pitch = UnityEngine.Random.Range(passiveAudioPitchRange.x, passiveAudioPitchRange.y);
            AudioSource.PlayOneShot(passiveSounds[UnityEngine.Random.Range(0, passiveSounds.Length)]);
        }
    }

    public void AttachEnemySpawnerObject(EnemySpawnerObject enemySpawnerObject)
    {
        this.enemySpawnerObject = enemySpawnerObject;
    }
}
