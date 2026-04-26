using UnityEngine;

public class FloorDoor : MonoBehaviour, IInteractable
{
    bool used = false;

    public void Interact()
    {
        if (!used)
        {
            RegionManager.instance.SpawnNextRegionFloor();
            used = true;
        }
    }
}
