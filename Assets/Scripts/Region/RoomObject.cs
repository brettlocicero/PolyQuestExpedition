using UnityEngine;

public class RoomObject : MonoBehaviour
{
    public Transform playerSpawnPoint;

    public void PositionPlayerInRoom()
    {
        PlayerInstance.instance.RepositionPlayer(playerSpawnPoint.position, playerSpawnPoint.rotation);
    }
}
