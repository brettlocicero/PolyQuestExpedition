using UnityEngine;

[CreateAssetMenu(fileName = "RegionSO", menuName = "Scriptable Objects/RegionSO")]
public class RegionSO : ScriptableObject
{
    public string regionName;
    public EnemyAI[] enemies;

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
}
