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
}
