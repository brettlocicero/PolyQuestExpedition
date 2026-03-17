using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Rigidbody))]
public class EnemyAI : MonoBehaviour
{
    [SerializeField] string enemyName;
    
    [Header("Stats")]
    [SerializeField] int health = 30;
    [SerializeField] int maxHealth = 30;

    [Header("VFX")]
    [SerializeField] AudioClip hitSFX;

    AudioSource audioSource;
    Rigidbody rb;

    void Start()
    {
        health = maxHealth;

        audioSource = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        PlayDamageAudio();

        if (health <= 0)
        {
            Die();
        }
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
        
    }
}
