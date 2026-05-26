using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Transform player;
    [SerializeField] EnemyAI[] enemyPrefabs;

    [Header("Spawn Settings")]
    [SerializeField] float spawnRadius = 35f;
    [SerializeField] float minSpawnDistance = 15f;
    [SerializeField] float spawnInterval = 3f;
    [SerializeField] int enemiesPerWave = 3;
    [SerializeField] int maxEnemiesAlive = 40;

    [Header("Difficulty Scaling")]
    [SerializeField] bool scaleDifficulty = true;
    [SerializeField] float difficultyIncreaseTime = 30f;
    [SerializeField] int enemiesIncreaseAmount = 1;
    [SerializeField] float spawnIntervalDecrease = 0.15f;
    [SerializeField] float minimumSpawnInterval = 0.5f;

    [Header("Ground Detection")]
    [SerializeField] LayerMask groundMask;
    [SerializeField] float raycastHeight = 50f;

    readonly List<EnemyAI> aliveEnemies = new();

    void Start()
    {
        if (!player)
            player = PlayerInstance.instance.transform;
    }

    public void StartSpawnLoop()
    {
        StartCoroutine(SpawnLoop());

        if (scaleDifficulty)
            StartCoroutine(DifficultyLoop());
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            CleanupDeadEnemies();

            if (aliveEnemies.Count >= maxEnemiesAlive)
                continue;

            SpawnWave();
        }
    }

    void SpawnWave()
    {
        for (int i = 0; i < enemiesPerWave; i++)
        {
            CleanupDeadEnemies();

            if (aliveEnemies.Count >= maxEnemiesAlive)
                return;

            SpawnEnemy();
        }
    }

    void SpawnEnemy()
    {
        if (enemyPrefabs.Length == 0 || player == null)
            return;

        Vector3 spawnPos = GetSpawnPosition();
        spawnPos.y = 3f;
        EnemyAI randomEnemy = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
        EnemyAI spawnedEnemy = Instantiate(randomEnemy, spawnPos, Quaternion.identity);

        aliveEnemies.Add(spawnedEnemy);
    }

    Vector3 GetSpawnPosition()
    {
        for (int i = 0; i < 20; i++)
        {
            Vector2 randomCircle = Random.insideUnitCircle.normalized * Random.Range(minSpawnDistance, spawnRadius);
            Vector3 randomPos = player.position + new Vector3(randomCircle.x, 0f, randomCircle.y);
            Vector3 rayOrigin = randomPos + Vector3.up * raycastHeight;

            if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, raycastHeight * 2f, groundMask))
            {
                return hit.point;
            }
        }

        return player.position + Random.onUnitSphere * minSpawnDistance;
    }

    void CleanupDeadEnemies()
    {
        aliveEnemies.RemoveAll(enemy => enemy == null);
    }

    IEnumerator DifficultyLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(difficultyIncreaseTime);

            enemiesPerWave += enemiesIncreaseAmount;

            spawnInterval -= spawnIntervalDecrease;

            if (spawnInterval < minimumSpawnInterval)
                spawnInterval = minimumSpawnInterval;

            Debug.Log($"Difficulty Increased | Wave Size: {enemiesPerWave} | Spawn Interval: {spawnInterval}");
        }
    }

    void OnDrawGizmosSelected()
    {
        if (!player)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(player.position, minSpawnDistance);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(player.position, spawnRadius);
    }
}