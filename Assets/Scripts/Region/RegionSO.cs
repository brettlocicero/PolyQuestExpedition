using UnityEngine;

[CreateAssetMenu(fileName = "RegionSO", menuName = "Scriptable Objects/RegionSO")]
public class RegionSO : ScriptableObject
{
    public string regionName;
    public EnemyAI[] enemies;

    [Header("Environment")]
    public GameObject groundObject;

    [Header("VFX")]
    public Material skybox;
    public Color fogColor;
    public Color sunColor;

    public void ApplyRegionVFX()
    {
        RenderSettings.skybox = skybox;
        RenderSettings.fogColor = fogColor;
        RenderSettings.sun.color = sunColor;
    }

    public GameObject SpawnRegion()
    {
        GameObject regionObj = new GameObject(regionName + " Object");
        GameObject groundObj = Instantiate(groundObject, Vector3.zero, Quaternion.identity);
        groundObj.transform.SetParent(regionObj.transform);

        return regionObj;
    }
}
