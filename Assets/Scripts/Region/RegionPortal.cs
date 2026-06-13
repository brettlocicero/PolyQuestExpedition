using UnityEngine;

public class RegionPortal : MonoBehaviour, IInteractable
{
    [SerializeField] RegionSO region;
    bool used = false;

    public void Interact()
    {
        if (used) return;
        
        RegionGenerator.instance.EnterRegion(region);
        used = true;
    }
}