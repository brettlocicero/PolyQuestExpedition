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
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float rotationSpeed = 10f;

    [Header("VFX")]
    [SerializeField] AudioClip hitSFX;
    [SerializeField] Animator anim;
    public ContactParticles contactParticles;

    AudioSource audioSource;
    Rigidbody rb;

    bool isStunned = false;
    float stunTimer = 0f;

    bool isAttacking = false;

    float sqrDistToTarget = 1000000f;

    void Start()
    {
        health = maxHealth;

        audioSource = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody>();
        
        anim.speed = Random.Range(0.95f, 1.05f);
        
        if (!target) target = PlayerInstance.instance.transform;
    }

    void Update()
    {
        HandleStunTimer();
        HandleAttacking();
    }

    void HandleAttacking()
    {
        if (isStunned || isAttacking) return;

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
    }

    void HandleMovement()
    {
        sqrDistToTarget = Vector3.SqrMagnitude(target.position - transform.position);
        
        if (target == null || isStunned || isAttacking) return;

        Vector3 dir = (target.position - transform.position).normalized;
        dir.y = 0f;

        Vector3 move = moveSpeed * Time.fixedDeltaTime * dir;
        if (!InAttackRange()) rb.MovePosition(rb.position + move);

        if (dir != Vector3.zero)
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
        health -= attack.damage;
        // PlayDamageAudio();
        PlayHitDirectionAnimation(attack.attackDirection);
        StunEnemy(attack.stunTime);

        if (health <= 0)
        {
            Die();
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
        audioSource.PlayOneShot(hitSFX);
    }

    void Die()
    {
        RoomManager.RegisterEnemyDeath();
        Destroy(gameObject);
    }
}
