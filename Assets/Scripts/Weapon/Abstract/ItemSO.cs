using UnityEngine;

public abstract class ItemSO : ScriptableObject
{
    [Header("Item Settings")]
    public string itemName = "Unnamed Item";
    [TextArea] public string description;
}
