using UnityEngine;

[CreateAssetMenu(fileName = "WeaponSO", menuName = "Scriptable Objects/WeaponSO")]
public class WeaponSO : ItemSO
{
    [Header("Weapon Settings")]
    public int damage = 10;
    public float attackRate = 1f;
    public float range = 3f;
}
