using UnityEngine;
using UnityEngine.UI;

public class SignPanelGrabHandler : MonoBehaviour
{
    public RectTransform targetPanel;
    private bool isGrabbing = false;
    private Vector3 initialHandlePosition;

    void Start()
    {
        if (targetPanel != null)
        {
            initialHandlePosition = transform.localPosition;
        }
    }

    void Update()
    {
        if (targetPanel == null) return;

        Vector3 delta = transform.localPosition - initialHandlePosition;

        // Ignore Z movement
        Vector2 delta2D = new Vector2(delta.x, delta.y);
        float resizeAmount = delta2D.magnitude * Mathf.Sign(Vector2.Dot(delta2D, Vector2.one));

        Vector2 size = targetPanel.sizeDelta;
        size += new Vector2(resizeAmount, resizeAmount);
        size.x = Mathf.Max(100, size.x);
        size.y = Mathf.Max(100, size.y);
        targetPanel.sizeDelta = size;

        initialHandlePosition = transform.localPosition;
    }
}
