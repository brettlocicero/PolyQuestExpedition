using System.Collections;
using UnityEngine;

public class EnemyWaveManager : MonoBehaviour
{
    [Header("Wave Settings")]
    [SerializeField] float waveDuration = 60f;
    [SerializeField] float spawnInterval = 2f;
    [SerializeField] int enemiesPerSpawn = 1;

    [Header("Enemy Settings")]
    [SerializeField] GameObject enemyPrefab;

    [Header("Player Reference")]
    [SerializeField] Transform player;

    [Header("Spawn Ring Settings")]
    [SerializeField] float minRadius = 8f;
    [SerializeField] float maxRadius = 15f;

    float waveTimer;
    bool isWaveActive;

    void Start()
    {
        StartWave();
    }

    public void StartWave()
    {
        waveTimer = waveDuration;
        isWaveActive = true;

        StartCoroutine(SpawnLoop());
    }

    IEnumerator SpawnLoop()
    {
        while (isWaveActive)
        {
            SpawnEnemies();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnEnemies()
    {
        for (int i = 0; i < enemiesPerSpawn; i++)
        {
            Vector3 spawnPos = GetRandomRingPosition();
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

        return player.position + offset;
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

        // Hook your dark fantasy transition here:
        // Ritual circle / portal / coffin / etc.
    }
}