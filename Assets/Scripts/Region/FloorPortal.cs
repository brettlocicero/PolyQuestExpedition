using UnityEngine;

public class FloorPortal : MonoBehaviour, IInteractable
{
    bool used = false;

    public void Interact()
    {
        if (used) return;
        
        RegionGenerator.instance.GenerateFloor();
        used = true;
    }
}
