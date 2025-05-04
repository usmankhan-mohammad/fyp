using UnityEngine;

public static class ManualSignLibrary
{
    public static void RegisterPhrases(SignManager manager)
    {
        //phrases
        manager.RegisterPhrase("how old are you", Resources.Load<Sprite>("BSL/phrases/how-old-are-you"));
        manager.RegisterPhrase("what is your age", Resources.Load<Sprite>("BSL/phrases/how-old-are-you"));
        manager.RegisterPhrase("you're welcome", Resources.Load<Sprite>("BSL/phrases/yw"));
        manager.RegisterPhrase("you are welcome", Resources.Load<Sprite>("BSL/phrases/yw"));
        manager.RegisterPhrase("coming home", Resources.Load<Sprite>("BSL/phrases/coming-home"));
        manager.RegisterPhrase("footballs coming home", Resources.Load<Sprite>("BSL/phrases/footballs-coming-home"));
        manager.RegisterPhrase("whats your name", Resources.Load<Sprite>("BSL/phrases/whats-your-name"));
        manager.RegisterPhrase("what is your name", Resources.Load<Sprite>("BSL/phrases/whats-your-name"));
        manager.RegisterPhrase("what's your name", Resources.Load<Sprite>("BSL/phrases/whats-your-name"));
        manager.RegisterPhrase("i love you", Resources.Load<Sprite>("BSL/phrases/ily"));
        manager.RegisterPhrase("how are you", Resources.Load<Sprite>("BSL/phrases/hru"));
        manager.RegisterPhrase("how're you", Resources.Load<Sprite>("BSL/phrases/hru"));
        manager.RegisterPhrase("happy new year", Resources.Load<Sprite>("BSL/phrases/happy-new-year"));
        manager.RegisterPhrase("where does it hurt", Resources.Load<Sprite>("BSL/phrases/where-does-it-hurt"));
        manager.RegisterPhrase("where's it hurt", Resources.Load<Sprite>("BSL/phrases/where-does-it-hurt"));
        
        //numbers
        manager.RegisterWord("one", Resources.Load<Sprite>("BSL/numbers/1"));
        manager.RegisterWord("two", Resources.Load<Sprite>("BSL/numbers/2"));
        manager.RegisterWord("three", Resources.Load<Sprite>("BSL/numbers/3"));
        manager.RegisterWord("four", Resources.Load<Sprite>("BSL/numbers/4"));
        manager.RegisterWord("five", Resources.Load<Sprite>("BSL/numbers/5"));
        manager.RegisterWord("six", Resources.Load<Sprite>("BSL/numbers/6"));
        manager.RegisterWord("seven", Resources.Load<Sprite>("BSL/numbers/7"));
        manager.RegisterWord("eight", Resources.Load<Sprite>("BSL/numbers/8"));
        manager.RegisterWord("nine", Resources.Load<Sprite>("BSL/numbers/9"));
        manager.RegisterWord("ten", Resources.Load<Sprite>("BSL/numbers/10"));
        

        
        //synonyms
        manager.RegisterSynonym("hi", "hello"); // internally maps "hi" → "hello"
        manager.RegisterSynonym("hey", "hello");
        manager.RegisterSynonym("greetings", "hello");
        manager.RegisterSynonym("yo", "hello");
        
    }
}
