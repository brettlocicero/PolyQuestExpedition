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

            UIManager.instance.DisplayHitmarker();
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
                enemy.ApplyKnockback(knockbackDir * attack.knockbackForce * 0.75f); // optional reduced knockback

                SpawnBloodParticle(sphereHit, attack);
            }
        }

        float intensity = hitEnemies.Count > 0 ? 3f : 1.5f;
        CinemachineShake.instance.ShakeCamera(intensity, 0.25f, 0.3f, 85f);

        return hitEnemies.Count > 0;
    }

    void SpawnBloodParticle(RaycastHit hit, WeaponAttack attack)
    {
        Quaternion hitRot = mainCamTform.rotation;
        BloodParticles particlesObj = Instantiate(weaponSO.bloodParticles, hit.point, hitRot);
        particlesObj.InitParticleSystem(attack);
        
        Destroy(particlesObj.gameObject, 4f);
    }
}
