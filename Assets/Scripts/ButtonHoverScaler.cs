using UnityEngine;

public class ButtonHoverScaler : MonoBehaviour
{
    private Vector3 originalScale;
    public Vector3 hoverScale = new Vector3(1.2f, 1.2f, 1f);

    private void Awake()
    {
        originalScale = transform.localScale;
    }

    public void OnHoverEnter()
    {
        transform.localScale = hoverScale;
    }

    public void OnHoverExit()
    {
        transform.localScale = originalScale;
    }
}