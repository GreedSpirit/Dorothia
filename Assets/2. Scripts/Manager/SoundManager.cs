using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
    static private SoundManager instance;

    static public SoundManager Instance { get => instance; private set => instance = value; }

    private void Awake()
    {
        if(instance != null && instance != this){
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    [SerializeField] private AudioSource bgmSource;

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
}
