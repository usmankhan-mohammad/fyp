using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class SignManager : MonoBehaviour
{
    // Singleton instance so other scripts can easily access this
    public static SignManager Instance;

    // Folder path to main sign images (e.g., "BSL" under Resources)
    public string signsFolder = "BSL";

    // Folder path to fingerspelling alphabet (e.g., "BSL/alphabet" under Resources)
    public string alphabetFolder = "BSL/alphabet";

    // If true, signs will be horizontally flipped for left-handed users
    public bool isLeftHanded = false;

    // Dictionary mapping words to sign images (e.g., "hello" -> hello.png)
    private Dictionary<string, Sprite> wordToSign = new Dictionary<string, Sprite>();

    // Dictionary mapping letters to fingerspelling sprites (e.g., 'h' -> h.png)
    private Dictionary<char, Sprite> charToFingerspell = new Dictionary<char, Sprite>();

    void Awake()
    {
        // Setup singleton instance and load all signs/alphabet at startup
        if (Instance == null) Instance = this;
        LoadSigns();
        LoadAlphabet();
    }

    void LoadSigns()
    {
        // Load all .png files from Resources/BSL folder
        Sprite[] signs = Resources.LoadAll<Sprite>(signsFolder);
        foreach (var sign in signs)
        {
            // Remove ".png" extension and lowercase the name to normalize keys
            string key = Path.GetFileNameWithoutExtension(sign.name).ToLower();

            // Add sign to dictionary if it's not already present
            if (!wordToSign.ContainsKey(key))
                wordToSign.Add(key, sign);
        }
    }

    void LoadAlphabet()
    {
        // Load all letter sprites from Resources/BSL/alphabet
        Sprite[] letters = Resources.LoadAll<Sprite>(alphabetFolder);
        foreach (var letter in letters)
        {
            // File name (e.g., "a.png" => "a")
            string name = Path.GetFileNameWithoutExtension(letter.name).ToLower();

            // Make sure it's a single alphabet character before adding
            if (name.Length == 1 && char.IsLetter(name[0]))
            {
                charToFingerspell[name[0]] = letter;
            }
        }
    }

    // Try to find a sign for a given word
    public bool TryGetSign(string word, out Sprite sign)
    {
        string key = word.ToLower();
        bool found = wordToSign.TryGetValue(key, out sign);
        Debug.Log($"[SignManager] Lookup '{key}' → {(found ? "FOUND" : "NOT FOUND")}");
        return found;
    }

    // Generate fingerspelling sprite list for a word (if sign doesn't exist)
    public List<Sprite> GetFingerspelling(string word)
    {
        List<Sprite> result = new List<Sprite>();
        foreach (char c in word.ToLower())
        {
            // Add letter sprite if available (e.g., 'h' -> h.png)
            if (charToFingerspell.TryGetValue(c, out Sprite letterImg))
                result.Add(letterImg);
        }
        return result;
    }

    // Optionally flip the sprite horizontally if left-handed mode is on
    public Sprite MaybeFlip(Sprite sprite)
    {
        return isLeftHanded ? FlipSprite(sprite) : sprite;
    }

    // Create a horizontally flipped copy of a sprite (for left-handed display)
    private Sprite FlipSprite(Sprite original)
    {
        // Create new texture the same size as the original
        Texture2D flipped = new Texture2D(original.texture.width, original.texture.height);

        // Copy each pixel from the original, flipping X-axis
        for (int y = 0; y < original.texture.height; y++)
        {
            for (int x = 0; x < original.texture.width; x++)
            {
                // Mirror horizontally: flip x
                Color pixel = original.texture.GetPixel(original.texture.width - x - 1, y);
                flipped.SetPixel(x, y, pixel);
            }
        }

        flipped.Apply(); // Apply pixel changes to texture

        // Create a new sprite from the flipped texture, using the same rect and pivot
        return Sprite.Create(flipped, original.rect, new Vector2(0.5f, 0.5f));
    }
}