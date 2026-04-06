using System;
using System.Collections;
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
    private Dictionary<string , AudioClip> sfxCache =new Dictionary<string, AudioClip>();

    [Header("Hit SFX")]
    [SerializeField] private AudioClip[] hitClips; // 몬스터 피격음
    [SerializeField] private int maxSimultaneousHitSFX = 10; // 동시 재생 제한

    private List<AudioSource> _sfxPool = new List<AudioSource>();
    private int _currentSfxIndex = 0;

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

        InitializeSfxPool();
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

    private Coroutine _bgmFadeCoroutine;

    public void PlayBGM(AudioClip clip, bool loop = true, float fadeTime = 1f)
    {
        if (clip == null)
            return;

        if (bgmSource.clip == clip && bgmSource.isPlaying)
            return;

        if (_bgmFadeCoroutine != null)
            StopCoroutine(_bgmFadeCoroutine);

        _bgmFadeCoroutine = StartCoroutine(CoFadeBGM(clip, loop, fadeTime));
    }

    private IEnumerator CoFadeBGM(AudioClip newClip, bool loop, float fadeTime)
    {
        float startVolume = bgmSource.volume;

        //Fade Out
        float t = 0f;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            bgmSource.volume = Mathf.Lerp(startVolume, 0f, t / fadeTime);
            yield return null;
        }

        bgmSource.volume = 0f;

        //Clip 교체
        bgmSource.Stop();
        bgmSource.clip = newClip;
        bgmSource.loop = loop;
        bgmSource.Play();

        //Fade In
        t = 0f;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            bgmSource.volume = Mathf.Lerp(0f, startVolume, t / fadeTime);
            yield return null;
        }

        bgmSource.volume = startVolume;
    }

    private void InitializeSfxPool()
    {
        for (int i = 0; i < maxSimultaneousHitSFX;  i++)
        {
            GameObject gameObject = new GameObject($"SFX_{i}");
            gameObject.transform.SetParent(transform);

            var source = gameObject.AddComponent<AudioSource>();
            source.outputAudioMixerGroup = sfxGroup;
            source.playOnAwake = false;

            _sfxPool.Add(source);
        }
    }

    private void PlayLimitedSFX(AudioClip clip)
    {
        if (_sfxPool.Count == 0 || clip == null)
            return;

        var source = _sfxPool[_currentSfxIndex];

        source.Stop();
        source.clip = clip;

        source.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
        source.Play();

        _currentSfxIndex = (_currentSfxIndex + 1) % _sfxPool.Count;
    }

    public void PlayHitSFX()
    {
        if (hitClips == null || hitClips.Length == 0)
            return;

        int randomIndex = UnityEngine.Random.Range(0, hitClips.Length);
        AudioClip clip = hitClips[randomIndex];

        PlayLimitedSFX(clip);
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
