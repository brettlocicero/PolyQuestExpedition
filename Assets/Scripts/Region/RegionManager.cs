using System.Collections;
using UnityEngine;

public class RegionManager : MonoBehaviour
{
    public static RegionManager instance;
    void Awake()
    {
        instance = this;
    }

    [SerializeField] RegionSO testRegion;

    [Header("VFX")]
    [SerializeField] Animator travelAnim;

    RegionSO currentRegion;
    GameObject regionFloorObj;
    int floorIndex = 0;
    Vector3 spawnPos = Vector3.zero;

    void Start()
    {
        EnterRegion(testRegion);
    }

    public void EnterRegion(RegionSO region)
    {
        currentRegion = region;
        floorIndex = 0;
        SpawnRegionFloor();
    }

    public void SpawnNextRegionFloor()
    {
        floorIndex++;
        SpawnRegionFloor();
    }

    void SpawnRegionFloor()
    {
        StartCoroutine(SpawnRegionFloorRoutine());
    }

    IEnumerator SpawnRegionFloorRoutine()
    {
        travelAnim.SetTrigger("RoomTransition");

        yield return new WaitForSeconds(0.5f);

        ClearCurrentFloor();

        regionFloorObj = new GameObject($"{currentRegion.name} - Floor {floorIndex}");

        if (floorIndex == 0)
        {
            SpawnEntranceRoom();
            yield break;
        }

        SpawnStandardFloor();
    }

    void ClearCurrentFloor()
    {
        if (regionFloorObj != null)
        {
            Destroy(regionFloorObj);
        }
    }

    void SpawnEntranceRoom()
    {
        RoomObject entranceRoom = Instantiate(currentRegion.entranceRoom, spawnPos, Quaternion.identity);
        SetupRoom(entranceRoom);
        
        PlayerInstance.instance.RepositionPlayer(entranceRoom.playerSpawn.position, entranceRoom.playerSpawn.rotation);
    }

    void SpawnStandardFloor()
    {
        for (int i = 0; i < currentRegion.floorLength; i++)
        {
            RoomObject room = Instantiate(currentRegion.GetRandomRoom(), spawnPos, Quaternion.identity);
            SetupRoom(room);

            // If first room, move the player to the spawn point.
            if (i == 0) PlayerInstance.instance.RepositionPlayer(room.playerSpawn.position, room.playerSpawn.rotation);

            // If not the last room, spawn a connecting hallway.
            bool isLastRoom = i >= currentRegion.floorLength - 1;
            if (!isLastRoom) SpawnHallway(room);
        }
    }

    void SetupRoom(RoomObject room)
    {
        room.transform.SetParent(regionFloorObj.transform);
    }

    void SpawnHallway(RoomObject room)
    {
        HallwayObject hallway = room.SpawnHallway();
        hallway.transform.SetParent(room.transform);
        spawnPos = hallway.connectionPoint.position;
    }
}
