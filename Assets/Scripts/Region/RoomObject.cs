using UnityEngine;

public class RoomObject : MonoBehaviour
{
    public Transform playerSpawn;
    public Transform connectionPoint;
    public HallwayObject[] hallwayObjects;

    [Header("Enemy Spawning")]
    [Range(0f, 1f)] public float enemyChance = 0f;
    public EnemyAI[] enemies;
    public Transform[] enemySpawnPoints;

    public HallwayObject SpawnHallway()
    {
        return Instantiate(hallwayObjects[Random.Range(0, hallwayObjects.Length)], connectionPoint.position, Quaternion.identity);
    }

    public void TrySpawnEnemies()
    {
        float p = Random.value;
        if (p > enemyChance) return;

        foreach (Transform point in enemySpawnPoints)
        {
            EnemyAI enemyObj = Instantiate(enemies[Random.Range(0, enemies.Length)], point.position, point.rotation);
        }
    }
}
