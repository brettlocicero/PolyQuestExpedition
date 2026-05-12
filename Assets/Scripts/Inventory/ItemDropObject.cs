using UnityEngine;

public class ItemDropObject : MonoBehaviour, IInteractable
{
    [SerializeField] ItemSO item;
    [Range(0f, 1f)] public float dropChance = 0.5f;

    public void Interact()
    {
        InventoryManager.instance.AddItem(item);
        Destroy(gameObject);
    }
}
