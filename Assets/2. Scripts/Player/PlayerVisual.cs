using System;
using UnityEngine;



public class PlayerVisual : MonoBehaviour
{
    [System.Serializable]
    public struct AttackEffectGroup
    {
        public ParticleSystem attackEffect1, attackEffect2, attackEffect3;
        public ParticleSystem hitEffect1, hitEffect2, hitEffect3;
    }

    [System.Serializable]
    public struct VisualPreset
    {
        public Mesh mesh;
        public Material material;
    }

    [Header("무기 오브젝트")]
    [SerializeField] private GameObject leftWeapon;
    [SerializeField] private GameObject rightWeapon;

    [Header("이펙트 그룹")]
    [SerializeField] private AttackEffectGroup _effectGroupA; // 승급 1~4
    [SerializeField] private AttackEffectGroup _effectGroupB; // 승급 5~8

    private AttackEffectGroup _currentEffectGroup;
    private Vector3[] _originAtkPos = new Vector3[3];
    private Quaternion[] _originAtkRot = new Quaternion[3];
    private Vector3[] _originHitPos = new Vector3[3];
    private Quaternion[] _originHitRot = new Quaternion[3];

    [Header("캐릭터 모델 위치")]
    [SerializeField] private GameObject character;

    [Header("캐릭터 외형 (인덱스 1~8 = 승급)")]
    [SerializeField] private VisualPreset[] characterPresets = new VisualPreset[8];

    [Header("기본 무기 외형 (인덱스 0 = 승급 1~4, 1 = 승급 5~8)")]
    [SerializeField] private VisualPreset[] originWeaponPresets = new VisualPreset[2];

    [Header("오버드라이브 무기 외형 (인덱스 0 = 승급 1~4, 1 = 승급 5~8)")]
    [SerializeField] private VisualPreset[] overdriveWeaponPresets = new VisualPreset[2];

    private int currentGrade = 0; // 0 = 기본, 1~8 = 승급

    private OverDriveMode _odm;

    private void Awake()
    {
        _odm = GetComponent<OverDriveMode>();
    }

    private void Start()
    {
        PlayerStats.Instance.OnPromotionChanged += SetGrade;
        SetGrade(PlayerStats.Instance.CurrentPromotion);
    }

    private void OnDestroy()
    {
        if (PlayerStats.Instance != null)
            PlayerStats.Instance.OnPromotionChanged -= SetGrade;
    }

   

    public void SetGrade(int grade)
    {
        currentGrade = Mathf.Clamp(grade - 1, 0, 7);
        ApplyCharacterVisual();
        ApplyOriginVisual();
        UpdateEffectGroup(grade); // ← 외형 변경과 함께
    }

    private void UpdateEffectGroup(int grade)
    {
        _currentEffectGroup = grade <= 4 ? _effectGroupA : _effectGroupB;
        CacheCurrentGroupTransforms();
    }

    private void CacheCurrentGroupTransforms()
    {
        var g = _currentEffectGroup;
        ParticleSystem[] atks = { g.attackEffect1, g.attackEffect2, g.attackEffect3 };
        ParticleSystem[] hits = { g.hitEffect1, g.hitEffect2, g.hitEffect3 };

        for (int i = 0; i < 3; i++)
        {
            _originAtkPos[i] = atks[i].transform.localPosition;
            _originAtkRot[i] = atks[i].transform.localRotation;
            _originHitPos[i] = hits[i].transform.localPosition;
            _originHitRot[i] = hits[i].transform.localRotation;
        }
    }

    /// <summary>PlayerCtrl 애니메이션 이벤트에서 호출</summary>
    public void EnableAttackEffect(int index)
    {
        int i = index - 1;
        var g = _currentEffectGroup;

        string attackSound = "attack" + index;
        if(Enum.TryParse(attackSound, out SFXType sfx)){
            SoundManager.Instance.PlaySFX(sfx);
        }

        if (_odm.IsModeOn)
        {
            g.attackEffect3.transform.localPosition = _originAtkPos[i];
            g.attackEffect3.transform.localRotation = _originAtkRot[i];
            g.hitEffect3.transform.localPosition = _originHitPos[i];
            g.hitEffect3.transform.localRotation = _originHitRot[i];
            g.attackEffect3.Play();
            g.hitEffect3.Play();
            //SoundManager.Instance.PlaySFX(_audioClip[i]);
            return;
        }

        g.attackEffect3.transform.localPosition = _originAtkPos[2];
        g.attackEffect3.transform.localRotation = _originAtkRot[2];
        g.hitEffect3.transform.localPosition = _originHitPos[2];
        g.hitEffect3.transform.localRotation = _originHitRot[2];

        switch (index)
        {
            case 1:
                g.attackEffect1.Play(); g.hitEffect1.Play();
                break;
            case 2:
                g.attackEffect2.Play(); g.hitEffect2.Play();
                break;
            case 3:
                g.attackEffect3.Play();
                g.hitEffect1.Play(); g.hitEffect2.Play(); g.hitEffect3.Play();
                break;
        }
    }

    public void ApplyOriginVisual() => ApplyWeaponPreset(originWeaponPresets);
    public void ApplyOverdriveVisual() => ApplyWeaponPreset(overdriveWeaponPresets);

    private int WeaponPresetIndex => currentGrade >= 4 ? 1 : 0;

    private void ApplyCharacterVisual()
    {
        if (character == null) return;
        ApplyPresetToObject(character, characterPresets[currentGrade]);
        ApplyWeaponPreset(originWeaponPresets);
    }

    private void ApplyWeaponPreset(VisualPreset[] presets)
    {
        VisualPreset preset = presets[WeaponPresetIndex];
        ApplyPresetToWeapon(leftWeapon, preset);
        ApplyPresetToWeapon(rightWeapon, preset);
    }

    private void ApplyPresetToWeapon(GameObject target, VisualPreset preset)
    {
        if (target == null) return;

        if (target.TryGetComponent<MeshFilter>(out var mf))
            mf.mesh = preset.mesh;

        if (target.TryGetComponent<MeshRenderer>(out var mr))
            mr.material = preset.material;
    }

    private void ApplyPresetToObject(GameObject target, VisualPreset preset)
    {
        if (target == null) return;

        if (target.TryGetComponent<SkinnedMeshRenderer>(out var smr))
        {
            smr.sharedMesh = preset.mesh;
            smr.sharedMaterial = preset.material;
        }
    }
}