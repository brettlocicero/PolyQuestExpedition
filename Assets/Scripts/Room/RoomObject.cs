using UnityEngine;

public class RoomObject : MonoBehaviour
{
    [SerializeField] bool combatRoom = true;
    [SerializeField] GameObject[] enemies;
    [SerializeField] float waveDuration = 60f;
    [SerializeField] float spawnInterval = 2f;
    
    public void StartRoom() 
    {
        if (combatRoom)
            EnemyWaveManager.instance.StartWave(enemies, waveDuration, spawnInterval);
    }
}
