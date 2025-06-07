using UnityEngine;
using UnityEngine.Audio;
public class SoundMixerManager : MonoBehaviour
{
    [SerializeField] private AudioMixer _audioMixer;

    public void SetMasterVolumeLevel(float level)
    {
        _audioMixer.SetFloat("masterVolume", Mathf.Log10(level) * 20f);
    }

    public void SetSFXVolumeLevel(float level)
    {
        _audioMixer.SetFloat("sfxVolume", Mathf.Log10(level) * 20f);
    }

    public void SetMusicVolumeLevel(float level)
    {
        _audioMixer.SetFloat("musicVolume", Mathf.Log10(level) * 20f);
    }
}
