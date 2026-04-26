using UnityEngine;

public class RoomObject : MonoBehaviour
{
    public Transform playerSpawn;
    public Transform connectionPoint;
    public HallwayObject[] hallwayObjects;

    public HallwayObject SpawnHallway()
    {
        return Instantiate(hallwayObjects[Random.Range(0, hallwayObjects.Length)], connectionPoint.position, Quaternion.identity);
    }
}
