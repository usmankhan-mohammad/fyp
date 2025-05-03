using UnityEngine;
using UnityEngine.EventSystems;

public class UIDragResize : MonoBehaviour, IDragHandler, IBeginDragHandler
{
    public RectTransform targetPanel;

    private Vector2 originalSize;
    private Vector2 originalMousePos;

    public void OnBeginDrag(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            targetPanel, eventData.position, eventData.pressEventCamera, out originalMousePos);
        originalSize = targetPanel.sizeDelta;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (targetPanel == null) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            targetPanel, eventData.position, eventData.pressEventCamera, out Vector2 currentMousePos);

        Vector2 delta = currentMousePos - originalMousePos;
        Vector2 newSize = originalSize + new Vector2(delta.x, -delta.y); // drag direction

        newSize.x = Mathf.Max(100, newSize.x);
        newSize.y = Mathf.Max(100, newSize.y);

        targetPanel.sizeDelta = newSize;
    }
}