using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(NetworkObject))]
public class NetworkPlayerSetup : NetworkBehaviour
{
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    private void Awake()
    {
        // Verify NetworkObject exists
        var netObj = GetComponent<NetworkObject>();
        if (netObj == null)
        {
            Debug.LogError("[NetworkPlayerSetup] NetworkObject component missing!");
        }
        
        // Verify required components
        if (GetComponent<PlayerMovement>() == null)
        {
            Debug.LogWarning("[NetworkPlayerSetup] PlayerMovement component missing!");
        }
        
        if (GetComponent<PlayerCameraController>() == null)
        {
            Debug.LogWarning("[NetworkPlayerSetup] PlayerCameraController component missing!");
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        if (showDebugLogs)
        {
            Debug.Log($"[NetworkPlayerSetup] Player spawned! IsOwner: {IsOwner}, IsServer: {IsServer}, ClientId: {OwnerClientId}");
        }
        
        // Set player name for debugging
        gameObject.name = IsOwner ? $"Player (Local - {OwnerClientId})" : $"Player (Remote - {OwnerClientId})";
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        
        if (showDebugLogs)
        {
            Debug.Log($"[NetworkPlayerSetup] Player despawned! ClientId: {OwnerClientId}");
        }
    }
}
