using UnityEngine;

public enum WeaponUpgradeType
{
    Passive,    // Permanent passive
    OnHit,      // Triggers effect on each enemy hit
    OnAttack,   // Triggers after any attack
    OnKill      // Triggers after any killed enemy
}
