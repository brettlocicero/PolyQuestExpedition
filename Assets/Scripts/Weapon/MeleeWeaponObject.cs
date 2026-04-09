using System.Collections;
using UnityEngine;

public class MeleeWeaponObject : WeaponObject
{
    protected override bool Attack(WeaponAttack attack)
    {
        int enemyLayer = LayerMask.GetMask("Enemy");

        bool hitEnemy = Physics.Raycast(
            mainCamTform.position,
            mainCamTform.forward,
            out RaycastHit hit,
            weaponSO.range,
            enemyLayer
        );

        if (hitEnemy)
        {
            if (hit.collider.TryGetComponent<EnemyAI>(out var enemy))
            {
                enemy.TakeDamage(attack);
                Vector3 knockbackDir = (hit.collider.transform.position - transform.position).normalized;
                enemy.ApplyKnockback(knockbackDir * attack.knockbackForce);
            }

            UIManager.instance.DisplayHitmarker();
            SpawnBloodParticle(hit, attack);
        }
        
        float intensity = hitEnemy ? 3f : 1.5f;
        CinemachineShake.instance.ShakeCamera(intensity, 0.25f, 0.3f, 85f);
        

        return hitEnemy;
    }

    void SpawnBloodParticle(RaycastHit hit, WeaponAttack attack)
    {
        Quaternion hitRot = mainCamTform.rotation;
        BloodParticles particlesObj = Instantiate(weaponSO.bloodParticles, hit.point, hitRot);
        particlesObj.InitParticleSystem(attack);
    }
}
