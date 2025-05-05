using UnityEngine;

public class UnitTests : MonoBehaviour
{
    void Awake()
    {
        RunAllTests();
    }

    void RunAllTests()
    {
        Debug.Log("=== Running Manual Unit Tests ===");

        int passed = 0, failed = 0;

        // SignManager test
        var manager = new GameObject().AddComponent<SignManager>();
        ManualSignLibrary.RegisterPhrases(manager);
        var hello = manager.GetSignSpriteForWord("hello");
        if (hello != null) passed++; else { failed++; Debug.LogWarning("SignManager: 'hello' should return a sprite"); }
        var unknown = manager.GetSignSpriteForWord("foobarbaz");
        if (unknown == null) passed++; else { failed++; Debug.LogWarning("SignManager: 'foobarbaz' should return null"); }
        

        // TranslationSaver test
        var saverGO = new GameObject("TestSaver");
        var saver = saverGO.AddComponent<TranslationSaver>();

        // Provide basic setup for required fields
        var subtitleGO = new GameObject("SubtitleText");
        var tmpText = subtitleGO.AddComponent<TMPro.TextMeshProUGUI>();
        tmpText.text = "This is a test subtitle";
        saver.subtitleText = tmpText;

        // Create container for image children
        var container = new GameObject("SignImageContainer").transform;
        container.SetParent(saverGO.transform);
        saver.signImageContainer = container;

        // Create a dummy image with a test sprite
        var imageGO = new GameObject("SignImage");
        imageGO.transform.SetParent(container);

        var img = imageGO.AddComponent<UnityEngine.UI.Image>();
        Texture2D tex = new Texture2D(2, 2);
        tex.SetPixel(0, 0, Color.red);
        tex.Apply();
        img.sprite = Sprite.Create(tex, new Rect(0, 0, 2, 2), Vector2.zero);

        try
        {
            saver.SaveTranslation();
            Debug.Log("TranslationSaver: SaveTranslation() executed");
            passed++;
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning("TranslationSaver test failed: " + ex.Message);
            failed++;
        }

        Debug.Log($"=== Test Summary: Passed = {passed}, Failed = {failed} ===");
    }
}