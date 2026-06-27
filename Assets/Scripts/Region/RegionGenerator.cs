using UnityEngine;

public class RegionGenerator : MonoBehaviour
{
    public static RegionGenerator instance;
    void Awake()
    {
        instance = this;
    }

    [Header("Runtime")]
    [SerializeField] int roomCount = 0;

    [Header("References")]
    [SerializeField] GameObject hubObjects;

    RegionSO currentRegion;
    GameObject lastRoomObj;

    public void EnterRegion(RegionSO region)
    {
        hubObjects.SetActive(false);

        currentRegion = region;
        roomCount = 0;

        currentRegion.ApplyVFX();

        SpawnNextRoom();
    }

    public void SpawnNextRoom()
    {
        if (lastRoomObj) Destroy(lastRoomObj);

        roomCount++;

        RoomObject roomToSpawn = currentRegion.GetRoomToSpawn();
        RoomObject roomObj = Instantiate(roomToSpawn, Vector3.zero, Quaternion.identity);
        roomObj.PositionPlayerInRoom();

        lastRoomObj = roomObj.gameObject;
    }
}
