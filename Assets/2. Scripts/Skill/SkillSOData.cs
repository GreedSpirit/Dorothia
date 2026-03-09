using UnityEngine;

[CreateAssetMenu(fileName = "NewSkillData", menuName = "SkillSystem/Skill Data")]
public class SkillSOData : ScriptableObject
{
    [Header("기본 정보")]
    public int Job_Skill_Id;
    public string Skill_Name;
    public Skill_Type Skill_Type;
    public float Skill_Cooltime;
    public Skill_Target Skill_Target;
    public int Skill_Status_Id;

    [Header("리소스 연결")]
    public Sprite Skill_Icon;       // 이름으로 로드
    public AudioClip Skill_Sfx;     // 경로로 로드
    public EffectData Skill_Effect; // 이름으로 로드
    public string Skill_Animation_Path;
}