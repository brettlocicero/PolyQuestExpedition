using System.Collections;
using System.Collections.Generic;
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

        HashSet<EnemyAI> hitEnemies = new();

        if (hitEnemy)
        {
            if (hit.collider.TryGetComponent<EnemyAI>(out var enemy))
            {
                hitEnemies.Add(enemy);

                enemy.TakeDamage(attack);

                Vector3 knockbackDir = (hit.collider.transform.position - transform.position).normalized;
                enemy.ApplyKnockback(knockbackDir * attack.knockbackForce);
            }

            SpawnBloodParticle(hit, attack);
        }

        Vector3 sphereCenter = mainCamTform.position + (mainCamTform.forward * (weaponSO.range - attack.cleaveRadius));
        Collider[] hits = Physics.OverlapSphere(
            sphereCenter,
            attack.cleaveRadius,
            enemyLayer
        );

        foreach (var sphereHit in hits)
        {
            if (sphereHit.TryGetComponent<EnemyAI>(out var enemy))
            {
                if (hitEnemies.Contains(enemy))
                    continue;

                hitEnemies.Add(enemy);

                enemy.TakeDamage(attack);

                Vector3 knockbackDir = (enemy.transform.position - transform.position).normalized;
                enemy.ApplyKnockback(0.75f * attack.knockbackForce * knockbackDir); // optional reduced knockback

                SpawnBloodParticle(sphereHit.ClosestPoint(sphereCenter), attack);
            }
        }

        bool successfulHit = hitEnemies.Count > 0;
        if (successfulHit) UIManager.instance.DisplayHitmarker();

        float intensity = successfulHit ? 3f : 1.5f;
        CinemachineShake.instance.ShakeCamera(intensity, 0.25f, 0.3f, 85f);

        return successfulHit;
    }

    void SpawnBloodParticle(RaycastHit hit, WeaponAttack attack)
    {
        Quaternion hitRot = mainCamTform.rotation;
        Vector3 hitPoint = hit.point;

        BloodParticles particlesObj = Instantiate(weaponSO.bloodParticles, hitPoint, hitRot);
        particlesObj.InitParticleSystem(attack);
        
        Destroy(particlesObj.gameObject, 4f);
    }

    void SpawnBloodParticle(Vector3 pos, WeaponAttack attack)
    {
        Quaternion hitRot = mainCamTform.rotation;

        BloodParticles particlesObj = Instantiate(weaponSO.bloodParticles, pos, hitRot);
        particlesObj.InitParticleSystem(attack);
        
        Destroy(particlesObj.gameObject, 4f);
    }
}
