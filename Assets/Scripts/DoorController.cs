using UnityEngine;

public class DoorController : MonoBehaviour
{
    private Animator door;
    public bool trigger = false; //false = close, true = open
    private bool pastTrigger;
    public string index;
    private string doorOpen, doorClose;
    public AudioClip doorOpenSFX, doorCloseSFX;

    void Start()
    {
        door = GetComponent<Animator>();
        doorOpen = "DoorOpen" + index;
        doorClose = "DoorClose" + index;
    }
    void Update()
    {
        if (trigger && !pastTrigger) OpenDoor();
        if (!trigger && pastTrigger) CloseDoor();   
        pastTrigger = trigger;
    }
    public void OpenDoor()
    {
        door.Play(doorOpen);
        PlaySFXClip(doorOpenSFX);
    }

    public void CloseDoor()
    {
        door.Play(doorClose);
        PlaySFXClip(doorCloseSFX);
    }
    public void Interact()
    {
        trigger = !trigger;
    }

    private void PlayRandomSFXClip(AudioClip[] soundClips)
    {
        if (soundClips == null || SFXManager.instance == null) return;

        SFXManager.instance.PlayRandomSFXClip(soundClips, transform, 1f);
    }
    private void PlaySFXClip(AudioClip soundClip)
    {
        if (soundClip == null || SFXManager.instance == null) return;
        SFXManager.instance.PlaySFXClip(soundClip, transform, 1f);
    }
}
