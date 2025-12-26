using UnityEngine;

// This script is deprecated - now using Cinemachine
// Keep for backwards compatibility or delete if not needed
public class MoveCamera : MonoBehaviour
{
    public Transform cameraPosition;
    
    [Header("Deprecated - Use PlayerCameraController with Cinemachine instead")]
    public bool useDeprecatedSystem = false;
    
    void Update()
    {
        if (useDeprecatedSystem && cameraPosition != null)
        {
            transform.position = cameraPosition.position;
        }
    }
}
