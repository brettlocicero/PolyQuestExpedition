using UnityEngine;

[System.Serializable]
public class WeaponRuntimeStats
{
    [SerializeField] float attackSpeedMultiplier = 1f;
    [SerializeField] float stunTimeMultiplier = 1f;
    [SerializeField] float stunTimeBonus;
    [SerializeField] float knockbackMultiplier = 1f;
    [SerializeField] float knockbackBonus;

    public float AttackSpeedMultiplier => attackSpeedMultiplier;
    public float StunTimeMultiplier => stunTimeMultiplier;
    public float StunTimeBonus => stunTimeBonus;
    public float KnockbackMultiplier => knockbackMultiplier;
    public float KnockbackBonus => knockbackBonus;

    public void AddAttackSpeedPercent(float percentBonus)
    {
        attackSpeedMultiplier = Mathf.Max(0.01f, attackSpeedMultiplier + percentBonus);
    }

    public void AddStunTime(float flatBonus, float percentBonus)
    {
        stunTimeBonus += flatBonus;
        stunTimeMultiplier = Mathf.Max(0f, stunTimeMultiplier + percentBonus);
    }

    public void AddKnockback(float flatBonus, float percentBonus)
    {
        knockbackBonus += flatBonus;
        knockbackMultiplier = Mathf.Max(0f, knockbackMultiplier + percentBonus);
    }

    public float GetAttackRate(float baseAttackRate)
    {
        return Mathf.Max(0.01f, baseAttackRate / attackSpeedMultiplier);
    }

    public WeaponAttack ApplyTo(WeaponAttack attack)
    {
        WeaponAttack modifiedAttack = attack.Copy();
        modifiedAttack.stunTime = Mathf.Max(0f, attack.stunTime * stunTimeMultiplier + stunTimeBonus);
        modifiedAttack.knockbackForce = Mathf.Max(0f, attack.knockbackForce * knockbackMultiplier + knockbackBonus);
        return modifiedAttack;
    }
}
