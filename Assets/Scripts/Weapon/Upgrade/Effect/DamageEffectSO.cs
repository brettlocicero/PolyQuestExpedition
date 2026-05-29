using UnityEngine;

[CreateAssetMenu(menuName = "Weapon Effects/DamageEffect")]
public class DamageEffectSO : WeaponEffectSO
{
    [SerializeField] int damage = 10;
    [SerializeField] float stunTime = 0f;

    public override void Apply(WeaponContext context)
    {
        if (context.hits != null && context.hits.Length > 0)
        {
            foreach (WeaponHit hit in context.hits)
            {
                if (hit.killed || !hit.enemy)
                    continue;

                hit.enemy.TakeDamage(damage, stunTime);
            }

            return;
        }

        if (context.targets == null)
            return;

        foreach (EnemyAI enemy in context.targets)
        {
            if (!enemy)
                continue;

            enemy.TakeDamage(damage, stunTime);
        }
    }
}
