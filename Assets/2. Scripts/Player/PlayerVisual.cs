using UnityEngine;

public class PlayerVisual : MonoBehaviour
{
    [System.Serializable]
    public struct VisualPreset
    {
        public Mesh mesh;
        public Material material;
    }

    [Header("무기 오브젝트")]
    [SerializeField] private GameObject leftWeapon;
    [SerializeField] private GameObject rightWeapon;

    [Header("캐릭터 모델 위치")]
    [SerializeField] private GameObject character;

    [Header("캐릭터 외형 (인덱스 1~8 = 승급)")]
    [SerializeField] private VisualPreset[] characterPresets = new VisualPreset[8];

    [Header("기본 무기 외형 (인덱스 0 = 승급 1~4, 1 = 승급 5~8)")]
    [SerializeField] private VisualPreset[] originWeaponPresets = new VisualPreset[2];

    [Header("오버드라이브 무기 외형 (인덱스 0 = 승급 1~4, 1 = 승급 5~8)")]
    [SerializeField] private VisualPreset[] overdriveWeaponPresets = new VisualPreset[2];

    private int currentGrade = 0; // 0 = 기본, 1~8 = 승급

    public void SetGrade(int grade)
    {
        currentGrade = Mathf.Clamp(grade-1, 0, 7);
        Debug.Log(currentGrade);
        ApplyCharacterVisual();
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