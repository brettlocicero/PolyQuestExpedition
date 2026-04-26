using UnityEngine;

[System.Serializable]
public class RegionFloor
{
    public int floorLength = 5;
    public RoomObject[] rooms;

    public RoomObject GetRandomRoom()
    {
        return rooms[Random.Range(0, rooms.Length)];
    }
}
