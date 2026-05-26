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
    [SerializeField] Animator regionEntranceAnim;

    RegionSO currentRegion;

    public void EnterRegion(RegionSO region)
    {
        StartCoroutine(EnterRegionWorker(region));
    }

    IEnumerator EnterRegionWorker(RegionSO region)
    {
        regionEntranceAnim.SetTrigger("RoomTransition");

        yield return new WaitForSeconds(0.75f); // Midpoint of the RoomTransition animation.

        currentRegion = region;
        region.ApplyRegionVFX();
        enemySpawner.StartSpawnLoop();
    }
}
