using System.Collections;
using UnityEngine;

public class RegionManager : MonoBehaviour
{
    public static RegionManager instance;
    void Awake()
    {
        instance = this;
    }

    [Header("Runtime")]
    [SerializeField] int currentRoomCount = 0;

    [Header("References")]
    [SerializeField] EnemySpawner enemySpawner;
    [SerializeField] GameObject hubObjects;
    [SerializeField] Animator regionEntranceAnim;

    GameObject roomObj;
    RegionSO currentRegion;

    public void EnterRegion(RegionSO region)
    {
        StartCoroutine(EnterRegionWorker(region));
    }

    IEnumerator EnterRegionWorker(RegionSO region)
    {
        regionEntranceAnim.SetTrigger("RoomTransition");
        currentRegion = region;
        currentRoomCount = 0;

        yield return new WaitForSeconds(0.75f); // Midpoint of the RoomTransition animation.

        region.ApplyRegionVFX();
        hubObjects.SetActive(false);
        SpawnRegionRoom();

        // enemySpawner.StartSpawnLoop();
    }

    public void SpawnRegionRoom()
    {
        // Cleanup old room object if still there
        if (roomObj) Destroy(roomObj);

        RoomSO regionRoom = currentRegion.GetRoom();
        roomObj = Instantiate(regionRoom.roomObject, Vector3.zero, Quaternion.identity);
        currentRoomCount++;
    }
}
