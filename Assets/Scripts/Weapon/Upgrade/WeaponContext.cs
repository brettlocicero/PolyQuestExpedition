using UnityEngine;

public class WeaponContext
{
    public WeaponObject weapon;
    public WeaponSO weaponSO;
    public WeaponRuntimeStats runtimeStats;
    public GameObject attacker;
    public WeaponAttack attack;
    public WeaponUpgradeType trigger;
    public EnemyAI[] targets;
    public WeaponHit[] hits;
    public Vector3 hitPoint;
}

[System.Serializable]
public struct WeaponHit
{
    public EnemyAI enemy;
    public Vector3 point;
    public bool killed;

    public WeaponHit(EnemyAI enemy, Vector3 point, bool killed)
    {
        this.enemy = enemy;
        this.point = point;
        this.killed = killed;
    }
}
