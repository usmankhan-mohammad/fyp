using UnityEngine;
using System.Collections.Generic;

public class SignTestRunner : MonoBehaviour
{
    public SignDisplayUI signDisplayUI;

    void Start()
    {
        Debug.Log("SignTestRunner started.");

        var testSprite = Resources.Load<Sprite>("BSL/hello");

        if (testSprite != null)
        {
            Debug.Log("Loaded hello sprite.");
            var signs = new List<Sprite> { testSprite };
            signDisplayUI.DisplaySigns(signs);
        }
        else
        {
            Debug.LogError("Could NOT load sprite: Resources/BSL/hello.png");
        }
    }
}