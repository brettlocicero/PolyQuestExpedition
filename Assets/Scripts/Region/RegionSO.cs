using UnityEngine;
using System.Collections.Generic; // Required for List

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
        GameObject regionObj = new(regionName + " Object");

        GameObject groundObj = Instantiate(regionObject, Vector3.zero, Quaternion.identity);
        groundObj.transform.SetParent(regionObj.transform);

        SpawnProps(regionObj);

        return regionObj;
    }

    public void SpawnProps(GameObject regionObj)
    {
        Vector3 center = regionObj.transform.position;
        List<Vector3> spawnedPositions = new List<Vector3>();
        foreach (PropSO prop in props)
        {
            for (int i = 0; i < prop.maxAmount; i++)
            {
                int maxAttempts = 10; 
                bool successfullySpawned = false;

                for (int attempt = 0; attempt < maxAttempts; attempt++)
                {
                    Vector2 randomCircle = Random.insideUnitCircle * regionRadius;
                    Vector3 rayOrigin = new(center.x + randomCircle.x, center.y + 500f, center.z + randomCircle.y);

                    if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 1000f, groundLayer))
                    {
                        Vector3 targetPos = hit.point;

                        if (IsPositionTooClose(targetPos, spawnedPositions, prop.minSpawnDistance))
                        {
                            continue;
                        }

                        Vector3 pos = new(targetPos.x, targetPos.y + prop.yOffset, targetPos.z);
                        GameObject spawnedProp = Instantiate(prop.propObj, pos, Quaternion.identity, regionObj.transform);
                        
                        spawnedPositions.Add(targetPos);

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

                        successfullySpawned = true;
                        break;
                    }
                }
                
                if (!successfullySpawned)
                {
                    Debug.LogWarning($"Could not find a valid non-overlapping spot for {prop.name} after {maxAttempts} tries.");
                }
            }
        }
    }

    bool IsPositionTooClose(Vector3 targetPos, List<Vector3> existingPositions, float minDistance)
    {
        float minDistanceSqr = minDistance * minDistance;
        foreach (Vector3 existingPos in existingPositions)
        {
            if ((targetPos - existingPos).sqrMagnitude < minDistanceSqr)
            {
                return true;
            }
        }

        return false;
    }
}