using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnerObject : MonoBehaviour, IInteractable
{
    [SerializeField] EnemyAI[] enemies;
    [SerializeField] int waves = 3;
    [SerializeField] int enemiesPerWave = 8;
    [SerializeField] float radius = 50f;

    bool used = false;
    int currentWave = 0;
    int deployedEnemies = 0;

    public void Interact()
    {
        if (!used)
        {
            StartEnemySpawning();
            used = true;
        }
    }

    void StartEnemySpawning()
    {
        StartCoroutine(SpawnWave());
    }

    IEnumerator SpawnWave()
    {
        currentWave++;

        yield return new WaitForSeconds(1f);

        EnemyAI enemy = enemies[Random.Range(0, enemies.Length)];
        for (int i = 0; i < enemiesPerWave; i++)
        {
            Vector3 randCircle = Random.insideUnitCircle * radius;
            Vector3 spawnPos = new Vector3(randCircle.x, 2f, randCircle.y) + transform.position;

            EnemyAI enemyObj = Instantiate(enemy, spawnPos, Quaternion.identity);
            enemyObj.AttachEnemySpawnerObject(this);

            deployedEnemies++;
        }
    }

    public void RegisterEnemyDeath(EnemyAI enemy)
    {
        deployedEnemies--;

        // Spawn next wave if all enemies have died, and have not finished all N waves.
        if (deployedEnemies == 0 && currentWave < waves)
        {
            StartCoroutine(SpawnWave());
        }

        else if (deployedEnemies == 0 && currentWave == waves)
        {
            SpawnRewards();
        }
    }

    void SpawnRewards()
    {
        Debug.Log("Spawning rewards");
    }
}
