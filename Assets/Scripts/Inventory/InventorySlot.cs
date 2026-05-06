using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    public int slotIndex;

    [SerializeField] private Image icon;

    private Canvas canvas;

    private GameObject dragIconObj;
    private Image dragIcon;

    void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
    }

    public void UpdateSlot(ItemSO item)
    {
        if (item == null)
        {
            icon.enabled = false;
            icon.sprite = null;
        }
        else
        {
            icon.enabled = true;
            icon.sprite = item.icon;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        ItemSO item = InventoryManager.instance.GetItem(slotIndex);
        if (item == null) return;

        dragIconObj = new GameObject("DragIcon");
        dragIconObj.transform.SetParent(canvas.transform, false);

        dragIcon = dragIconObj.AddComponent<Image>();
        dragIcon.sprite = item.icon;
        dragIcon.raycastTarget = false;

        RectTransform rt = dragIcon.rectTransform;
        // rt.sizeDelta = icon.rectTransform.sizeDelta;
        rt.sizeDelta = new Vector2(50f, 50f);

        rt.pivot = new Vector2(0.5f, 0.5f);

        UpdateDragPosition(eventData);

        dragIconObj.transform.SetAsLastSibling();
        icon.color = Color.clear;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragIconObj == null) return;

        UpdateDragPosition(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (dragIconObj != null)
        {
            Destroy(dragIconObj);
        }

        icon.color = Color.white;
    }

    public void OnDrop(PointerEventData eventData)
    {
        InventorySlot draggedSlot = eventData.pointerDrag?.GetComponent<InventorySlot>();

        if (draggedSlot == null || draggedSlot == this)
            return;

        InventoryManager.instance.SwapItems(slotIndex, draggedSlot.slotIndex);
    }

    private void UpdateDragPosition(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 pos
        );

        dragIcon.rectTransform.anchoredPosition = pos;
    }
}