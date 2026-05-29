using UnityEngine;

[CreateAssetMenu(fileName = "RegionSO", menuName = "Scriptable Objects/RegionSO")]
public class RegionSO : ScriptableObject
{
    public string regionName;
    public float regionRadius = 1000f;
    public LayerMask groundLayer;

    [Header("")]
    public EnemyAI[] enemies;

    [Header("Environment")]
    public GameObject regionObject;
    public PropSO[] props;

    [Header("VFX")]
    public Material skybox;
    public Color fogColor;
    public Color sunColor;
    public Color intensityColor;

    public void ApplyRegionVFX()
    {
        RenderSettings.skybox = skybox;
        RenderSettings.fogColor = fogColor;
        RenderSettings.sun.color = sunColor;
        RenderSettings.ambientSkyColor = intensityColor;
    }

    public GameObject SpawnRegion()
    {
        GameObject regionObj = new GameObject(regionName + " Object");

        GameObject groundObj = Instantiate(regionObject, Vector3.zero, Quaternion.identity);
        groundObj.transform.SetParent(regionObj.transform);

        SpawnProps(regionObj);

        return regionObj;
    }

    public GameObject SpawnChunk(Vector2Int chunkCoord, float chunkSize, int seed, Transform parent)
    {
        Vector3 chunkOrigin = new Vector3(chunkCoord.x * chunkSize, 0f, chunkCoord.y * chunkSize);
        GameObject chunkObj = new GameObject(regionName + " Chunk " + chunkCoord.x + ", " + chunkCoord.y);
        chunkObj.transform.SetParent(parent);
        chunkObj.transform.position = chunkOrigin;

        GameObject groundObj = Instantiate(regionObject, chunkOrigin, Quaternion.identity);
        groundObj.transform.SetParent(chunkObj.transform);

        SpawnPropsInChunk(chunkObj, chunkOrigin, chunkSize, seed);

        return chunkObj;
    }

    public void SpawnProps(GameObject regionObj)
    {
        Vector3 center = regionObj.transform.position;
        foreach (PropSO prop in props)
        {
            for (int i = 0; i < prop.maxAmount; i++)
            {
                Vector2 randomCircle = Random.insideUnitCircle * regionRadius;
                Vector3 rayOrigin = new Vector3(center.x + randomCircle.x, center.y + 500f, center.z + randomCircle.y);

                if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 1000f, groundLayer))
                {
                    Vector3 pos = new Vector3(hit.point.x, hit.point.y + prop.yOffset, hit.point.z);
                    GameObject spawnedProp = Instantiate(prop.propObj, pos, Quaternion.identity, regionObj.transform);
                    if (prop.randomizeScale)
                    {
                        float randomScale = Random.Range(prop.scaleRange.x, prop.scaleRange.y);
                        spawnedProp.transform.localScale *= randomScale;
                    }

                    if (prop.randomizeRotation)
                    {
                        float randomYRotation = Random.Range(prop.rotationRange.x, prop.rotationRange.y);
                        spawnedProp.transform.rotation = Quaternion.Euler(0f, randomYRotation, 0f);
                    }
                }
            }
        }
    }

    void SpawnPropsInChunk(GameObject chunkObj, Vector3 chunkOrigin, float chunkSize, int seed)
    {
        System.Random random = new System.Random(seed);

        foreach (PropSO prop in props)
        {
            for (int i = 0; i < prop.maxAmount; i++)
            {
                float x = chunkOrigin.x + NextFloat(random, 0f, chunkSize);
                float z = chunkOrigin.z + NextFloat(random, 0f, chunkSize);
                Vector3 rayOrigin = new Vector3(x, chunkOrigin.y + 500f, z);

                if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 1000f, groundLayer))
                {
                    Vector3 pos = new Vector3(hit.point.x, hit.point.y + prop.yOffset, hit.point.z);
                    GameObject spawnedProp = Instantiate(prop.propObj, pos, Quaternion.identity, chunkObj.transform);
                    if (prop.randomizeScale)
                    {
                        float randomScale = NextFloat(random, prop.scaleRange.x, prop.scaleRange.y);
                        spawnedProp.transform.localScale *= randomScale;
                    }

                    if (prop.randomizeRotation)
                    {
                        float randomYRotation = NextFloat(random, prop.rotationRange.x, prop.rotationRange.y);
                        spawnedProp.transform.rotation = Quaternion.Euler(0f, randomYRotation, 0f);
                    }
                }
            }
        }
    }

    float NextFloat(System.Random random, float min, float max)
    {
        return Mathf.Lerp(min, max, (float)random.NextDouble());
    }
}
