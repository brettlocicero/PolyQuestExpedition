using UnityEngine;

public class RoomManager : MonoBehaviour
{
    public static RoomManager instance;
    void Awake() => instance = this;

    [Header("Runtime")]
    [SerializeField] int roomIndex = 0;
    [SerializeField] int regionIndex = 0;
    
    [Header("")]
    [SerializeField] RoomObject[] rooms;
    
    GameObject roomObj;
    
    public void StartRun()
    {
        roomIndex = 0;
        SpawnRoom();
    }

    public void SpawnRoom() 
    {
        if (roomObj) Destroy(roomObj);
        
        roomIndex++;
        
        RoomObject instancedRoom = Instantiate(rooms[Random.Range(0, rooms.Length)], Vector3.zero, Quaternion.identity);
        roomObj = instancedRoom.gameObject;

        // TODO: Add a way to start the room in the room itself, so the player does got ambushed?
        instancedRoom.StartRoom();
    }
}
