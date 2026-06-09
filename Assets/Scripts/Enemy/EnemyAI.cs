using System.Collections;
using UnityEngine;
using UnityEngine.AI; // Required for NavMeshAgent

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(NavMeshAgent))] // Replaced Rigidbody with NavMeshAgent
public class EnemyAI : MonoBehaviour
{
    enum EnemyState
    {
        Idle,
        Chasing,
        Attacking,
        Stunned
    }

    [SerializeField] string enemyName;
    [SerializeField] Transform target;

    [Header("Stats")]
    [SerializeField] int maxHealth = 30;
    int health;

    [Header("Attack")]
    [SerializeField] float attackRange = 2f;
    [SerializeField] float attackCooldown = 1f;

    [Header("Movement")]
    [SerializeField] float engageDistance = 30f;
    [SerializeField] bool alwaysLookAtPlayer = false;
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float rotationSpeed = 10f;

    [Header("FX")]
    [SerializeField] AudioClip hitSFX;
    [SerializeField] GameObject deathFX;
    [SerializeField] Animator anim;
    [SerializeField] ParticleSystem damagedParticles;
    public ContactParticles contactParticles;
    [SerializeField] GameObject[] randomizedObjects;
    [SerializeField] AudioClip[] passiveSounds;
    [SerializeField] Vector2 passiveAudioPitchRange;

    [Header("Drops")]
    [SerializeField] ItemDropObject[] itemDropObjects;

    AudioSource audioSource;
    NavMeshAgent agent; // Replaced Rigidbody reference

    EnemyState state = EnemyState.Idle;

    float stunTimer = 0f;
    float sqrDistToTarget = Mathf.Infinity;

    bool tookDamage = false;
    bool attackOnCooldown = false;

    Coroutine attackCoroutine;

    void Start()
    {
        health = maxHealth;

        audioSource = GetComponent<AudioSource>();
        agent = GetComponent<NavMeshAgent>();

        // Configure NavMeshAgent using your inspector values
        agent.speed = moveSpeed;
        agent.stoppingDistance = attackRange;
        agent.updateRotation = alwaysLookAtPlayer; // Let NavMesh handle rotation unless overridden

        if (!target && PlayerInstance.instance != null)
            target = PlayerInstance.instance.transform;

        if (anim)
            anim.speed = Random.Range(0.95f, 1.05f);

        RandomizeAppearance();
        PlayPassiveSound();
    }

    void Update()
    {
        if (target == null)
            return;

        sqrDistToTarget = (target.position - transform.position).sqrMagnitude;

        UpdateTimers();
        UpdateState();
        HandleMovementState();
    }

    void FixedUpdate()
    {
        if (target == null)
            return;

        // If not using NavMesh built-in rotation, manually rotate
        if (!alwaysLookAtPlayer)
        {
            RotateTowardsTarget();
        }
    }

    void UpdateTimers()
    {
        if (stunTimer > 0f)
        {
            stunTimer -= Time.deltaTime;
            if (stunTimer <= 0f)
            {
                stunTimer = 0f;
            }
        }
    }

    void UpdateState()
    {
        if (stunTimer > 0f)
        {
            state = EnemyState.Stunned;
            return;
        }

        bool isEngaged = sqrDistToTarget <= engageDistance * engageDistance || tookDamage;

        if (!isEngaged)
        {
            state = EnemyState.Idle;
            return;
        }

        bool inAttackRange = sqrDistToTarget <= attackRange * attackRange;

        if (inAttackRange)
        {
            if (!attackOnCooldown && state != EnemyState.Attacking)
                attackCoroutine = StartCoroutine(AttackRoutine());

            return;
        }

        if (state != EnemyState.Attacking)
            state = EnemyState.Chasing;
    }

    void HandleMovementState()
    {
        switch (state)
        {
            case EnemyState.Idle:
            case EnemyState.Stunned:
            case EnemyState.Attacking:
                // Safely halt the agent pathfinding
                if (agent.isOnNavMesh)
                {
                    agent.isStopped = true;
                    agent.velocity = Vector3.zero;
                }
                break;

            case EnemyState.Chasing:
                if (agent.isOnNavMesh)
                {
                    agent.isStopped = false;
                    agent.SetDestination(target.position);
                }
                break;
        }
    }

    void RotateTowardsTarget()
    {
        Vector3 dir = target.position - transform.position;
        dir.y = 0f;

        if (dir == Vector3.zero)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    IEnumerator AttackRoutine()
    {
        state = EnemyState.Attacking;
        attackOnCooldown = true;

        if (anim)
            anim.SetTrigger("Attack");

        yield return new WaitForSeconds(attackCooldown);

        attackOnCooldown = false;

        if (stunTimer > 0f)
        {
            state = EnemyState.Stunned;
            yield break;
        }

        state = EnemyState.Idle;
    }

    public bool TakeDamage(WeaponAttack attack)
    {
        tookDamage = true;
        health -= attack.damage;

        if (health <= 0)
        {
            Die();
            return true;
        }

        PlayDamageAudio();
        PlayHitDirectionAnimation(attack.attackDirection);
        StunEnemy(attack.stunTime);

        if (damagedParticles)
            damagedParticles.Play();

        return false;
    }

    public bool TakeDamage(int damage, float stunTime)
    {
        tookDamage = true;
        health -= damage;

        if (health <= 0)
        {
            Die();
            return true;
        }

        PlayDamageAudio();
        StunEnemy(stunTime);

        if (damagedParticles)
            damagedParticles.Play();

        return false;
    }

    void PlayHitDirectionAnimation(AttackDirection direction)
    {
        if (!anim)
            return;

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

    public void StunEnemy(float duration)
    {
        stunTimer = Mathf.Max(stunTimer, duration);
        
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            attackOnCooldown = false;
        }
    }

    public void ApplyKnockback(Vector3 force)
    {
        // NavMeshAgents don't natively react to Rigidbody forces well. 
        // We temporarily disable the agent component so it can be pushed, then re-enable it.
        StartCoroutine(KnockbackRoutine(force));
    }

    IEnumerator KnockbackRoutine(Vector3 force)
    {
        Rigidbody knockbackRb = GetComponent<Rigidbody>();
        
        // If you don't keep a Rigidbody attached, you can instead use agent.Move(force)
        if (knockbackRb != null)
        {
            agent.enabled = false;
            knockbackRb.isKinematic = false;
            knockbackRb.AddForce(force, ForceMode.Impulse);
            
            yield return new WaitForSeconds(0.2f); // Duration of velocity override
            
            knockbackRb.isKinematic = true;
            agent.enabled = true;
        }
    }

    void PlayDamageAudio()
    {
        if (!hitSFX)
            return;

        audioSource.pitch = Random.Range(0.9f, 1.1f);
        audioSource.PlayOneShot(hitSFX);
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
            audioSource.pitch = Random.Range(passiveAudioPitchRange.x, passiveAudioPitchRange.y);
            audioSource.PlayOneShot(passiveSounds[Random.Range(0, passiveSounds.Length)]);

            yield return new WaitForSeconds(Random.Range(3f, 6f));

            audioSource.pitch = Random.Range(passiveAudioPitchRange.x, passiveAudioPitchRange.y);
            audioSource.PlayOneShot(passiveSounds[Random.Range(0, passiveSounds.Length)]);
        }
    }
}