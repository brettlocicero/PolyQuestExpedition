using UnityEngine;

public class EnemySpawnArea : MonoBehaviour
{
    [SerializeField] EnemyAI[] enemies;
    [SerializeField] Transform[] spawnPos;
    [SerializeField] Vector2Int enemyCountRange;
    [SerializeField] SphereCollider trigger;
    bool used = false;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !used) 
        {
            SpawnEnemies();
            used = true;
        }
    }
    
    // TODO: Add a spawn delay?
    void SpawnEnemies() 
    {
        int amount = Random.Range(enemyCountRange.x, enemyCountRange.y);
        for (int i = 0; i < amount; i++) 
        {
            EnemyAI enemy = enemies[Random.Range(0, enemies.Length)];
            Transform pos = spawnPos[Random.Range(0, spawnPos.Length)];
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
    }
}
