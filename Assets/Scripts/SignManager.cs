using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class SignManager : MonoBehaviour
{
    // Manages the loading, storage, and retrieval of BSL sign and fingerspelling sprites.
    // Supports phrase matching, synonym resolution, and fallback to fingerspelling when no sign is available.
    
    // Singleton instance so other scripts can easily access this
    public static SignManager Instance;

    // Folder path to sign images
    public string signsFolder = "BSL";

    // Folder path to fingerspelling alphabet 
    public string alphabetFolder = "BSL/alphabet";

    // If true, signs will be horizontally flipped for left-handed users
    // //NOT USED because sign images include text so they cannot be flipped
    public bool isLeftHanded = false;

    // Dictionary mapping words to sign images (e.g., "hello" -> hello.png)
    private Dictionary<string, Sprite> wordToSign = new Dictionary<string, Sprite>();

    // Dictionary mapping letters to fingerspelling sprites
    private Dictionary<char, Sprite> charToFingerspell = new Dictionary<char, Sprite>();

    // Dictionary mapping phrases to sign images
    private Dictionary<string, Sprite> phraseToSign = new Dictionary<string, Sprite>();

    // Dictionary mapping synonyms to sign images
    private Dictionary<string, string> synonymMap = new Dictionary<string, string>();

    void Awake()
    {
        // Setup singleton instance and load all signs/alphabet at startup
        if (Instance == null) Instance = this;
        LoadSigns();
        LoadAlphabet();
    }

    void Start()
    {
        // Example manual phrase registration
        ManualSignLibrary.RegisterPhrases(this);
    }

    void LoadSigns()
    {
        // Load all .png files
        Sprite[] signs = Resources.LoadAll<Sprite>(signsFolder);
        foreach (var sign in signs)
        {
            // Remove ".png" extension and lowercase the name
            string key = Path.GetFileNameWithoutExtension(sign.name).ToLower();

            // Add sign to dictionary if it's not already there
            if (!wordToSign.ContainsKey(key))
                wordToSign.Add(key, sign);
        }
    }

    void LoadAlphabet()
    {
        // Load all letter sprites
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

        // Check if the entire input contains a known phrase
        foreach (var phrase in phraseToSign.Keys.OrderByDescending(p => p.Length))
        {
            if (key.Contains(phrase))
            {
                sign = phraseToSign[phrase];
                Debug.Log($"[SignManager] Phrase match found in input: '{phrase}'");
                return true;
            }
        }

        // Resolve synonym
        if (synonymMap.TryGetValue(key, out var canonicalWord))
            key = canonicalWord;

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
    // NOT USED BECAUSE SIGN IMAGES INCLUDE TEXT - could be useful with different imgset
    public Sprite MaybeFlip(Sprite sprite)
    {
        return isLeftHanded ? FlipSprite(sprite) : sprite;
    }

    // Create a horizontally flipped copy of a sprite (for left-handed display)
    // NOT USED BECAUSE SIGN IMAGES INCLUDE TEXT - could be useful with different imgset
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

    // Adds a new phrase and its associated sprite to the phrase-to-sign dictionary
    public void RegisterPhrase(string phraseKey, Sprite phraseSprite)
    {
        string key = phraseKey.ToLower().Trim();
        if (!phraseToSign.ContainsKey(key))
        {
            phraseToSign.Add(key, phraseSprite);
        }
    }
    
    // Adds a new word and its associated sprite to the word-to-sign dictionary.
    public void RegisterWord(string word, Sprite sprite)
    {
        string key = word.ToLower().Trim();
        if (!wordToSign.ContainsKey(key))
            wordToSign.Add(key, sprite);
    }

    // Checks if an exact phrase match exists in the phrase dictionary
    public bool TryGetPhrase(string subtitle, out Sprite phraseSign)
    {
        string key = subtitle.ToLower().Trim();
        bool found = phraseToSign.TryGetValue(key, out phraseSign);
        Debug.Log($"[SignManager] Phrase Lookup '{key}' → {(found ? "FOUND" : "NOT FOUND")}");
        return found;
    }

    // Adds a synonym and maps it to its actual word
    public void RegisterSynonym(string synonym, string canonicalWord)
    {
        string key = synonym.ToLower().Trim();
        string value = canonicalWord.ToLower().Trim();

        if (!synonymMap.ContainsKey(key))
            synonymMap.Add(key, value);
    }
    
}