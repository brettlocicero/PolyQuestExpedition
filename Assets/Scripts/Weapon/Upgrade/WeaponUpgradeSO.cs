using UnityEngine;

[CreateAssetMenu(fileName = "WeaponUpgradeSO", menuName = "Scriptable Objects/WeaponUpgradeSO")]
public class WeaponUpgradeSO : ScriptableObject
{
    public WeaponUpgradeType weaponUpgradeType;

    public WeaponEffectSO effect;
}