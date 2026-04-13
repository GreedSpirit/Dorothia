using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OptionPopup : BaseUI
{
    [SerializeField] private Button saveBtn;
    [SerializeField] private TMP_Dropdown languageDropdown;
    [SerializeField] private Slider mainVolumeSlider;
    [SerializeField] private Slider bgmVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;

    private SystemLanguage currentLanguage = SystemLanguage.Korean;

    private void OnEnable()
    {
        EventRegister();
    }

    private void EventRegister()
    {
        saveBtn.onClick.RemoveAllListeners();
        saveBtn.onClick.AddListener(SaveOption);

        languageDropdown.onValueChanged.RemoveAllListeners();
        languageDropdown.onValueChanged.AddListener(LanaguageSettings);

        mainVolumeSlider.onValueChanged.RemoveAllListeners();
        mainVolumeSlider.onValueChanged.AddListener(value =>
        {
            SoundManager.Instance.SetMainVolume(value);
            SettingManager.Instance.SetMain(value);
        });

        bgmVolumeSlider.onValueChanged.RemoveAllListeners();
        bgmVolumeSlider.onValueChanged.AddListener(value =>
        {
            SoundManager.Instance.SetBGMVolume(value);
            SettingManager.Instance.SetBGM(value);
        });

        sfxVolumeSlider.onValueChanged.RemoveAllListeners();
        sfxVolumeSlider.onValueChanged.AddListener(value =>
        {
            SoundManager.Instance.SetSFXVolume(value);
            SettingManager.Instance.SetSFX(value);
        });
    }

    private void SaveOption()
    {
        //옵션저장하기 구현 
        //언어만 저장해주면 될 듯
        //사운드는 실시간
        SettingManager.Instance.Save();
        UIManager.Instance.CloseTopPanel();
    }

    private void LanaguageSettings(int value)
    {
        switch (value)
        {
            case 0:
                currentLanguage = SystemLanguage.Korean;
                break;
            default:
                currentLanguage = SystemLanguage.Korean;
                break;
        }
    }

    protected override void OnOpen()
    {
        mainVolumeSlider.SetValueWithoutNotify(SettingManager.Instance.MainVolume);
        bgmVolumeSlider.SetValueWithoutNotify(SettingManager.Instance.BGMVolume);
        sfxVolumeSlider.SetValueWithoutNotify(SettingManager.Instance.SFXVolume);
    }

    protected override void OnClose()
    {
    }
}
