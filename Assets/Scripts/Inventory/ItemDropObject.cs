using UnityEngine;

public class ItemDropObject : MonoBehaviour, IInteractable
{
    [SerializeField] ItemSO item;

    public void Interact()
    {
        InventoryManager.instance.AddItem(item);
        // Destroy(gameObject);
    }
}
