using UnityEngine;

public class SFXManager : MonoBehaviour
{

    [SerializeField] private AudioSource sfxObject;
    public static SFXManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }
    public void PlaySFXClip(AudioClip audioClip, Transform spawnTransform, float volume)
    {
        AudioSource audioSource = Instantiate(sfxObject, spawnTransform.position, Quaternion.identity);
        audioSource.clip = audioClip;

        audioSource.volume = volume;
        audioSource.Play();

        float length = audioSource.clip.length;
        Destroy(audioSource.gameObject, length);
    }

    public void PlayRandomSFXClip(AudioClip[] audioClip, Transform spawnTransform, float volume)
    {
        int random = Random.Range(0, audioClip.Length);
        AudioSource audioSource = Instantiate(sfxObject, spawnTransform.position, Quaternion.identity);
        audioSource.clip = audioClip[random];

        audioSource.volume = volume;
        audioSource.Play();

        float length = audioSource.clip.length;
        Destroy(audioSource.gameObject, length);
    }
}
