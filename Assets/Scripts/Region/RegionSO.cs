using UnityEngine;

[CreateAssetMenu(fileName = "RegionSO", menuName = "Scriptable Objects/RegionSO")]
public class RegionSO : ScriptableObject
{
    public string regionName;
    public RoomSO[] rooms;

    [Header("Map")]
    public int mapLength = 10;
    public int maxNodesPerLevel = 4;

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

    public RoomSO GetRoom()
    {
        return rooms[Random.Range(0, rooms.Length)];
    }

    public RoomSO GetRoomFromMapLevel(int level)
    {
        return rooms[Random.Range(0, rooms.Length)];
    }
}