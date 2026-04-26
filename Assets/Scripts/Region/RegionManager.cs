using UnityEngine;

public class RegionManager : MonoBehaviour
{
    [SerializeField] RegionSO testRegion;

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
        if (regionFloorObj) Destroy(regionFloorObj);

        regionFloorObj = new GameObject($"{currentRegion.name} - {floorIndex}");

        RegionFloor floor = currentRegion.floors[floorIndex];
        for (int i = 0; i < floor.floorLength; i++)
        {
            RoomObject roomObj = Instantiate(floor.GetRandomRoom(), spawnPos, Quaternion.identity);
            HallwayObject hallwayObj = roomObj.SpawnHallway();
            
            hallwayObj.transform.SetParent(roomObj.transform);
            roomObj.transform.SetParent(regionFloorObj.transform);

            spawnPos = hallwayObj.connectionPoint.position;
        }
    }
}
