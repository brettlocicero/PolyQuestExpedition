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

            print("hit");
        }
    }
}
