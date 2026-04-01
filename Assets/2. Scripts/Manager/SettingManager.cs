using System;
using UnityEngine;

public class SettingManager : MonoBehaviour
{
    public static SettingManager Instance;

    private const string KEY_MAIN = "MAIN_VOL";
    private const string KEY_BGM = "BGM_VOL";
    private const string KEY_SFX = "SFX_VOL";

    public Action OnVolumeLoaded;

    public float MainVolume { get; private set; } = 0.7f;
    public float BGMVolume { get; private set; } = 0.5f;
    public float SFXVolume { get; private set; } = 0.8f;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        Load();
    }

    public void SetMain(float value)
    {
        MainVolume = value;
    }

    public void SetBGM(float value)
    {
        BGMVolume = value;
    }

    public void SetSFX(float value)
    {
        SFXVolume = value;
    }

    public void Save()
    {
        PlayerPrefs.SetFloat(KEY_MAIN, MainVolume);
        PlayerPrefs.SetFloat(KEY_BGM, BGMVolume);
        PlayerPrefs.SetFloat(KEY_SFX, SFXVolume);
        PlayerPrefs.Save();
    }

    public void Load()
    {
        MainVolume = PlayerPrefs.GetFloat(KEY_MAIN, 0.7f);
        BGMVolume = PlayerPrefs.GetFloat(KEY_BGM, 0.5f);
        SFXVolume = PlayerPrefs.GetFloat(KEY_SFX, 0.8f);

        OnVolumeLoaded?.Invoke();
    }
}
