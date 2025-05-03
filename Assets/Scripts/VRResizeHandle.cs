using Meta.XR.Interaction;
using UnityEngine;

public class VRResizeHandle : MonoBehaviour, IGrabEventHandler
{
    public RectTransform targetPanel;
    private Vector3 initialHandlePosition;
    private bool isGrabbing = false;

    void Update()
    {
        if (isGrabbing && targetPanel != null)
        {
            Vector3 delta = transform.position - initialHandlePosition;
            Vector2 size = targetPanel.sizeDelta;
            size.x += delta.x * 1000f;
            size.y += delta.y * 1000f;
            size.x = Mathf.Max(100, size.x);
            size.y = Mathf.Max(100, size.y);
            targetPanel.sizeDelta = size;
            initialHandlePosition = transform.position;
        }
    }

    public void OnGrabStarted(GrabEventArgs args)
    {
        isGrabbing = true;
        initialHandlePosition = transform.position;
    }

    public void OnGrabEnded(GrabEventArgs args)
    {
        isGrabbing = false;
    }
}