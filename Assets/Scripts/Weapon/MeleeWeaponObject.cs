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

        float radius = attack.cleaveRadius;
        RaycastHit[] hits = Physics.SphereCastAll(
            mainCamTform.position,
            radius,
            mainCamTform.forward,
            weaponSO.range,
            enemyLayer
        );

        foreach (var sphereHit in hits)
        {
            if (sphereHit.collider.TryGetComponent<EnemyAI>(out var enemy))
            {
                if (hitEnemies.Contains(enemy))
                    continue;

                hitEnemies.Add(enemy);

                enemy.TakeDamage(attack);

                Vector3 knockbackDir = (enemy.transform.position - transform.position).normalized;
                enemy.ApplyKnockback(0.75f * attack.knockbackForce * knockbackDir); // optional reduced knockback

                SpawnBloodParticle(sphereHit, attack);
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
        BloodParticles particlesObj = Instantiate(weaponSO.bloodParticles, hit.point, hitRot);
        particlesObj.InitParticleSystem(attack);
        
        Destroy(particlesObj.gameObject, 4f);
    }
}
