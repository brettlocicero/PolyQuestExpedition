using System.Collections;
using UnityEngine;

public class EnemyWaveManager : MonoBehaviour
{
    public static EnemyWaveManager instance;
    void Awake() => instance = this;

    [Header("Player Reference")]
    [SerializeField] Transform player;

    [Header("Spawn Ring Settings")]
    [SerializeField] float minRadius = 8f;
    [SerializeField] float maxRadius = 15f;

    float waveTimer;
    bool isWaveActive;

    public void StartWave(GameObject[] enemies, float waveDuration, float spawnInterval)
    {
        waveTimer = waveDuration;
        isWaveActive = true;

        StartCoroutine(SpawnLoop(enemies, waveDuration, spawnInterval));
    }

    IEnumerator SpawnLoop(GameObject[] enemies, float waveDuration, float spawnInterval)
    {
        while (isWaveActive)
        {
            SpawnEnemies(enemies, waveDuration, spawnInterval);
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnEnemies(GameObject[] enemies, float waveDuration, float spawnInterval)
    {
        int enemiesPerSpawn = 1;
        for (int i = 0; i < enemiesPerSpawn; i++)
        {
            Vector3 spawnPos = GetRandomRingPosition();
            GameObject enemyPrefab = enemies[Random.Range(0, enemies.Length)];
            Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        }
    }

    Vector3 GetRandomRingPosition()
    {
        if (player == null)
        {
            Debug.LogWarning("Player not assigned!");
            return transform.position;
        }

        // Random angle around player
        float angle = Random.Range(0f, Mathf.PI * 2f);

        // Random radius between min and max
        float radius = Random.Range(minRadius, maxRadius);
        Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;
        Vector3 finalPos = player.position + offset;
        finalPos.y = 5f;

        return finalPos;
    }

    void Update()
    {
        if (!isWaveActive) return;

        waveTimer -= Time.deltaTime;

        if (waveTimer <= 0f)
        {
            EndWave();
        }
    }

    void EndWave()
    {
        isWaveActive = false;
        StopAllCoroutines();

        Debug.Log("Wave Complete!");
    }
}