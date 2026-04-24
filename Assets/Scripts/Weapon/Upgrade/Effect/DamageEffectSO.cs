using UnityEngine;

[CreateAssetMenu(menuName = "Weapon Effects/DamageEffect")]
public class DamageEffectSO : WeaponEffectSO
{
    [SerializeField] int damage = 10;
    [SerializeField] float stunTime = 0f;

    public override void Apply(WeaponContext context)
    {
        foreach (EnemyAI enemy in context.targets)
        {
            enemy.TakeDamage(damage, stunTime);
        }
    }
}