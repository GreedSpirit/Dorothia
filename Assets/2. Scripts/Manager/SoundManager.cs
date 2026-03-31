using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public enum SFXType
{
    attack1 = 0,
    attack2 = 1,
    attack3 = 2
}

public class SoundManager : MonoBehaviour
{
    static private SoundManager instance;

    static public SoundManager Instance { get => instance; private set => instance = value; }

    //효과음 매핑용 딕셔너리
    private Dictionary<SFXType, AudioClip> sfxDict;

    // 어드레서블 용
    private Dictionary<string , AudioClip> sfxCache;

    private void Awake()
    {
        if(instance != null && instance != this){
            Destroy(gameObject);
            return;
        }

        instance = this;

        DontDestroyOnLoad(gameObject);

        sfxDict = new Dictionary<SFXType, AudioClip>();

        //SFXType enum순서대로 매핑
        foreach (SFXType type in Enum.GetValues(typeof(SFXType)))
        {
            //순서대로 해야하니까 형변환
            int index = (int)type;
            //매핑
            sfxDict[type] = sfxClip[index];
        }

    }

    private void Start()
    {
        if (SettingManager.Instance != null)
        {
            SettingManager.Instance.OnVolumeLoaded += ApplySavedVolume;
        }

        ApplySavedVolume();
    }

    private void OnDestroy()
    {
        if (SettingManager.Instance != null)
        {
            SettingManager.Instance.OnVolumeLoaded -= ApplySavedVolume;
        }
    }

    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip[] sfxClip;
    [SerializeField] private AudioMixer mainMixer;
    [SerializeField] private AudioMixerGroup bgmGroup;
    [SerializeField] private AudioMixerGroup sfxGroup;

    //볼륨 설정을 위한 파라미터 이름 (믹서와 일치해야 함)
    private const string MAIN_PARAM = "MAINVol";
    private const string BGM_PARAM = "BGMVol";
    private const string SFX_PARAM = "SFXVol";

    // 0~1 슬라이더 값을 데시벨로 변환
    public void SetVolume(string paramName, float value)
    {
        //로그 스케일 변환: 0은 -80dB, 1은 0dB (또는 그 이상)
        //유니티 오디오 믹서가 사용하는 dB(데시벨) 단위로 변환하는 공식
        float db = value <= 0 ? -80f : Mathf.Log10(value) * 20f;
        mainMixer.SetFloat(paramName, db);
    }

    public void SetMainVolume(float value) => SetVolume(MAIN_PARAM, value);
    public void SetBGMVolume(float value) => SetVolume(BGM_PARAM, value);
    public void SetSFXVolume(float value) => SetVolume(SFX_PARAM, value);

    public void PlayBGM(AudioClip clip, bool loop = true)
    {
        if (clip == null)
            return;

        if (bgmSource.clip == clip && bgmSource.isPlaying)
            return;

        bgmSource.Stop();

        bgmSource.clip = clip;
        bgmSource.loop = loop;
        bgmSource.Play();
    }

    public void PlaySFX(SFXType type)
    {
        //타입으로 키로 값찾아서
        if (sfxDict.TryGetValue(type, out var clip))
        {
            //있으면 클립에 담기
            sfxSource.PlayOneShot(clip);
        }
    }

    public void PlaySFX(string address)
    {
        if (sfxCache.TryGetValue(address, out var cached))
        {
            sfxSource.PlayOneShot(cached);
            return;
        }

        AddressableManager.Instance.LoadAsset<AudioClip>(address, clip =>
        {
            if (clip == null) return;

            sfxCache[address] = clip;   // 캐시 저장
            sfxSource.PlayOneShot(clip);
        });
    }

    public void ApplySavedVolume()
    {
        if (SettingManager.Instance == null) return;

        SetMainVolume(SettingManager.Instance.MainVolume);
        SetBGMVolume(SettingManager.Instance.BGMVolume);
        SetSFXVolume(SettingManager.Instance.SFXVolume);
    }
}
