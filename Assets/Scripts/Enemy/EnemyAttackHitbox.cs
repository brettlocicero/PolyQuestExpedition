using UnityEngine;

public class EnemyAttackHitbox : MonoBehaviour
{
    [SerializeField] float radius;
    [SerializeField] Transform attackOrigin;
    [SerializeField] int damage = 10;
    [SerializeField] LayerMask playerLayer;

    void OnDrawGizmos()
    {
        if (attackOrigin != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackOrigin.position, radius);
        }
    }

    public void TriggerHitbox()
    {
        if (attackOrigin == null) return;

        Collider[] hitColliders = Physics.OverlapSphere(attackOrigin.position, radius, playerLayer);
        foreach (Collider hitCollider in hitColliders)
        {
            if (hitCollider.TryGetComponent<PlayerInstance>(out var damageable))
            {
                damageable.TakeDamage(damage);
                Debug.Log("Player took damage");
            }
        }
    }
}
