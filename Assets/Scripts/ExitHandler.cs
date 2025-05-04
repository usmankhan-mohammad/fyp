using UnityEngine;

public class ExitHandler : MonoBehaviour
{
    public void QuitProgram()
    {
        Debug.Log("[ExitButtonHandler] Exiting application...");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}