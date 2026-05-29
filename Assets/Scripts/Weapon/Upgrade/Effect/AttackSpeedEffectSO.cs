using UnityEngine;

[CreateAssetMenu(menuName = "Weapon Effects/Attack Speed")]
public class AttackSpeedEffectSO : WeaponEffectSO
{
    [SerializeField] float percentBonus = 0.2f;

    public override void Apply(WeaponContext context)
    {
        if (!context.weapon)
            return;

        context.weapon.AddAttackSpeedPercent(percentBonus);
    }
}
