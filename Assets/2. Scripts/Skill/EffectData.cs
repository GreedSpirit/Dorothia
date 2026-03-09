using UnityEngine;

[CreateAssetMenu(fileName = "NewEffectData", menuName = "SkillSystem/Effect Data")]
public class EffectData : ScriptableObject
{
    [Header("기본 설정")]
    public string effectName;         // 풀링 시스템에서 식별자로 사용(스킬아이디랑 같아야함)
    public GameObject prefab;         // 재생할 파티클 프리팹

    [Header("재생 옵션")]
    public float duration = 2.0f;     // 이펙트가 유지되는 시간 (이후 자동으로 풀에 회수)
    public bool followTarget = false; // 타겟(캐릭터)을 따라다닐지 여부

    [Header("사운드")]
    public AudioClip soundEffect;     // 이펙트 발생 시 출력할 효과음
    [Range(0f, 1f)]
    public float soundVolume = 1.0f;
}