using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class AutoNetworkConnect : MonoBehaviour
{
    [Header("Auto Connect Settings")]
    [SerializeField] private float connectionTimeout = 3f;
    [SerializeField] private bool startOnAwake = true;

    private void Start()
    {
        if (startOnAwake)
        {
            StartCoroutine(AutoConnectRoutine());
        }
    }

    private IEnumerator AutoConnectRoutine()
    {
        // Wait a frame to ensure NetworkManager is ready
        yield return null;

        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("NetworkManager not found!");
            yield break;
        }

        Debug.Log("[AutoConnect] Trying to join as client...");
        NetworkManager.Singleton.StartClient();
        
        float timeElapsed = 0f;
        
        // Wait for connection or timeout
        while (timeElapsed < connectionTimeout)
        {
            if (NetworkManager.Singleton.IsConnectedClient && !NetworkManager.Singleton.IsHost)
            {
                Debug.Log("[AutoConnect] Successfully joined as client!");
                yield break;
            }
            
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        
        // Connection failed, shutdown and become host
        Debug.Log("[AutoConnect] Connection timeout, shutting down client...");
        NetworkManager.Singleton.Shutdown();
        
        // Wait for shutdown to complete
        yield return new WaitForSeconds(0.5f);
        
        Debug.Log("[AutoConnect] Starting as host...");
        NetworkManager.Singleton.StartHost();
        
        yield return new WaitForSeconds(0.5f);
        
        if (NetworkManager.Singleton.IsHost)
        {
            Debug.Log("[AutoConnect] Successfully started as host! Waiting for other players...");
        }
    }
}
