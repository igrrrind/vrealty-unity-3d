using UnityEngine;

public class ToggleController : MonoBehaviour
{
    public bool state;
    public GameObject gameObj;
    public AudioClip[] soundEffects;
    public void Interact()
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
