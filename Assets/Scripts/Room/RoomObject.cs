using UnityEngine;

public class RoomObject : MonoBehaviour
{
    [Header("Wave Settings")]
    [SerializeField] GameObject[] enemies;
    [SerializeField] float waveDuration = 60f;
    [SerializeField] float spawnInterval = 2f;
    
    public void StartRoom() 
    {
        EnemyWaveManager.instance.StartWave(enemies, waveDuration, spawnInterval);
    }
}
