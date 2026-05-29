using UnityEngine;

[CreateAssetMenu(menuName = "Weapon Effects/Stun Time")]
public class StunTimeEffectSO : WeaponEffectSO
{
    [SerializeField] float flatBonus = 0.25f;
    [SerializeField] float percentBonus;

    public override void Apply(WeaponContext context)
    {
        if (!context.weapon)
            return;

        context.weapon.AddStunTime(flatBonus, percentBonus);
    }
}
