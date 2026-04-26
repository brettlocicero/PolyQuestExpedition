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
        StartCoroutine(Worker());
        IEnumerator Worker()
        {
            travelAnim.SetTrigger("RoomTransition");

            yield return new WaitForSeconds(0.5f);

            if (regionFloorObj) Destroy(regionFloorObj);
            regionFloorObj = new GameObject($"{currentRegion.name} - {floorIndex}");

            RegionFloor floor = currentRegion.floors[floorIndex];
            for (int i = 0; i < floor.floorLength; i++)
            {
                RoomObject roomObj = Instantiate(floor.GetRandomRoom(), spawnPos, Quaternion.identity);
                roomObj.transform.SetParent(regionFloorObj.transform);

                // If this is the first room, move the player to the spawn point.
                if (i == 0)
                {
                    PlayerInstance.instance.RepositionPlayer(roomObj.playerSpawn.position, roomObj.playerSpawn.rotation);
                }

                // Spawn a hallway connector between the rooms if not on the last room of the floor.
                if (i < floor.floorLength - 1)
                {
                    HallwayObject hallwayObj = roomObj.SpawnHallway();
                    hallwayObj.transform.SetParent(roomObj.transform);
                    spawnPos = hallwayObj.connectionPoint.position;
                }
            }
        }
    }
}
