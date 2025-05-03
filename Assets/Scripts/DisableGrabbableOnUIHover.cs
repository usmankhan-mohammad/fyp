using UnityEngine;
using Oculus.Interaction;

public class DisableGrabbableOnUIHover : MonoBehaviour
{
    public Grabbable grabbable;

    void Update()
    {
        if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            if (grabbable != null)
                grabbable.enabled = false;
        }
        else
        {
            if (grabbable != null)
                grabbable.enabled = true;
        }
    }
}