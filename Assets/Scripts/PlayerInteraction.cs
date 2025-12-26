using UnityEngine;
using Unity.Netcode;

public class PlayerInteraction : NetworkBehaviour
{
    public float reachLength = 3f;  
    public Interactable currentInteractable;
    private Camera mainCamera;
    
    private void Start()
    {
        // Get main camera (Cinemachine will use this)
        mainCamera = Camera.main;
    }
    
    void Update()
    {
        // Only local player can interact
        if (!IsOwner) return;
        
        CheckInteraction();
        if (Input.GetKeyDown(KeyCode.F) && currentInteractable != null)
        {
            currentInteractable.Interact();
        }
    }

    private void CheckInteraction()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null) return;
        }
        
        RaycastHit hit;
        Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
        if (Physics.Raycast(ray, out hit, reachLength))
        {
            if (hit.collider.CompareTag("Interactable"))
            {
                Interactable newInteractable = hit.collider.GetComponentInParent<Interactable>();
                if (newInteractable.enabled && newInteractable != null)
                {
                    SetCurrentInteractable(newInteractable);
                }
                else
                {
                    DisableCurrentInteractable();
                }
            }
            else
            {
                DisableCurrentInteractable();
            }
        }
        else
        {
            DisableCurrentInteractable();
        }
    }
    void SetCurrentInteractable(Interactable interactable)
    {
        currentInteractable = interactable;
        currentInteractable.EnableOutline();
        HUDController.instance.EnableInteractionText(currentInteractable.message);
    }

    void DisableCurrentInteractable()
    {
        HUDController.instance.DisableInteractionText();
        if (currentInteractable)
        {
            currentInteractable.DisableOutline();
            currentInteractable = null;
        }
    }
}
