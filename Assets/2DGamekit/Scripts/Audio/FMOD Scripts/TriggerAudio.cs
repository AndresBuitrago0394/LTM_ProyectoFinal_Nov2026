using UnityEngine;

public class TriggerAudio : MonoBehaviour
{
    // Replaced obsolete string and [EventRef] with the modern EventReference API
    public FMODUnity.EventReference audioEvent;
    public bool PlayOnAwake;

    public void PlayOneShot()
    {
        FMODUnity.RuntimeManager.PlayOneShotAttached(audioEvent, gameObject);
    }

    private void Start()
    {
        if (PlayOnAwake)
            PlayOneShot();
    }
}