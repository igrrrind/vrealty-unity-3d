using UnityEngine;
using Unity.Netcode;

public class PlayerCameraController : NetworkBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float minVerticalAngle = -80f;
    [SerializeField] private float maxVerticalAngle = 80f;
    
    [Header("References")]
    public Transform orientation;
    public Transform cameraPos; // Use existing Camera Pos from hierarchy
    
    private float xRotation = 0f;
    private float yRotation = 0f;
    private Camera mainCamera;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        if (IsOwner)
        {
            SetupLocalPlayerCamera();
        }
    }

    private void SetupLocalPlayerCamera()
    {
        Debug.Log("[Camera] Starting MANUAL camera setup for local player...");
        
        // Find camera pos in children if not assigned
        if (cameraPos == null)
        {
            cameraPos = transform.Find("Camera Pos");
            if (cameraPos == null)
            {
                Debug.LogError("[Camera] Camera Pos not found in Player hierarchy!");
                return;
            }
            Debug.Log($"[Camera] Found Camera Pos at position: {cameraPos.position}");
        }
        
        // Find orientation if not assigned
        if (orientation == null)
        {
            orientation = transform.Find("Orientation");
            if (orientation == null)
            {
                Debug.LogWarning("[Camera] Orientation not found, using player transform for rotation.");
                orientation = transform;
            }
            else
            {
                Debug.Log("[Camera] Found Orientation transform");
            }
        }
        
        // Find Main Camera and unparent it
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("[Camera] Main Camera not found! Make sure there's a camera with MainCamera tag.");
            return;
        }
        
        Debug.Log($"[Camera] Found Main Camera at position: {mainCamera.transform.position}");
        
        // Unparent Main Camera so we can control it directly
        if (mainCamera.transform.parent != null)
        {
            Debug.Log($"[Camera] Unparenting Main Camera from {mainCamera.transform.parent.name}");
            mainCamera.transform.SetParent(null);
        }
        
        Debug.Log("[Camera] ✓ Manual camera control setup complete!");
        
        // Lock and hide cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (!IsOwner) return;
        
        HandleMouseLook();
    }

    private void HandleMouseLook()
    {
        // Don't process mouse look if cursor is unlocked (e.g., in pause menu)
        if (Cursor.lockState != CursorLockMode.Locked)
        {
            return;
        }
        
        // Get mouse input
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        
        // Rotate camera up/down
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, minVerticalAngle, maxVerticalAngle);
        
        // Rotate player left/right
        yRotation += mouseX;
        
        // Apply rotations
        if (cameraPos != null)
        {
            cameraPos.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        }
        
        if (orientation != null)
        {
            orientation.rotation = Quaternion.Euler(0f, yRotation, 0f);
        }
        
        // Unlock cursor on ESC
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        
        // Lock cursor on click
        if (Input.GetMouseButtonDown(0))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
    
    void LateUpdate()
    {
        if (!IsOwner) return;
        
        // Manual camera control - directly move Main Camera to follow Camera Pos
        if (mainCamera != null && cameraPos != null && orientation != null)
        {
            // Position follows camera pos
            mainCamera.transform.position = cameraPos.position;
            
            // Rotation combines orientation (horizontal) and camera pos (vertical)
            // cameraPos has vertical rotation (pitch), orientation has horizontal rotation (yaw)
            mainCamera.transform.rotation = orientation.rotation * cameraPos.localRotation;
        }
    }
}
