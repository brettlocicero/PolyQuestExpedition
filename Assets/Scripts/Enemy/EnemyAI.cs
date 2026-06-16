using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(AudioSource))]
public class EnemyAI : MonoBehaviour
{
    // --- State Machine ---
    private EnemyBaseState currentState;
    public ChaseState ChaseState { get; private set; }
    public StrafeState StrafeState { get; private set; }
    public AttackState AttackState { get; private set; }
    public StunnedState StunnedState { get; private set; }
    public ChargeState ChargeState { get; private set; }

    [Header("Identity & Target")]
    [SerializeField] string enemyName;
    public Transform target;

    [Header("Stats")]
    [SerializeField] int maxHealth = 30;
    private int health;

    [Header("Attack Settings")]
    public float attackRange = 2f;
    public float combatThresholdRange = 7f; // Triggers tactical movement
    public float attackCooldown = 1f;
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

    void Awake()
    {
        Agent = GetComponent<NavMeshAgent>();
        AudioSource = GetComponent<AudioSource>();
        Rb = GetComponent<Rigidbody>(); // Handled safely if you use knockback impulses
    }

    void Start()
    {
        health = maxHealth;
        target = PlayerInstance.instance.transform;
        InitStates();

        if (anim)
            anim.speed = Random.Range(0.95f, 1.05f);

        RandomizeAppearance();
        PlayPassiveSound();

        SwitchState(ChaseState);
    }

    void InitStates()
    {
        ChaseState = new ChaseState(this);
        StrafeState = new StrafeState(this);
        AttackState = new AttackState(this);
        StunnedState = new StunnedState(this);
        ChargeState = new ChargeState(this);
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
                if (currentState == StunnedState)
                    DetermineNextState();
            }
        }

        // Delegate logic execution to active state
        currentState?.UpdateState();
    }

    public void SwitchState(EnemyBaseState newState)
    {
        currentState?.ExitState();
        currentState = newState;
        currentState.EnterState();
    }

    public void DetermineNextState()
    {
        if (stunTimer > 0f)
        {
            SwitchState(StunnedState);
            return;
        }

        bool isEngaged = sqrDistToTarget <= engageDistance * engageDistance || tookDamage;
        if (!isEngaged)
        {
            SwitchState(ChaseState);
            return;
        }

        // --- RANDOMIZED COMBAT SELECTION ---
        if (sqrDistToTarget <= combatThresholdRange * combatThresholdRange)
        {
            // If we are already right next to them and ready to attack, prioritize striking
            if (sqrDistToTarget <= attackRange * attackRange && CanAttack())
            {
                SwitchState(AttackState);
                return;
            }

            // Otherwise, roll a random value between 0.0 and 1.0 to pick a tactical approach
            if (Random.value <= chargeChance)
            {
                SwitchState(ChargeState); // Go hyper-aggressive
            }
            else
            {
                SwitchState(StrafeState); // Play it smart and circle
            }
        }
        else
        {
            SwitchState(ChaseState); // Close the distance across the map
        }
    }

    public bool CanAttack()
    {
        return Time.time >= lastAttackTime + attackCooldown;
    }

    // --- Combat / Damage Integrations ---

    public bool TakeDamage(WeaponAttack attack)
    {
        PlayHitDirectionAnimation(attack.attackDirection);
        return ApplyDamageCalculation(attack.damage, attack.stunTime);
    }

    public bool TakeDamage(int damage, float stunTime)
    {
        return ApplyDamageCalculation(damage, stunTime);
    }

    private bool ApplyDamageCalculation(int damage, float stunDuration)
    {
        tookDamage = true;
        health -= damage;

        if (health <= 0)
        {
            Die();
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
        SwitchState(StunnedState);
    }

    public void ApplyKnockback(Vector3 force)
    {
        if (Rb != null)
        {
            Rb.AddForce(force, ForceMode.Impulse);
        }
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
        AudioSource.pitch = Random.Range(0.9f, 1.1f);
        AudioSource.PlayOneShot(hitSFX);
    }

    void Die()
    {
        SpawnDrops();

        if (deathFX)
        {
            GameObject deathFXObj = Instantiate(deathFX, transform.position, transform.rotation);
            ApplyForcesToBody(deathFXObj);
            Destroy(deathFXObj, 10f);
        }

        Destroy(gameObject);
    }

    void SpawnDrops()
    {
        foreach (ItemDropObject drop in itemDropObjects)
        {
            if (Random.value <= drop.dropChance)
                Instantiate(drop, transform.position, transform.rotation);
        }
    }

    void ApplyForcesToBody(GameObject deathFXObj)
    {
        Rigidbody[] rigidbodies = deathFXObj.GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody body in rigidbodies)
        {
            body.AddForce(-transform.forward * 300f);
        }
    }

    void RandomizeAppearance()
    {
        foreach (GameObject obj in randomizedObjects)
        {
            if (Random.value <= 0.4f)
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
            AudioSource.pitch = Random.Range(passiveAudioPitchRange.x, passiveAudioPitchRange.y);
            AudioSource.PlayOneShot(passiveSounds[Random.Range(0, passiveSounds.Length)]);

            yield return new WaitForSeconds(Random.Range(3f, 6f));

            AudioSource.pitch = Random.Range(passiveAudioPitchRange.x, passiveAudioPitchRange.y);
            AudioSource.PlayOneShot(passiveSounds[Random.Range(0, passiveSounds.Length)]);
        }
    }
}