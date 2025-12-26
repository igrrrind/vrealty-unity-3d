using UnityEngine;
using Unity.Netcode;

public class ToggleController : NetworkBehaviour
{
    public bool state;
    public GameObject gameObj;
    public AudioClip[] soundEffects;
    
    public void Interact()
    {
        // Request server to toggle for all clients
        if (IsSpawned)
        {
            ToggleServerRpc();
        }
        else
        {
            // Fallback for non-networked objects
            ToggleState();
        }
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void ToggleServerRpc()
    {
        // Server toggles and syncs to all clients
        ToggleClientRpc();
    }
    
    [ClientRpc]
    private void ToggleClientRpc()
    {
        ToggleState();
    }
    
    private void ToggleState()
    {
        state = !state;
        if (gameObj != null) gameObj.SetActive(state);
        PlayRandomSFXClip(soundEffects);
    }
    private void PlayRandomSFXClip(AudioClip[] soundClips)
    {
        if (soundClips == null || SFXManager.instance == null) return;

        SFXManager.instance.PlayRandomSFXClip(soundClips, transform, 1f);
    }
}
