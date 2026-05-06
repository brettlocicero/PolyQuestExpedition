using UnityEngine;

[CreateAssetMenu(fileName = "ItemSO", menuName = "Scriptable Objects/ItemSO")]
public class ItemSO : ScriptableObject
{
    [Header("Item Settings")]
    public string itemName = "Unnamed Item";
    [TextArea] public string description;
    public Sprite icon;
}
