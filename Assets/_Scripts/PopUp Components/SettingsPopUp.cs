using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsPopUp : PopUp
{
    [SerializeField] Button closeButton;
    [SerializeField] Slider volumeSlider;
    [SerializeField] AudioMixer masterMixer;

    private void Start()
    {
        closeButton.onClick.AddListener(() =>
        {
            Destroy(gameObject);
        });

        SetVolume(PlayerPrefs.GetFloat("SavedMasterVolume", 50));
    }

    public void SetVolume(float _value)
    {
        if (_value < 1) _value = .001f;

        RefreshSlider(_value);
        PlayerPrefs.SetFloat("SavedMasterVolume", _value);
        masterMixer.SetFloat("MasterVolume", Mathf.Log10(_value / 100) * 20f);       
    }

    public void SetVolumeFromSlider()
    {
        SetVolume(volumeSlider.value);
    }

    public void RefreshSlider(float _value)
    {
        volumeSlider.value = _value;
    }
}
