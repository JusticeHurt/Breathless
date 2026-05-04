using UnityEngine;
using UnityEngine.Audio; // talk to the Mixer
using UnityEngine.UI;    //talk with sliders
public class VolumeSettings : MonoBehaviour
{
    public AudioMixer mainMixer;
    public Slider masterSlider;
    public Slider musicSlider;
    public Slider sfxSlider;

    public void SetMasterVolume()
    {
        float volume = masterSlider.value;
        mainMixer.SetFloat("MasterVol", Mathf.Log10(Mathf.Max(0.0001f, volume)) * 20);
    }

    public void SetMusicVolume()
    {
        float volume = musicSlider.value;
        // Converts 0-1 linear value to -80 to 0 decibels
        mainMixer.SetFloat("MusicVol",Mathf.Log10(Mathf.Max(0.0001f, volume)) * 20);
    }

    public void SetSFXVolume()
    {
        float volume = sfxSlider.value;
        mainMixer.SetFloat("SFXVol",  Mathf.Log10(Mathf.Max(0.0001f, volume)) * 20);
    }
}