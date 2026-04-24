using UnityEngine;

public abstract class WeaponEffectSO : ScriptableObject
{
    public abstract void Apply(WeaponContext context);
}