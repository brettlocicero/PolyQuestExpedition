using UnityEngine;

[CreateAssetMenu(menuName = "Weapon Effects/Knockback")]
public class KnockbackEffectSO : WeaponEffectSO
{
    [SerializeField] float flatBonus = 2f;
    [SerializeField] float percentBonus;

    public override void Apply(WeaponContext context)
    {
        if (!context.weapon)
            return;

        context.weapon.AddKnockback(flatBonus, percentBonus);
    }
}
