using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Unity.Netcode;

public class Interactable : NetworkBehaviour
{
    private Outline outline;
    public string message;

    public UnityEvent onInteraction;
    void Start()
    {
        outline = GetComponent<Outline>();
        //DisableOutline();

    }
    public void Interact()
    {
        onInteraction.Invoke();
    }
    public void DisableOutline()
    {
        //outline.enabled = false;
    }
    public void EnableOutline()
    {
        //outline.enabled = true;
    }



}
