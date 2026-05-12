using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Rigidbody))]
public class EnemyAI : MonoBehaviour
{
    [SerializeField] string enemyName;
    [SerializeField] Transform target;
    
    [Header("Stats")]
    [SerializeField] int health = 30;
    [SerializeField] int maxHealth = 30;

    [Header("Attack")]
    [SerializeField] float attackRange = 2f;
    [SerializeField] float attackTime = 1f;

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

    bool isStunned = false;
    bool isEngaged = false;
    float stunTimer = 0f;

    bool tookDamage = false;
    bool isAttacking = false;

    float sqrDistToTarget = 1000000f;

    void Start()
    {
        health = maxHealth;

        audioSource = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody>();
        
        anim.speed = Random.Range(0.95f, 1.05f);
        
        if (!target) target = PlayerInstance.instance.transform;

        RandomizeAppearance();
    }

    void Update()
    {
        HandleStunTimer();
        HandleAttacking();
    }

    void HandleEngaging()
    {
        isEngaged = sqrDistToTarget <= engageDistance * engageDistance || tookDamage;
    }

    void HandleAttacking()
    {
        if (isStunned || isAttacking || !isEngaged) return;

        if (InAttackRange())
            StartCoroutine(AttackWorker());

        IEnumerator AttackWorker()
        {
            anim.SetTrigger("Attack");
            isAttacking = true;

            yield return new WaitForSeconds(attackTime);

            isAttacking = false;
        }
    }

    void HandleStunTimer()
    {
        isStunned = stunTimer > 0f;
        if (stunTimer >= 0f) stunTimer -= Time.deltaTime;
    }

    void FixedUpdate()
    {
        HandleMovement();
        HandleEngaging();
    }

    void HandleMovement()
    {
        sqrDistToTarget = Vector3.SqrMagnitude(target.position - transform.position);
        
        if (target == null || isStunned || !isEngaged) return;

        Vector3 dir = (target.position - transform.position).normalized;
        dir.y = 0f;

        if (!isAttacking)
        {
            Vector3 move = moveSpeed * Time.fixedDeltaTime * dir;
            if (!InAttackRange()) rb.MovePosition(rb.position + move);
        }


        if (dir != Vector3.zero || alwaysLookAtPlayer)
        {
            Quaternion targetRotation = Quaternion.LookRotation(dir);
            Quaternion smoothRotation = Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);

            rb.MoveRotation(smoothRotation);
        }
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
        }
        
        else 
        {
            PlayDamageAudio();
            PlayHitDirectionAnimation(attack.attackDirection);
            StunEnemy(attack.stunTime);

            if (damagedParticles) damagedParticles.Play();
        }
    }
    
    public void TakeDamage(int damage, float stunTime)
    {
        tookDamage = true;
        health -= damage;
        if (health <= 0)
        {
            Die();
        }
        
        else 
        {
            PlayDamageAudio();
            // PlayHitDirectionAnimation(attack.attackDirection);
            StunEnemy(stunTime);

            if (damagedParticles) damagedParticles.Play();
        }
    }

    void PlayHitDirectionAnimation(AttackDirection direction)
    {
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
    
    public void StunEnemy(float t) 
    {
        stunTimer = Mathf.Max(stunTimer, stunTimer + t);
    }

    public void ApplyKnockback(Vector3 force)
    {
        rb.AddForce(force, ForceMode.Impulse);
    }

    void PlayDamageAudio()
    {
        audioSource.pitch = Random.Range(0.9f, 1.1f);
        if (hitSFX) audioSource.PlayOneShot(hitSFX);
    }

    void Die()
    {
        SpawnDrops();
        
        GameObject deathFXObj = Instantiate(deathFX, transform.position, transform.rotation);
        ApplyForcesToBody(deathFXObj);
        
        Destroy(deathFXObj, 10f);
        Destroy(gameObject);
    }

    void SpawnDrops()
    {
        foreach (ItemDropObject drop in itemDropObjects)
        {
            float p = Random.value;
            if (p <= drop.dropChance)
            {
                ItemDropObject dropObj = Instantiate(drop, transform.position, transform.rotation);
            }
        }
    }
    
    void ApplyForcesToBody(GameObject deathFXObj) 
    {
        foreach (Rigidbody rb in deathFXObj.GetComponentsInChildren<Rigidbody>()) 
        {
            rb.AddForce(300f * -transform.forward);
        }
    }

    void RandomizeAppearance()
    {
        foreach (GameObject obj in randomizedObjects)
        {
            float n = Random.value;
            if (n <= 0.5f) obj.SetActive(false);
        }
    }
}
