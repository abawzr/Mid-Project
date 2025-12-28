using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioSettings : MonoBehaviour
{
    [Header("Mixer")]
    [SerializeField] private AudioMixer audioMixer;

    [Header("Sliders")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    private void Start()
    {
        LoadVolume("MasterVolume", masterSlider, 0);
        LoadVolume("MusicVolume", musicSlider, 0);
        LoadVolume("SFXVolume", sfxSlider, 0);
    }

    private void SetVolume(string parameter, float dBValue)
    {
        audioMixer.SetFloat(parameter, dBValue);
        PlayerPrefs.SetFloat(parameter, dBValue);
    }

    private void LoadVolume(string parameter, Slider slider, float defaultValue)
    {
        float saved = PlayerPrefs.GetFloat(parameter, defaultValue);
        slider.SetValueWithoutNotify(saved);
        audioMixer.SetFloat(parameter, saved);
    }

    public void OnMasterVolumeChanged()
    {
        SetVolume("MasterVolume", masterSlider.value);
    }

    public void OnMusicVolumeChanged()
    {
        SetVolume("MusicVolume", musicSlider.value);
    }

    public void OnSFXVolumeChanged()
    {
        SetVolume("SFXVolume", sfxSlider.value);
    }
}
