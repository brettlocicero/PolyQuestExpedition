using UnityEngine;

[CreateAssetMenu(fileName = "RegionSO", menuName = "Scriptable Objects/RegionSO")]
public class RegionSO : ScriptableObject
{
    [Header("Region Settings")]
    public string regionName;

    [Header("")]
    public RoomObject[] rooms;

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

    public RoomObject GetRoomToSpawn()
    {
        return rooms[Random.Range(0, rooms.Length)];
    }
}