using System.Collections;
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
    PlayerInstance playerInstance;

    void Start()
    {
        playerInstance = PlayerInstance.instance;
    }

    public void StartRun()
    {
        roomIndex = 0;
        SpawnRoom();
    }

    public void SpawnRoom() 
    {
        roomIndex++;
        playerInstance.PlayRoomTransitionAnimation();

        StartCoroutine(Worker());
        IEnumerator Worker()
        {
            yield return new WaitForSeconds(1f);

            if (roomObj) Destroy(roomObj);

            RoomObject instancedRoom = Instantiate(rooms[Random.Range(0, rooms.Length)], Vector3.zero, Quaternion.identity);
            roomObj = instancedRoom.gameObject;

            // TODO: Add a way to start the room in the room itself, so the player does got ambushed?
            instancedRoom.StartRoom();
            
        }
    }
}
