using UnityEngine;

public class MeleeWeaponObject : WeaponObject
{
    protected override void Attack(WeaponAttack attack)
    {
        int enemyLayer = LayerMask.GetMask("Enemy");
        Collider[] hits = Physics.OverlapSphere(attackOrigin.position, weaponSO.range, enemyLayer);

        foreach (Collider hit in hits)
        {
            // Enemy enemy = hit.GetComponent<Enemy>();
            // if (enemy != null)
            // {
            //     enemy.TakeDamage(attackDamage);
            // }

        }

        if (hits.Length > 0)
            CinemachineShake.instance.ShakeCamera(2f, 0.25f, 0.15f, 85f);
    }

    void OnDrawGizmos()
    {
        if (attackOrigin == null || weaponSO == null)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackOrigin.position, weaponSO.range);
    }
}
