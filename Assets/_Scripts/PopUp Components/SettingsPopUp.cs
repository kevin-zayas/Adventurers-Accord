using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsPopUp : PopUp
{
    [SerializeField] Button closeButton;
    [SerializeField] Button goBackButton;
    [SerializeField] Slider volumeSlider;
    [SerializeField] Toggle confirmationToggle;
    [SerializeField] AudioMixer masterMixer;

    protected override void Start()
    {
        base.Start();
        closeButton.onClick.AddListener(() =>
        {
            Destroy(gameObject);
        });

        goBackButton.onClick.AddListener(() =>
        {
            gameObject.SetActive(false);
            MenuPopUpManager.Instance.CreateMenuPopUp();
            Destroy(gameObject);
        });

        confirmationToggle.onValueChanged.AddListener((value) =>
        {
            PlayerPrefs.SetInt("ShowEndTurnConfirmation", value ? 1 : 0);
        });

        confirmationToggle.isOn = PlayerPrefs.GetInt("ShowEndTurnConfirmation", 0) == 1;
        SetVolume(PlayerPrefs.GetFloat("SavedMasterVolume", 50));
    }

    public void DisableGoBackButton()
    {
        goBackButton.gameObject.SetActive(false);
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
