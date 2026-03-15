using UnityEngine;

public class MeleeWeaponObject : WeaponObject
{
    protected override void Attack(WeaponAttack attack)
    {
        Vector3 center = transform.position + transform.forward * 1.5f;

        int enemyLayer = LayerMask.GetMask("Enemy");
        Collider[] hits = Physics.OverlapSphere(center, weaponSO.range, enemyLayer);

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
