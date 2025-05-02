using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SignDisplayUI : MonoBehaviour
{
    public GameObject signImagePrefab;  // Assign your prefab here
    public Transform contentPanel;      // Drag in SignScrollContent

    public ScrollRect scrollRect; // drag in the ScrollRect from the Inspector

    public void DisplaySigns(List<Sprite> signs)
    {
        foreach (Transform child in contentPanel)
            Destroy(child.gameObject);

        foreach (var sprite in signs)
        {
            var go = Instantiate(signImagePrefab, contentPanel);
            var img = go.GetComponent<Image>();
            img.sprite = sprite;
            img.preserveAspect = true;
            var rt = img.GetComponent<RectTransform>();
            float panelHeight = ((RectTransform)contentPanel).rect.height;
            float minHeight = 300; // or whatever minimum you prefer
            float dynamicHeight = Mathf.Max(minHeight, panelHeight * 0.9f);
            rt.sizeDelta = new Vector2(rt.sizeDelta.x, dynamicHeight);
        }

        // 🧠 Force scroll to the left after layout updates
        StartCoroutine(ScrollToStart());
    }

    private IEnumerator ScrollToStart()
    {
        // Wait a frame for layout system to update
        yield return null;

        scrollRect.horizontalNormalizedPosition = 0f;
    }
}