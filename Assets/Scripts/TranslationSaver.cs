using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class TranslationSaver : MonoBehaviour
{
    public TMP_Text subtitleText;
    public Transform signImageContainer;
    [SerializeField] private TMP_Text saveButtonText;

    private string originalSaveButtonText;

    public void SaveTranslation()
    {
        if (signImageContainer.childCount == 0)
        {
            Debug.LogWarning("No sign images found to save.");
            return;
        }

        string timestamp = DateTime.Now.ToLocalTime().ToString("yyyyMMdd_HHmmss");
        string folderPath = Path.Combine(Application.persistentDataPath, "SavedTranslation_" + timestamp);
        Directory.CreateDirectory(folderPath);

        // Save the subtitle
        string subtitlePath = Path.Combine(folderPath, "subtitle.txt");
        File.WriteAllText(subtitlePath, subtitleText.text);

        int index = 0;
        foreach (Transform child in signImageContainer)
        {
            Image img = child.GetComponent<Image>();
            if (img != null && img.sprite != null)
            {
                var metadata = img.GetComponent<SignDisplayMetadata>();
                string spriteName = metadata != null ? metadata.mappedSign : img.sprite.name;
                string subfolder;
                if (spriteName.Length == 1 && char.IsLetter(spriteName[0]))
                    subfolder = "BSL/alphabet";
                else if (int.TryParse(spriteName, out _))
                    subfolder = "BSL/numbers";
                else if (File.Exists(Path.Combine(Application.dataPath, "Resources", "BSL/phrases", spriteName + ".png")))
                    subfolder = "BSL/phrases";
                else
                    subfolder = "BSL";

                Rect rect = img.sprite.rect;
                Texture2D sourceTex = img.sprite.texture;

                Texture2D texture = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.RGBA32, false);
                texture.SetPixels(sourceTex.GetPixels((int)rect.x, (int)rect.y, (int)rect.width, (int)rect.height));
                texture.Apply();

                if (texture != null)
                {
                    byte[] pngData = texture.EncodeToPNG();
                    if (pngData != null)
                    {
                        string destPath = Path.Combine(folderPath, $"sign_{index}_{spriteName}.png");
                        File.WriteAllBytes(destPath, pngData);
                        index++;
                    }
                    else
                    {
                        Debug.LogWarning($"Failed to encode sprite: {spriteName}");
                    }
                }
                else
                {
                    Debug.LogWarning($"No texture found for sprite: {spriteName}");
                }
            }
        }

        Debug.Log("Saved to: " + folderPath);
        // subtitleText.text = "Saved!";

        if (saveButtonText != null)
        {
            originalSaveButtonText = saveButtonText.text;
            saveButtonText.text = "Saved!";
            saveButtonText.color = Color.green;
            Invoke(nameof(RestoreSaveButtonText), 2f);
        }
    }

    private void RestoreSaveButtonText()
    {
        if (saveButtonText != null)
        {
            saveButtonText.text = originalSaveButtonText;
            saveButtonText.color = Color.black;
        }
    }
}