using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] Transform player;

    [Header("Infinite Generation")]
    [SerializeField] int worldSeed = 12345;
    [SerializeField] float chunkSize = 200f;
    [SerializeField] int loadRadius = 2;

    GameObject regionObj;
    RegionSO currentRegion;
    Vector2Int currentPlayerChunk;
    readonly Dictionary<Vector2Int, GameObject> loadedChunks = new Dictionary<Vector2Int, GameObject>();
    Coroutine enterRegionCoroutine;

    void OnValidate()
    {
        chunkSize = Mathf.Max(1f, chunkSize);
        loadRadius = Mathf.Max(0, loadRadius);
    }

    void Update()
    {
        if (!currentRegion || !regionObj)
            return;

        if (!player && PlayerInstance.instance)
            player = PlayerInstance.instance.transform;

        if (!player)
            return;

        Vector2Int playerChunk = WorldToChunk(player.position);
        if (playerChunk == currentPlayerChunk && loadedChunks.Count > 0)
            return;

        currentPlayerChunk = playerChunk;
        UpdateLoadedChunks();
    }

    public void EnterRegion(RegionSO region)
    {
        if (enterRegionCoroutine != null)
            StopCoroutine(enterRegionCoroutine);

        if (regionObj)
            Destroy(regionObj);

        loadedChunks.Clear();
        enterRegionCoroutine = StartCoroutine(EnterRegionWorker(region));
    }

    IEnumerator EnterRegionWorker(RegionSO region)
    {
        regionEntranceAnim.SetTrigger("RoomTransition");

        yield return new WaitForSeconds(0.75f); // Midpoint of the RoomTransition animation.

        currentRegion = region;
        region.ApplyRegionVFX();
        regionObj = new GameObject(region.regionName + " Object");
        hubObjects.SetActive(false);

        if (!player && PlayerInstance.instance)
            player = PlayerInstance.instance.transform;

        currentPlayerChunk = player ? WorldToChunk(player.position) : Vector2Int.zero;
        UpdateLoadedChunks();

        enemySpawner.StartSpawnLoop();
        enterRegionCoroutine = null;
    }

    Vector2Int WorldToChunk(Vector3 position)
    {
        return new Vector2Int(Mathf.FloorToInt(position.x / chunkSize), Mathf.FloorToInt(position.z / chunkSize));
    }

    void UpdateLoadedChunks()
    {
        HashSet<Vector2Int> chunksToKeep = new HashSet<Vector2Int>();
        for (int x = -loadRadius; x <= loadRadius; x++)
        {
            for (int z = -loadRadius; z <= loadRadius; z++)
            {
                Vector2Int chunkCoord = currentPlayerChunk + new Vector2Int(x, z);
                chunksToKeep.Add(chunkCoord);

                if (!loadedChunks.ContainsKey(chunkCoord))
                    LoadChunk(chunkCoord);
            }
        }

        List<Vector2Int> chunksToUnload = new List<Vector2Int>();
        foreach (Vector2Int chunkCoord in loadedChunks.Keys)
        {
            if (!chunksToKeep.Contains(chunkCoord))
                chunksToUnload.Add(chunkCoord);
        }

        foreach (Vector2Int chunkCoord in chunksToUnload)
        {
            Destroy(loadedChunks[chunkCoord]);
            loadedChunks.Remove(chunkCoord);
        }
    }

    void LoadChunk(Vector2Int chunkCoord)
    {
        int chunkSeed = GetChunkSeed(chunkCoord);
        GameObject chunkObj = currentRegion.SpawnChunk(chunkCoord, chunkSize, chunkSeed, regionObj.transform);
        loadedChunks.Add(chunkCoord, chunkObj);
    }

    int GetChunkSeed(Vector2Int chunkCoord)
    {
        unchecked
        {
            int hash = worldSeed;
            hash = hash * 397 ^ GetStableStringHash(currentRegion.regionName);
            hash = hash * 397 ^ chunkCoord.x;
            hash = hash * 397 ^ chunkCoord.y;
            return hash;
        }
    }

    int GetStableStringHash(string value)
    {
        unchecked
        {
            int hash = 23;

            if (string.IsNullOrEmpty(value))
                return hash;

            for (int i = 0; i < value.Length; i++)
                hash = hash * 31 + value[i];

            return hash;
        }
    }
}
