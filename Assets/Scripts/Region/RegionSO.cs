using UnityEngine;

[CreateAssetMenu(fileName = "RegionSO", menuName = "Scriptable Objects/RegionSO")]
public class RegionSO : ScriptableObject
{
    public string regionName = "Unnamed Region";
    public RoomObject entranceRoom;

    [Header("Floor Settings")]
    public int floorLength = 5;
    public RoomObject[] rooms;

    [Header("VFX")]
    public Color fogColor;
    public Material skybox;

    public RoomObject GetRandomRoom()
    {
        return rooms[Random.Range(0, rooms.Length)];
    }
}