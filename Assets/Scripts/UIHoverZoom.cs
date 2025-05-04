using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class UIHoverZoom : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public float zoomScale = 1.5f;
    public float zoomSpeed = 5f;

    private Vector3 originalScale;
    private bool isHovered = false;
    private List<RectMask2D> disabledMasks = new List<RectMask2D>();

    void Start()
    {
        originalScale = transform.localScale;
    }

    void Update()
    {
        Vector3 targetScale = isHovered ? originalScale * zoomScale : originalScale;
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * zoomSpeed);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        disabledMasks.Clear();

        Transform current = transform.parent;
        while (current != null)
        {
            var mask = current.GetComponent<RectMask2D>();
            if (mask != null && mask.enabled)
            {
                mask.enabled = false;
                disabledMasks.Add(mask);
            }
            current = current.parent;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;

        foreach (var mask in disabledMasks)
        {
            if (mask != null)
                mask.enabled = true;
        }

        disabledMasks.Clear();
    }
}