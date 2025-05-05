using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

// Responsible for dynamically displaying BSL sign images in the UI
// Instantiates sign prefabs based on a given list and adjusts layout and scrolling behavior accordingly
public class SignDisplayUI : MonoBehaviour
{
    public GameObject signImagePrefab;  // SignImage prefab
    public Transform contentPanel;      // SignScrollContent

    public ScrollRect scrollRect; // ScrollRect

    // Clears the current content panel and populates it with sign images based on the provided list
    // Each sign prefab is configured with its corresponding sprite and metadata
    // Resizes each image based on the content panel height and ensures aspect ratio is preserved
    // Finally, initiates a coroutine to auto-scroll to the start of the list
    public void DisplaySigns(List<(Sprite sprite, string originalWord)> signs)
    {
        foreach (Transform child in contentPanel)
            Destroy(child.gameObject);

        foreach (var pair in signs)
        {
            var go = Instantiate(signImagePrefab, contentPanel);
            var img = go.GetComponent<Image>();
            img.sprite = pair.sprite;
            img.preserveAspect = true;

            var metadata = go.AddComponent<SignDisplayMetadata>();
            metadata.originalWord = pair.originalWord;
            metadata.mappedSign = pair.sprite.name;

            var rt = img.GetComponent<RectTransform>();
            float panelHeight = ((RectTransform)contentPanel).rect.height;
            float minHeight = 300;
            float dynamicHeight = Mathf.Max(minHeight, panelHeight * 0.9f);
            rt.sizeDelta = new Vector2(rt.sizeDelta.x, dynamicHeight);
        }

        // Force scroll to the left after layout updates
        StartCoroutine(ScrollToStart());
    }

    // Coroutine that ensures the scroll view resets to the start position after UI layout updates
    // This is done one frame after changes to ensure proper layout timing
    private IEnumerator ScrollToStart()
    {
        // Wait a frame for layout system to update
        yield return null;

        scrollRect.horizontalNormalizedPosition = 0f;
    }
}