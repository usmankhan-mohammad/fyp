using UnityEngine;
using UnityEditor;
using System.IO;

public class BulkSpriteImporter
{
    [MenuItem("Tools/Convert BSL PNGs to Sprites")]
    public static void ConvertAllPNGsToSprites()
    {
        string path = "Assets/Resources/BSL";
        string[] files = Directory.GetFiles(path, "*.png", SearchOption.AllDirectories);

        foreach (var file in files)
        {
            string assetPath = file.Replace("\\", "/");
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;

            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.SaveAndReimport();
                Debug.Log($"Converted to Sprite: {assetPath}");
            }
        }

        AssetDatabase.Refresh();
        Debug.Log("✅ All PNGs converted to Sprites.");
    }
}