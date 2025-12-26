using UnityEngine;
using Unity.Netcode;

public class Movement : MonoBehaviour
{
    [Header("Deprecated - Use PlayerCameraController instead")]
    public bool useDeprecatedSystem = false;
    
    public float mouseSensitivity = 100f;
    public Transform orientation;
    public float xRotation = 0f;
    public float yRotation = 0f;

    public PauseMenu pauseMenu;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (!useDeprecatedSystem)
        {
            Debug.Log("[Movement] This script is deprecated. Using PlayerCameraController with Cinemachine instead.");
            enabled = false;
            return;
        }
        
        //this lock the cursor at the middle of the screen and make it invisible
        Cursor.lockState = CursorLockMode.Locked;
        
        // Try to find local player orientation if not set
        if (orientation == null)
        {
            FindLocalPlayerOrientation();
        }
    }

    private void FindLocalPlayerOrientation()
    {
        // Find local player (the one owned by this client)
        var players = FindObjectsByType<PlayerCameraController>(FindObjectsSortMode.None);
        foreach (var player in players)
        {
            if (player.TryGetComponent<NetworkBehaviour>(out var netBehaviour) && netBehaviour.IsOwner)
            {
                orientation = player.orientation;
                Debug.Log("[Movement] Found local player orientation");
                return;
            }
        }
        
        Debug.LogWarning("[Movement] Could not find local player orientation");
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!useDeprecatedSystem) return;

        if (pauseMenu != null && pauseMenu.isPaused)
        {
            UnlockCursor();
        }
        else
        {
            LockCursor();
            
            if (orientation == null)
            {
                FindLocalPlayerOrientation();
                return;
            }
            
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            xRotation -= mouseY;
            yRotation += mouseX;

            transform.rotation = Quaternion.Euler(xRotation, yRotation, 0f);
            orientation.rotation = Quaternion.Euler(0f, yRotation, 0f);
        }
    }

    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
    }

    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }
}
