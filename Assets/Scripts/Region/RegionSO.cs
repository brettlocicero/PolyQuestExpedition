using UnityEngine;

[CreateAssetMenu(fileName = "RegionSO", menuName = "Scriptable Objects/RegionSO")]
public class RegionSO : ScriptableObject
{
    [Header("Rooms")]
    public RegionRoom startingRoomPrefab;
    public RegionRoom endingRoomPrefab;
    public RegionRoom[] roomPrefabs;
    public int roomCount = 7;

    [Header("Hallways")]
    public GameObject hallwayPrefab;
    public float hallwayPrefabLength = 1f;

    [Header("Placement")]
    public int maxPlacementAttempts = 100;
    public float colliderShrink = 0.05f;
    public float roomSpacing = 5f;

    [Header("VFX")]
    public Material skybox;
    public Color sunColor;
    public Color ambientColor;

    public void ApplyVFX()
    {
        RenderSettings.skybox = skybox;
        RenderSettings.sun.color = sunColor;
        RenderSettings.ambientSkyColor = ambientColor;
    }
}