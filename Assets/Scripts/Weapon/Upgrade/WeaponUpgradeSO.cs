using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ShardSO", menuName = "Scriptable Objects/ShardSO")]
public class ShardSO : ScriptableObject
{
    [SerializeField] string upgradeName;
    [SerializeField, TextArea] string description;
    [SerializeField, Min(1)] int maxStacks = 1;
    [SerializeField] List<WeaponUpgradeEffect> effects = new();

    [SerializeField, HideInInspector] WeaponUpgradeType weaponUpgradeType;
    [SerializeField, HideInInspector] WeaponEffectSO effect;

    public string UpgradeName => string.IsNullOrEmpty(upgradeName) ? name : upgradeName;
    public string Description => description;
    public int MaxStacks => maxStacks;

    public void Apply(WeaponContext context)
    {
        if (effects != null)
        {
            foreach (WeaponUpgradeEffect upgradeEffect in effects)
            {
                if (upgradeEffect == null || !upgradeEffect.CanApply(context.trigger))
                    continue;

                upgradeEffect.Apply(context);
            }
        }

        if ((effects == null || effects.Count == 0) && effect && weaponUpgradeType == context.trigger)
            effect.Apply(context);
    }

    void OnValidate()
    {
        if (maxStacks < 1)
            maxStacks = 1;

        if (!effect)
            return;

        if (effects == null)
            effects = new List<WeaponUpgradeEffect>();

        bool alreadyMigrated = false;
        foreach (WeaponUpgradeEffect upgradeEffect in effects)
        {
            if (upgradeEffect != null && upgradeEffect.Trigger == weaponUpgradeType && upgradeEffect.Effect == effect)
            {
                alreadyMigrated = true;
                break;
            }
        }

        if (!alreadyMigrated)
            effects.Add(new WeaponUpgradeEffect(weaponUpgradeType, effect));
    }
}

[System.Serializable]
public class WeaponUpgradeEffect
{
    [SerializeField] WeaponUpgradeType trigger;
    [SerializeField] WeaponEffectSO effect;

    public WeaponUpgradeType Trigger => trigger;
    public WeaponEffectSO Effect => effect;

    public WeaponUpgradeEffect()
    {
    }

    public WeaponUpgradeEffect(WeaponUpgradeType trigger, WeaponEffectSO effect)
    {
        this.trigger = trigger;
        this.effect = effect;
    }

    public bool CanApply(WeaponUpgradeType triggerType)
    {
        return effect && trigger == triggerType;
    }

    public void Apply(WeaponContext context)
    {
        effect.Apply(context);
    }
}
