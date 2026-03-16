using UnityEngine;

[CreateAssetMenu(fileName = "WeaponSO", menuName = "Scriptable Objects/WeaponSO")]
public class WeaponSO : ItemSO
{
    [Header("Weapon Settings")]
    public int damage = 10;
    public float attackRate = 1f;
    public float range = 3f;

    [Header("VFX")]
    public WeaponAttack[] attacks;
    public WeaponAttack heavyAttack;
}

[System.Serializable]
public class WeaponAttack
{
    public AnimationClip attackAnimation;
    public float damage = 10f;
    public float attackDelay = 0.333f;
    public float attackRatePenalty = 0f;
}