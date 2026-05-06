using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instance;

    [Header("References")]
    [SerializeField] CanvasGroup inventoryPanelCG;

    [Header("")]
    [SerializeField] List<ItemSO> items = new();
    public List<InventorySlot> slots = new();

    [HideInInspector] public bool isOpen = false;

    void Awake()
    {
        instance = this;

        for (int i = 0; i < slots.Count; i++)
        {
            items.Add(null);
            slots[i].slotIndex = i;
        }
    }

    void Update()
    {
        if (InputManager.Actions.Player.Tab.triggered)
        {
            ToggleInventory();
        }
    }

    void ToggleInventory()
    {
        isOpen = !isOpen;
        if (isOpen)
        {
            inventoryPanelCG.alpha = 1f;
            inventoryPanelCG.interactable = true;
            inventoryPanelCG.blocksRaycasts = true;
            CursorManager.UnlockCursor();
            PlayerInstance.instance.GetPlayerController().SetSensitivity(true);
        }

        else
        {
            inventoryPanelCG.alpha = 0f;
            inventoryPanelCG.interactable = false;
            inventoryPanelCG.blocksRaycasts = false;
            CursorManager.LockCursor();
            PlayerInstance.instance.GetPlayerController().SetSensitivity(false);
        }
    }

    public ItemSO GetItem(int index)
    {
        return items[index];
    }

    public void SetItem(int index, ItemSO item)
    {
        items[index] = item;
        slots[index].UpdateSlot(item);
    }

    public void SwapItems(int indexA, int indexB)
    {
        (items[indexB], items[indexA]) = (items[indexA], items[indexB]);
        slots[indexA].UpdateSlot(items[indexA]);
        slots[indexB].UpdateSlot(items[indexB]);
    }

    public bool AddItem(ItemSO item)
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i] == null)
            {
                SetItem(i, item);
                return true;
            }
        }
        
        return false;
    }
}