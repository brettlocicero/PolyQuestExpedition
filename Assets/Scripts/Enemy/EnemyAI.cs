using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Rigidbody))]
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

    [Header("VFX")]
    [SerializeField] AudioClip hitSFX;
    [SerializeField] GameObject deathFX;
    [SerializeField] Animator anim;
    [SerializeField] ParticleSystem damagedParticles;
    public ContactParticles contactParticles;
    [SerializeField] GameObject[] randomizedObjects;

    [Header("Drops")]
    [SerializeField] ItemDropObject[] itemDropObjects;

    AudioSource audioSource;
    Rigidbody rb;

    EnemyState state = EnemyState.Idle;

    float stunTimer = 0f;
    float sqrDistToTarget = Mathf.Infinity;

    bool tookDamage = false;
    bool attackOnCooldown = false;

    void Start()
    {
        health = maxHealth;

        audioSource = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody>();

        if (!target)
            target = PlayerInstance.instance.transform;

        if (anim)
            anim.speed = Random.Range(0.95f, 1.05f);

        RandomizeAppearance();
    }

    void Update()
    {
        if (target == null)
            return;

        sqrDistToTarget = (target.position - transform.position).sqrMagnitude;

        UpdateTimers();
        UpdateState();
    }

    void FixedUpdate()
    {
        if (target == null)
            return;

        RotateTowardsTarget();

        switch (state)
        {
            case EnemyState.Chasing:
                HandleMovement();
                break;
        }
    }

    void UpdateTimers()
    {
        if (stunTimer > 0f)
        {
            stunTimer -= Time.deltaTime;

            if (stunTimer < 0f)
                stunTimer = 0f;
        }
    }

    void UpdateState()
    {
        if (state == EnemyState.Attacking)
            return;

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
            if (!attackOnCooldown)
                StartCoroutine(AttackRoutine());

            return;
        }

        state = EnemyState.Chasing;
    }

    void HandleMovement()
    {
        Vector3 dir = (target.position - transform.position).normalized;
        dir.y = 0f;

        Vector3 move = dir * moveSpeed * Time.fixedDeltaTime;

        rb.MovePosition(rb.position + move);
    }

    void RotateTowardsTarget()
    {
        Vector3 dir = target.position - transform.position;
        dir.y = 0f;

        if (dir == Vector3.zero && !alwaysLookAtPlayer)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(dir);
        Quaternion smoothRotation = Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);

        rb.MoveRotation(smoothRotation);
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
            state = EnemyState.Stunned;
        else
            state = EnemyState.Chasing;
    }

    bool InAttackRange()
    {
        return sqrDistToTarget <= attackRange * attackRange;
    }

    public void TakeDamage(WeaponAttack attack)
    {
        tookDamage = true;
        health -= attack.damage;

        if (health <= 0)
        {
            Die();
            return;
        }

        PlayDamageAudio();
        PlayHitDirectionAnimation(attack.attackDirection);
        StunEnemy(attack.stunTime);

        if (damagedParticles)
            damagedParticles.Play();
    }

    public void TakeDamage(int damage, float stunTime)
    {
        tookDamage = true;
        health -= damage;

        if (health <= 0)
        {
            Die();
            return;
        }

        PlayDamageAudio();
        StunEnemy(stunTime);

        if (damagedParticles)
            damagedParticles.Play();
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
    }

    public void ApplyKnockback(Vector3 force)
    {
        rb.AddForce(force, ForceMode.Impulse);
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
            if (Random.value <= 0.3f)
                obj.SetActive(false);
        }
    }
}