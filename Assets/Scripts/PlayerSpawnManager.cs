using UnityEngine;
using Unity.Netcode;

public class PlayerSpawnManager : NetworkBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private Transform[] spawnPoints; // Drag spawn point transforms here
    [SerializeField] private float spawnRadius = 2f;
    
    private int nextSpawnIndex = 0;

    private void Start()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        // Server spawns player at next available spawn point
        Vector3 spawnPosition = GetNextSpawnPosition();
        
        // Get player object for this client
        NetworkObject playerObject = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject;
        
        if (playerObject != null)
        {
            playerObject.transform.position = spawnPosition;
        }
    }

    private Vector3 GetNextSpawnPosition()
    {
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            // Use designated spawn points
            Vector3 spawnPos = spawnPoints[nextSpawnIndex % spawnPoints.Length].position;
            nextSpawnIndex++;
            return spawnPos;
        }
        else
        {
            // Default: spawn in a circle pattern
            float angle = nextSpawnIndex * (360f / 4f); // Assuming max 4 players
            Vector3 offset = new Vector3(
                Mathf.Cos(angle * Mathf.Deg2Rad) * spawnRadius,
                0,
                Mathf.Sin(angle * Mathf.Deg2Rad) * spawnRadius
            );
            nextSpawnIndex++;
            return transform.position + offset;
        }
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
    }
}
