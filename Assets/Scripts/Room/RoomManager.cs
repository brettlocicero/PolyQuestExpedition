using UnityEngine;

public class RoomManager : MonoBehaviour
{
    [Header("Runtime")]
    [SerializeField] int roomIndex = 0;
    [SerializeField] int regionIndex = 0;
    
    [Header("")]
    [SerializeField] RoomObject[] rooms;
    
    GameObject roomObj;
    
    static int currentEnemyCount = 0;

    void Start()
    {
        SpawnRoom();
    }

    public void SpawnRoom() 
    {
        if (roomObj) Destroy(roomObj);
        
        roomIndex++;
        
        RoomObject instancedRoom = Instantiate(rooms[Random.Range(0, rooms.Length)], Vector3.zero, Quaternion.identity);
        currentEnemyCount = instancedRoom.InitRoom();
        
        roomObj = instancedRoom.gameObject;
    }
    
    public static void RegisterEnemyDeath() 
    {
        currentEnemyCount--;
    }
}
