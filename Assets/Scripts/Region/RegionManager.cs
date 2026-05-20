using System.Collections;
using DG.Tweening;
using UnityEngine;

public class RegionManager : MonoBehaviour
{
    public static RegionManager instance;
    void Awake()
    {
        instance = this;
    }

    [SerializeField] GameObject hubObject;

    [Header("VFX")]
    [SerializeField] Animator travelAnim;

    RegionSO currentRegion;
    GameObject regionFloorObj;
    int floorIndex = 0;
    Vector3 spawnPos = Vector3.zero;

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

        // Fade out asynchronously
        DOTween.To(
            () => AudioListener.volume,
            x => AudioListener.volume = x,
            0f,
            0.5f
        );

        yield return new WaitForSeconds(0.5f);

        hubObject.SetActive(false);
        ApplyRegionVFX(currentRegion);

        ClearCurrentFloor();

        regionFloorObj = new GameObject($"{currentRegion.name} - Floor {floorIndex}");

        // Spawn the entrance room only on the first floor of the region.
        if (floorIndex == 0)
            SpawnEntranceRoom();
        else
            SpawnStandardFloor();

        // Transition animation cycles back after 0.5 secs.
        yield return new WaitForSeconds(0.5f);

        DOTween.To(
            () => AudioListener.volume,
            x => AudioListener.volume = x,
            1f,
            0.5f
        );
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
        entranceRoom.transform.SetParent(regionFloorObj.transform);
        
        PlayerInstance.instance.RepositionPlayer(entranceRoom.playerSpawn.position, entranceRoom.playerSpawn.rotation);
    }

    void SpawnStandardFloor()
    {
        for (int i = 0; i < currentRegion.floorLength; i++)
        {
            RoomObject room = Instantiate(currentRegion.GetRandomRoom(), spawnPos, Quaternion.identity);
            room.transform.SetParent(regionFloorObj.transform);

            // If first room, move the player to the spawn point.
            if (i == 0) PlayerInstance.instance.RepositionPlayer(room.playerSpawn.position, room.playerSpawn.rotation);

            // If not the last room, spawn a connecting hallway.
            bool isLastRoom = i >= currentRegion.floorLength - 1;
            if (!isLastRoom) SpawnHallway(room);

            // Spawn enemies given the room's chances to have enemies
            room.TrySpawnEnemies();
        }
    }

    void SpawnHallway(RoomObject room)
    {
        HallwayObject hallway = room.SpawnHallway();
        hallway.transform.SetParent(room.transform);
        spawnPos = hallway.connectionPoint.position;
    }

    void ApplyRegionVFX(RegionSO region)
    {
        RenderSettings.fogColor = region.fogColor;
        RenderSettings.skybox = region.skybox;
        RenderSettings.sun.color = region.sunColor;
    }
}
