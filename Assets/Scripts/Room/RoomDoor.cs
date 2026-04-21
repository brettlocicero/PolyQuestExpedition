using UnityEngine;

public class RoomDoor : MonoBehaviour, IInteractable
{
    bool used = false;

    public void Interact()
    {
        if (!used)
        {
            RoomManager.instance.StartRun();
            used = true;
        }
    }
}
