using UnityEngine;

public class RoomObject : MonoBehaviour
{
    public GameObject[] enemies;
    public int enemyCount = 3;
    
    public int InitRoom() 
    {
        SpawnEnemies();
        
        return enemyCount;
    }
    
    void SpawnEnemies() 
    {
        for (int i = 0; i < enemyCount; i++) 
        {
            Vector3 playerPos = PlayerInstance.instance.transform.position;
            Vector3 circlePos = Random.onUnitCircle;
            circlePos.z = circlePos.y;
            circlePos.y = playerPos.y;
            
            Vector3 pos = playerPos + circlePos;
            GameObject enemyObj = Instantiate(enemies[Random.Range(0, enemies.Length)], pos, Quaternion.identity);
        }
    }
}
