using System.Collections;
using UnityEngine;

public class RegionManager : MonoBehaviour
{
    public static RegionManager instance;
    void Awake()
    {
        instance = this;
    }

    [Header("References")]
    [SerializeField] EnemySpawner enemySpawner;
    [SerializeField] GameObject hubObjects;
    [SerializeField] Animator regionEntranceAnim;

    GameObject regionObj;
    RegionSO currentRegion;

    public void EnterRegion(RegionSO region)
    {
        if (regionObj) Destroy(regionObj);
        StartCoroutine(EnterRegionWorker(region));
    }

    IEnumerator EnterRegionWorker(RegionSO region)
    {
        regionEntranceAnim.SetTrigger("RoomTransition");

        yield return new WaitForSeconds(0.75f); // Midpoint of the RoomTransition animation.

        currentRegion = region;
        region.ApplyRegionVFX();
        regionObj = region.SpawnRegion();
        hubObjects.SetActive(false);

        enemySpawner.StartSpawnLoop();
    }
}
