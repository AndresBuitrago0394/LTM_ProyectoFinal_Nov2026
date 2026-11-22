using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FMODListenerFader : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        FMODUnity.RuntimeManager.StudioSystem.setListenerWeight(1, 0f);
    }

    public void SwitchToListener1 ()
    {
        FMODUnity.RuntimeManager.StudioSystem.setListenerWeight(1, 1f);
        FMODUnity.RuntimeManager.StudioSystem.setListenerWeight(0, 0f);
        Debug.Log("Now Listening To Audio Through Camera");
    }

    public void SwitchToListener0 ()
    {
        FMODUnity.RuntimeManager.StudioSystem.setListenerWeight(1, 0f);
        FMODUnity.RuntimeManager.StudioSystem.setListenerWeight(0, 1f);
        Debug.Log("Now Listening To Audio From Player");
    }
}