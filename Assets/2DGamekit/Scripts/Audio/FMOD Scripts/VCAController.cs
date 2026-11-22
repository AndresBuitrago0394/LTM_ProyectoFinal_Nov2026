using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VCAController : MonoBehaviour
{
    private FMOD.Studio.VCA vcaController;
    public string VcaName;

    private Slider slider;

    void Start()
    {
        vcaController = FMODUnity.RuntimeManager.GetVCA("vca:/" + VcaName);
        slider = GetComponent<Slider>();
    }
    
    public void SetVolume(float volume)
    {
        // FIX: FMOD's API requires a lowercase 's' for setVolume
        vcaController.setVolume(volume);
    }
}