using System.Collections;
using UnityEngine;

public class EnemySpawnArea : MonoBehaviour
{
    [SerializeField] EnemyAI[] enemies;
    [SerializeField] Transform[] spawnPos;

    [Header("Spawn Settings")]
    [SerializeField] Vector2Int enemyCountRange = new Vector2Int(2, 5);
    [SerializeField] float spawnDelay = 0.5f;

    [Header("Raycast")]
    [SerializeField] float raycastHeight = 10f;
    [SerializeField] float raycastDistance = 50f;
    [SerializeField] LayerMask groundMask;

    [Header("Trigger")]
    [SerializeField] SphereCollider trigger;

    bool used = false;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !used)
        {
            used = true;
            StartCoroutine(SpawnEnemiesRoutine());
        }
    }

    IEnumerator SpawnEnemiesRoutine()
    {
        int amount = Random.Range(enemyCountRange.x, enemyCountRange.y + 1);

        for (int i = 0; i < amount; i++)
        {
            SpawnEnemy();

            if (spawnDelay > 0f)
                yield return new WaitForSeconds(spawnDelay);
        }
    }

    void SpawnEnemy()
    {
        if (enemies.Length == 0 || spawnPos.Length == 0)
            return;

        EnemyAI enemy = enemies[Random.Range(0, enemies.Length)];
        Transform pos = spawnPos[Random.Range(0, spawnPos.Length)];

        Vector3 rayOrigin = pos.position + Vector3.up * raycastHeight;

        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, raycastDistance, groundMask))
        {
            Instantiate(enemy, hit.point, Quaternion.identity);
        }
        else
        {
            // fallback if no ground found
            Instantiate(enemy, pos.position, Quaternion.identity);
        }
    }

    void OnDrawGizmos()
    {
        if (trigger)
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.4f);
            Gizmos.DrawSphere(transform.position, trigger.radius);
        }

        if (spawnPos == null)
            return;

        Gizmos.color = Color.green;

        foreach (Transform pos in spawnPos)
        {
            if (!pos)
                continue;

            Vector3 rayOrigin = pos.position + Vector3.up * raycastHeight;

            Gizmos.DrawLine(rayOrigin, rayOrigin + Vector3.down * raycastDistance);
        }
    }
}