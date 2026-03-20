using UnityEngine;

public class MeleeWeaponObject : WeaponObject
{
    protected override void Attack(WeaponAttack attack)
    {
        int enemyLayer = LayerMask.GetMask("Enemy");

        RaycastHit[] hits = Physics.SphereCastAll(
            attackOrigin.position,
            weaponSO.range,
            attackOrigin.forward,
            0.01f,
            enemyLayer
        );

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.TryGetComponent<EnemyAI>(out var enemy))
            {
                enemy.TakeDamage(attack.damage);
                Vector3 knockbackDir = (hit.collider.transform.position - attackOrigin.position).normalized;
                enemy.ApplyKnockback(knockbackDir * attack.knockbackForce);
            }
        }
        
        bool hitEnemies = hits.Length > 0;

        float intensity = hitEnemies ? 3f : 1.5f;
        CinemachineShake.instance.ShakeCamera(intensity, 0.25f, 0.3f, 85f);
        
        if (hitEnemies) UIManager.instance.DisplayHitmarker();
    }

    void OnDrawGizmos()
    {
        if (attackOrigin == null || weaponSO == null)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackOrigin.position, weaponSO.range);
    }
}
