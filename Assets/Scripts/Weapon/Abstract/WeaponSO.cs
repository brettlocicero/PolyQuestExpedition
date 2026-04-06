using UnityEngine;

[CreateAssetMenu(fileName = "WeaponSO", menuName = "Scriptable Objects/WeaponSO")]
public class WeaponSO : ItemSO
{
    [Header("Weapon Settings")]
    public float attackRate = 1f;
    public float range = 3f;

    [Header("Attacks")]
    public WeaponAttack[] attacks;
    public WeaponAttack heavyAttack;

    [Header("VFX")]
    public BloodParticles bloodParticles;
}

[System.Serializable]
public class WeaponAttack
{
    public AnimationClip attackAnimation;
    public int damage = 10;
    public AttackDirection attackDirection = AttackDirection.Left;
    public float attackDelay = 0.333f;
    public float attackRatePenalty = 0f;
    public float knockbackForce = 2f;
    public AudioClip sound;
    public Vector2 pitchRange = new(0.9f, 1.1f);
}