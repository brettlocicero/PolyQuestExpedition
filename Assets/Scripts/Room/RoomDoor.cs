using UnityEngine;

public class RoomDoor : MonoBehaviour, IInteractable
{
    [SerializeField] bool runStarter = false;
    bool used = false;

    public void Interact()
    {
        if (!used)
        {
            if (runStarter)
                RoomManager.instance.StartRun();
            else
                RoomManager.instance.SpawnRoom();

            used = true;
        }
    }
}
